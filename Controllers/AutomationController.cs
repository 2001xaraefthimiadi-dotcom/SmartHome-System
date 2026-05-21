using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.API.DTOs;
using SmartHome.Core.Entities;
using SmartHome.Core.Enums;
using SmartHome.Infrastructure.Data;
using System.Security.Claims;

namespace SmartHome.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AutomationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AutomationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAutomations()
        {
            var userId = GetUserId();

            var automations = await _context.Automations
                .Where(a => a.UserId == userId)
                .Include(a => a.TargetDevice)
                .ToListAsync();

            return Ok(automations.Select(a => new
            {
                a.Id,
                a.Name,
                a.ConditionType,
                a.ConditionValue,
                a.TargetDeviceId,
                TargetDeviceName = a.TargetDevice.Name,
                a.Action,
                a.IsActive,
                a.CreatedAt
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAutomation(int id)
        {
            var userId = GetUserId();

            var automation = await _context.Automations
                .Include(a => a.TargetDevice)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Automation not found");

            return Ok(new
            {
                automation.Id,
                automation.Name,
                automation.ConditionType,
                automation.ConditionValue,
                automation.TargetDeviceId,
                TargetDeviceName = automation.TargetDevice.Name,
                automation.Action,
                automation.IsActive,
                automation.CreatedAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAutomation(CreateAutomationRequest request)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == request.TargetDeviceId && d.UserId == userId);

            if (device == null)
                return BadRequest("Target device not found");

            var automation = new AutomationRule
            {
                Name = request.Name,
                ConditionType = request.ConditionType,
                ConditionValue = request.ConditionValue,
                TargetDeviceId = request.TargetDeviceId,
                Action = request.Action,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Automations.Add(automation);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                automation.Id,
                automation.Name,
                automation.ConditionType,
                automation.ConditionValue,
                automation.TargetDeviceId,
                automation.Action,
                automation.IsActive,
                automation.CreatedAt,
                automation.UserId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAutomation(int id, UpdateAutomationRequest request)
        {
            var userId = GetUserId();

            var automation = await _context.Automations
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Automation not found");

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == request.TargetDeviceId && d.UserId == userId);

            if (device == null)
                return BadRequest("Target device not found");

            automation.Name = request.Name;
            automation.ConditionType = request.ConditionType;
            automation.ConditionValue = request.ConditionValue;
            automation.TargetDeviceId = request.TargetDeviceId;
            automation.Action = request.Action;
            automation.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                automation.Id,
                automation.Name,
                automation.ConditionType,
                automation.ConditionValue,
                automation.TargetDeviceId,
                automation.Action,
                automation.IsActive
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAutomation(int id)
        {
            var userId = GetUserId();

            var automation = await _context.Automations
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Automation not found");

            _context.Automations.Remove(automation);
            await _context.SaveChangesAsync();

            return Ok("Automation deleted");
        }

        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteAutomation(int id, [FromQuery] double currentValue)
        {
            var userId = GetUserId();

            var automation = await _context.Automations
                .Include(a => a.TargetDevice)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Automation not found");

            if (!automation.IsActive)
                return BadRequest("Automation is inactive");

            var conditionMet = automation.ConditionType switch
            {
                ConditionType.TemperatureGreaterThan => currentValue > automation.ConditionValue,
                ConditionType.TemperatureLessThan => currentValue < automation.ConditionValue,
                ConditionType.EnergyGreaterThan => currentValue > automation.ConditionValue,
                _ => false
            };

            if (!conditionMet)
                return Ok("Condition not met");

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

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Automation executed successfully",
                deviceId = automation.TargetDevice.Id,
                newStatus = automation.TargetDevice.Status.ToString()
            });
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Invalid token");

            return int.Parse(userIdClaim);
        }
    }
}
