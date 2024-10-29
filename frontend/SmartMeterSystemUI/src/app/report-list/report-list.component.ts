import { Component, OnInit } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReportService } from '../services/report.service';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../mat.module';
import { Report } from '../models/report.models';

@Component({
  selector: 'app-report-list',
  templateUrl: './report-list.component.html',
  styleUrls: ['./report-list.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule
  ]
})
export class ReportListComponent implements OnInit {
  reports: Report[] = [];
  loading = false;
  error: string | null = null;
  serialNumberControl = new FormControl('', [
    Validators.required,
    Validators.minLength(8),
    Validators.maxLength(8)
  ]);

  constructor(private reportService: ReportService) { }

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.reportService.getReports()
      .subscribe({
        next: (reports) => {
          const jsonData: Report = JSON.parse(reports[0].content as string);
          this.reports = reports.map((item) => {
            return {
              ...item,
              contentDetail: JSON.parse((item.content) as string),
            }
          });
          this.loading = false;
        },
        error: (error) => {
          // todo will be handle later
          this.loading = false;
        }
      });
  }

  createNewReport(): void {
    if (this.serialNumberControl.valid) {
      this.loading = true;
      this.reportService.createReport(this.serialNumberControl.value!)
        .subscribe({
          next: () => {
            this.loadReports();
            this.serialNumberControl.reset();
          },
          error: (error) => {
            // todo will be handle later
            this.loading = false;
          }
        });
    }
  }
}