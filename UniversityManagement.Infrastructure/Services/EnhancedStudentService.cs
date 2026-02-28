using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class EnhancedStudentService
{
    private readonly UniversityDbContext _context;

    public EnhancedStudentService(UniversityDbContext context)
    {
        _context = context;
    }

    // Enhanced Pagination with Search, Filtering, and Sorting
    public async Task<EnhancedPaginatedResponseDto<StudentResponseDto>> GetEnhancedPaginated(AdvancedSearchParametersDto searchParams)
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .AsNoTracking() // Performance optimization for read-only operations
            .AsQueryable();

        // Apply filters
        ApplyFilters(ref query, searchParams);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, searchParams.SortBy, searchParams.SortDescending);

        // Apply pagination
        var pagedQuery = query
            .Skip((searchParams.Page - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize);

        // Execute query and map to DTOs
        var students = await MapToStudentResponseDtos(pagedQuery);

        // Build response
        return new EnhancedPaginatedResponseDto<StudentResponseDto>
        {
            Data = students,
            Pagination = new PaginationMetadataDto
            {
                CurrentPage = searchParams.Page,
                PageSize = searchParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchParams.PageSize),
                HasPreviousPage = searchParams.Page > 1,
                HasNextPage = searchParams.Page < (int)Math.Ceiling((double)totalCount / searchParams.PageSize),
                FirstItemIndex = (searchParams.Page - 1) * searchParams.PageSize + 1,
                LastItemIndex = Math.Min(searchParams.Page * searchParams.PageSize, totalCount)
            },
            Filters = new FilterMetadataDto
            {
                Query = searchParams.Query,
                DepartmentId = searchParams.DepartmentId,
                TeacherId = searchParams.TeacherId,
                Role = searchParams.Role
            },
            Sorting = new SortMetadataDto
            {
                Field = searchParams.SortBy,
                Direction = searchParams.SortDescending ? "desc" : "asc"
            },
            Timestamp = DateTime.UtcNow
        };
    }

    // Advanced Search with multiple criteria
    public async Task<List<StudentResponseDto>> AdvancedSearch(AdvancedSearchParametersDto searchParams)
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .AsNoTracking()
            .AsQueryable();

        ApplyFilters(ref query, searchParams);
        query = ApplySorting(query, searchParams.SortBy, searchParams.SortDescending);

        return await MapToStudentResponseDtos(query);
    }

    // Quick Search (simplified version)
    public async Task<List<StudentResponseDto>> QuickSearch(string searchTerm, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<StudentResponseDto>();

        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .AsNoTracking()
            .Where(s => 
                s.FullName.Contains(searchTerm) ||
                s.User.Email.Contains(searchTerm) ||
                s.Department.Name.Contains(searchTerm))
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

    // Filter by Department with statistics
    public async Task<DepartmentStatisticsDto> GetDepartmentStatistics(int departmentId)
    {
        var department = await _context.Departments.FindAsync(departmentId);
        if (department == null)
            return null;

        var students = await _context.Students
            .Include(s => s.User)
            .Where(s => s.DepartmentId == departmentId)
            .ToListAsync();

        var teachers = await _context.Teachers
            .Where(t => t.DepartmentId == departmentId)
            .ToListAsync();

        var courses = await _context.Courses
            .Where(c => c.DepartmentId == departmentId)
            .ToListAsync();

        var enrollments = await _context.StudentCourses
            .Join(_context.Courses, sc => sc.CourseId, c => c.Id, (sc, c) => new { sc, c })
            .Where(x => x.c.DepartmentId == departmentId)
            .ToListAsync();

        var grades = await _context.Grades
            .Join(_context.Students, g => g.StudentId, s => s.Id, (g, s) => new { g, s })
            .Where(x => x.s.DepartmentId == departmentId)
            .ToListAsync();

        return new DepartmentStatisticsDto
        {
            DepartmentId = departmentId,
            DepartmentName = department.Name,
            TotalStudents = students.Count,
            TotalTeachers = teachers.Count,
            TotalCourses = courses.Count,
            AverageGrade = grades.Any() ? grades.Average(x => x.g.Value) : 0,
            TotalEnrollments = enrollments.Count
        };
    }

    // Bulk Operations
    public async Task<BulkOperationResultDto<StudentDto>> BulkCreateStudents(BulkOperationDto<StudentDto> bulkOperation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new BulkOperationResultDto<StudentDto>
        {
            TotalItems = bulkOperation.Items.Count,
            Results = new List<BulkOperationItemResultDto<StudentDto>>()
        };

        foreach (var studentDto in bulkOperation.Items)
        {
            try
            {
                // Validation
                if (bulkOperation.ValidateBeforeExecute)
                {
                    var existingStudent = await _context.Students
                        .FirstOrDefaultAsync(s => s.UserId == studentDto.UserId);

                    if (existingStudent != null)
                    {
                        result.Results.Add(new BulkOperationItemResultDto<StudentDto>
                        {
                            Item = studentDto,
                            Success = false,
                            Message = "Student with this UserId already exists",
                            ErrorCode = "DUPLICATE_USER"
                        });
                        continue;
                    }
                }

                // Create student
                var student = new Student
                {
                    FullName = studentDto.FullName,
                    DepartmentId = studentDto.DepartmentId,
                    UserId = studentDto.UserId
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                result.Results.Add(new BulkOperationItemResultDto<StudentDto>
                {
                    Item = studentDto,
                    Success = true,
                    Message = "Student created successfully"
                });

                result.SuccessfulItems++;
            }
            catch (Exception ex)
            {
                result.Results.Add(new BulkOperationItemResultDto<StudentDto>
                {
                    Item = studentDto,
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "CREATION_ERROR"
                });

                result.FailedItems++;

                if (bulkOperation.StopOnError)
                    break;
            }
        }

        stopwatch.Stop();
        result.ExecutionTime = stopwatch.Elapsed;

        return result;
    }

    // Export functionality
    public async Task<byte[]> ExportStudents(ExportParametersDto exportParams)
    {
        var students = await AdvancedSearch(exportParams.Filters);

        // For simplicity, return JSON bytes. In real implementation, 
        // you'd use libraries like CsvHelper or EPPlus for Excel
        var json = System.Text.Json.JsonSerializer.Serialize(students, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    // Helper methods
    private static void ApplyFilters(ref IQueryable<Student> query, AdvancedSearchParametersDto searchParams)
    {
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

        if (!string.IsNullOrEmpty(searchParams.Role))
        {
            // This would require joining with Users table
            // For simplicity, we'll skip this implementation
        }
    }

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
            "email" => descending ? query.OrderByDescending(s => s.User.Email) : query.OrderBy(s => s.User.Email),
            "department" => descending ? query.OrderByDescending(s => s.Department.Name) : query.OrderBy(s => s.Department.Name),
            "id" => descending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
            _ => query.OrderBy(s => s.Id)
        };
    }

    private static async Task<List<StudentResponseDto>> MapToStudentResponseDtos(IQueryable<Student> query)
    {
        return await query
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.FullName,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department.Name,
                UserId = s.UserId,
                Email = s.User.Email,
                EnrolledCoursesCount = 0, // Would need subquery for performance
                AverageGrade = 0, // Would need subquery for performance
                AverageLetterGrade = "N/A"
            })
            .ToListAsync();
    }

    private static string CalculateLetterGrade(double score)
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
