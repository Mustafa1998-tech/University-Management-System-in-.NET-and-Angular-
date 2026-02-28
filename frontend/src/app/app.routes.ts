import { Route, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { TeacherGuard } from './core/guards/teacher.guard';
import { StudentGuard } from './core/guards/student.guard';

interface PageDesignData {
  title: string;
  description: string;
  cards?: string[];
  formFields?: string[];
  filters?: string[];
  tableHeaders?: string[];
  actions?: string[];
  sections?: string[];
}

const pageDesign = {
  profile: {
    title: 'Profile Page',
    description: 'Editable user profile with image, contact data, and short bio.',
    formFields: ['Full Name', 'Email', 'Phone', 'Address', 'Bio'],
    sections: ['Profile Picture (Editable)'],
    actions: ['Upload New Picture', 'Save Changes']
  },
  changePassword: {
    title: 'Change Password',
    description: 'Secure password update flow with old password validation.',
    formFields: ['Old Password', 'New Password', 'Confirm Password'],
    actions: ['Change Password']
  },
  notifications: {
    title: 'Notifications',
    description: 'Notifications list with read actions and cleanup actions.',
    tableHeaders: ['Title', 'Message', 'Date', 'Mark As Read'],
    actions: ['Mark All Read', 'Delete Read'],
    sections: ['Header: Notifications']
  },
  uploadProfilePicture: {
    title: 'Upload Profile Picture',
    description: 'Upload profile image with preview and validation pipeline.',
    formFields: ['Select Image File'],
    sections: ['Preview Area', 'File Validation Rules'],
    actions: ['Upload Picture', 'Cancel']
  },
  adminDashboard: {
    title: 'Admin Dashboard',
    description: 'System-wide overview for administrators.',
    cards: ['Total Students', 'Total Teachers', 'Total Courses', 'Total Enrollments'],
    sections: [
      'Enrollment Chart',
      'Students per Department',
      'Latest Notifications (Top 5)'
    ],
    actions: ['Refresh Dashboard']
  },
  students: {
    title: 'Students Management',
    description: 'Students list with search, filtering, sorting, and paging.',
    filters: [
      'Search by name/email',
      'Filter by department',
      'Sort by name',
      'Sort by email',
      'Pagination'
    ],
    tableHeaders: ['Photo', 'Name', 'Email', 'Department', 'Actions'],
    actions: ['Add Student', 'Edit Student', 'Delete Student', 'View Details']
  },
  studentDetails: {
    title: 'Student Details',
    description: 'Student profile details, enrollments, grades, and quick actions.',
    sections: ['Profile Picture (Large)', 'Name, Email, Department, Phone, Address'],
    tableHeaders: ['Course', 'Teacher', 'Grade', 'Actions'],
    actions: ['Edit Student', 'Send Notification']
  },
  teachers: {
    title: 'Teachers Management',
    description: 'Teachers list with search, filtering, sorting, and paging.',
    filters: [
      'Search by name/email',
      'Filter by department',
      'Sort by name',
      'Sort by email',
      'Pagination'
    ],
    tableHeaders: ['Photo', 'Name', 'Email', 'Department', 'Courses Count', 'Actions'],
    actions: ['Add Teacher', 'Edit Teacher', 'Delete Teacher', 'View Details']
  },
  teacherDetails: {
    title: 'Teacher Details',
    description: 'Teacher profile, assigned courses, and related quick actions.',
    sections: ['Profile Picture', 'Name, Email, Department, Phone'],
    tableHeaders: ['Course', 'Department', 'Students Count', 'Actions'],
    actions: ['Edit Teacher', 'Send Notification']
  },
  departments: {
    title: 'Departments Management',
    description: 'Department list for administration and structure maintenance.',
    filters: ['Search by department name', 'Sort by department name'],
    tableHeaders: ['Department Name', 'Head', 'Courses Count', 'Actions'],
    actions: ['Add Department', 'Edit Department', 'Delete Department']
  },
  courses: {
    title: 'Courses Management',
    description: 'Courses list with department filtering and course actions.',
    filters: ['Search by course name', 'Filter by department'],
    tableHeaders: ['Name', 'Department', 'Students Count', 'Actions'],
    actions: ['Add Course', 'Edit Course', 'Delete Course']
  },
  courseDetails: {
    title: 'Course Details',
    description: 'Course profile with enrolled students and grade snapshots.',
    sections: ['Course Name, Department, Responsible Teacher', 'Enrollment Analytics'],
    tableHeaders: ['Student', 'Email', 'Grade', 'Actions'],
    actions: ['Edit Course', 'Send Notification']
  },
  enrollments: {
    title: 'Enrollments Management',
    description: 'Student-course registration table and enrollment actions.',
    tableHeaders: ['Student', 'Course', 'Date', 'Actions'],
    actions: ['Add Enrollment', 'Delete Enrollment']
  },
  grades: {
    title: 'Grades Management',
    description: 'Grades table with fast edit/update actions.',
    tableHeaders: ['Student', 'Course', 'Grade', 'Edit Button'],
    actions: ['Edit Grade', 'Bulk Update']
  },
  settings: {
    title: 'System Settings',
    description: 'Optional global settings screen for admin preferences.',
    sections: ['General Settings', 'Security Settings', 'Notification Preferences'],
    actions: ['Save Settings', 'Reset Defaults']
  },
  teacherDashboard: {
    title: 'Teacher Dashboard',
    description: 'Teacher overview with workload and notification context.',
    cards: ['My Courses Count', 'My Students Count'],
    sections: ['Upcoming Tasks / Grades to Update', 'Latest Notifications']
  },
  teacherCourses: {
    title: 'Teacher Courses',
    description: 'Courses assigned to the current teacher.',
    tableHeaders: ['Course', 'Students Count', 'Actions (View Students)'],
    actions: ['View Students']
  },
  teacherCourseStudents: {
    title: 'Students in Course',
    description: 'Students enrolled in a selected teacher course.',
    tableHeaders: ['Student', 'Email', 'Grade', 'Edit Grade'],
    actions: ['Add Grade', 'Update Grade']
  },
  teacherGrades: {
    title: 'Manage Grades',
    description: 'Teacher grade update workflow with filters.',
    filters: ['Filter by course', 'Search by student'],
    tableHeaders: ['Student', 'Course', 'Current Grade', 'New Grade', 'Actions'],
    actions: ['Save Grade Updates']
  },
  studentDashboard: {
    title: 'Student Dashboard',
    description: 'Student overview with enrollment and performance highlights.',
    cards: ['Enrolled Courses', 'Completed Courses', 'GPA (Optional)'],
    sections: ['Latest Grades', 'Latest Notifications']
  },
  availableCourses: {
    title: 'Available Courses',
    description: 'Course catalog for new enrollment requests.',
    filters: ['Search by course name', 'Filter by department'],
    tableHeaders: ['Course', 'Teacher', 'Seats', 'Enroll'],
    actions: ['Enroll']
  },
  enrolledCourses: {
    title: 'My Enrolled Courses',
    description: 'Current student enrollments and withdrawal actions.',
    tableHeaders: ['Course', 'Teacher', 'Status', 'Unenroll'],
    actions: ['Unenroll']
  },
  myGrades: {
    title: 'My Grades',
    description: 'Student grade table with evaluation column.',
    tableHeaders: ['Course', 'Grade', 'Evaluation'],
    sections: ['Performance Summary']
  }
} satisfies Record<string, PageDesignData>;

type PageKey = keyof typeof pageDesign;

const loadLoginComponent = () =>
  import('./modules/auth/login/login.component').then((m) => m.LoginComponent);
const loadAccessDeniedComponent = () =>
  import('./modules/shared/access-denied/access-denied.component').then(
    (m) => m.AccessDeniedComponent
  );
const loadMainLayoutComponent = () =>
  import('./modules/layout/main-layout.component').then((m) => m.MainLayoutComponent);
const loadPagePlaceholderComponent = () =>
  import('./modules/shared/page-placeholder/page-placeholder.component').then(
    (m) => m.PagePlaceholderComponent
  );

const pageRoute = (path: string, designKey: PageKey): Route => ({
  path,
  loadComponent: loadPagePlaceholderComponent,
  data: pageDesign[designKey]
});

const redirectRoute = (path: string, redirectTo: string): Route => ({
  path,
  pathMatch: 'full',
  redirectTo
});

const sharedRoutes: Routes = [
  pageRoute('profile', 'profile'),
  pageRoute('change-password', 'changePassword'),
  pageRoute('notifications', 'notifications'),
  pageRoute('upload-profile-picture', 'uploadProfilePicture')
];

const adminRoutes: Routes = [
  redirectRoute('', 'dashboard'),
  pageRoute('dashboard', 'adminDashboard'),
  pageRoute('students', 'students'),
  pageRoute('students/details', 'studentDetails'),
  pageRoute('teachers', 'teachers'),
  pageRoute('teachers/details', 'teacherDetails'),
  pageRoute('departments', 'departments'),
  pageRoute('courses', 'courses'),
  pageRoute('courses/details', 'courseDetails'),
  pageRoute('enrollments', 'enrollments'),
  pageRoute('grades', 'grades'),
  pageRoute('settings', 'settings')
];

const teacherRoutes: Routes = [
  redirectRoute('', 'dashboard'),
  pageRoute('dashboard', 'teacherDashboard'),
  pageRoute('courses', 'teacherCourses'),
  pageRoute('course-students', 'teacherCourseStudents'),
  pageRoute('grades', 'teacherGrades'),
  redirectRoute('notifications', '/app/notifications'),
  redirectRoute('profile', '/app/profile')
];

const studentRoutes: Routes = [
  redirectRoute('', 'dashboard'),
  pageRoute('dashboard', 'studentDashboard'),
  pageRoute('available-courses', 'availableCourses'),
  pageRoute('enrolled-courses', 'enrolledCourses'),
  pageRoute('grades', 'myGrades'),
  redirectRoute('notifications', '/app/notifications'),
  redirectRoute('profile', '/app/profile')
];

export const routes: Routes = [
  redirectRoute('', 'auth/login'),
  {
    path: 'auth/login',
    loadComponent: loadLoginComponent,
    canActivate: [GuestGuard]
  },
  {
    path: 'access-denied',
    loadComponent: loadAccessDeniedComponent
  },
  {
    path: 'app',
    loadComponent: loadMainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      redirectRoute('', 'profile'),
      ...sharedRoutes,
      {
        path: 'admin',
        canActivate: [AdminGuard],
        children: adminRoutes
      },
      {
        path: 'teacher',
        canActivate: [TeacherGuard],
        children: teacherRoutes
      },
      {
        path: 'student',
        canActivate: [StudentGuard],
        children: studentRoutes
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'auth/login'
  }
];
