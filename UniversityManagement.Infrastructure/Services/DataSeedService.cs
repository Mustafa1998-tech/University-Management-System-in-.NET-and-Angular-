using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class DataSeedService
{
    private const string SeedEmailDomain = "seed.university.local";

    private static readonly string[] DefaultDepartmentNames =
    [
        "Computer Science",
        "Information Technology",
        "Engineering",
        "Business Administration",
        "Mathematics",
        "Physics",
        "Chemistry",
        "Economics",
        "Law",
        "Medicine"
    ];

    private static readonly string[] FirstNames =
    [
        "Ahmed", "Mohamed", "Omar", "Yousef", "Mahmoud", "Mostafa", "Ali", "Ibrahim",
        "Khaled", "Hassan", "Abdelrahman", "Karim", "Amr", "Tamer", "Eslam", "Wael",
        "Fatma", "Mariam", "Nour", "Sara", "Heba", "Aya", "Laila", "Hana",
        "Noor", "Mona", "Reem", "Salma", "Rana", "Dina", "Yara", "Menna"
    ];

    private static readonly string[] LastNames =
    [
        "Ali", "Hassan", "Ibrahim", "Mahmoud", "Saeed", "Kamel", "Nasser", "Fahmy",
        "Ragab", "Mostafa", "Hamdy", "Salah", "Yehia", "Samir", "Adel", "Ezzat",
        "Farouk", "Gamal", "Nabil", "Shawky", "Soliman", "Abbas", "Saleh", "Zaki"
    ];

    private static readonly string[] CoursePrefixes =
    [
        "Fundamentals of",
        "Advanced",
        "Applied",
        "Introduction to",
        "Principles of",
        "Modern",
        "Systems of",
        "Laboratory in",
        "Seminar in"
    ];

    private readonly UniversityDbContext _context;
    private readonly ILogger<DataSeedService> _logger;

    public DataSeedService(UniversityDbContext context, ILogger<DataSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HistoricalDataSeedResultDto> SeedHistoricalDataAsync(
        HistoricalDataSeedRequestDto? request,
        CancellationToken cancellationToken = default)
    {
        request ??= new HistoricalDataSeedRequestDto();
        ValidateRequest(request);

        var runTag = BuildRunTag(request.RunTag);
        var random = request.RandomSeed.HasValue ? new Random(request.RandomSeed.Value) : new Random();
        var nowUtc = DateTime.UtcNow;
        var startDate = nowUtc.AddYears(-request.Years);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.DefaultPassword);

        var result = new HistoricalDataSeedResultDto
        {
            RunTag = runTag,
            StartedAt = nowUtc,
            DefaultPassword = request.DefaultPassword
        };

        _logger.LogInformation(
            "Starting historical seed. RunTag={RunTag}, Years={Years}, StudentsPerYear={StudentsPerYear}, TeachersPerDepartment={TeachersPerDepartment}",
            runTag,
            request.Years,
            request.StudentsPerYear,
            request.TeachersPerDepartment);

        var autoDetectChangesWasEnabled = _context.ChangeTracker.AutoDetectChangesEnabled;
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            await EnsureSeederSchemaAsync(cancellationToken);

            if (request.ClearPreviousSeedData)
            {
                var cleanup = await ClearSeedDataAsync(cancellationToken);
                result.DeletedUsers = cleanup.DeletedUsers;
                result.DeletedTeachers = cleanup.DeletedTeachers;
                result.DeletedStudents = cleanup.DeletedStudents;
                result.DeletedCourses = cleanup.DeletedCourses;
                result.DeletedEnrollments = cleanup.DeletedEnrollments;
                result.DeletedGrades = cleanup.DeletedGrades;
                result.DeletedPayments = cleanup.DeletedPayments;
            }

            var departmentResult = await EnsureDepartmentsAsync(request.DepartmentsCount, cancellationToken);
            var departments = departmentResult.Departments;
            result.DepartmentsCreated = departmentResult.CreatedCount;

            var teacherBlueprints = BuildTeacherBlueprints(
                departments,
                request.TeachersPerDepartment,
                startDate,
                nowUtc,
                runTag,
                passwordHash,
                random);

            var teacherUsers = teacherBlueprints.Select(x => x.User).ToList();
            await InsertEntitiesInBatchesAsync(_context.Users, teacherUsers, request.BatchSize, cancellationToken);
            result.TeacherUsersCreated = teacherUsers.Count;

            var teachers = teacherBlueprints
                .Select(x => new Teacher
                {
                    UserId = x.User.Id,
                    FullName = x.User.FullName ?? BuildPersonName(random),
                    DepartmentId = x.DepartmentId
                })
                .ToList();

            await InsertEntitiesInBatchesAsync(_context.Teachers, teachers, request.BatchSize, cancellationToken);
            result.TeachersCreated = teachers.Count;

            var teachersByDepartment = teachers
                .GroupBy(t => t.DepartmentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var courses = BuildCourses(
                departments,
                request.CoursesPerDepartmentPerYear,
                request.Years,
                startDate,
                nowUtc,
                teachersByDepartment,
                random);

            await InsertEntitiesInBatchesAsync(_context.Courses, courses, request.BatchSize, cancellationToken);
            result.CoursesCreated = courses.Count;

            var coursesByDepartment = courses
                .GroupBy(c => c.DepartmentId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => new CourseSeedMeta(c.Id, c.CreatedAt)).ToList());

            var studentBlueprints = BuildStudentBlueprints(
                departments.Select(d => d.Id).ToArray(),
                request.StudentsPerYear,
                request.Years,
                startDate,
                nowUtc,
                runTag,
                passwordHash,
                random);

            var studentUsers = studentBlueprints.Select(x => x.User).ToList();
            await InsertEntitiesInBatchesAsync(_context.Users, studentUsers, request.BatchSize, cancellationToken);
            result.StudentUsersCreated = studentUsers.Count;

            var studentCreatedAtByUserId = studentBlueprints.ToDictionary(x => x.User.Id, x => x.User.CreatedAt);

            var students = studentBlueprints
                .Select(x => new Student
                {
                    UserId = x.User.Id,
                    FullName = x.User.FullName ?? BuildPersonName(random),
                    DepartmentId = x.DepartmentId
                })
                .ToList();

            await InsertEntitiesInBatchesAsync(_context.Students, students, request.BatchSize, cancellationToken);
            result.StudentsCreated = students.Count;

            var studentMetas = students
                .Select(s => new StudentSeedMeta(
                    s.Id,
                    s.DepartmentId,
                    studentCreatedAtByUserId.TryGetValue(s.UserId, out var createdAt) ? createdAt : startDate))
                .ToList();

            var enrollmentsAndGrades = await InsertEnrollmentsAndGradesAsync(
                studentMetas,
                coursesByDepartment,
                request,
                random,
                nowUtc,
                cancellationToken);

            result.EnrollmentsCreated = enrollmentsAndGrades.Enrollments;
            result.GradesCreated = enrollmentsAndGrades.Grades;

            if (request.GeneratePayments && request.PaymentsPerStudentPerYear > 0)
            {
                result.PaymentsCreated = await InsertPaymentsAsync(
                    studentMetas,
                    request,
                    random,
                    nowUtc,
                    cancellationToken);
            }

            result.CompletedAt = DateTime.UtcNow;
            result.DurationMs = (long)(result.CompletedAt - result.StartedAt).TotalMilliseconds;

            _logger.LogInformation(
                "Historical seed completed. RunTag={RunTag}, Users={UsersCount}, Students={StudentsCount}, Teachers={TeachersCount}, Courses={CoursesCount}, Enrollments={EnrollmentsCount}, Grades={GradesCount}, Payments={PaymentsCount}, DurationMs={DurationMs}",
                result.RunTag,
                result.TotalUsersCreated,
                result.StudentsCreated,
                result.TeachersCreated,
                result.CoursesCreated,
                result.EnrollmentsCreated,
                result.GradesCreated,
                result.PaymentsCreated,
                result.DurationMs);

            return result;
        }
        finally
        {
            _context.ChangeTracker.Clear();
            _context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesWasEnabled;
        }
    }

    private static void ValidateRequest(HistoricalDataSeedRequestDto request)
    {
        if (request.Years is < 1 or > 20)
            throw new ArgumentException("Years must be between 1 and 20.");

        if (request.DepartmentsCount is < 1 or > 50)
            throw new ArgumentException("DepartmentsCount must be between 1 and 50.");

        if (request.TeachersPerDepartment is < 1 or > 5000)
            throw new ArgumentException("TeachersPerDepartment must be between 1 and 5000.");

        if (request.CoursesPerDepartmentPerYear is < 1 or > 2000)
            throw new ArgumentException("CoursesPerDepartmentPerYear must be between 1 and 2000.");

        if (request.StudentsPerYear is < 1 or > 200000)
            throw new ArgumentException("StudentsPerYear must be between 1 and 200000.");

        if (request.MinEnrollmentsPerStudent is < 1 or > 100)
            throw new ArgumentException("MinEnrollmentsPerStudent must be between 1 and 100.");

        if (request.MaxEnrollmentsPerStudent is < 1 or > 100)
            throw new ArgumentException("MaxEnrollmentsPerStudent must be between 1 and 100.");

        if (request.MaxEnrollmentsPerStudent < request.MinEnrollmentsPerStudent)
            throw new ArgumentException("MaxEnrollmentsPerStudent must be greater than or equal to MinEnrollmentsPerStudent.");

        if (request.GradeCoverageRate <= 0 || request.GradeCoverageRate > 1)
            throw new ArgumentException("GradeCoverageRate must be greater than 0 and less than or equal to 1.");

        if (request.BatchSize is < 100 or > 10000)
            throw new ArgumentException("BatchSize must be between 100 and 10000.");

        if (string.IsNullOrWhiteSpace(request.DefaultPassword))
            throw new ArgumentException("DefaultPassword is required.");
    }

    private static string BuildRunTag(string? requestedTag)
    {
        var source = string.IsNullOrWhiteSpace(requestedTag)
            ? DateTime.UtcNow.ToString("yyyyMMddHHmmss")
            : requestedTag.Trim().ToLowerInvariant();

        var validChars = source.Where(char.IsLetterOrDigit).ToArray();
        var normalized = new string(validChars);

        return string.IsNullOrWhiteSpace(normalized)
            ? DateTime.UtcNow.ToString("yyyyMMddHHmmss")
            : normalized;
    }

    private async Task EnsureSeederSchemaAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF COL_LENGTH('Users', 'FullName') IS NULL
                ALTER TABLE Users ADD FullName NVARCHAR(256) NULL;

            IF COL_LENGTH('Users', 'IsActive') IS NULL
                ALTER TABLE Users ADD IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1);

            IF COL_LENGTH('Users', 'CreatedAt') IS NULL
                ALTER TABLE Users ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME());

            IF COL_LENGTH('Users', 'UpdatedAt') IS NULL
                ALTER TABLE Users ADD UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT (SYSUTCDATETIME());

            IF COL_LENGTH('Courses', 'CreatedAt') IS NULL
                ALTER TABLE Courses ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Courses_CreatedAt DEFAULT (SYSUTCDATETIME());

            IF COL_LENGTH('StudentCourses', 'EnrollmentDate') IS NULL
                ALTER TABLE StudentCourses ADD EnrollmentDate DATETIME2 NOT NULL CONSTRAINT DF_StudentCourses_EnrollmentDate DEFAULT (SYSUTCDATETIME());

            IF COL_LENGTH('Grades', 'GradeDate') IS NULL
                ALTER TABLE Grades ADD GradeDate DATETIME2 NOT NULL CONSTRAINT DF_Grades_GradeDate DEFAULT (SYSUTCDATETIME());
            """;

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task<CleanupResult> ClearSeedDataAsync(CancellationToken cancellationToken)
    {
        const string seedEmailFilter = "%@seed.university.local";

        var deletedGradesByStudents = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE g
            FROM Grades g
            INNER JOIN Students s ON s.Id = g.StudentId
            INNER JOIN Users u ON u.Id = s.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedGradesByCourses = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE g
            FROM Grades g
            INNER JOIN Courses c ON c.Id = g.CourseId
            INNER JOIN Teachers t ON t.Id = c.TeacherId
            INNER JOIN Users u ON u.Id = t.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedEnrollmentsByStudents = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE sc
            FROM StudentCourses sc
            INNER JOIN Students s ON s.Id = sc.StudentId
            INNER JOIN Users u ON u.Id = s.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedEnrollmentsByCourses = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE sc
            FROM StudentCourses sc
            INNER JOIN Courses c ON c.Id = sc.CourseId
            INNER JOIN Teachers t ON t.Id = c.TeacherId
            INNER JOIN Users u ON u.Id = t.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedPayments = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE p
            FROM Payments p
            INNER JOIN Students s ON s.Id = p.StudentId
            INNER JOIN Users u ON u.Id = s.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedCourses = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE c
            FROM Courses c
            INNER JOIN Teachers t ON t.Id = c.TeacherId
            INNER JOIN Users u ON u.Id = t.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedStudents = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE s
            FROM Students s
            INNER JOIN Users u ON u.Id = s.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedTeachers = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE t
            FROM Teachers t
            INNER JOIN Users u ON u.Id = t.UserId
            WHERE u.Email LIKE {seedEmailFilter};
            """, cancellationToken);

        var deletedUsers = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM Users
            WHERE Email LIKE {seedEmailFilter};
            """, cancellationToken);

        _context.ChangeTracker.Clear();

        return new CleanupResult
        {
            DeletedUsers = deletedUsers,
            DeletedTeachers = deletedTeachers,
            DeletedStudents = deletedStudents,
            DeletedCourses = deletedCourses,
            DeletedEnrollments = deletedEnrollmentsByStudents + deletedEnrollmentsByCourses,
            DeletedGrades = deletedGradesByStudents + deletedGradesByCourses,
            DeletedPayments = deletedPayments
        };
    }

    private async Task<DepartmentSeedResult> EnsureDepartmentsAsync(
        int targetDepartmentCount,
        CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

        var createdCount = 0;

        if (departments.Count < targetDepartmentCount)
        {
            var toCreate = new List<Department>(targetDepartmentCount - departments.Count);

            for (var i = departments.Count; i < targetDepartmentCount; i++)
            {
                var baseName = i < DefaultDepartmentNames.Length
                    ? DefaultDepartmentNames[i]
                    : $"Department {i + 1:00}";

                var name = baseName;
                var suffix = 2;
                while (departments.Any(d => d.Name == name) || toCreate.Any(d => d.Name == name))
                {
                    name = $"{baseName} {suffix++}";
                }

                toCreate.Add(new Department { Name = name });
            }

            await _context.Departments.AddRangeAsync(toCreate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            createdCount = toCreate.Count;
            _context.ChangeTracker.Clear();

            departments = await _context.Departments
                .OrderBy(d => d.Id)
                .Take(targetDepartmentCount)
                .ToListAsync(cancellationToken);
        }
        else
        {
            departments = departments.Take(targetDepartmentCount).ToList();
        }

        return new DepartmentSeedResult(departments, createdCount);
    }

    private static List<UserDepartmentBlueprint> BuildTeacherBlueprints(
        IReadOnlyList<Department> departments,
        int teachersPerDepartment,
        DateTime startDate,
        DateTime endDate,
        string runTag,
        string passwordHash,
        Random random)
    {
        var capacity = departments.Count * teachersPerDepartment;
        var blueprints = new List<UserDepartmentBlueprint>(capacity);

        foreach (var department in departments)
        {
            for (var i = 1; i <= teachersPerDepartment; i++)
            {
                var fullName = BuildPersonName(random);
                var createdAt = RandomDateBetween(random, startDate, endDate);

                blueprints.Add(new UserDepartmentBlueprint(
                    new User
                    {
                        Email = $"teacher.{runTag}.{department.Id}.{i:00000}@{SeedEmailDomain}",
                        PasswordHash = passwordHash,
                        Role = "Teacher",
                        FullName = fullName,
                        IsActive = true,
                        CreatedAt = createdAt,
                        UpdatedAt = createdAt
                    },
                    department.Id));
            }
        }

        return blueprints;
    }

    private static List<UserDepartmentBlueprint> BuildStudentBlueprints(
        IReadOnlyList<int> departmentIds,
        int studentsPerYear,
        int years,
        DateTime startDate,
        DateTime endDate,
        string runTag,
        string passwordHash,
        Random random)
    {
        var capacity = years * studentsPerYear;
        var blueprints = new List<UserDepartmentBlueprint>(capacity);

        for (var yearOffset = 0; yearOffset < years; yearOffset++)
        {
            var yearStart = startDate.AddYears(yearOffset);
            var yearEnd = yearStart.AddYears(1);
            if (yearEnd > endDate)
                yearEnd = endDate;

            var yearLabel = yearStart.Year;

            for (var i = 1; i <= studentsPerYear; i++)
            {
                var departmentId = departmentIds[random.Next(departmentIds.Count)];
                var fullName = BuildPersonName(random);
                var createdAt = RandomDateBetween(random, yearStart, yearEnd);

                blueprints.Add(new UserDepartmentBlueprint(
                    new User
                    {
                        Email = $"student.{runTag}.{yearLabel}.{i:00000}@{SeedEmailDomain}",
                        PasswordHash = passwordHash,
                        Role = "Student",
                        FullName = fullName,
                        IsActive = true,
                        CreatedAt = createdAt,
                        UpdatedAt = createdAt
                    },
                    departmentId));
            }
        }

        return blueprints;
    }

    private static List<Course> BuildCourses(
        IReadOnlyList<Department> departments,
        int coursesPerDepartmentPerYear,
        int years,
        DateTime startDate,
        DateTime endDate,
        IReadOnlyDictionary<int, List<Teacher>> teachersByDepartment,
        Random random)
    {
        var capacity = departments.Count * years * coursesPerDepartmentPerYear;
        var courses = new List<Course>(capacity);

        foreach (var department in departments)
        {
            if (!teachersByDepartment.TryGetValue(department.Id, out var departmentTeachers) || departmentTeachers.Count == 0)
                continue;

            for (var yearOffset = 0; yearOffset < years; yearOffset++)
            {
                var yearStart = startDate.AddYears(yearOffset);
                var yearEnd = yearStart.AddYears(1);
                if (yearEnd > endDate)
                    yearEnd = endDate;

                for (var courseIndex = 1; courseIndex <= coursesPerDepartmentPerYear; courseIndex++)
                {
                    var teacher = departmentTeachers[random.Next(departmentTeachers.Count)];
                    var prefix = CoursePrefixes[random.Next(CoursePrefixes.Length)];
                    var createdAt = RandomDateBetween(random, yearStart, yearEnd);

                    courses.Add(new Course
                    {
                        Name = $"{prefix} {department.Name} {yearStart.Year}-{courseIndex:000}",
                        DepartmentId = department.Id,
                        TeacherId = teacher.Id,
                        CreatedAt = createdAt
                    });
                }
            }
        }

        return courses;
    }

    private async Task InsertEntitiesInBatchesAsync<TEntity>(
        DbSet<TEntity> dbSet,
        List<TEntity> entities,
        int batchSize,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        if (entities.Count == 0)
            return;

        for (var offset = 0; offset < entities.Count; offset += batchSize)
        {
            var count = Math.Min(batchSize, entities.Count - offset);
            var batch = entities.GetRange(offset, count);

            await dbSet.AddRangeAsync(batch, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _context.ChangeTracker.Clear();
        }
    }

    private async Task<(int Enrollments, int Grades)> InsertEnrollmentsAndGradesAsync(
        IReadOnlyList<StudentSeedMeta> studentMetas,
        IReadOnlyDictionary<int, List<CourseSeedMeta>> coursesByDepartment,
        HistoricalDataSeedRequestDto request,
        Random random,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var totalEnrollments = 0;
        var totalGrades = 0;
        var processedStudents = 0;

        var enrollmentBuffer = new List<StudentCourse>(request.BatchSize);
        var gradeBuffer = new List<Grade>(request.BatchSize);

        foreach (var student in studentMetas)
        {
            processedStudents++;

            if (!coursesByDepartment.TryGetValue(student.DepartmentId, out var departmentCourses) || departmentCourses.Count == 0)
                continue;

            var maxEnrollments = Math.Min(request.MaxEnrollmentsPerStudent, departmentCourses.Count);
            if (maxEnrollments <= 0)
                continue;

            var minEnrollments = Math.Min(request.MinEnrollmentsPerStudent, maxEnrollments);
            var enrollmentsCount = random.Next(minEnrollments, maxEnrollments + 1);

            var pickedIndexes = new HashSet<int>();
            while (pickedIndexes.Count < enrollmentsCount)
            {
                pickedIndexes.Add(random.Next(departmentCourses.Count));
            }

            foreach (var pickedIndex in pickedIndexes)
            {
                var course = departmentCourses[pickedIndex];
                var enrollmentStartDate = MaxDate(student.CreatedAt, course.CreatedAt);
                var enrollmentDate = RandomDateBetween(random, enrollmentStartDate, nowUtc);

                enrollmentBuffer.Add(new StudentCourse
                {
                    StudentId = student.StudentId,
                    CourseId = course.CourseId,
                    EnrollmentDate = enrollmentDate
                });

                if (random.NextDouble() <= request.GradeCoverageRate)
                {
                    gradeBuffer.Add(new Grade
                    {
                        StudentId = student.StudentId,
                        CourseId = course.CourseId,
                        Value = GenerateGradeValue(random),
                        GradeDate = RandomDateBetween(random, enrollmentDate, nowUtc)
                    });
                }
            }

            if (enrollmentBuffer.Count >= request.BatchSize)
            {
                await _context.StudentCourses.AddRangeAsync(enrollmentBuffer, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                totalEnrollments += enrollmentBuffer.Count;
                enrollmentBuffer.Clear();
                _context.ChangeTracker.Clear();
            }

            if (gradeBuffer.Count >= request.BatchSize)
            {
                await _context.Grades.AddRangeAsync(gradeBuffer, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                totalGrades += gradeBuffer.Count;
                gradeBuffer.Clear();
                _context.ChangeTracker.Clear();
            }

            if (processedStudents % 1000 == 0)
            {
                _logger.LogInformation(
                    "Enrollment/grade generation progress: {ProcessedStudents}/{TotalStudents} students.",
                    processedStudents,
                    studentMetas.Count);
            }
        }

        if (enrollmentBuffer.Count > 0)
        {
            await _context.StudentCourses.AddRangeAsync(enrollmentBuffer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            totalEnrollments += enrollmentBuffer.Count;
            enrollmentBuffer.Clear();
            _context.ChangeTracker.Clear();
        }

        if (gradeBuffer.Count > 0)
        {
            await _context.Grades.AddRangeAsync(gradeBuffer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            totalGrades += gradeBuffer.Count;
            gradeBuffer.Clear();
            _context.ChangeTracker.Clear();
        }

        return (totalEnrollments, totalGrades);
    }

    private async Task<int> InsertPaymentsAsync(
        IReadOnlyList<StudentSeedMeta> studentMetas,
        HistoricalDataSeedRequestDto request,
        Random random,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var totalPayments = 0;
        var processedStudents = 0;
        var paymentBuffer = new List<Payment>(request.BatchSize);

        foreach (var student in studentMetas)
        {
            processedStudents++;
            for (var year = student.CreatedAt.Year; year <= nowUtc.Year; year++)
            {
                var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var yearEnd = year == nowUtc.Year
                    ? nowUtc
                    : yearStart.AddYears(1).AddTicks(-1);

                if (yearEnd <= student.CreatedAt)
                    continue;

                var paymentStart = MaxDate(yearStart, student.CreatedAt);

                for (var i = 0; i < request.PaymentsPerStudentPerYear; i++)
                {
                    paymentBuffer.Add(new Payment
                    {
                        StudentId = student.StudentId,
                        Amount = Math.Round((decimal)(2500 + (random.NextDouble() * 7500)), 2),
                        Date = RandomDateBetween(random, paymentStart, yearEnd),
                        Status = random.NextDouble() <= 0.93 ? "Paid" : "Pending"
                    });
                }
            }

            if (paymentBuffer.Count >= request.BatchSize)
            {
                await _context.Set<Payment>().AddRangeAsync(paymentBuffer, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                totalPayments += paymentBuffer.Count;
                paymentBuffer.Clear();
                _context.ChangeTracker.Clear();
            }

            if (processedStudents % 1000 == 0)
            {
                _logger.LogInformation(
                    "Payment generation progress: {ProcessedStudents}/{TotalStudents} students.",
                    processedStudents,
                    studentMetas.Count);
            }
        }

        if (paymentBuffer.Count > 0)
        {
            await _context.Set<Payment>().AddRangeAsync(paymentBuffer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            totalPayments += paymentBuffer.Count;
            paymentBuffer.Clear();
            _context.ChangeTracker.Clear();
        }

        return totalPayments;
    }

    private static double GenerateGradeValue(Random random)
    {
        var raw = 35 + ((random.NextDouble() + random.NextDouble()) * 32.5);
        return Math.Round(Math.Clamp(raw, 35, 100), 2);
    }

    private static string BuildPersonName(Random random)
    {
        var first = FirstNames[random.Next(FirstNames.Length)];
        var last = LastNames[random.Next(LastNames.Length)];
        return $"{first} {last}";
    }

    private static DateTime RandomDateBetween(Random random, DateTime start, DateTime end)
    {
        if (end <= start)
            return start;

        var range = end - start;
        var randomSeconds = random.NextInt64(0, (long)range.TotalSeconds + 1);
        return start.AddSeconds(randomSeconds);
    }

    private static DateTime MaxDate(DateTime a, DateTime b) => a >= b ? a : b;

    private sealed record UserDepartmentBlueprint(User User, int DepartmentId);

    private sealed record DepartmentSeedResult(List<Department> Departments, int CreatedCount);

    private sealed record CourseSeedMeta(int CourseId, DateTime CreatedAt);

    private sealed record StudentSeedMeta(int StudentId, int DepartmentId, DateTime CreatedAt);

    private sealed class CleanupResult
    {
        public int DeletedUsers { get; set; }
        public int DeletedTeachers { get; set; }
        public int DeletedStudents { get; set; }
        public int DeletedCourses { get; set; }
        public int DeletedEnrollments { get; set; }
        public int DeletedGrades { get; set; }
        public int DeletedPayments { get; set; }
    }
}
