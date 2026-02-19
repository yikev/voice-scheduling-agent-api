using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Optional but helpful for local dev + later front-end calls
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// If deploying behind HTTPS, keep HTTPS redirection on.
// app.UseHttpsRedirection();

// ---- Endpoints ----

// GET /health
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        service = "voice-scheduling-agent",
        utc = DateTime.UtcNow
    });
});

// POST /availability
app.MapPost("/availability", ([FromBody] AvailabilityRequest req) =>
{
    // Dummy availability response (we'll replace with real calendar logic later)
    return Results.Ok(new
    {
        ok = true,
        received = req,
        available = new[]
        {
            new { start = "2026-02-19T18:00:00Z", end = "2026-02-19T18:30:00Z" },
            new { start = "2026-02-19T19:00:00Z", end = "2026-02-19T19:30:00Z" }
        }
    });
});

// POST /book
app.MapPost("/book", ([FromBody] BookRequest req) =>
{
    // Dummy booking confirmation (later: create Google Calendar event here)
    return Results.Ok(new
    {
        ok = true,
        message = "Booked (dummy)",
        bookingId = Guid.NewGuid(),
        confirmed = new
        {
            name = req.Name,
            title = req.Title ?? "Meeting",
            start = req.StartUtc,
            end = req.EndUtc
        }
    });
});

app.Run();

// ---- Request DTOs ----
public record AvailabilityRequest(
    string Name,
    string? Title,
    string? Timezone,
    string? Date,          // e.g. "2026-02-19"
    string? RangeStartUtc, // e.g. "2026-02-19T17:00:00Z"
    string? RangeEndUtc    // e.g. "2026-02-19T22:00:00Z"
);

public record BookRequest(
    string Name,
    string? Title,
    string StartUtc,       // e.g. "2026-02-19T19:00:00Z"
    string EndUtc,         // e.g. "2026-02-19T19:30:00Z"
    string? Timezone
);