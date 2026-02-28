import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = environment.apiUrl;
  private readonly accessTokenKey = 'access_token';
  private readonly refreshTokenKey = 'refresh_token';

  constructor(private http: HttpClient) {}

  login(data: { email: string; password: string }) {
    return this.http
      .post<any>(`${this.baseUrl}/auth/login`, data)
      .pipe(
        tap((res) => {
          this.setTokens(res.token, res.refreshToken);
        })
      );
  }

  refreshToken() {
    const token = this.getRefreshToken();
    return this.http
      .post<any>(`${this.baseUrl}/auth/refresh`, { token })
      .pipe(
        tap((res) => {
          if (res?.token && res?.refreshToken) {
            this.setTokens(res.token, res.refreshToken);
            return;
          }

          if (res?.token) {
            this.setAccessToken(res.token);
          }
        })
      );
  }

  setTokens(access: string, refresh: string) {
    localStorage.setItem(this.accessTokenKey, access);
    localStorage.setItem(this.refreshTokenKey, refresh);
  }

  setAccessToken(access: string) {
    localStorage.setItem(this.accessTokenKey, access);
  }

  getAccessToken() {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken() {
    return localStorage.getItem(this.refreshTokenKey);
  }

  logout() {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
  }

  isLoggedIn(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;
    if (this.isTokenExpired()) return false;
    return true;
  }

  getUserRole(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return (
        payload.role ||
        payload.Role ||
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      );
    } catch (error) {
      console.error('Error parsing JWT token:', error);
      return null;
    }
  }

  getUserId(): number | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return parseInt(
        payload.UserId ||
          payload.userId ||
          payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        10
      );
    } catch (error) {
      console.error('Error parsing JWT token:', error);
      return null;
    }
  }

  getUserEmail(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    } catch (error) {
      console.error('Error parsing JWT token:', error);
      return null;
    }
  }

  isTokenExpired(): boolean {
    const token = this.getAccessToken();
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp;
      if (!exp) return false;

      return Date.now() >= exp * 1000;
    } catch (error) {
      console.error('Error parsing JWT token:', error);
      return true;
    }
  }

  getHomeRouteByRole(role: string | null = this.getUserRole()): string {
    const normalizedRole = role?.toLowerCase();

    switch (normalizedRole) {
      case 'admin':
        return '/app/admin/dashboard';
      case 'teacher':
        return '/app/teacher/dashboard';
      case 'student':
        return '/app/student/dashboard';
      default:
        return '/auth/login';
    }
  }
}
