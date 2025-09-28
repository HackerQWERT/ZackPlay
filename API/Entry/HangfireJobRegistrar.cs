using System;
using Application.FlightBooking;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ZackPlay.Entry;

public static class HangfireJobRegistrar
{
    private const string JobId = "generate-daily-mock-flights";

    public static async Task ConfigureAsync(WebApplication app)
    {
        // RecurringJob.AddOrUpdate<FlightDataMockService>(
        //     JobId,
        //     service => service.GenerateDailyMockFlightsAsync(),
        //     Cron.Daily(2, 0),
        //     new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

        // using var scope = app.Services.CreateScope();
        // var mockService = scope.ServiceProvider.GetRequiredService<FlightDataMockService>();
        // await mockService.GenerateDailyMockFlightsAsync();
    }
}
