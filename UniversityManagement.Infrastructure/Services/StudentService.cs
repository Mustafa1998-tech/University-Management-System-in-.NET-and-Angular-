using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class StudentService
{
    private readonly UniversityDbContext _context;

    public StudentService(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentResponseDto>> GetAll()
    {
        var students = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.FullName,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.Name,
                UserId = s.UserId,
                Email = s.User.Email,
                EnrolledCoursesCount = _context.StudentCourses.Count(sc => sc.StudentId == s.Id),
                AverageGrade = _context.Grades
                    .Where(g => g.StudentId == s.Id)
                    .Select(g => g.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageLetterGrade = CalculateLetterGrade(
                    _context.Grades
                        .Where(g => g.StudentId == s.Id)
                        .Select(g => g.Value)
                        .DefaultIfEmpty(0)
                        .Average()
                )
            })
            .ToListAsync();

        return students;
    }

    public async Task<StudentResponseDto> GetById(int id)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.FullName,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.Name,
                UserId = s.UserId,
                Email = s.User.Email,
                EnrolledCoursesCount = _context.StudentCourses.Count(sc => sc.StudentId == s.Id),
                AverageGrade = _context.Grades
                    .Where(g => g.StudentId == s.Id)
                    .Select(g => g.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageLetterGrade = CalculateLetterGrade(
                    _context.Grades
                        .Where(g => g.StudentId == s.Id)
                        .Select(g => g.Value)
                        .DefaultIfEmpty(0)
                        .Average()
                )
            })
            .FirstOrDefaultAsync();

        return student;
    }

    public async Task<StudentResponseDto> Add(StudentDto dto)
    {
        // Check if UserId already has a student record
        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == dto.UserId);

        if (existingStudent != null)
            return null;

        // Check if department exists
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return null;

        var student = new Student
        {
            FullName = dto.FullName,
            DepartmentId = dto.DepartmentId,
            UserId = dto.UserId
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return await GetById(student.Id);
    }

    public async Task<StudentResponseDto> Update(int id, StudentDto dto)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return null;

        // Check if department exists
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return null;

        student.FullName = dto.FullName;
        student.DepartmentId = dto.DepartmentId;

        await _context.SaveChangesAsync();
        return await GetById(id);
    }

    public async Task<bool> Delete(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return false;

        // Remove related records
        var enrollments = await _context.StudentCourses.Where(sc => sc.StudentId == id).ToListAsync();
        _context.StudentCourses.RemoveRange(enrollments);

        var grades = await _context.Grades.Where(g => g.StudentId == id).ToListAsync();
        _context.Grades.RemoveRange(grades);

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<StudentResponseDto>> GetByDepartment(int departmentId)
    {
        var students = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .Where(s => s.DepartmentId == departmentId)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.FullName,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.Name,
                UserId = s.UserId,
                Email = s.User.Email,
                EnrolledCoursesCount = _context.StudentCourses.Count(sc => sc.StudentId == s.Id),
                AverageGrade = _context.Grades
                    .Where(g => g.StudentId == s.Id)
                    .Select(g => g.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageLetterGrade = CalculateLetterGrade(
                    _context.Grades
                        .Where(g => g.StudentId == s.Id)
                        .Select(g => g.Value)
                        .DefaultIfEmpty(0)
                        .Average()
                )
            })
            .ToListAsync();

        return students;
    }

    public async Task<PaginatedResponseDto<StudentResponseDto>> GetPaginated(SearchParametersDto searchParams)
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchParams.Query))
        {
            query = query.Where(s => 
                s.FullName.Contains(searchParams.Query) ||
                s.User.Email.Contains(searchParams.Query));
        }

        if (searchParams.DepartmentId.HasValue)
        {
            query = query.Where(s => s.DepartmentId == searchParams.DepartmentId.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(searchParams.SortBy))
        {
            query = searchParams.SortBy.ToLower() switch
            {
                "name" => searchParams.SortDescending 
                    ? query.OrderByDescending(s => s.FullName)
                    : query.OrderBy(s => s.FullName),
                "email" => searchParams.SortDescending 
                    ? query.OrderByDescending(s => s.User.Email)
                    : query.OrderBy(s => s.User.Email),
                "department" => searchParams.SortDescending 
                    ? query.OrderByDescending(s => s.Department.Name)
                    : query.OrderBy(s => s.Department.Name),
                _ => query.OrderBy(s => s.FullName)
            };
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / searchParams.PageSize);

        var students = await query
            .Skip((searchParams.Page - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.FullName,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.Name,
                UserId = s.UserId,
                Email = s.User.Email,
                EnrolledCoursesCount = _context.StudentCourses.Count(sc => sc.StudentId == s.Id),
                AverageGrade = _context.Grades
                    .Where(g => g.StudentId == s.Id)
                    .Select(g => g.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageLetterGrade = CalculateLetterGrade(
                    _context.Grades
                        .Where(g => g.StudentId == s.Id)
                        .Select(g => g.Value)
                        .DefaultIfEmpty(0)
                        .Average()
                )
            })
            .ToListAsync();

        return new PaginatedResponseDto<StudentResponseDto>
        {
            Data = students,
            TotalCount = totalCount,
            Page = searchParams.Page,
            PageSize = searchParams.PageSize,
            TotalPages = totalPages,
            HasNextPage = searchParams.Page < totalPages,
            HasPreviousPage = searchParams.Page > 1
        };
    }

    public async Task<List<StudentResponseDto>> QuickSearch(string searchTerm, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<StudentResponseDto>();

        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .Where(s =>
                s.FullName.Contains(searchTerm) ||
                s.User.Email.Contains(searchTerm))
            .Take(limit)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.FullName,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.Name,
                UserId = s.UserId,
                Email = s.User.Email,
                EnrolledCoursesCount = _context.StudentCourses.Count(sc => sc.StudentId == s.Id),
                AverageGrade = _context.Grades
                    .Where(g => g.StudentId == s.Id)
                    .Select(g => g.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageLetterGrade = CalculateLetterGrade(
                    _context.Grades
                        .Where(g => g.StudentId == s.Id)
                        .Select(g => g.Value)
                        .DefaultIfEmpty(0)
                        .Average()
                )
            })
            .ToListAsync();
    }

    private string CalculateLetterGrade(double score)
    {
        return score switch
        {
            >= 90 => "A+",
            >= 85 => "A",
            >= 80 => "A-",
            >= 75 => "B+",
            >= 70 => "B",
            >= 65 => "B-",
            >= 60 => "C+",
            >= 55 => "C",
            >= 50 => "C-",
            >= 45 => "D+",
            >= 40 => "D",
            _ => "F"
        };
    }
}
