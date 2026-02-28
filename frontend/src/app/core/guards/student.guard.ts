import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class StudentGuard implements CanActivate {
  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    if (!this.auth.isLoggedIn()) {
      this.auth.logout();
      this.router.navigate(['/auth/login']);
      return false;
    }

    if (this.auth.getUserRole()?.toLowerCase() === 'student') {
      return true;
    }

    this.router.navigate(['/access-denied']);
    return false;
  }
}
