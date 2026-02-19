# Voice Scheduling Agent

A fully deployed, real-time voice assistant that collects meeting details and creates a real Google Calendar event using a live backend API.

This project demonstrates end-to-end voice orchestration, LLM tool calling, OAuth-secured Google Calendar integration, and cloud deployment.

## Live Demo

**Voice Agent (Vapi Hosted Link)**  
https://vapi.ai/?demo=true&shareKey=cdcac83a-d3c4-4850-9983-d214b020ff28&assistantId=12e7c505-e3e9-4db4-bb87-ecc52d623860

**Backend API (Render Deployment)**  
https://voice-scheduling-agent-api.onrender.com

## Overview

This system allows a user to schedule a meeting entirely through voice interaction.

The assistant:
- Greets the user
- Collects their full name
- Collects preferred meeting date and time
- Collects an optional meeting title
- Confirms all details before booking
- Converts the time to ISO-8601 UTC format
- Creates a real Google Calendar event
- Responds with confirmation

The entire system is deployed and publicly accessible.

## How to Test

1. Open the Voice Agent link above.
2. Click **Start Call**.
3. Say something like:
   - `Schedule a meeting tomorrow at 4 PM Pacific.`
   - `Book a meeting next Friday at 2 PM Pacific titled "Interview Prep".`
4. Provide your name when prompted.
5. Confirm the meeting details.
6. The assistant will create a real Google Calendar event.

Successful tool calls and responses are visible in the Vapi web demo interface.

> Note: Events are created in a dedicated demo Google Calendar owned by the developer. Access to the calendar account is not required for evaluation.

## Architecture

```text
User (Voice)
  -> Vapi Assistant (LLM + Tool Calling)
    -> GET /now (Resolve Current Date Context)
      -> POST /book (.NET 8 API on Render)
        -> Google Calendar API (OAuth 2.0)
          -> Calendar Event Created

```

## Date and Time Handling

Large language models do not inherently know the current date.  
To correctly interpret relative expressions such as "today" or "tomorrow," the system:

1. Calls `GET /now` to retrieve:
   - Current UTC time
   - Local time
   - Current local date
2. Uses the returned local date as the anchor for resolving relative expressions.
3. Explicitly confirms the full calendar date including year before booking.
4. Converts confirmed times to ISO-8601 UTC strings before calling the booking endpoint.

This prevents incorrect year assumptions and ensures accurate scheduling.

---

## Calendar Integration

The system integrates directly with the Google Calendar API using OAuth 2.0.

A refresh token was generated using the Desktop App OAuth flow and stored securely in Render environment variables:

- `GOOGLE_CLIENT_ID`
- `GOOGLE_CLIENT_SECRET`
- `GOOGLE_REFRESH_TOKEN`
- `GOOGLE_CALENDAR_ID`
- `GOOGLE_TIMEZONE`

Events are created in a dedicated demo Google Calendar.

Successful bookings return:

- `bookingId`
- `htmlLink`
- Confirmed meeting details

---

## Backend API Endpoints

### `GET /health`

Health check endpoint.

```
curl https://voice-scheduling-agent-api.onrender.com/health
```
### `GET /now`

Returns current date and time context used for resolving relative date expressions.

```
curl https://voice-scheduling-agent-api.onrender.com/now
```

```Example Response:
{
  "utc": "2026-02-19T07:47:17.1716104Z",
  "timezone": "America/Vancouver",
  "local": "2026-02-18T23:47:17.1716104",
  "localDate": "2026-02-18",
  "localTime": "23:47:17"
}
```

### `GET /book`

Creates a real Google Calendar event.

```Example request:
curl -X POST https://voice-scheduling-agent-api.onrender.com/book \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Kevin Test",
    "title": "Demo Meeting",
    "startUtc": "2026-02-19T23:00:00Z",
    "endUtc": "2026-02-20T00:00:00Z",
    "timezone": "America/Vancouver"
  }'
```

```Example Response:
{
  "ok": true,
  "message": "Booked",
  "bookingId": "...",
  "htmlLink": "...",
  "confirmed": {
    "name": "Kevin Test",
    "title": "Demo Meeting",
    "start": "2026-02-19T23:00:00Z",
    "end": "2026-02-20T00:00:00Z",
    "timezone": "America/Vancouver"
  }
}
```

## Technology Stack

- **Backend:** .NET 8 (Minimal API)
- **Voice & Orchestration:** Vapi
- **Calendar Integration:** Google Calendar API (OAuth 2.0)
- **Hosting:** Render

---

## Assignment Requirements Covered

- Initiates a voice conversation
- Asks for name, preferred date/time, and optional meeting title
- Confirms final details before booking
- Creates a real calendar event
- Deployed and accessible via hosted URL

---

## Author

**Kevin Yi**  
Computer Science Graduate  
Full-stack and AI-focused Software Engineer
