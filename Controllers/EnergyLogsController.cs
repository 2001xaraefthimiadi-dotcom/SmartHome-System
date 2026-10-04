using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.API.DTOs;
using SmartHome.Core.Entities;
using SmartHome.Infrastructure.Data;
using System.Security.Claims;

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnergyLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnergyLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnergyLog(CreateEnergyLogRequest request)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == request.DeviceId && d.UserId == userId);

            if (device == null)
                return NotFound("Η συσκευή δεν βρέθηκε.");

            var energyLog = new EnergyLog
            {
                DeviceId = request.DeviceId,
                ConsumedWatts = request.ConsumedWatts,
                RecordedAt = DateTime.UtcNow
            };

            _context.EnergyLogs.Add(energyLog);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                energyLog.Id,
                energyLog.DeviceId,
                energyLog.ConsumedWatts,
                energyLog.RecordedAt
            });
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetEnergyLogsByDevice(int deviceId)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId);

            if (device == null)
                return NotFound("Η συσκευή δεν βρέθηκε.");

            var logs = await _context.EnergyLogs
                .Where(e => e.DeviceId == deviceId)
                .OrderByDescending(e => e.RecordedAt)
                .Select(e => new
                {
                    e.Id,
                    e.DeviceId,
                    e.ConsumedWatts,
                    e.RecordedAt
                })
                .ToListAsync();

            return Ok(logs);
        }

        [HttpGet("device/{deviceId}/total")]
        public async Task<IActionResult> GetTotalEnergyByDevice(int deviceId)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId);

            if (device == null)
                return NotFound("Η συσκευή δεν βρέθηκε.");

            var total = await _context.EnergyLogs
                .Where(e => e.DeviceId == deviceId)
                .SumAsync(e => (double?)e.ConsumedWatts) ?? 0;

            return Ok(new
            {
                DeviceId = deviceId,
                TotalConsumedWatts = total
            });
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Μη έγκυρο διακριτικό σύνδεσης.");

            return int.Parse(userIdClaim);
        }
    }
}