using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[ApiController]
[Route("api/[controller]")]
public class DataSeedController : ControllerBase
{
    private readonly DataSeedService _dataSeedService;
    private readonly IWebHostEnvironment _environment;

    public DataSeedController(
        DataSeedService dataSeedService,
        IWebHostEnvironment environment)
    {
        _dataSeedService = dataSeedService;
        _environment = environment;
    }

    [HttpPost("historical")]
    public async Task<IActionResult> SeedHistorical(
        [FromBody] HistoricalDataSeedRequestDto? request,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return Forbid("Data seeding endpoint is available in Development environment only.");

        try
        {
            var result = await _dataSeedService.SeedHistoricalDataAsync(
                request ?? new HistoricalDataSeedRequestDto(),
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
