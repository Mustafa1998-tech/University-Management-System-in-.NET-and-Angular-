using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class TeacherService
{
    private readonly UniversityDbContext _context;

    public TeacherService(UniversityDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeacherResponseDto>> GetAll()
    {
        var teachers = await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Department)
            .Select(t => new TeacherResponseDto
            {
                Id = t.Id,
                FullName = t.FullName,
                DepartmentId = t.DepartmentId,
                UserId = t.UserId,
                Email = t.User.Email,
                DepartmentName = t.Department.Name
            })
            .ToListAsync();

        return teachers;
    }

    public async Task<TeacherResponseDto> GetById(int id)
    {
        var teacher = await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Department)
            .Where(t => t.Id == id)
            .Select(t => new TeacherResponseDto
            {
                Id = t.Id,
                FullName = t.FullName,
                DepartmentId = t.DepartmentId,
                UserId = t.UserId,
                Email = t.User.Email,
                DepartmentName = t.Department.Name
            })
            .FirstOrDefaultAsync();

        return teacher;
    }

    public async Task<TeacherResponseDto> Add(TeacherDto dto)
    {
        // Check if UserId already has a teacher record
        var existingTeacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.UserId == dto.UserId);

        if (existingTeacher != null)
            return null;

        // Check if department exists
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return null;

        var teacher = new Teacher
        {
            FullName = dto.FullName,
            DepartmentId = dto.DepartmentId,
            UserId = dto.UserId
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        return await GetById(teacher.Id);
    }

    public async Task<TeacherResponseDto> Update(int id, TeacherDto dto)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null) return null;

        // Check if department exists
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return null;

        teacher.FullName = dto.FullName;
        teacher.DepartmentId = dto.DepartmentId;

        await _context.SaveChangesAsync();
        return await GetById(id);
    }

    public async Task<bool> Delete(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null) return false;

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TeacherResponseDto>> GetByDepartment(int departmentId)
    {
        var teachers = await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Department)
            .Where(t => t.DepartmentId == departmentId)
            .Select(t => new TeacherResponseDto
            {
                Id = t.Id,
                FullName = t.FullName,
                DepartmentId = t.DepartmentId,
                UserId = t.UserId,
                Email = t.User.Email,
                DepartmentName = t.Department.Name
            })
            .ToListAsync();

        return teachers;
    }
}
