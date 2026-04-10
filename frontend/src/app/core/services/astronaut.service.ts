import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timeout } from 'rxjs';
import { GetAstronautDutiesResponse } from '../models/astronaut.model';

@Injectable({
  providedIn: 'root'
})
export class AstronautService {
  private readonly API_URL = '/api/AstronautDuty';

  constructor(private http: HttpClient) {}

  getAstronautDutiesByName(name: string): Observable<GetAstronautDutiesResponse> {
    return this.http.get<GetAstronautDutiesResponse>(`${this.API_URL}/${encodeURIComponent(name)}`).pipe(
      timeout(30000)
    );
  }
}
