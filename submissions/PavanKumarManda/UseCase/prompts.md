Prompt 1:
---------
here is the project Context
We are building a Flight Status application using .NET and Angular.

consider below requirement
Create a response model named `FlightStatusResultResponse` with the following fields:

* FlightNumber
* Status
* ScheduledDepartureTime
* ScheduledArrivalTime
* ActualDepartureTime (optional)
* ActualArrivalTime (optional)
* Terminal (optional)
* Gate (optional)
* DelayReason (optional)
* LastUpdatedTimestamp
* Message (optional)

Use appropriate .NET data types and nullable types for optional fields.


Prompt 2:
---------
Requirement

Create sample provider data files:

* aerospace-data.json
* quickflight-data.json

The JSON structure must match the corresponding provider models located in the Models folder.

Unified Status Requirements

Create a `UnifiedFlightStatus` enum with the following values:

* OnTime
* Delayed
* Cancelled
* Diverted
* Unknown

Provider Status Mapping

AeroTrack:

* ON_TIME - OnTime
* LATE - Delayed
* CANCELLED - Cancelled
* DIVERTED - Diverted
* Any other value - Unknown

QuickFlight:

* ON_SCHEDULE - OnTime
* DELAYED - Delayed
* CANCELED - Cancelled
* REROUTED - Diverted
* Any other value - Unknown

Create data in such a way it covers below Business Scenarios

1. Flight data is sourced from static JSON files only.
2. If multiple providers return a flight, select the record with the most recent LastUpdatedTimestamp.
3. If only one provider returns a flight, return that result.
4. If no provider returns a flight, return a response with status Unknown.
5. Include FlightDate in all provider models and sample data.


Prompt 3:
---------
Requirement - Implement the interface IFlightStatusProvider.
provide implementations to AeroTrackProvider, QuickFlightProvider which inherits IFlightStatusProvider

requirement:
Read provider-specific data from the corresponding JSON file in the Data folder.
Accept flightNumber and flightDate as input parameters.
Return the matching flight record.


Prompt 4:
---------
NormalizationService Responsibilities

Map provider-specific models to FlightStatusResultResponse.
Convert provider statuses to UnifiedFlightStatus.
Contain only normalization and mapping logic.

FlightStatusService Responsibilities

Call all configured providers.
Compare LastUpdatedTimestamp values.
Select the latest provider response.
pass the response to NormalizationService to normalize and have unified status.

Do not place provider lastupdatedtime comparison logic inside NormalizationService.


Prompt 5:
---------
Refactoring Requirements

1. Replace fully qualified namespace references with appropriate using statements.
2. Reduce duplicate code across services and providers where possible.
3. Preserve existing functionality and business behavior.
4. Improve readability and maintainability.


Prompt 6:
---------
Configure CORS for Angular frontend access.

Allowed Origin - http://localhost:4200

consider below points while adding cors policy -
Store allowed origins in appsettings.json.
Read configuration values through IConfiguration.
Configure the CORS policy in Program.cs.
Avoid hardcoded origins inside Program.cs.


Prompt 7:
---------
Angular UI Requirements

Search Screen
Provide an Angular Material text input for Flight Number, DatePicker for Flight Date.
display validation and API errors in a dedicated error card beneath the inputs.

Prompt 8:
---------
do api integration for below endpoint
GET /flights/status?flightNumber={flightNumber}&flightDate={flightDate}
add endpoint details in environment.ts - http://localhost:5120

Result Display

display flight details in card-based layout.
Display all fields from FlightStatusResultResponse.
Show Gate, Terminal and DelayReason only when values are available.

Prompt 9:
---------
Status Colour Coding

OnTime -Green
Delayed -Amber
Cancelled -Red
Diverted -Red
Unknown -Grey

also include below points : 
Use exact API response field names when mapping data.
Add a header with the application title "SkyRoute".
Adjust spacing, margins and padding for a clean layout.
Display meaningful error messages for API failures.
