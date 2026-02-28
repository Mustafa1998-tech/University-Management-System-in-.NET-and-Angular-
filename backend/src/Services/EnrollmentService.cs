using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class EnrollmentService
{
    private readonly UniversityDbContext _context;

    public EnrollmentService(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<List<EnrollmentResponseDto>> GetAll()
    {
        var enrollments = await _context.StudentCourses
            .Include(sc => sc.Student)
                .ThenInclude(s => s.User)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Department)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Teacher)
            .Select(sc => new EnrollmentResponseDto
            {
                Id = sc.StudentId, // Using composite key, so we'll use StudentId as identifier
                StudentId = sc.StudentId,
                CourseId = sc.CourseId,
                StudentName = sc.Student.FullName,
                StudentEmail = sc.Student.User.Email,
                CourseName = sc.Course.Name,
                DepartmentName = sc.Course.Department.Name,
                TeacherName = sc.Course.Teacher != null ? sc.Course.Teacher.FullName : null,
                EnrollmentDate = DateTime.Now // Placeholder - you might want to add this to StudentCourse entity
            })
            .ToListAsync();

        return enrollments;
    }

    public async Task<List<EnrollmentResponseDto>> GetCoursesForStudent(int studentId)
    {
        var enrollments = await _context.StudentCourses
            .Include(sc => sc.Student)
                .ThenInclude(s => s.User)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Department)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Teacher)
            .Where(sc => sc.StudentId == studentId)
            .Select(sc => new EnrollmentResponseDto
            {
                Id = sc.StudentId,
                StudentId = sc.StudentId,
                CourseId = sc.CourseId,
                StudentName = sc.Student.FullName,
                StudentEmail = sc.Student.User.Email,
                CourseName = sc.Course.Name,
                DepartmentName = sc.Course.Department.Name,
                TeacherName = sc.Course.Teacher != null ? sc.Course.Teacher.FullName : null,
                EnrollmentDate = DateTime.Now
            })
            .ToListAsync();

        return enrollments;
    }

    public async Task<List<EnrollmentResponseDto>> GetStudentsInCourse(int courseId)
    {
        var enrollments = await _context.StudentCourses
            .Include(sc => sc.Student)
                .ThenInclude(s => s.User)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Department)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Teacher)
            .Where(sc => sc.CourseId == courseId)
            .Select(sc => new EnrollmentResponseDto
            {
                Id = sc.StudentId,
                StudentId = sc.StudentId,
                CourseId = sc.CourseId,
                StudentName = sc.Student.FullName,
                StudentEmail = sc.Student.User.Email,
                CourseName = sc.Course.Name,
                DepartmentName = sc.Course.Department.Name,
                TeacherName = sc.Course.Teacher != null ? sc.Course.Teacher.FullName : null,
                EnrollmentDate = DateTime.Now
            })
            .ToListAsync();

        return enrollments;
    }

    public async Task<StudentCourse> Add(EnrollmentDto dto)
    {
        // Check if student exists
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            return null;

        // Check if course exists
        var course = await _context.Courses.FindAsync(dto.CourseId);
        if (course == null)
            return null;

        // Check if already enrolled
        var existingEnrollment = await _context.StudentCourses
            .FindAsync(dto.StudentId, dto.CourseId);

        if (existingEnrollment != null)
            return null;

        var enrollment = new StudentCourse
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId
        };

        _context.StudentCourses.Add(enrollment);
        await _context.SaveChangesAsync();

        return enrollment;
    }

    public async Task<bool> Delete(int studentId, int courseId)
    {
        var enrollment = await _context.StudentCourses
            .FindAsync(studentId, courseId);

        if (enrollment == null) 
            return false;

        _context.StudentCourses.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsStudentEnrolled(int studentId, int courseId)
    {
        return await _context.StudentCourses
            .AnyAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);
    }

    public async Task<List<EnrollmentResponseDto>> GetEnrollmentsByDepartment(int departmentId)
    {
        var enrollments = await _context.StudentCourses
            .Include(sc => sc.Student)
                .ThenInclude(s => s.User)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Department)
            .Include(sc => sc.Course)
                .ThenInclude(c => c.Teacher)
            .Where(sc => sc.Course.DepartmentId == departmentId)
            .Select(sc => new EnrollmentResponseDto
            {
                Id = sc.StudentId,
                StudentId = sc.StudentId,
                CourseId = sc.CourseId,
                StudentName = sc.Student.FullName,
                StudentEmail = sc.Student.User.Email,
                CourseName = sc.Course.Name,
                DepartmentName = sc.Course.Department.Name,
                TeacherName = sc.Course.Teacher != null ? sc.Course.Teacher.FullName : null,
                EnrollmentDate = DateTime.Now
            })
            .ToListAsync();

        return enrollments;
    }

    public async Task<int> GetStudentCourseCount(int studentId)
    {
        return await _context.StudentCourses
            .CountAsync(sc => sc.StudentId == studentId);
    }

    public async Task<int> GetCourseStudentCount(int courseId)
    {
        return await _context.StudentCourses
            .CountAsync(sc => sc.CourseId == courseId);
    }
}
