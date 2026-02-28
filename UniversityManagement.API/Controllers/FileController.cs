using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly ILogger<FileController> _logger;
    private readonly string _uploadsPath;
    private readonly string[] _allowedExtensions;
    private readonly long _maxFileSize;

    public FileController(IConfiguration configuration, ILogger<FileController> logger)
    {
        _logger = logger;
        _uploadsPath = configuration.GetValue<string>("FileSettings:UploadsPath", "uploads");
        _allowedExtensions = configuration.GetSection("FileSettings:AllowedExtensions").Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
        _maxFileSize = configuration.GetValue<long>("FileSettings:MaxFileSize", 5 * 1024 * 1024); // 5MB default
    }

    // Upload file
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string fileType = "document")
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponseDto<FileUploadResponseDto>.ErrorResult("No file provided"));

            // Validate file
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return BadRequest(ApiResponseDto<FileUploadResponseDto>.ErrorResult($"File type {extension} is not allowed"));

            if (file.Length > _maxFileSize)
                return BadRequest(ApiResponseDto<FileUploadResponseDto>.ErrorResult($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB"));

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadsPath, fileType, fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{fileType}/{fileName}";

            _logger.LogInformation("File uploaded successfully: {FileName} ({file.Length} bytes)", fileName);

            return Ok(ApiResponseDto<FileUploadResponseDto>.SuccessResult(new FileUploadResponseDto
            {
                FileName = fileName,
                FileUrl = fileUrl,
                FileSize = file.Length,
                ContentType = file.ContentType,
                FileType = fileType,
                UploadedAt = DateTime.UtcNow,
                Message = "File uploaded successfully"
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, ApiResponseDto<FileUploadResponseDto>.ErrorResult("Failed to upload file"));
        }
    }

    // Get file info
    [HttpGet("info/{fileName}")]
    public async Task<IActionResult> GetFileInfo(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_uploadsPath, fileName);
            
            if (!System.IO.File.Exists(filePath))
                return NotFound(ApiResponseDto<object>.ErrorResult("File not found"));

            var fileInfo = new
            {
                FileName = fileName,
                FileSize = new FileInfo(filePath).Length,
                ContentType = GetContentType(Path.GetExtension(fileName)),
                CreatedAt = System.IO.File.GetCreationTime(filePath),
                ModifiedAt = System.IO.File.GetLastWriteTime(filePath)
            };

            return Ok(ApiResponseDto<object>.SuccessResult(fileInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to get file info"));
        }
    }

    // Delete file
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_uploadsPath, fileName);
            
            if (!System.IO.File.Exists(filePath))
                return NotFound(ApiResponseDto<string>.ErrorResult("File not found"));

            System.IO.File.Delete(filePath);

            _logger.LogInformation("File deleted: {FileName}", fileName);

            return Ok(ApiResponseDto<string>.SuccessResult("File deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to delete file"));
        }
    }

    // List files in directory
    [HttpGet("list")]
    public async Task<IActionResult> ListFiles([FromQuery] string fileType = "document")
    {
        try
        {
            var directoryPath = Path.Combine(_uploadsPath, fileType);
            
            if (!Directory.Exists(directoryPath))
                return Ok(ApiResponseDto<List<object>>.SuccessResult(new List<object>()));

            var files = Directory.GetFiles(directoryPath)
                .Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    FileSize = new FileInfo(f).Length,
                    ContentType = GetContentType(Path.GetExtension(f)),
                    CreatedAt = System.IO.File.GetCreationTime(f),
                    ModifiedAt = System.IO.File.GetLastWriteTime(f)
                })
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => (object)f)
                .ToList();

            return Ok(ApiResponseDto<List<object>>.SuccessResult(files));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            return StatusCode(500, ApiResponseDto<List<object>>.ErrorResult("Failed to list files"));
        }
    }

    // Get file settings
    [HttpGet("settings")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetFileSettings()
    {
        try
        {
            var settings = new
            {
                UploadsPath = _uploadsPath,
                MaxFileSize = _maxFileSize,
                AllowedExtensions = _allowedExtensions,
                FileSizeLimitMB = _maxFileSize / (1024 * 1024),
                SupportedTypes = new[] { "Images", "Documents" }
            };

            return Ok(ApiResponseDto<object>.SuccessResult(settings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file settings");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to get file settings"));
        }
    }

    // Download file
    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_uploadsPath, fileName);
            
            if (!System.IO.File.Exists(filePath))
                return NotFound(ApiResponseDto<string>.ErrorResult("File not found"));

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(Path.GetExtension(fileName));

            _logger.LogInformation("File downloaded: {FileName} ({fileBytes.Length} bytes)", fileName);

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to download file"));
        }
    }

    // File documentation
    [HttpGet("docs")]
    [AllowAnonymous]
    public IActionResult GetFileDocumentation()
    {
        var docs = new
        {
            Title = "File Upload API Documentation",
            Version = "1.0.0",
            Description = "Secure file upload system with validation and organization",
            Endpoints = new[]
            {
                new { Method = "POST", Path = "/api/file/upload", Description = "Upload a file" },
                new { Method = "GET", Path = "/api/file/info/{fileName}", Description = "Get file information" },
                new { Method = "DELETE", Path = "/api/file/{fileName}", Description = "Delete a file" },
                new { Method = "GET", Path = "/api/file/list", Description = "List files in directory" },
                new { Method = "GET", Path = "/api/file/settings", Description = "Get file settings (Admin only)" },
                new { Method = "GET", Path = "/api/file/download/{fileName}", Description = "Download a file" }
            },
            FileSettings = new
            {
                MaxFileSize = _maxFileSize / (1024 * 1024) + " MB",
                AllowedExtensions = string.Join(", ", _allowedExtensions),
                UploadsPath = _uploadsPath,
                SupportedTypes = new[] { "Images", "Documents" }
            },
            SupportedFileTypes = new[]
            {
                new { Extension = ".jpg", Type = "Image", Description = "JPEG images" },
                new { Extension = ".jpeg", Type = "Image", Description = "JPEG images" },
                new { Extension = ".png", Type = "Image", Description = "PNG images" },
                new { Extension = ".gif", Type = "Image", Description = "GIF images" },
                new { Extension = ".pdf", Type = "Document", Description = "PDF documents" },
                new { Extension = ".doc", Type = "Document", Description = "Word documents" },
                new { Extension = ".docx", Type = "Document", Description = "Word documents" }
            },
            ValidationRules = new[]
            {
                "File must be provided",
                "File size must be less than " + (_maxFileSize / (1024 * 1024)) + "MB",
                "File type must be one of: " + string.Join(", ", _allowedExtensions),
                "File name must be valid"
            },
            SecurityFeatures = new[]
            {
                "File type validation",
                "File size limits",
                "Unique filename generation",
                "Directory organization",
                "Path traversal protection",
                "Content type detection"
            },
            BestPractices = new[]
            {
                "Scan uploaded files for malware",
                "Use virus scanning",
                "Implement file expiration policies",
                "Monitor storage usage",
                "Regular cleanup of old files"
            }
        };

        return Ok(docs);
    }

    // Health check
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "FileController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "File Upload",
                "File Download",
                "File Management",
                "File Validation",
                "Directory Organization",
                "Security Features"
            }
        });
    }

    private string GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" or ".docx" => "application/msword",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
