using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

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

// GET /now
// Returns current time context for the assistant (used to resolve relative dates like "tomorrow")
app.MapGet("/now", () =>
{
    var utcNow = DateTime.UtcNow;
    var tz = GetPacificTimeZone();
    var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);

    return Results.Ok(new
    {
        utc = utcNow.ToString("o"),
        timezone = tz.Id,
        local = localNow.ToString("o"),
        localDate = localNow.ToString("yyyy-MM-dd"),
        localTime = localNow.ToString("HH:mm:ss")
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

// POST /book  (creates a real Google Calendar event)
app.MapPost("/book", async ([FromBody] BookRequest req) =>
{
    // Validate + parse times
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { ok = false, error = "Name is required." });

    if (!TryParseUtc(req.StartUtc, out var startUtc) || !TryParseUtc(req.EndUtc, out var endUtc))
        return Results.BadRequest(new { ok = false, error = "StartUtc/EndUtc must be ISO-8601 UTC like 2026-02-19T19:00:00Z" });

    if (endUtc <= startUtc)
        return Results.BadRequest(new { ok = false, error = "EndUtc must be after StartUtc." });

    // Load secrets from Render env vars
    var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
    var refreshToken = Environment.GetEnvironmentVariable("GOOGLE_REFRESH_TOKEN");
    var calendarId = Environment.GetEnvironmentVariable("GOOGLE_CALENDAR_ID");
    var tz = Environment.GetEnvironmentVariable("GOOGLE_TIMEZONE") ?? (req.Timezone ?? "America/Vancouver");

    if (string.IsNullOrWhiteSpace(clientId) ||
        string.IsNullOrWhiteSpace(clientSecret) ||
        string.IsNullOrWhiteSpace(refreshToken) ||
        string.IsNullOrWhiteSpace(calendarId))
    {
        return Results.Problem(
            title: "Server not configured",
            detail: "Missing one or more of GOOGLE_CLIENT_ID, GOOGLE_CLIENT_SECRET, GOOGLE_REFRESH_TOKEN, GOOGLE_CALENDAR_ID."
        );
    }

    // Build Google credential using refresh token
    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
    {
        ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
        Scopes = new[] { CalendarService.Scope.CalendarEvents }
    });

    var credential = new UserCredential(flow, "demo-user", new TokenResponse { RefreshToken = refreshToken });

    var calendar = new CalendarService(new BaseClientService.Initializer
    {
        HttpClientInitializer = credential,
        ApplicationName = "voice-scheduling-agent"
    });

    var title = string.IsNullOrWhiteSpace(req.Title)
        ? $"Meeting with {req.Name}"
        : req.Title;

    var ev = new Event
    {
        Summary = title,
        Description = $"Booked via deployed Voice Scheduling Agent demo. Name: {req.Name}",
        Start = new EventDateTime { DateTime = startUtc, TimeZone = "UTC" },
        End = new EventDateTime { DateTime = endUtc, TimeZone = "UTC" }
    };

    try
    {
        var created = await calendar.Events.Insert(ev, calendarId).ExecuteAsync();

        return Results.Ok(new
        {
            ok = true,
            message = "Booked",
            bookingId = created.Id,
            htmlLink = created.HtmlLink,
            confirmed = new
            {
                name = req.Name,
                title,
                start = startUtc,
                end = endUtc,
                timezone = tz
            }
        });
    }
    catch (Exception ex)
    {
        // Keep the error message lightweight (don’t leak secrets)
        return Results.Problem(title: "Google Calendar booking failed", detail: ex.Message);
    }
});

static bool TryParseUtc(string isoUtc, out DateTime utc)
{
    // Always assign out param first to satisfy the compiler.
    utc = default;

    // Parse an ISO-8601 datetime string (with Z or an offset) and normalize to UTC.
    if (!DateTimeOffset.TryParse(
            isoUtc,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var dto))
    {
        return false;
    }

    utc = dto.UtcDateTime;
    return utc != default;
}

static TimeZoneInfo GetPacificTimeZone()
{
    // Render runs on Linux where IANA IDs work. Provide fallbacks.
    var candidates = new[]
    {
        "America/Vancouver",
        "America/Los_Angeles",
        "Canada/Pacific"
    };

    foreach (var id in candidates)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch
        {
            // try next
        }
    }

    return TimeZoneInfo.Utc;
}

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
