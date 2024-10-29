import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReportService } from '../services/report.service';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../mat.module';
import { Report } from '../models/report.models';
import { utils, write } from 'xlsx';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-report-list',
  templateUrl: './report-list.component.html',
  styleUrls: ['./report-list.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    MatIconModule,
    MatMenuModule,
    MatSortModule,
    MatTableModule
  ]
})
export class ReportListComponent implements OnInit, AfterViewInit {
  @ViewChild(MatSort) sort!: MatSort;

  public reports: Report[] = [];
  public loading = false;
  public error: string | null = null;
  public serialNumberControl = new FormControl('', [
    Validators.required,
    Validators.minLength(8),
    Validators.maxLength(8)
  ]);
  public displayedColumns: string[] = [
    'meterSerialNumber',
    'readingTime',
    'lastIndex',
    'voltageValue',
    'currentValue',
    'requestDate',
    'status'
  ];
  public dataSource: MatTableDataSource<any>;

  constructor(private reportService: ReportService) {
    this.dataSource = new MatTableDataSource(this.reports);
  }

  ngOnInit(): void {
    this.loadReports();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
    this.configureSortingAccessor();
  }

  public loadReports(): void {
    this.loading = true;
    this.reportService.getReports()
      .subscribe({
        next: (reports) => {
          this.reports = reports.map((item) => {
            return {
              ...item,
              contentDetail: this.toCamelCase(JSON.parse(item.content as string)),
            }
          });
          this.dataSource = new MatTableDataSource(this.reports);
          this.dataSource.sort = this.sort;
          this.configureSortingAccessor();
          this.loading = false;
        },
        error: (error) => {
          // todo will be handle later
          this.loading = false;
        }
      });
  }

  public createNewReport(): void {
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

  private toCamelCase<T>(obj: T): T {
    if (obj !== null && typeof obj === 'object') {
      return Object.keys(obj).reduce((acc, key) => {
        const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
        (acc as any)[camelKey] = this.toCamelCase((obj as any)[key]);
        return acc;
      }, {} as T);
    }
    return obj;
  }

  private configureSortingAccessor(): void {
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'readingTime':
          return new Date(item.contentDetail?.readingTime || '').getTime();
        case 'lastIndex':
          return Number(item.contentDetail?.lastIndex || 0);
        case 'voltageValue':
          return Number(item.contentDetail?.voltageValue || 0);
        case 'currentValue':
          return Number(item.contentDetail?.currentValue || 0);
        case 'requestDate':
          return new Date(item.requestDate || '').getTime();
        case 'status':
          return item.status;
        default:
          return item[property];
      }
    };
  }

  public downloadFile(fileType: 'excel' | 'csv' | 'text'): void {
    if (!this.reports.length) {
      this.error = 'İndirilecek rapor bulunamadı';
      return;
    }

    switch (fileType) {
      case 'excel':
        this.downloadExcel();
        break;
      case 'csv':
        this.downloadCSV();
        break;
      case 'text':
        this.downloadText();
        break;
    }
  }

  private downloadExcel(): void {
    const data = this.prepareDataForExport();

    const ws = utils.json_to_sheet(data);
    const wb = utils.book_new();
    utils.book_append_sheet(wb, ws, 'Raporlar');

    const excelBuffer = write(wb, { bookType: 'xlsx', type: 'array' });
    this.saveAsFile(excelBuffer, 'raporlar.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
  }

  private downloadCSV(): void {
    const data = this.prepareDataForExport();
    const csvContent = this.convertToCSV(data);
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    this.saveAsFile(blob, 'raporlar.csv', 'text/csv');
  }

  private downloadText(): void {
    const data = this.prepareDataForExport();
    const textContent = this.convertToText(data);
    const blob = new Blob([textContent], { type: 'text/plain;charset=utf-8;' });
    this.saveAsFile(blob, 'raporlar.txt', 'text/plain');
  }

  private saveAsFile(buffer: any, fileName: string, fileType: string): void {
    const blob = new Blob([buffer], { type: fileType });
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(link.href);
  }

  private prepareDataForExport(): any[] {
    return this.reports.map(report => ({
      'Sayaç Seri Numarası': report.meterSerialNumber,
      'Okunma Tarihi': new Date(report.contentDetail?.readingTime as Date).toLocaleString(),
      'Son Endeks': report.contentDetail?.lastIndex,
      'Voltaj': report.contentDetail?.voltageValue,
      'Akım': report.contentDetail?.currentValue,
      'Talep Tarihi': new Date(report.requestDate).toLocaleString(),
      'Durum': report.status === 1 ? 'Tamamlandı' : 'Hazırlanıyor'
    }));
  }

  private convertToCSV(data: any[]): string {
    const header = Object.keys(data[0]).join(',') + '\n';
    const rows = data.map(obj => Object.values(obj).join(','));
    return header + rows.join('\n');
  }

  private convertToText(data: any[]): string {
    const header = Object.keys(data[0]).join('\t') + '\n';
    const rows = data.map(obj => Object.values(obj).join('\t'));
    return header + rows.join('\n');
  }
}