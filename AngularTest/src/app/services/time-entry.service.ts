import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Entry } from '../models/entry.model';
import { environment } from '../../environments/environment'


@Injectable({
  providedIn: 'root'
})
export class TimeEntryService {
  private apiUrl = environment.apiUrl;
  private apiKey = environment.apiKey;

  constructor(private http: HttpClient) {}

  getEntries(): Observable<Entry[]> {
    return this.http.get<Entry[]>(`${this.apiUrl}?code=${this.apiKey}`);
  }
}