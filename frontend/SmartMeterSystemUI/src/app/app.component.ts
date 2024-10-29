import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { MaterialModule } from './mat.module';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: true,
  imports: [RouterModule, MaterialModule, CommonModule],
})
export class AppComponent {
  title = 'Akıllı Sayaç Verisi İşleme ve Raporlama Sistemi';
}
