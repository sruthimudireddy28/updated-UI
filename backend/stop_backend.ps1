# stop_backend.ps1
Write-Host "Stopping all running dotnet microservices..." -ForegroundColor Yellow

# Find and stop dotnet processes matching our microservices
$services = @("ApiGateway", "AuthService", "HotelService", "RoomService", "BookingService", "PaymentService", "ReviewService", "LoyaltyService")

Get-Process -Name dotnet -ErrorAction SilentlyContinue | ForEach-Object {
    $proc = $_
    $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)").CommandLine
    
    foreach ($service in $services) {
        if ($cmdLine -like "*$service*") {
            Write-Host "Stopping process for $service (PID: $($proc.Id))..." -ForegroundColor Cyan
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "Stop command finished." -ForegroundColor Green
