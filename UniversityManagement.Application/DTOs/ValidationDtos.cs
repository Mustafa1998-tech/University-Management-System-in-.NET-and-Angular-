using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Application.DTOs.Validation;

// Enhanced StudentDto with comprehensive validation
public class StudentDto
{
    [Required(ErrorMessage = "Full name is required")]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Department ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid user")]
    public int UserId { get; set; }
}

// Enhanced TeacherDto with comprehensive validation
public class TeacherDto
{
    [Required(ErrorMessage = "Full name is required")]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Department ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid user")]
    public int UserId { get; set; }
}

// Enhanced CourseDto with comprehensive validation
public class CourseDto
{
    [Required(ErrorMessage = "Course name is required")]
    [MinLength(3, ErrorMessage = "Course name must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "Course name can only contain letters, numbers, spaces, and hyphens")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Department ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department")]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid teacher")]
    public int? TeacherId { get; set; }
}

// Enhanced GradeDto with comprehensive validation
public class GradeDto
{
    [Required(ErrorMessage = "Student ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid student")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Course ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Score is required")]
    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Score can have at most 2 decimal places")]
    public double Score { get; set; }

    [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string Remarks { get; set; }
}

// Enhanced EnrollmentDto with comprehensive validation
public class EnrollmentDto
{
    [Required(ErrorMessage = "Student ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid student")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Course ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid course")]
    public int CourseId { get; set; }
}

// Enhanced RegisterDto with comprehensive validation
public class RegisterDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [RegularExpression(@"^(Student|Teacher|Admin)$", ErrorMessage = "Role must be Student, Teacher, or Admin")]
    public string Role { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can only contain letters and spaces")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Department ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department")]
    public int DepartmentId { get; set; }
}

// Enhanced LoginDto with comprehensive validation
public class LoginDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(1, ErrorMessage = "Password is required")]
    public string Password { get; set; }
}

// DepartmentDto with validation
public class DepartmentDto
{
    [Required(ErrorMessage = "Department name is required")]
    [MinLength(3, ErrorMessage = "Department name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z\s\-]+$", ErrorMessage = "Department name can only contain letters, spaces, and hyphens")]
    public string Name { get; set; }
}

// Bulk operation DTOs with validation
public class BulkCreateStudentsDto
{
    [Required(ErrorMessage = "Students list is required")]
    [MinLength(1, ErrorMessage = "At least one student is required")]
    [MaxLength(100, ErrorMessage = "Cannot create more than 100 students at once")]
    public List<StudentDto> Students { get; set; }
}

public class BulkCreateGradesDto
{
    [Required(ErrorMessage = "Grades list is required")]
    [MinLength(1, ErrorMessage = "At least one grade is required")]
    [MaxLength(100, ErrorMessage = "Cannot create more than 100 grades at once")]
    public List<GradeDto> Grades { get; set; }
}

// Search parameters with validation
public class SearchParametersDto
{
    [MinLength(2, ErrorMessage = "Search term must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string Query { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [RegularExpression(@"^(name|email|department|id|createddate)$", ErrorMessage = "Sort by must be name, email, department, id, or createddate")]
    public string SortBy { get; set; } = "id";

    public bool SortDescending { get; set; } = false;
}

// Advanced search parameters with validation
public class AdvancedSearchParametersDto
{
    [MinLength(2, ErrorMessage = "Search term must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string Query { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Department ID must be greater than 0")]
    public int? DepartmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be greater than 0")]
    public int? TeacherId { get; set; }

    [RegularExpression(@"^(Student|Teacher|Admin)$", ErrorMessage = "Role must be Student, Teacher, or Admin")]
    public string Role { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [RegularExpression(@"^(name|email|department|id|createddate|averagegrade|enrolledcourses)$", ErrorMessage = "Sort by must be name, email, department, id, createddate, averagegrade, or enrolledcourses")]
    public string SortBy { get; set; } = "id";

    public bool SortDescending { get; set; } = false;

    [Range(0, 100, ErrorMessage = "Minimum grade must be between 0 and 100")]
    public double? MinGrade { get; set; }

    [Range(0, 100, ErrorMessage = "Maximum grade must be between 0 and 100")]
    public double? MaxGrade { get; set; }
}

// Export parameters with validation
public class ExportParametersDto
{
    [Required(ErrorMessage = "Format is required")]
    [RegularExpression(@"^(json|csv|excel)$", ErrorMessage = "Format must be json, csv, or excel")]
    public string Format { get; set; } = "json";

    public List<string> Fields { get; set; } = new List<string>();

    public AdvancedSearchParametersDto Filters { get; set; }

    public bool IncludeMetadata { get; set; } = true;
}

// Custom validation attributes
public class NotEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is Guid guid)
        {
            return guid != Guid.Empty;
        }
        return false;
    }
}

public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime > DateTime.Now;
        }
        return false;
    }
}

public class PastDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime < DateTime.Now;
        }
        return false;
    }
}
