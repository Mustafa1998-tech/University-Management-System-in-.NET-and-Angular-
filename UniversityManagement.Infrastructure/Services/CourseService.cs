using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class CourseService
{
    private readonly UniversityDbContext _context;

    public CourseService(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseResponseDto>> GetAll()
    {
        var courses = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentId = c.DepartmentId,
                TeacherId = c.TeacherId,
                DepartmentName = c.Department.Name,
                TeacherName = c.Teacher != null ? c.Teacher.FullName : null,
                StudentCount = _context.StudentCourses.Count(sc => sc.CourseId == c.Id)
            })
            .ToListAsync();

        return courses;
    }

    public async Task<CourseResponseDto> GetById(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentId = c.DepartmentId,
                TeacherId = c.TeacherId,
                DepartmentName = c.Department.Name,
                TeacherName = c.Teacher != null ? c.Teacher.FullName : null,
                StudentCount = _context.StudentCourses.Count(sc => sc.CourseId == c.Id)
            })
            .FirstOrDefaultAsync();

        return course;
    }

    public async Task<CourseResponseDto> Add(CourseDto dto)
    {
        // Check if department exists
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return null;

        // Check if teacher exists (if provided)
        if (dto.TeacherId.HasValue)
        {
            var teacher = await _context.Teachers.FindAsync(dto.TeacherId.Value);
            if (teacher == null)
                return null;
        }

        var course = new Course
        {
            Name = dto.Name,
            DepartmentId = dto.DepartmentId,
            TeacherId = dto.TeacherId
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return await GetById(course.Id);
    }

    public async Task<CourseResponseDto> Update(int id, CourseDto dto)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return null;

        // Check if department exists
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return null;

        // Check if teacher exists (if provided)
        if (dto.TeacherId.HasValue)
        {
            var teacher = await _context.Teachers.FindAsync(dto.TeacherId.Value);
            if (teacher == null)
                return null;
        }

        course.Name = dto.Name;
        course.DepartmentId = dto.DepartmentId;
        course.TeacherId = dto.TeacherId;

        await _context.SaveChangesAsync();
        return await GetById(id);
    }

    public async Task<bool> Delete(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return false;

        // Check if course has students enrolled
        var enrollments = await _context.StudentCourses
            .Where(sc => sc.CourseId == id)
            .ToListAsync();

        if (enrollments.Any())
        {
            // Remove enrollments first
            _context.StudentCourses.RemoveRange(enrollments);
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CourseResponseDto>> GetByDepartment(int departmentId)
    {
        var courses = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .Where(c => c.DepartmentId == departmentId)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentId = c.DepartmentId,
                TeacherId = c.TeacherId,
                DepartmentName = c.Department.Name,
                TeacherName = c.Teacher != null ? c.Teacher.FullName : null,
                StudentCount = _context.StudentCourses.Count(sc => sc.CourseId == c.Id)
            })
            .ToListAsync();

        return courses;
    }

    public async Task<List<CourseResponseDto>> GetByTeacher(int teacherId)
    {
        var courses = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .Where(c => c.TeacherId == teacherId)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentId = c.DepartmentId,
                TeacherId = c.TeacherId,
                DepartmentName = c.Department.Name,
                TeacherName = c.Teacher != null ? c.Teacher.FullName : null,
                StudentCount = _context.StudentCourses.Count(sc => sc.CourseId == c.Id)
            })
            .ToListAsync();

        return courses;
    }

    public async Task<List<CourseResponseDto>> GetAvailableForStudent(int studentId)
    {
        // Get student's department
        var student = await _context.Students
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null)
            return new List<CourseResponseDto>();

        // Get courses in student's department that they're not enrolled in
        var availableCourses = await _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .Where(c => c.DepartmentId == student.DepartmentId)
            .Where(c => !_context.StudentCourses
                .Any(sc => sc.CourseId == c.Id && sc.StudentId == studentId))
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentId = c.DepartmentId,
                TeacherId = c.TeacherId,
                DepartmentName = c.Department.Name,
                TeacherName = c.Teacher != null ? c.Teacher.FullName : null,
                StudentCount = _context.StudentCourses.Count(sc => sc.CourseId == c.Id)
            })
            .ToListAsync();

        return availableCourses;
    }
}
