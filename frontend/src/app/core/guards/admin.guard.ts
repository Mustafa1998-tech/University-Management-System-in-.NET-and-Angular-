import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot
} from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (!this.auth.isLoggedIn()) {
      this.auth.logout();
      this.router.navigate(['/auth/login']);
      return false;
    }

    const role = this.auth.getUserRole();
    if (role?.toLowerCase() === 'admin') return true;

    // Redirect to appropriate dashboard based on role
    if (role?.toLowerCase() === 'student') {
      this.router.navigateByUrl('/app/student/dashboard');
    } else if (role?.toLowerCase() === 'teacher') {
      this.router.navigateByUrl('/app/teacher/dashboard');
    } else {
      this.router.navigateByUrl('/access-denied');
    }

    return false;
  }
}
