import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface FlightStatusResultResponse {
  flightNumber: string;
  status: string;
  scheduledDepartureTime?: Date | null;
  scheduledArrivalTime?: Date | null;
  actualDepartureTime?: Date | null;
  actualArrivalTime?: Date | null;
  terminal?: string | null;
  gate?: string | null;
  delayReason?: string | null;
  lastUpdatedTimestamp?: Date | null;
  message?: string | null;
}

@Injectable({ providedIn: 'root' })
export class FlightStatusService {
  constructor(private http: HttpClient) { }

  // Convert flightDate to a UTC date-only string (YYYY-MM-DD) because we work with UTC everywhere.
  private formatUtcDate(date: Date): string {
    const pad = (value: number) => value.toString().padStart(2, '0');
    return `${date.getUTCFullYear()}-${pad(date.getUTCMonth() + 1)}-${pad(date.getUTCDate())}`;
  }

  getFlightStatus(flightNumber: string, flightDate: Date | string): Observable<FlightStatusResultResponse[]> {
    const dateStr = flightDate instanceof Date ? this.formatUtcDate(flightDate) : String(flightDate).slice(0, 10);
    const params = new HttpParams()
      .set('flightNumber', flightNumber)
      .set('flightDate', dateStr);

    const url = `${environment.apiBaseUrl}/flights/status`;

    return this.http.get<any[]>(url, { params }).pipe(
      map(res => (res || []).map(item => ({
        flightNumber: item.flightNumber ?? '',
        status: item.status ?? '',
        scheduledDepartureTime: item.scheduledDepartureTime ? new Date(item.scheduledDepartureTime) : null,
        scheduledArrivalTime: item.scheduledArrivalTime ? new Date(item.scheduledArrivalTime) : null,
        actualDepartureTime: item.actualDepartureTime ? new Date(item.actualDepartureTime) : null,
        actualArrivalTime: item.actualArrivalTime ? new Date(item.actualArrivalTime) : null,
        terminal: item.terminal ?? null,
        gate: item.gate ?? null,
        delayReason: item.delayReason ?? null,
        lastUpdatedTimestamp: item.lastUpdatedTimestamp ? new Date(item.lastUpdatedTimestamp) : null,
        message: item.message ?? null
      }))),

      catchError((err: HttpErrorResponse) => {

        let errorMessage = 'An unexpected error occurred.';

        if (err.status === 0) {
          errorMessage = 'Unable to connect to the server.';
        }
        else if (err.status === 400) {
          errorMessage =
            typeof err.error === 'string'
              ? err.error
              : 'Invalid request.';
        }
        else if (err.status >= 500) {
          errorMessage = 'Server error occurred. Please try again later.';
        }

        return throwError(() => new Error(errorMessage));
      })
    );
  }
}
