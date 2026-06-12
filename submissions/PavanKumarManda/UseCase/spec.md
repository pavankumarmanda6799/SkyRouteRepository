Technical details/specifications:
----------------------------------
Implementing a feature to know flight status in the SkyRoute Platform. 
details: Customer support agent will enter flight number and travel date and the application retrieves flight details by calling multiple providers and normalize them and return as single response based on latest data from available providers.

Functionality behaving details:
-------------------------------
1. responses are from 2 providers who provides flight details (static data we create), it is not from external APIs
2. flight number and date are mandatory inputs (entered from UI)
3. if multiple providers return data of a flight, response would be which provider has latest updated data based on LastUpdated field.
4. if only one provider has data of a flight, we return that alone.
5. if no provider has data, we return status as Unknown.

Functional Requirements:
------------------------
Input : Fight Number, Travel Date (both are mandatory)
Output Response fields(Data Model):
* Flight Number
* Flight Date
* Status
* Scheduled Departure Time
* Scheduled Arrival Time
* Actual Departure Time (when available)
* Actual Arrival Time (when available)
* Terminal (when available)
* Gate (when available)
* Delay Reason (when available)
* Last Updated Timestamp
* Message

Providers: One who provides status of flight like where the flight is, scheduled and actual timings, gate at which flight could land, such kind of data those providers will provide.
----------
AeroTrack, QuickFlight
AeroTrack provides status, details like gate,terminal,delay reasons etc
QuickFlight provides minimal flight information such as status and scheduled timings only.

AeroTrack has one way indicating status and QuickFlight has another way of indicating status.
as we do normalize and return response from latestupdated provider, we are normalizing status from 2 providers as below

UnifiedFlightStatus
- OnTime
- Delayed
- Cancelled
- Diverted
- Unknown

Provider Status Mapping:
-----------------------
## AeroTrack Mapping

| AeroTrack Status | Unified Status |
| ---------------- | -------------- |
| ON_TIME          | OnTime         |
| LATE             | Delayed        |
| CANCELLED        | Cancelled      |
| DIVERTED         | Diverted       |
| Any Other Value  | Unknown        |

## QuickFlight Mapping

| QuickFlight Status | Unified Status |
| ------------------ | -------------- |
| ON_SCHEDULE        | OnTime         |
| DELAYED            | Delayed        |
| CANCELED           | Cancelled      |
| REROUTED           | Diverted       |
| Any Other Value    | Unknown        |

Response Structures & AeroTrack, QuickFlight providers structures:
------------------------------------------------------------------
FlightStatusResult 
------------------
* FlightNumber  
* Status		 
* ScheduledDepartureTime
* ScheduledArrivalTime
* ActualDepartureTime 
* ActualArrivalTime 
* Terminal 
* Gate 
* DelayReason 
* LastUpdatedTimestamp
* Message

AeroTrackResponse Model
-----------------------
* FlightNumber 
* FlightDate 
* ProviderStatus		 
* ScheduledDepartureTime
* ScheduledArrivalTime
* ActualDepartureTime 
* ActualArrivalTime 
* Terminal 
* Gate 
* DelayReason 
* LastUpdatedTimestamp

QuickFlightResponse Model
-------------------------
* FlightNumber 
* FlightDate 
* ProviderStatus		 
* ScheduledDepartureTime
* ScheduledArrivalTime
* LastUpdatedTimestamp

High level Architecture:
------------------------
Client(frontend(Angular)) -> minimal API endpoint -> FlightStatusService -> Calls Flight Providers for flight data (definition by IFlightStatus Provider) -> normalization service -> FlightStatusResult

Responsibilities of each component:

**Client 
1.agent enter flight details and send request and gets flight details in UI.

**FlightStatusService
1. Query all providers
2. Select latest provider response
3. calls Normalize service for each provider's response to get unified status 
	Provider Selection Logic
	------------------------
	Scenario 1:
	Both providers return data
	=> Select provider with latest LastUpdatedUtc

	Scenario 2:
	Only one provider returns data
	=> Use available provider response

	Scenario 3:
	No provider returns data
	=> Return Unknown status
	
**Provider Abstraction
IFlightStatusProvider

Purpose:
Provide a common contract for all flight data providers.
Methods:
Task<IEnumerable<T>> GetAsync(string flightNumber, DateTime flightDate);

this will be implemented by both AeroTrackProvider and QuickFlightProvider classes which shares info of flights.

**normalization service(FlightStatusNormalizeService)
this service will pick response provided by one or more providers and also generate unified status based on unified statuses designed and generate response of flight status.

ErrorHandling:
--------------
Validation errors - missing flight number or travel date - Return HTTP 400.

Unit tests will validate:
--------------------------
1. AeroTrack status normalization
2. QuickFlight status normalization
3. Latest LastUpdatedUtc selection
4. Single provider success scenario
5. No provider response scenario
6. Provider failure handling