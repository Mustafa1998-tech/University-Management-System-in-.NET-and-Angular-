-- Fix duplicate emails by updating each student with unique email
UPDATE Users SET Email = 'student1@example.com' WHERE Id = 1;
UPDATE Users SET Email = 'student2@example.com' WHERE Id = 2;
UPDATE Users SET Email = 'student3@example.com' WHERE Id = 3;
UPDATE Users SET Email = 'student4@example.com' WHERE Id = 4;
UPDATE Users SET Email = 'student5@example.com' WHERE Id = 5;

-- Fix duplicate names by updating each student with unique name
UPDATE Students SET FullName = 'Ahmed Ali' WHERE UserId = 1;
UPDATE Students SET FullName = 'Mohammed Hassan' WHERE UserId = 2;
UPDATE Students SET FullName = 'Omar Khalid' WHERE UserId = 3;
UPDATE Students SET FullName = 'Yousef Salem' WHERE UserId = 4;
UPDATE Students SET FullName = 'Abdullah Nader' WHERE UserId = 5;

-- Add unique constraint to prevent future duplicate emails
ALTER TABLE Users
ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);

-- Check the results
SELECT 
    u.Id AS UserId,
    u.Email,
    u.Role,
    s.Id AS StudentId,
    s.FullName AS StudentName,
    s.DepartmentId AS StudentDepartmentId
FROM Users u
LEFT JOIN Students s ON u.Id = s.UserId
WHERE u.Role = 'Student';
