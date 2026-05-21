using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.API.DTOs;
using SmartHome.Core.Entities;
using SmartHome.Core.Enums;
using SmartHome.Infrastructure.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using SmartHome.API.Hubs;

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeviceReadingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeviceReadingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]

        public async Task<IActionResult> CreateReading(CreateReadingRequest request)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == request.DeviceId && d.UserId == userId);

            if (device == null)
                return NotFound("Device not found");

            var reading = new DeviceReading
            {
                DeviceId = request.DeviceId,
                ReadingType = request.ReadingType,
                Value = request.Value,
                Timestamp = DateTime.UtcNow
            };

            _context.DeviceReadings.Add(reading);

            var automations = await _context.Automations
                .Include(a => a.TargetDevice)
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            var executedAutomations = new List<object>();

            foreach (var automation in automations)
            {
                var conditionMet = automation.ConditionType switch
                {
                    ConditionType.TemperatureGreaterThan
                        => request.ReadingType == ReadingType.Temperature &&
                           request.Value > automation.ConditionValue,

                    ConditionType.TemperatureLessThan
                        => request.ReadingType == ReadingType.Temperature &&
                           request.Value < automation.ConditionValue,

                    ConditionType.EnergyGreaterThan
                        => request.ReadingType == ReadingType.Energy &&
                           request.Value > automation.ConditionValue,

                    _ => false
                };

                if (!conditionMet)
                    continue;

                switch (automation.Action)
                {
                    case ActionType.TurnOn:
                        automation.TargetDevice.Status = DeviceStatus.On;
                        break;

                    case ActionType.TurnOff:
                        automation.TargetDevice.Status = DeviceStatus.Off;
                        break;

                    case ActionType.Toggle:
                        automation.TargetDevice.Status =
                            automation.TargetDevice.Status == DeviceStatus.On
                            ? DeviceStatus.Off
                            : DeviceStatus.On;
                        break;
                }

                executedAutomations.Add(new
                {
                    automation.Id,
                    automation.Name,
                    DeviceId = automation.TargetDevice.Id,
                    NewStatus = automation.TargetDevice.Status.ToString()
                });
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("DeviceStatusChanged");

            return Ok(new
            {
                message = "Reading stored successfully",
                reading = new
                {
                    reading.Id,
                    reading.DeviceId,
                    reading.ReadingType,
                    reading.Value,
                    reading.Timestamp
                },
                executedAutomations
            });
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetReadingsByDevice(int deviceId)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId);

            if (device == null)
                return NotFound("Device not found");

            var readings = await _context.DeviceReadings
                .Where(r => r.DeviceId == deviceId)
                .OrderByDescending(r => r.Timestamp)
                .Select(r => new
                {
                    r.Id,
                    r.DeviceId,
                    r.ReadingType,
                    r.Value,
                    r.Timestamp
                })
                .ToListAsync();

            return Ok(readings);
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Invalid token");

            return int.Parse(userIdClaim);
        }

        private readonly IHubContext<SmartHomeHub> _hubContext;

        public DeviceReadingsController(AppDbContext context, IHubContext<SmartHomeHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
    }
}