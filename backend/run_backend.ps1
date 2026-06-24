# run_backend.ps1
$services = @("ApiGateway", "AuthService", "HotelService", "RoomService", "BookingService", "PaymentService", "ReviewService", "LoyaltyService")

Write-Host "Starting all Smart Hotel microservices on Windows..." -ForegroundColor Green

foreach ($service in $services) {
    Write-Host "Starting $service..." -ForegroundColor Cyan
    if (!(Test-Path "$service\Logs")) {
        New-Item -ItemType Directory -Force -Path "$service\Logs" | Out-Null
    }
    
    # Start dotnet run in a new cmd window to allow easy monitoring and logging
    Start-Process cmd -ArgumentList "/c title $service & dotnet run --project $service\$service.csproj" -WindowStyle Normal
    Start-Sleep -Seconds 1
}

Write-Host "All microservices started successfully in separate command windows!" -ForegroundColor Green
Write-Host "You can close the windows individually to stop them, or run stop_backend.ps1." -ForegroundColor Green
