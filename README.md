Prerequisites
--------------
.NET 8 SDK
Node.js 22+
Angular CLI

Project Structure
-----------------
FlightStatus.Api
FlightStatus.UI

Running Application
-------------------
Backend:
dotnet build
dotnet run

Frontend:
npm install
ng serve

API Endpoint
------------
GET flights/status
Example: GET /flightStatus?flightNumber=sr200&flightDate=2026-06-10

Troubleshooting
---------------
Port conflict
CORS issues
Missing node_modules
