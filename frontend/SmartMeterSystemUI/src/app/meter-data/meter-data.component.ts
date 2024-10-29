import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MeterService } from '../services/meter-data.service';
import { MeterData } from '../models/meter-data.models';
import { MaterialModule } from '../mat.module';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-meter-data',
  templateUrl: './meter-data.component.html',
  styleUrls: ['./meter-data.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule],
})
export class MeterDataComponent implements OnInit {
  public meterForm: FormGroup;
  public meterList: MeterData[] = [];
  public modifiedMeterList: MeterData[] = [];
  public loading = false;
  public error: string | null = null;
  public showModal = false;

  constructor(private meterService: MeterService, private formBuilder: FormBuilder) {
    this.meterForm = this.formBuilder.group({
      serialNumber: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(8)]],
      lastIndex: ['', [Validators.required, Validators.min(0)]],
      voltage: ['', [Validators.required, Validators.min(0)]],
      current: ['', [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadMeters();
  }

  public onSerialNumberChange(event: Event): void {
    const input: HTMLInputElement = event.target as HTMLInputElement;
    let value = input.value;

    if (value !== '') {
      this.modifiedMeterList = this.meterList.filter((meter) => meter.serialNumber.includes(value));
    } else {
      this.modifiedMeterList = this.meterList;
    }
  }

  public loadMeters(): void {
    this.loading = true;
    this.error = null;

    this.meterService.getMeters()
      .subscribe({
        next: (meters: MeterData[]) => {
          this.meterList = meters;
          this.modifiedMeterList = meters;
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
}
