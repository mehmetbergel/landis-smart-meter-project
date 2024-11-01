import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FileType, Report } from '../models/report.models';
import { environment } from '../../environment/env';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = `${environment.reportApiUrl}/report`;
  FileType = FileType;
  constructor(private http: HttpClient) {}

  getReports(): Observable<Report[]> {
    return this.http.get<Report[]>(this.apiUrl);
  }

  createReport(meterSerialNumber: string): Observable<Report> {
    return this.http.post<Report>(this.apiUrl, { meterSerialNumber });
  }

  downloadFile(fileType: FileType) {
    return this.http.get(`${this.apiUrl}/download/${fileType}`, {
      responseType: 'blob',
      observe: 'response'
    });
  }
}
