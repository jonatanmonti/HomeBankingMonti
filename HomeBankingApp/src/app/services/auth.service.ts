import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = '/api/auth';

constructor(private http: HttpClient) { }

register(user: any): Observable<any> {
  return this.http.post(`${this.apiUrl}/register`, user);
}

login(credentials: any): Observable<any> {
  return this.http.post(`${this.apiUrl}/login`, credentials);
}

verifyEmail(token: string): Observable<any> {
  return this.http.get(`${this.apiUrl}/verify?token=${token}`);
}
}
