import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface NavLink {
  label: string;
  path: string;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="layout">
      <aside class="sidebar">
        <h2>University MS</h2>
        <p class="role">{{ role || 'User' }}</p>

        <nav>
          <a
            *ngFor="let link of links"
            [routerLink]="link.path"
            routerLinkActive="active"
            class="nav-link"
          >
            {{ link.label }}
          </a>
        </nav>

        <button class="logout" (click)="logout()">Logout</button>
      </aside>

      <main class="content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .layout {
      display: grid;
      grid-template-columns: 280px 1fr;
      min-height: 100vh;
      background: var(--color-bg);
      color: var(--color-text-primary);
      font-family: 'Segoe UI', Tahoma, sans-serif;
    }

    .sidebar {
      padding: 24px 16px;
      border-right: 1px solid rgba(255, 255, 255, 0.14);
      background: linear-gradient(180deg, var(--color-primary-700) 0%, var(--color-primary-900) 100%);
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .sidebar h2 {
      margin: 0;
      font-size: 1.2rem;
      color: #ffffff;
    }

    .role {
      margin: 0 0 8px;
      color: var(--color-primary-100);
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }

    nav {
      display: flex;
      flex-direction: column;
      gap: 6px;
      overflow: auto;
      padding-right: 4px;
    }

    .nav-link {
      text-decoration: none;
      color: #ffffff;
      padding: 8px 10px;
      border-radius: 8px;
      font-size: 0.92rem;
      border: 1px solid transparent;
    }

    .nav-link:hover {
      background: rgba(255, 255, 255, 0.1);
      border-color: rgba(255, 255, 255, 0.15);
    }

    .nav-link.active {
      background: var(--color-primary-500);
      border-color: rgba(255, 255, 255, 0.2);
      font-weight: 700;
    }

    .logout {
      margin-top: auto;
      border: none;
      background: var(--color-danger);
      color: #fff;
      padding: 10px;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 600;
    }

    .content {
      padding: 24px;
      color: var(--color-text-primary);
    }

    @media (max-width: 900px) {
      .layout {
        grid-template-columns: 1fr;
      }

      .sidebar {
        border-right: none;
        border-bottom: 1px solid #d9e2ec;
      }
    }
  `]
})
export class MainLayoutComponent {
  private static readonly COMMON_LINKS: NavLink[] = [
    { label: 'My Profile', path: '/app/profile' },
    { label: 'Change Password', path: '/app/change-password' },
    { label: 'Notifications', path: '/app/notifications' },
    { label: 'Upload Profile Picture', path: '/app/upload-profile-picture' }
  ];

  private static readonly ROLE_LINKS: Record<string, NavLink[]> = {
    admin: [
      { label: 'Admin Dashboard', path: '/app/admin/dashboard' },
      { label: 'Students', path: '/app/admin/students' },
      { label: 'Teachers', path: '/app/admin/teachers' },
      { label: 'Departments', path: '/app/admin/departments' },
      { label: 'Courses', path: '/app/admin/courses' },
      { label: 'Enrollments', path: '/app/admin/enrollments' },
      { label: 'Grades', path: '/app/admin/grades' },
      { label: 'Student Details', path: '/app/admin/students/details' },
      { label: 'Teacher Details', path: '/app/admin/teachers/details' },
      { label: 'Course Details', path: '/app/admin/courses/details' },
      { label: 'Settings', path: '/app/admin/settings' }
    ],
    teacher: [
      { label: 'Teacher Dashboard', path: '/app/teacher/dashboard' },
      { label: 'My Courses', path: '/app/teacher/courses' },
      { label: 'Course Students', path: '/app/teacher/course-students' },
      { label: 'Manage Grades', path: '/app/teacher/grades' }
    ],
    student: [
      { label: 'Student Dashboard', path: '/app/student/dashboard' },
      { label: 'Available Courses', path: '/app/student/available-courses' },
      { label: 'My Enrollments', path: '/app/student/enrolled-courses' },
      { label: 'My Grades', path: '/app/student/grades' }
    ]
  };

  role: string | null = null;
  links: NavLink[] = [];

  constructor(
    private auth: AuthService,
    private router: Router
  ) {
    this.role = this.auth.getUserRole();
    const normalizedRole = this.role?.toLowerCase() ?? '';
    this.links = [
      ...MainLayoutComponent.COMMON_LINKS,
      ...(MainLayoutComponent.ROLE_LINKS[normalizedRole] ?? [])
    ];
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }
}
