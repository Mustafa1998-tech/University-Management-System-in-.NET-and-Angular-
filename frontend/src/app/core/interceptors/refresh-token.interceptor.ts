import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Injectable()
export class RefreshTokenInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (this.isAuthUrl(req.url)) {
      return next.handle(req);
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status !== 401) {
          return throwError(() => error);
        }

        if (!this.auth.getRefreshToken()) {
          this.forceLogout();
          return throwError(() => error);
        }

        return this.handle401Error(req, next);
      })
    );
  }

  private handle401Error(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.auth.refreshToken().pipe(
        switchMap(() => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(this.auth.getAccessToken());
          return next.handle(this.addAuthorizationHeader(req));
        }),
        catchError((error) => {
          this.isRefreshing = false;
          this.forceLogout();
          return throwError(() => error);
        })
      );
    }

    return this.refreshTokenSubject.pipe(
      filter((token): token is string => token !== null),
      take(1),
      switchMap(() => next.handle(this.addAuthorizationHeader(req)))
    );
  }

  private addAuthorizationHeader(req: HttpRequest<any>): HttpRequest<any> {
    const token = this.auth.getAccessToken();
    if (!token) {
      return req;
    }

    return req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private isAuthUrl(url: string): boolean {
    return url.includes('/auth/login') || url.includes('/auth/refresh');
  }

  private forceLogout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }
}
