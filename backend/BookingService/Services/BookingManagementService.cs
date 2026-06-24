using BookingService.Data;
using BookingService.DTOs;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookingService.Services
{
    public class BookingManagementService : IBookingManagementService
    {
        private readonly BookingDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BookingManagementService(BookingDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ApiResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto request, int userId)
        {
            // Validate dates
            if (request.CheckInDate >= request.CheckOutDate)
            {
                return ApiResponse<BookingResponseDto>.FailResponse("Check-out date must be after check-in date");
            }

            if (request.CheckInDate < DateTime.UtcNow.Date)
            {
                return ApiResponse<BookingResponseDto>.FailResponse("Check-in date cannot be in the past");
            }

            // Check for overlapping bookings on the same room (non-cancelled)
            var overlappingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.RoomId == request.RoomId 
                    && b.Status != "Cancelled"
                    && request.CheckInDate < b.CheckOutDate 
                    && request.CheckOutDate > b.CheckInDate);

            if (overlappingBooking != null)
            {
                return ApiResponse<BookingResponseDto>.FailResponse($"Room is already booked from {overlappingBooking.CheckInDate:yyyy-MM-dd} to {overlappingBooking.CheckOutDate:yyyy-MM-dd}");
            }

            // Check for duplicate booking: same user booking the exact same room + dates
            var duplicateBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == userId 
                    && b.RoomId == request.RoomId 
                    && b.CheckInDate == request.CheckInDate 
                    && b.CheckOutDate == request.CheckOutDate
                    && b.Status != "Cancelled");

            if (duplicateBooking != null)
            {
                return ApiResponse<BookingResponseDto>.FailResponse("You already have a booking for this room with the same dates");
            }

            // Check room availability
            var isAvailable = await IsRoomAvailableAsync(request.RoomId, request.CheckInDate, request.CheckOutDate);
            if (!isAvailable)
            {
                return ApiResponse<BookingResponseDto>.FailResponse("Room is not available for the selected dates");
            }

            // Calculate total amount - call Room service to get price
            var totalNights = (request.CheckOutDate - request.CheckInDate).Days;
            var roomDetails = await GetRoomDetailsAsync(request.RoomId);
            var hotelName = await GetHotelNameAsync(request.HotelId);
            var totalAmount = totalNights * roomDetails.Price;

            var booking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                HotelId = request.HotelId,
                HotelName = hotelName,
                RoomType = roomDetails.RoomType,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfGuests = request.NumberOfGuests,
                TotalAmount = totalAmount,
                Status = "Pending",
                SpecialRequests = request.SpecialRequests,
                GuestName = request.GuestName,
                GuestEmail = request.GuestEmail,
                GuestPhone = request.GuestPhone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Update room availability via HttpClient
            await UpdateRoomAvailabilityAsync(request.RoomId, false);

            var response = MapToBookingResponse(booking);
            return ApiResponse<BookingResponseDto>.SuccessResponse(response, "Booking created successfully");
        }

        public async Task<ApiResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return ApiResponse<BookingResponseDto>.FailResponse("Booking not found");
            }

            var response = MapToBookingResponse(booking);
            return ApiResponse<BookingResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<BookingResponseDto>>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings.ToListAsync();
            var response = bookings.Select(MapToBookingResponse).ToList();
            return ApiResponse<List<BookingResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<BookingResponseDto>>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var response = bookings.Select(MapToBookingResponse).ToList();
            return ApiResponse<List<BookingResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<BookingResponseDto>>> GetHotelBookingsAsync(int hotelId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.HotelId == hotelId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var response = bookings.Select(MapToBookingResponse).ToList();
            return ApiResponse<List<BookingResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<BookingResponseDto>>> SearchBookingsAsync(BookingSearchDto searchDto)
        {
            var query = _context.Bookings.AsQueryable();

            if (searchDto.UserId.HasValue)
            {
                query = query.Where(b => b.UserId == searchDto.UserId.Value);
            }

            if (searchDto.HotelId.HasValue)
            {
                query = query.Where(b => b.HotelId == searchDto.HotelId.Value);
            }

            if (searchDto.RoomId.HasValue)
            {
                query = query.Where(b => b.RoomId == searchDto.RoomId.Value);
            }

            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                query = query.Where(b => b.Status == searchDto.Status);
            }

            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(b => b.CheckInDate >= searchDto.FromDate.Value);
            }

            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(b => b.CheckOutDate <= searchDto.ToDate.Value);
            }

            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            var response = bookings.Select(MapToBookingResponse).ToList();
            return ApiResponse<List<BookingResponseDto>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<BookingResponseDto>> UpdateBookingAsync(int bookingId, UpdateBookingDto request, int userId, string role)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return ApiResponse<BookingResponseDto>.FailResponse("Booking not found");
            }

            // Check permission
            if (role != "Admin" && booking.UserId != userId)
            {
                return ApiResponse<BookingResponseDto>.FailResponse("You don't have permission to update this booking");
            }

            // Cannot update cancelled or checked-out bookings
            if (booking.Status == "Cancelled" || booking.Status == "CheckedOut")
            {
                return ApiResponse<BookingResponseDto>.FailResponse("Cannot update cancelled or completed bookings");
            }

            // Update properties
            if (request.CheckInDate.HasValue) booking.CheckInDate = request.CheckInDate.Value;
            if (request.CheckOutDate.HasValue) booking.CheckOutDate = request.CheckOutDate.Value;
            if (request.NumberOfGuests.HasValue) booking.NumberOfGuests = request.NumberOfGuests.Value;
            if (!string.IsNullOrEmpty(request.SpecialRequests)) booking.SpecialRequests = request.SpecialRequests;
            if (!string.IsNullOrEmpty(request.GuestName)) booking.GuestName = request.GuestName;
            if (!string.IsNullOrEmpty(request.GuestEmail)) booking.GuestEmail = request.GuestEmail;
            if (!string.IsNullOrEmpty(request.GuestPhone)) booking.GuestPhone = request.GuestPhone;

            // Recalculate total if dates changed
            if (request.CheckInDate.HasValue || request.CheckOutDate.HasValue)
            {
                var totalNights = (booking.CheckOutDate - booking.CheckInDate).Days;
                var pricePerNight = await GetRoomPriceAsync(booking.RoomId);
                booking.TotalAmount = totalNights * pricePerNight;
            }

            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = MapToBookingResponse(booking);
            return ApiResponse<BookingResponseDto>.SuccessResponse(response, "Booking updated successfully");
        }

        public async Task<ApiResponse> CancelBookingAsync(int bookingId, CancelBookingDto request, int userId, string role)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return ApiResponse.FailResponse("Booking not found");
            }

            // Check permission
            if (role != "Admin" && booking.UserId != userId)
            {
                if (role == "Manager")
                {
                    var isManaged = await IsManagerOfHotelAsync(booking.HotelId, userId);
                    if (!isManaged)
                    {
                        return ApiResponse.FailResponse("You do not have permission to manage bookings for this hotel");
                    }
                }
                else
                {
                    return ApiResponse.FailResponse("You don't have permission to cancel this booking");
                }
            }

            // Cannot cancel already cancelled bookings
            if (booking.Status == "Cancelled")
            {
                return ApiResponse.FailResponse("Booking is already cancelled");
            }

            // Check cancellation policy (24 hours before check-in)
            if (booking.CheckInDate <= DateTime.UtcNow.AddHours(24) && role != "Admin")
            {
                return ApiResponse.FailResponse("Cannot cancel booking within 24 hours of check-in");
            }

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = request.CancellationReason;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update room availability
            await UpdateRoomAvailabilityAsync(booking.RoomId, true);

            return ApiResponse.SuccessResponse("Booking cancelled successfully");
        }

        public async Task<ApiResponse> UpdateBookingStatusAsync(int bookingId, string status)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return ApiResponse.FailResponse("Booking not found");
            }

            booking.Status = status;
            booking.UpdatedAt = DateTime.UtcNow;

            // If checked out, make room available
            if (status == "CheckedOut")
            {
                await UpdateRoomAvailabilityAsync(booking.RoomId, true);
            }

            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse($"Booking status updated to {status}");
        }

        public async Task<ApiResponse> UpdateBookingPaymentAsync(int bookingId, int paymentId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return ApiResponse.FailResponse("Booking not found");
            }

            booking.PaymentId = paymentId;
            booking.Status = "Confirmed";
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Booking payment updated successfully");
        }

        public async Task<ApiResponse<bool>> CheckRoomAvailabilityAsync(CheckAvailabilityDto request)
        {
            var isAvailable = await IsRoomAvailableAsync(request.RoomId, request.CheckInDate, request.CheckOutDate);
            return ApiResponse<bool>.SuccessResponse(isAvailable);
        }

        private async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var conflictingBooking = await _context.Bookings
                .Where(b => b.RoomId == roomId &&
                            b.Status != "Cancelled" &&
                            ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                             (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                             (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)))
                .FirstOrDefaultAsync();

            return conflictingBooking == null;
        }

        private async Task<decimal> GetRoomPriceAsync(int roomId)
        {
            try
            {
                var roomServiceUrl = _configuration["ServiceUrls:RoomService"];
                var response = await _httpClient.GetAsync($"{roomServiceUrl}/api/rooms/{roomId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<RoomApiResponse>(content, options);
                    return result?.Data?.PricePerNight ?? 100;
                }
            }
            catch
            {
                // If service call fails, return default price
            }

            return 100;
        }

        private async Task<(decimal Price, string RoomType)> GetRoomDetailsAsync(int roomId)
        {
            try
            {
                var roomServiceUrl = _configuration["ServiceUrls:RoomService"];
                var response = await _httpClient.GetAsync($"{roomServiceUrl}/api/rooms/{roomId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<RoomApiResponse>(content, options);
                    return (result?.Data?.PricePerNight ?? 100, result?.Data?.RoomType ?? "Standard Room");
                }
            }
            catch
            {
                // Fallback
            }

            return (100, "Standard Room");
        }

        private async Task<string> GetHotelNameAsync(int hotelId)
        {
            try
            {
                var hotelServiceUrl = _configuration["ServiceUrls:HotelService"];
                var response = await _httpClient.GetAsync($"{hotelServiceUrl}/api/hotels/{hotelId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<HotelApiResponse>(content, options);
                    return result?.Data?.Name ?? "Smart Hotel";
                }
            }
            catch
            {
                // Fallback
            }

            return "Smart Hotel";
        }

        private async Task UpdateRoomAvailabilityAsync(int roomId, bool isAvailable)
        {
            try
            {
                var roomServiceUrl = _configuration["ServiceUrls:RoomService"];
                var content = new StringContent(
                    JsonSerializer.Serialize(new { RoomId = roomId, IsAvailable = isAvailable }),
                    System.Text.Encoding.UTF8,
                    "application/json");

                await _httpClient.PutAsync($"{roomServiceUrl}/api/rooms/{roomId}/availability", content);
            }
            catch
            {
                // Log error but don't fail the booking
            }
        }

        private BookingResponseDto MapToBookingResponse(Booking booking)
        {
            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                RoomId = booking.RoomId,
                HotelId = booking.HotelId,
                HotelName = booking.HotelName,
                RoomType = booking.RoomType,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfGuests = booking.NumberOfGuests,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status,
                PaymentId = booking.PaymentId,
                SpecialRequests = booking.SpecialRequests,
                GuestName = booking.GuestName,
                GuestEmail = booking.GuestEmail,
                GuestPhone = booking.GuestPhone,
                CreatedAt = booking.CreatedAt,
                TotalNights = (booking.CheckOutDate - booking.CheckInDate).Days
            };
        }

        public async Task<bool> IsManagerOfHotelAsync(int hotelId, int managerId)
        {
            try
            {
                var hotelServiceUrl = _configuration["ServiceUrls:HotelService"];
                var response = await _httpClient.GetAsync($"{hotelServiceUrl}/api/hotels/{hotelId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = System.Text.Json.JsonSerializer.Deserialize<HotelApiResponse>(content, options);
                    return result?.Data?.ManagerId == managerId;
                }
            }
            catch
            {
                // Fallback to false
            }
            return false;
        }
    }

    // Helper classes for deserializing room service response
    public class RoomApiResponse
    {
        public bool Success { get; set; }
        public RoomData? Data { get; set; }
    }

    public class RoomData
    {
        public int RoomId { get; set; }
        public decimal PricePerNight { get; set; }
        public string RoomType { get; set; } = string.Empty;
    }

    public class HotelApiResponse
    {
        public bool Success { get; set; }
        public HotelData? Data { get; set; }
    }

    public class HotelData
    {
        public int HotelId { get; set; }
        public int ManagerId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
