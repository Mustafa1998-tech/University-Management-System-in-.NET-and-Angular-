-- Step 1: Clean up duplicate emails
-- Keep one user with student1@example.com and update others
UPDATE Users SET Email = 'student2@example.com' WHERE Id = 2;
UPDATE Users SET Email = 'student3@example.com' WHERE Id = 3;
UPDATE Users SET Email = 'student4@example.com' WHERE Id = 4;
UPDATE Users SET Email = 'student5@example.com' WHERE Id = 5;

-- Step 2: Update student names to be unique
UPDATE Students SET FullName = 'Ahmed Ali' WHERE UserId = 1;
UPDATE Students SET FullName = 'Mohammed Hassan' WHERE UserId = 2;
UPDATE Students SET FullName = 'Omar Khalid' WHERE UserId = 3;
UPDATE Students SET FullName = 'Yousef Salem' WHERE UserId = 4;
UPDATE Students SET FullName = 'Abdullah Nader' WHERE UserId = 5;

-- Step 3: Add unique constraint to prevent future duplicates
ALTER TABLE Users
ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);

-- Verify the cleanup
SELECT 
    u.Id AS UserId,
    u.Email,
    u.Role,
    s.Id AS StudentId,
    s.FullName AS StudentName,
    s.DepartmentId AS StudentDepartmentId
FROM Users u
LEFT JOIN Students s ON u.Id = s.UserId
WHERE u.Role = 'Student'
ORDER BY u.Id;
