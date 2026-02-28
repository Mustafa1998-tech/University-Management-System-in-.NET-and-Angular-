using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class GradeService
{
    private readonly UniversityDbContext _context;

    public GradeService(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<List<GradeResponseDto>> GetAll()
    {
        var grades = await _context.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Course)
                .ThenInclude(c => c.Department)
            .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
            .Select(g => new GradeResponseDto
            {
                Id = g.StudentId, // Using composite key, so we'll use StudentId as identifier
                StudentId = g.StudentId,
                CourseId = g.CourseId,
                Score = g.Value,
                Remarks = null, // Not in entity
                StudentName = g.Student.FullName,
                StudentEmail = g.Student.User.Email,
                CourseName = g.Course.Name,
                DepartmentName = g.Course.Department.Name,
                TeacherName = g.Course.Teacher != null ? g.Course.Teacher.FullName : null,
                GradeDate = DateTime.Now, // Not in entity, using current date
                LetterGrade = CalculateLetterGrade(g.Value)
            })
            .ToListAsync();

        return grades;
    }

    public async Task<List<GradeResponseDto>> GetGradesForStudent(int studentId)
    {
        var grades = await _context.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Course)
                .ThenInclude(c => c.Department)
            .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
            .Where(g => g.StudentId == studentId)
            .Select(g => new GradeResponseDto
            {
                Id = g.StudentId,
                StudentId = g.StudentId,
                CourseId = g.CourseId,
                Score = g.Value,
                Remarks = null,
                StudentName = g.Student.FullName,
                StudentEmail = g.Student.User.Email,
                CourseName = g.Course.Name,
                DepartmentName = g.Course.Department.Name,
                TeacherName = g.Course.Teacher != null ? g.Course.Teacher.FullName : null,
                GradeDate = DateTime.Now,
                LetterGrade = CalculateLetterGrade(g.Value)
            })
            .ToListAsync();

        return grades;
    }

    public async Task<List<GradeResponseDto>> GetGradesForCourse(int courseId)
    {
        var grades = await _context.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Course)
                .ThenInclude(c => c.Department)
            .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
            .Where(g => g.CourseId == courseId)
            .Select(g => new GradeResponseDto
            {
                Id = g.StudentId,
                StudentId = g.StudentId,
                CourseId = g.CourseId,
                Score = g.Value,
                Remarks = null,
                StudentName = g.Student.FullName,
                StudentEmail = g.Student.User.Email,
                CourseName = g.Course.Name,
                DepartmentName = g.Course.Department.Name,
                TeacherName = g.Course.Teacher != null ? g.Course.Teacher.FullName : null,
                GradeDate = DateTime.Now,
                LetterGrade = CalculateLetterGrade(g.Value)
            })
            .ToListAsync();

        return grades;
    }

    public async Task<GradeResponseDto> Add(GradeDto dto)
    {
        // Check if student exists
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            return null;

        // Check if course exists
        var course = await _context.Courses.FindAsync(dto.CourseId);
        if (course == null)
            return null;

        // Check if student is enrolled in the course
        var isEnrolled = await _context.StudentCourses
            .AnyAsync(sc => sc.StudentId == dto.StudentId && sc.CourseId == dto.CourseId);

        if (!isEnrolled)
            return null;

        // Check if grade already exists for this student and course
        var existingGrade = await _context.Grades
            .FirstOrDefaultAsync(g => g.StudentId == dto.StudentId && g.CourseId == dto.CourseId);

        if (existingGrade != null)
            return null;

        var grade = new Grade
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            Value = dto.Score
        };

        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();

        return await GetGradeById(grade.StudentId, grade.CourseId);
    }

    public async Task<GradeResponseDto> Update(int studentId, int courseId, GradeDto dto)
    {
        var grade = await _context.Grades
            .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);
        
        if (grade == null) return null;

        grade.Value = dto.Score;

        await _context.SaveChangesAsync();
        return await GetGradeById(studentId, courseId);
    }

    public async Task<bool> Delete(int studentId, int courseId)
    {
        var grade = await _context.Grades
            .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);
        
        if (grade == null) return false;

        _context.Grades.Remove(grade);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GradeResponseDto> GetGradeById(int studentId, int courseId)
    {
        var grade = await _context.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Course)
                .ThenInclude(c => c.Department)
            .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
            .Where(g => g.StudentId == studentId && g.CourseId == courseId)
            .Select(g => new GradeResponseDto
            {
                Id = g.StudentId,
                StudentId = g.StudentId,
                CourseId = g.CourseId,
                Score = g.Value,
                Remarks = null,
                StudentName = g.Student.FullName,
                StudentEmail = g.Student.User.Email,
                CourseName = g.Course.Name,
                DepartmentName = g.Course.Department.Name,
                TeacherName = g.Course.Teacher != null ? g.Course.Teacher.FullName : null,
                GradeDate = DateTime.Now,
                LetterGrade = CalculateLetterGrade(g.Value)
            })
            .FirstOrDefaultAsync();

        return grade;
    }

    public async Task<List<GradeResponseDto>> GetGradesByTeacher(int teacherId)
    {
        var grades = await _context.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Course)
                .ThenInclude(c => c.Department)
            .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
            .Where(g => g.Course.TeacherId == teacherId)
            .Select(g => new GradeResponseDto
            {
                Id = g.StudentId,
                StudentId = g.StudentId,
                CourseId = g.CourseId,
                Score = g.Value,
                Remarks = null,
                StudentName = g.Student.FullName,
                StudentEmail = g.Student.User.Email,
                CourseName = g.Course.Name,
                DepartmentName = g.Course.Department.Name,
                TeacherName = g.Course.Teacher != null ? g.Course.Teacher.FullName : null,
                GradeDate = DateTime.Now,
                LetterGrade = CalculateLetterGrade(g.Value)
            })
            .ToListAsync();

        return grades;
    }

    public async Task<GradeStatisticsDto> GetStudentStatistics(int studentId)
    {
        var grades = await _context.Grades
            .Where(g => g.StudentId == studentId)
            .ToListAsync();

        if (!grades.Any())
            return new GradeStatisticsDto { StudentId = studentId };

        return new GradeStatisticsDto
        {
            StudentId = studentId,
            AverageScore = grades.Average(g => g.Value),
            HighestScore = grades.Max(g => g.Value),
            LowestScore = grades.Min(g => g.Value),
            TotalCourses = grades.Count,
            AverageLetterGrade = CalculateLetterGrade(grades.Average(g => g.Value))
        };
    }

    public async Task<GradeStatisticsDto> GetCourseStatistics(int courseId)
    {
        var grades = await _context.Grades
            .Where(g => g.CourseId == courseId)
            .ToListAsync();

        if (!grades.Any())
            return new GradeStatisticsDto { CourseId = courseId };

        return new GradeStatisticsDto
        {
            CourseId = courseId,
            AverageScore = grades.Average(g => g.Value),
            HighestScore = grades.Max(g => g.Value),
            LowestScore = grades.Min(g => g.Value),
            TotalStudents = grades.Count,
            AverageLetterGrade = CalculateLetterGrade(grades.Average(g => g.Value))
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

public class GradeStatisticsDto
{
    public int? StudentId { get; set; }
    public int? CourseId { get; set; }
    public double AverageScore { get; set; }
    public double HighestScore { get; set; }
    public double LowestScore { get; set; }
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public string AverageLetterGrade { get; set; }
}
