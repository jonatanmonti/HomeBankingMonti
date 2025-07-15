import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import {
  IonContent,
  IonInput,
  IonButton,
  IonItem,
  IonIcon,
  IonLabel,
  IonGrid,
  IonRow,
  IonCol,
  IonSegment,
  IonSegmentButton 
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    IonContent,
    IonInput,
    IonButton,
    IonItem,
    IonIcon,
    IonLabel,
    IonGrid,
    IonRow,
    IonCol,
    IonSegment,
    IonSegmentButton 
  ]
})
export class LoginPage {
  selectedSegment: string = 'login';
}