-- Test the unique constraint by trying to insert duplicate email
-- This should fail with a constraint violation error

BEGIN TRY
    INSERT INTO Users (Email, PasswordHash, Role)
    VALUES ('student1@example.com', 'hashed_password', 'Student')
END TRY
BEGIN CATCH
    SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_MESSAGE() AS ErrorMessage
END CATCH

-- Test with new unique email (this should succeed)
BEGIN TRY
    INSERT INTO Users (Email, PasswordHash, Role)
    VALUES ('newstudent@example.com', 'hashed_password', 'Student')
    
    SELECT 'New user inserted successfully' AS Result
END TRY
BEGIN CATCH
    SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_MESSAGE() AS ErrorMessage
END CATCH

-- Clean up the test user
DELETE FROM Users WHERE Email = 'newstudent@example.com';
