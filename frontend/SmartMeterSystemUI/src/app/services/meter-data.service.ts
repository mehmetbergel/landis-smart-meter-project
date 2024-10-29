import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MeterData } from '../models/meter-data.models';
import { environment } from '../../environment/env';

@Injectable({
    providedIn: 'root'
})
export class MeterService {
    private apiUrl = `${environment.meterDataApiUrl}/meter`;

    constructor(private http: HttpClient) { }


    getMeters(): Observable<MeterData[]> {
        return this.http.get<MeterData[]>(this.apiUrl);
    }

    addMeter(meterData: MeterData): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}`, meterData);
    }
}