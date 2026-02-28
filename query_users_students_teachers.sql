-- Correct query to get Users, Students, and Teachers information
SELECT 
    u.Id AS UserId,
    u.Email,
    u.Role,
    u.PasswordHash,
    s.Id AS StudentId,
    s.FullName AS StudentName,
    s.DepartmentId AS StudentDepartmentId,
    t.Id AS TeacherId,
    t.FullName AS TeacherName,
    t.DepartmentId AS TeacherDepartmentId
FROM Users u
LEFT JOIN Students s ON u.Id = s.UserId
LEFT JOIN Teachers t ON u.Id = t.UserId;
