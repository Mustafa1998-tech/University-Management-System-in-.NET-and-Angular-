import { Component } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loading = false;
  error: string | null = null;

  form!: ReturnType<LoginComponent['createForm']>;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.form = this.createForm();
  }

  private createForm() {
    return this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  submit() {
    if (this.form.invalid) return;

    this.loading = true;
    this.error = null;

    this.auth.login(this.form.value as any).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigateByUrl(this.auth.getHomeRouteByRole());
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || err.error || 'Login failed';
      }
    });
  }
}
