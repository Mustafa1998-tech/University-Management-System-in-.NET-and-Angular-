import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="access-denied">
      <h1>Access Denied</h1>
      <p>You do not have permission to view this page.</p>
      <a routerLink="/auth/login">Return to Login</a>
    </section>
  `,
  styles: [`
    .access-denied {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      gap: 10px;
      font-family: 'Segoe UI', Tahoma, sans-serif;
      color: var(--color-text-primary);
      background: var(--color-bg);
      text-align: center;
      padding: 20px;
    }

    h1 {
      color: var(--color-primary-700);
      margin: 0;
    }

    p {
      color: var(--color-text-secondary);
      margin: 0;
    }

    a {
      color: var(--color-primary-600);
      text-decoration: none;
      font-weight: 600;
    }
  `]
})
export class AccessDeniedComponent {}
