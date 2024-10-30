import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReportService } from '../services/report.service';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../mat.module';
import { FileType, Report } from '../models/report.models';
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

  public FileType = FileType;
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
          console.log('error :', error);
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
            console.log('error :', error);
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

  public downloadFile(fileType: FileType) {
    this.reportService.downloadFile(fileType)
      .subscribe(response => {
        const blob = response.body;
        const contentDisposition = response.headers.get('content-disposition');
        let filename = 'download';

        if (contentDisposition) {
          const match = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
          if (match && match[1]) {
            filename = match[1].replace(/['"]/g, '');
          }
        }

        const url = window.URL.createObjectURL(blob as Blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
        window.URL.revokeObjectURL(url);
      });
  }
}