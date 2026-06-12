import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FlightStatusService, FlightStatusResultResponse } from './services/flight-status.service';

/* FlightStatusResultResponse moved to service model */

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'flight-status-ui';
  flightForm!: FormGroup;
  minDate: Date = this.toUtcDate(new Date());
  errorMessage: string = '';
  showError: boolean = false;
  flightResults: FlightStatusResultResponse[] = [];
  showResults: boolean = false;

  constructor(private fb: FormBuilder, private flightService: FlightStatusService) {}

  ngOnInit() {
    this.flightForm = this.fb.group({
      flightNumber: ['', [Validators.required, Validators.minLength(2)]],
      flightDate: ['', Validators.required]
    });
  }

  onSearch() {
    this.errorMessage = '';
    this.showError = false;
    this.showResults = false;

    if (this.flightForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly.';
      this.showError = true;
      return;
    }

    const { flightNumber, flightDate } = this.flightForm.value;

    if (!flightNumber.trim()) {
      this.errorMessage = 'Flight number is required.';
      this.showError = true;
      return;
    }

    if (!flightDate) {
      this.errorMessage = 'Please select a valid date.';
      this.showError = true;
      return;
    }

    const selectedDate = this.toUtcDate(new Date(flightDate));

    this.flightService.getFlightStatus(flightNumber, selectedDate).subscribe({
      next: (results: FlightStatusResultResponse[]) => {
        this.flightResults = results;
        this.showResults = true;
      },
      error: (err) => {
        this.errorMessage = err.message;
        this.showError = true;
        console.error('API error', err);
      }
    });
  }

  resetForm() {
    this.flightForm.reset();
    this.errorMessage = '';
    this.showError = false;
    this.showResults = false;
    this.flightResults = [];
  }

  toUtcDate(date: Date): Date {
    return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
  }

  formatDate(value: Date | string | null | undefined): string {
    if (!value) {
      return '-';
    }
    const date = new Date(value);
    return date.toISOString().replace('T', ' ').replace('Z', ' UTC');
  }

  getStatusClass(status: string): string {
    const normalized = (status || '').toLowerCase();
    if (normalized === 'ontime') {
      return 'status-on-time';
    }
    if (normalized.includes('delayed')) {
      return 'status-delayed';
    }
    if (normalized.includes('cancelled') || normalized.includes('diverted')) {
      return 'status-cancelled';
    }
    return 'status-unknown';
  }
}
