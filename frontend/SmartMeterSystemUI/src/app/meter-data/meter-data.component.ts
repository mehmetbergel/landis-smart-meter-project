import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MeterService } from '../services/meter-data.service';
import { MeterData } from '../models/meter-data.models';
import { MaterialModule } from '../mat.module';
import { CommonModule } from '@angular/common';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-meter-data',
  templateUrl: './meter-data.component.html',
  styleUrls: ['./meter-data.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    MatSortModule,
    MatTableModule],
})
export class MeterDataComponent implements OnInit {
  @ViewChild(MatSort) sort!: MatSort;

  public meterForm: FormGroup;
  public meterList: MeterData[] = [];
  public modifiedMeterList: MeterData[] = [];
  public loading = false;
  public error: string | null = null;
  public showModal = false;
  public displayColumns: string[] = ['serialNumber', 'readingTime', 'lastIndex', 'voltageValue', 'currentValue']
  public dataSource: MatTableDataSource<any>;

  constructor(private meterService: MeterService, private formBuilder: FormBuilder) {
    this.dataSource = new MatTableDataSource(this.modifiedMeterList);
    this.meterForm = this.formBuilder.group({
      serialNumber: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(8)]],
      lastIndex: ['', [Validators.required, Validators.min(0)]],
      voltageValue: ['', [Validators.required, Validators.min(0)]],
      currentValue: ['', [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadMeters();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
    this.configureSortingAccessor();
  }

  public onSerialNumberChange(event: Event): void {
    const input: HTMLInputElement = event.target as HTMLInputElement;
    let value = input.value;

    if (value !== '') {
      this.modifiedMeterList = this.meterList.filter((meter) => meter.serialNumber.includes(value));
    } else {
      this.modifiedMeterList = this.meterList;
    }

    this.dataSource = new MatTableDataSource(this.modifiedMeterList);
    this.dataSource.sort = this.sort;
    this.configureSortingAccessor();
  }

  public loadMeters(): void {
    this.loading = true;
    this.error = null;

    this.meterService.getMeters()
      .subscribe({
        next: (meters: MeterData[]) => {
          this.meterList = meters;
          this.modifiedMeterList = meters;
          this.dataSource = new MatTableDataSource(this.modifiedMeterList);
          this.dataSource.sort = this.sort;
          this.configureSortingAccessor();
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
        }
      });
  }

  public openAddMeterModal(): void {
    this.showModal = true;
  }

  public closeModal(): void {
    this.showModal = false;
    this.meterForm?.reset();
  }

  public onSubmit(): void {
    if (this.meterForm?.valid) {
      this.meterService.addMeter(this.meterForm.value)
        .pipe()
        .subscribe({
          next: (response) => {
            this.loadMeters();
            this.closeModal();
            alert('Sayaç başarıyla eklendi');
          },
          error: (error) => {
            this.error = 'Sayaç eklenirken bir hata oluştu';
            this.closeModal();
          }
        });
    }
  }


  private configureSortingAccessor(): void {
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'readingTime':
          return new Date(item.readingTime || '').getTime();
        case 'currentValue':
          return Number(item.currentValue || 0);
        case 'lastIndex':
          return Number(item.lastIndex || 0);
        case 'voltageValue':
          return Number(item.voltageValue || 0);
        default:
          return item[property];
      }
    };
  }
}
