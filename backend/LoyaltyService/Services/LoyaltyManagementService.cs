using LoyaltyService.Data;
using LoyaltyService.DTOs;
using LoyaltyService.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyService.Services
{
    public class LoyaltyManagementService : ILoyaltyManagementService
    {
        private readonly LoyaltyDbContext _context;
        private readonly IConfiguration _configuration;

        // Points configuration
        private const int PointsPerDollar = 10;
        private const decimal PointValueInDollars = 0.01m;

        public LoyaltyManagementService(LoyaltyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ApiResponse<LoyaltyAccountResponseDto>> GetLoyaltyAccountAsync(int userId)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId && l.IsActive);

            if (account == null)
            {
                // Auto-create loyalty account if doesn't exist
                return await CreateLoyaltyAccountAsync(userId);
            }

            var response = MapToLoyaltyResponse(account);
            return ApiResponse<LoyaltyAccountResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<LoyaltyAccountResponseDto>> CreateLoyaltyAccountAsync(int userId)
        {
            var existingAccount = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (existingAccount != null)
            {
                if (!existingAccount.IsActive)
                {
                    existingAccount.IsActive = true;
                    existingAccount.LastUpdated = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                var response = MapToLoyaltyResponse(existingAccount);
                return ApiResponse<LoyaltyAccountResponseDto>.SuccessResponse(response);
            }

            var account = new LoyaltyAccount
            {
                UserId = userId,
                PointsBalance = 0,
                MembershipTier = "Bronze",
                MemberSince = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.LoyaltyAccounts.Add(account);
            await _context.SaveChangesAsync();

            var result = MapToLoyaltyResponse(account);
            return ApiResponse<LoyaltyAccountResponseDto>.SuccessResponse(result, "Loyalty account created successfully");
        }

        public async Task<ApiResponse<LoyaltyAccountResponseDto>> EarnPointsAsync(int userId, EarnPointsDto request)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId && l.IsActive);

            if (account == null)
            {
                var createResult = await CreateLoyaltyAccountAsync(userId);
                if (!createResult.Success)
                {
                    return ApiResponse<LoyaltyAccountResponseDto>.FailResponse("Failed to create loyalty account");
                }
                account = await _context.LoyaltyAccounts.FirstOrDefaultAsync(l => l.UserId == userId);
            }

            // Calculate points: 10 points per dollar spent
            var pointsEarned = (int)(request.BookingAmount * PointsPerDollar);

            account!.PointsBalance += pointsEarned;
            account.TotalPointsEarned += pointsEarned;
            account.LastUpdated = DateTime.UtcNow;

            // Update membership tier based on total points earned
            account.MembershipTier = GetMembershipTier(account.TotalPointsEarned);

            // Record transaction
            var transaction = new PointTransaction
            {
                UserId = userId,
                Points = pointsEarned,
                TransactionType = "Earned",
                Description = $"Points earned from booking #{request.BookingId}",
                BookingId = request.BookingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PointTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var response = MapToLoyaltyResponse(account);
            return ApiResponse<LoyaltyAccountResponseDto>.SuccessResponse(response, $"Earned {pointsEarned} points!");
        }

        public async Task<ApiResponse<RedemptionResponseDto>> RedeemPointsAsync(int userId, RedeemPointsDto request)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId && l.IsActive);

            if (account == null)
            {
                return ApiResponse<RedemptionResponseDto>.FailResponse("Loyalty account not found");
            }

            // Check if user already redeemed points on the same booking
            var existingRedemption = await _context.Redemptions
                .FirstOrDefaultAsync(r => r.UserId == userId && r.BookingId == request.BookingId);

            if (existingRedemption != null)
            {
                return ApiResponse<RedemptionResponseDto>.FailResponse($"You have already redeemed {existingRedemption.PointsUsed} points on this booking");
            }

            if (account.PointsBalance < request.PointsToRedeem)
            {
                return ApiResponse<RedemptionResponseDto>.FailResponse($"Insufficient points. Available: {account.PointsBalance}");
            }

            if (request.PointsToRedeem < 100)
            {
                return ApiResponse<RedemptionResponseDto>.FailResponse("Minimum 100 points required for redemption");
            }

            // Calculate discount amount
            var discountAmount = request.PointsToRedeem * PointValueInDollars;

            // Deduct points
            account.PointsBalance -= request.PointsToRedeem;
            account.TotalPointsRedeemed += request.PointsToRedeem;
            account.LastUpdated = DateTime.UtcNow;

            // Create redemption record
            var redemption = new Redemption
            {
                UserId = userId,
                BookingId = request.BookingId,
                PointsUsed = request.PointsToRedeem,
                DiscountAmount = discountAmount,
                Description = request.Description,
                Status = "Completed",
                RedeemedAt = DateTime.UtcNow
            };

            _context.Redemptions.Add(redemption);

            // Record transaction
            var transaction = new PointTransaction
            {
                UserId = userId,
                Points = -request.PointsToRedeem,
                TransactionType = "Redeemed",
                Description = $"Points redeemed for discount. {request.Description}",
                BookingId = request.BookingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PointTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var response = new RedemptionResponseDto
            {
                RedemptionId = redemption.RedemptionId,
                UserId = redemption.UserId,
                BookingId = redemption.BookingId,
                PointsUsed = redemption.PointsUsed,
                DiscountAmount = redemption.DiscountAmount,
                Description = redemption.Description,
                Status = redemption.Status,
                RedeemedAt = redemption.RedeemedAt
            };

            return ApiResponse<RedemptionResponseDto>.SuccessResponse(response, $"Redeemed {request.PointsToRedeem} points for ${discountAmount} discount!");
        }

        public async Task<ApiResponse<List<PointTransactionResponseDto>>> GetPointHistoryAsync(int userId)
        {
            var transactions = await _context.PointTransactions
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var response = transactions.Select(t => new PointTransactionResponseDto
            {
                TransactionId = t.TransactionId,
                UserId = t.UserId,
                Points = t.Points,
                TransactionType = t.TransactionType,
                Description = t.Description,
                BookingId = t.BookingId,
                CreatedAt = t.CreatedAt
            }).ToList();

            return ApiResponse<List<PointTransactionResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<RedemptionResponseDto>>> GetRedemptionHistoryAsync(int userId)
        {
            var redemptions = await _context.Redemptions
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RedeemedAt)
                .ToListAsync();

            var response = redemptions.Select(r => new RedemptionResponseDto
            {
                RedemptionId = r.RedemptionId,
                UserId = r.UserId,
                BookingId = r.BookingId,
                PointsUsed = r.PointsUsed,
                DiscountAmount = r.DiscountAmount,
                Description = r.Description,
                Status = r.Status,
                RedeemedAt = r.RedeemedAt
            }).ToList();

            return ApiResponse<List<RedemptionResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<DiscountResultDto>> CalculateDiscountAsync(CalculateDiscountDto request)
        {
            var discountAmount = request.PointsToUse * PointValueInDollars;

            var result = new DiscountResultDto
            {
                PointsUsed = request.PointsToUse,
                DiscountAmount = discountAmount,
                ConversionRate = PointValueInDollars
            };

            return await Task.FromResult(ApiResponse<DiscountResultDto>.SuccessResponse(result));
        }

        public async Task<ApiResponse> AddBonusPointsAsync(int userId, int points, string reason)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(l => l.UserId == userId && l.IsActive);

            if (account == null)
            {
                return ApiResponse.FailResponse("Loyalty account not found");
            }

            account.PointsBalance += points;
            account.TotalPointsEarned += points;
            account.LastUpdated = DateTime.UtcNow;

            // Update tier
            account.MembershipTier = GetMembershipTier(account.TotalPointsEarned);

            // Record transaction
            var transaction = new PointTransaction
            {
                UserId = userId,
                Points = points,
                TransactionType = "Bonus",
                Description = reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.PointTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse($"Added {points} bonus points. Reason: {reason}");
        }

        private string GetMembershipTier(int totalPointsEarned)
        {
            return totalPointsEarned switch
            {
                >= 50000 => "Platinum",
                >= 25000 => "Gold",
                >= 10000 => "Silver",
                _ => "Bronze"
            };
        }

        private LoyaltyAccountResponseDto MapToLoyaltyResponse(LoyaltyAccount account)
        {
            return new LoyaltyAccountResponseDto
            {
                LoyaltyId = account.LoyaltyId,
                UserId = account.UserId,
                PointsBalance = account.PointsBalance,
                TotalPointsEarned = account.TotalPointsEarned,
                TotalPointsRedeemed = account.TotalPointsRedeemed,
                MembershipTier = account.MembershipTier,
                MemberSince = account.MemberSince,
                LastUpdated = account.LastUpdated
            };
        }
    }
}
