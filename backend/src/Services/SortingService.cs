using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;
using UniversityManagement.Infrastructure.Helpers;

namespace UniversityManagement.Infrastructure.Services;

public class SortingService
{
    private readonly UniversityDbContext _context;

    public SortingService(UniversityDbContext context)
    {
        _context = context;
    }

    // Student sorting with full DTO mapping
    public async Task<List<StudentResponseDto>> GetSortedStudents(string sortBy = "name", string sortDirection = "asc")
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .AsQueryable();

        query = SortingHelper.ApplyStudentSorting(query, sortBy, sortDirection.ToLower() == "desc");

        return await query
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

    // Teacher sorting with full DTO mapping
    public async Task<List<TeacherResponseDto>> GetSortedTeachers(string sortBy = "name", string sortDirection = "asc")
    {
        var query = _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Department)
            .AsQueryable();

        query = SortingHelper.ApplyTeacherSorting(query, sortBy, sortDirection.ToLower() == "desc");

        return await query
            .Select(t => new TeacherResponseDto
            {
                Id = t.Id,
                FullName = t.FullName,
                DepartmentId = t.DepartmentId,
                DepartmentName = t.Department.Name,
                UserId = t.UserId,
                Email = t.User.Email,
                CoursesCount = _context.Courses.Count(c => c.TeacherId == t.Id),
                TotalStudents = _context.StudentCourses
                    .Join(_context.Courses, sc => sc.CourseId, c => c.Id, (sc, c) => new { sc, c })
                    .Where(x => x.c.TeacherId == t.Id)
                    .Select(x => x.sc.StudentId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync();
    }

    // Course sorting with full DTO mapping
    public async Task<List<CourseResponseDto>> GetSortedCourses(string sortBy = "name", string sortDirection = "asc")
    {
        var query = _context.Courses
            .Include(c => c.Department)
            .Include(c => c.Teacher)
            .AsQueryable();

        query = SortingHelper.ApplyCourseSorting(query, sortBy, sortDirection.ToLower() == "desc");

        return await query
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.Department.Name,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null ? c.Teacher.FullName : null,
                EnrolledStudentsCount = _context.StudentCourses.Count(sc => sc.CourseId == c.Id),
                AverageGrade = _context.Grades
                    .Where(g => g.CourseId == c.Id)
                    .Select(g => g.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                AverageLetterGrade = CalculateLetterGrade(
                    _context.Grades
                        .Where(g => g.CourseId == c.Id)
                        .Select(g => g.Value)
                        .DefaultIfEmpty(0)
                        .Average()
                )
            })
            .ToListAsync();
    }

    // Grade sorting with full DTO mapping
    public async Task<List<GradeResponseDto>> GetSortedGrades(string sortBy = "score", string sortDirection = "desc")
    {
        var query = _context.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Course)
                .ThenInclude(c => c.Department)
            .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
            .AsQueryable();

        query = SortingHelper.ApplyGradeSorting(query, sortBy, sortDirection.ToLower() == "desc");

        return await query
            .Select(g => new GradeResponseDto
            {
                StudentId = g.StudentId,
                CourseId = g.CourseId,
                Score = g.Value,
                LetterGrade = CalculateLetterGrade(g.Value),
                StudentName = g.Student.FullName,
                StudentEmail = g.Student.User.Email,
                CourseName = g.Course.Name,
                DepartmentName = g.Course.Department.Name,
                TeacherName = g.Course.Teacher != null ? g.Course.Teacher.FullName : null,
                GradeDate = DateTime.Now,
                Remarks = null
            })
            .ToListAsync();
    }

    // Department sorting with full DTO mapping
    public async Task<List<DepartmentResponseDto>> GetSortedDepartments(string sortBy = "name", string sortDirection = "asc")
    {
        var query = _context.Departments
            .AsQueryable();

        query = SortingHelper.ApplyDepartmentSorting(query, sortBy, sortDirection.ToLower() == "desc");

        return await query
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                StudentsCount = _context.Students.Count(s => s.DepartmentId == d.Id),
                TeachersCount = _context.Teachers.Count(t => t.DepartmentId == d.Id),
                CoursesCount = _context.Courses.Count(c => c.DepartmentId == d.Id),
                AverageGrade = _context.Grades
                    .Join(_context.Students, g => g.StudentId, s => s.Id, (g, s) => new { g, s })
                    .Where(x => x.s.DepartmentId == d.Id)
                    .Select(x => x.g.Value)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .ToListAsync();
    }

    // Multi-level sorting (sort by multiple fields)
    public async Task<List<StudentResponseDto>> GetMultiSortedStudents(string primarySort = "name", string primaryDirection = "asc", 
                                                                   string secondarySort = "email", string secondaryDirection = "asc")
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .AsQueryable();

        // Apply primary sort
        query = SortingHelper.ApplyStudentSorting(query, primarySort, primaryDirection.ToLower() == "desc");

        // Apply secondary sort (this is a simplified approach - in reality you'd use more complex expression building)
        var students = await query.ToListAsync();

        // Apply secondary sort in memory for demonstration
        students = secondarySort.ToLower() switch
        {
            "email" when secondaryDirection.ToLower() == "desc" => students.OrderByDescending(s => s.User.Email).ToList(),
            "email" => students.OrderBy(s => s.User.Email).ToList(),
            "department" when secondaryDirection.ToLower() == "desc" => students.OrderByDescending(s => s.Department.Name).ToList(),
            "department" => students.OrderBy(s => s.Department.Name).ToList(),
            _ => students
        };

        return students
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
            .ToList();
    }

    // Get sorting metadata for UI
    public object GetSortingMetadata(string entityType)
    {
        return entityType.ToLower() switch
        {
            "student" => new
            {
                AvailableFields = SortingHelper.GetStudentSortFields(),
                DefaultSort = "name",
                DefaultDirection = "asc"
            },
            "teacher" => new
            {
                AvailableFields = SortingHelper.GetTeacherSortFields(),
                DefaultSort = "name",
                DefaultDirection = "asc"
            },
            "course" => new
            {
                AvailableFields = SortingHelper.GetCourseSortFields(),
                DefaultSort = "name",
                DefaultDirection = "asc"
            },
            "grade" => new
            {
                AvailableFields = SortingHelper.GetGradeSortFields(),
                DefaultSort = "score",
                DefaultDirection = "desc"
            },
            "department" => new
            {
                AvailableFields = SortingHelper.GetDepartmentSortFields(),
                DefaultSort = "name",
                DefaultDirection = "asc"
            },
            _ => new { Error = "Unknown entity type" }
        };
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
