Voice Scheduling Agent (Deployed)

A real-time voice assistant that collects meeting details and creates a real Google Calendar event.

⸻

Live Demo

Voice Agent (Hosted via Vapi):
https://vapi.ai/?demo=true&shareKey=cdcac83a-d3c4-4850-9983-d214b020ff28&assistantId=12e7c505-e3e9-4db4-bb87-ecc52d623860

Backend API (Render Deployment):
https://voice-scheduling-agent-api.onrender.com

⸻

Overview

This project implements a deployed voice scheduling assistant capable of:
	•	Initiating a real-time voice conversation
	•	Collecting the user’s full name
	•	Collecting preferred meeting date and time
	•	Collecting an optional meeting title
	•	Confirming meeting details before booking
	•	Creating a real Google Calendar event
	•	Operating entirely through a hosted public link

The system is fully deployed and publicly accessible.

⸻

How to Test
	1.	Open the Live Voice Agent link above.
	2.	Click “Start Call.”
	3.	Say something such as:
	•	“Schedule a meeting tomorrow at 4 PM Pacific.”
	4.	Provide your name when prompted.
	5.	Confirm the meeting details.
	6.	The assistant will create a real Google Calendar event.

Tool calls and booking confirmation are visible in the web demo interface.

⸻

Architecture

User (Voice)
→ Vapi Assistant (LLM + Tool Calling)
→ GET /now (Current Date Resolution)
→ POST /book (.NET 8 API on Render)
→ Google Calendar API (OAuth 2.0)
→ Calendar Event Created

⸻

Date and Time Handling

Large language models do not have inherent access to the current date. To correctly interpret relative expressions such as “today” and “tomorrow,” this system:
	1.	Calls a backend endpoint (GET /now) to retrieve the current date and time context.
	2.	Uses the returned local date as the anchor for resolving relative time expressions.
	3.	Confirms the exact full calendar date (including year) before booking.
	4.	Converts confirmed times to ISO-8601 UTC format before calling the booking endpoint.

This ensures accurate scheduling and prevents incorrect year assumptions.

⸻

Calendar Integration

The system integrates directly with the Google Calendar API using OAuth 2.0.

A refresh token was generated using the Desktop App flow and securely stored as environment variables in Render:
	•	GOOGLE_CLIENT_ID
	•	GOOGLE_CLIENT_SECRET
	•	GOOGLE_REFRESH_TOKEN
	•	GOOGLE_CALENDAR_ID
	•	GOOGLE_TIMEZONE

Events are created in a dedicated demo Google Calendar owned by the developer.

Evaluator access to the calendar account is not required. Successful bookings are verifiable through:
	•	Tool response logs in the web demo
	•	The included demonstration video (if provided)
