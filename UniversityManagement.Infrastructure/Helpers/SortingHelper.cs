using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UniversityManagement.Domain.Entities;

namespace UniversityManagement.Infrastructure.Helpers;

public static class SortingHelper
{
    // Generic sorting method for any entity
    public static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, bool descending = false)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var methodCall = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            query.Expression,
            lambda
        );

        return query.Provider.CreateQuery<T>(methodCall);
    }

    // Student-specific sorting with multiple fields
    public static IQueryable<Student> ApplyStudentSorting(IQueryable<Student> query, string sortBy, bool descending = false)
    {
        return sortBy.ToLower() switch
        {
            "name" or "fullname" => descending 
                ? query.OrderByDescending(s => s.FullName)
                : query.OrderBy(s => s.FullName),
            "email" => descending
                ? query.OrderByDescending(s => s.User.Email)
                : query.OrderBy(s => s.User.Email),
            "department" or "departmentname" => descending
                ? query.OrderByDescending(s => s.Department.Name)
                : query.OrderBy(s => s.Department.Name),
            "id" => descending
                ? query.OrderByDescending(s => s.Id)
                : query.OrderBy(s => s.Id),
            "enrolledcourses" => descending
                ? query.OrderByDescending(s => s.StudentCourses.Count)
                : query.OrderBy(s => s.StudentCourses.Count),
            "averagegrade" => descending
                ? query.OrderByDescending(s => s.Grades.Average(g => g.Value))
                : query.OrderBy(s => s.Grades.Average(g => g.Value)),
            _ => query.OrderBy(s => s.Id)
        };
    }

    // Teacher-specific sorting
    public static IQueryable<Teacher> ApplyTeacherSorting(IQueryable<Teacher> query, string sortBy, bool descending = false)
    {
        return sortBy.ToLower() switch
        {
            "name" or "fullname" => descending 
                ? query.OrderByDescending(t => t.FullName)
                : query.OrderBy(t => t.FullName),
            "email" => descending
                ? query.OrderByDescending(t => t.User.Email)
                : query.OrderBy(t => t.User.Email),
            "department" or "departmentname" => descending
                ? query.OrderByDescending(t => t.Department.Name)
                : query.OrderBy(t => t.Department.Name),
            "id" => descending
                ? query.OrderByDescending(t => t.Id)
                : query.OrderBy(t => t.Id),
            "coursescount" => descending
                ? query.OrderByDescending(t => t.Courses.Count)
                : query.OrderBy(t => t.Courses.Count),
            _ => query.OrderBy(t => t.Id)
        };
    }

    // Course-specific sorting
    public static IQueryable<Course> ApplyCourseSorting(IQueryable<Course> query, string sortBy, bool descending = false)
    {
        return sortBy.ToLower() switch
        {
            "name" or "coursename" => descending 
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "department" or "departmentname" => descending
                ? query.OrderByDescending(c => c.Department.Name)
                : query.OrderBy(c => c.Department.Name),
            "teacher" or "teachername" => descending
                ? query.OrderByDescending(c => c.Teacher.FullName)
                : query.OrderBy(c => c.Teacher.FullName),
            "id" => descending
                ? query.OrderByDescending(c => c.Id)
                : query.OrderBy(c => c.Id),
            "studentscount" => descending
                ? query.OrderByDescending(c => c.StudentCourses.Count)
                : query.OrderBy(c => c.StudentCourses.Count),
            "averagegrade" => descending
                ? query.OrderByDescending(c => c.Grades.Average(g => g.Value))
                : query.OrderBy(c => c.Grades.Average(g => g.Value)),
            _ => query.OrderBy(c => c.Id)
        };
    }

    // Grade-specific sorting
    public static IQueryable<Grade> ApplyGradeSorting(IQueryable<Grade> query, string sortBy, bool descending = false)
    {
        return sortBy.ToLower() switch
        {
            "score" or "value" => descending 
                ? query.OrderByDescending(g => g.Value)
                : query.OrderBy(g => g.Value),
            "student" or "studentname" => descending
                ? query.OrderByDescending(g => g.Student.FullName)
                : query.OrderBy(g => g.Student.FullName),
            "course" or "coursename" => descending
                ? query.OrderByDescending(g => g.Course.Name)
                : query.OrderBy(g => g.Course.Name),
            "department" or "departmentname" => descending
                ? query.OrderByDescending(g => g.Course.Department.Name)
                : query.OrderBy(g => g.Course.Department.Name),
            "teacher" or "teachername" => descending
                ? query.OrderByDescending(g => g.Course.Teacher.FullName)
                : query.OrderBy(g => g.Course.Teacher.FullName),
            "studentid" => descending
                ? query.OrderByDescending(g => g.StudentId)
                : query.OrderBy(g => g.StudentId),
            "courseid" => descending
                ? query.OrderByDescending(g => g.CourseId)
                : query.OrderBy(g => g.CourseId),
            _ => query.OrderByDescending(g => g.Value) // Default: highest grade first
        };
    }

    // Department-specific sorting
    public static IQueryable<Department> ApplyDepartmentSorting(IQueryable<Department> query, string sortBy, bool descending = false)
    {
        return sortBy.ToLower() switch
        {
            "name" or "departmentname" => descending 
                ? query.OrderByDescending(d => d.Name)
                : query.OrderBy(d => d.Name),
            "id" => descending
                ? query.OrderByDescending(d => d.Id)
                : query.OrderBy(d => d.Id),
            "studentscount" => descending
                ? query.OrderByDescending(d => d.Students.Count)
                : query.OrderBy(d => d.Students.Count),
            "teacherscount" => descending
                ? query.OrderByDescending(d => d.Teachers.Count)
                : query.OrderBy(d => d.Teachers.Count),
            "coursescount" => descending
                ? query.OrderByDescending(d => d.Courses.Count)
                : query.OrderBy(d => d.Courses.Count),
            _ => query.OrderBy(d => d.Id)
        };
    }

    // Get available sort fields for each entity
    public static Dictionary<string, string> GetStudentSortFields()
    {
        return new Dictionary<string, string>
        {
            { "id", "Student ID" },
            { "name", "Full Name" },
            { "email", "Email Address" },
            { "department", "Department Name" },
            { "enrolledcourses", "Number of Enrolled Courses" },
            { "averagegrade", "Average Grade" }
        };
    }

    public static Dictionary<string, string> GetTeacherSortFields()
    {
        return new Dictionary<string, string>
        {
            { "id", "Teacher ID" },
            { "name", "Full Name" },
            { "email", "Email Address" },
            { "department", "Department Name" },
            { "coursescount", "Number of Courses" }
        };
    }

    public static Dictionary<string, string> GetCourseSortFields()
    {
        return new Dictionary<string, string>
        {
            { "id", "Course ID" },
            { "name", "Course Name" },
            { "department", "Department Name" },
            { "teacher", "Teacher Name" },
            { "studentscount", "Number of Students" },
            { "averagegrade", "Average Grade" }
        };
    }

    public static Dictionary<string, string> GetGradeSortFields()
    {
        return new Dictionary<string, string>
        {
            { "score", "Grade Score" },
            { "student", "Student Name" },
            { "course", "Course Name" },
            { "department", "Department Name" },
            { "teacher", "Teacher Name" }
        };
    }

    public static Dictionary<string, string> GetDepartmentSortFields()
    {
        return new Dictionary<string, string>
        {
            { "id", "Department ID" },
            { "name", "Department Name" },
            { "studentscount", "Number of Students" },
            { "teacherscount", "Number of Teachers" },
            { "coursescount", "Number of Courses" }
        };
    }
}
