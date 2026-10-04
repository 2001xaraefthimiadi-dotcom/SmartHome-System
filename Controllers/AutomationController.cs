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
                .Include(a => a.SourceDevice)
                .ToListAsync();

            return Ok(automations.Select(a => new
            {
                a.Id,
                a.Name,
                a.ConditionType,
                a.ConditionValue,
                a.SourceDeviceId,
                SourceDeviceName = a.SourceDevice != null
                    ? a.SourceDevice.Name
                    : null,
                a.TargetDeviceId,
                TargetDeviceName = a.TargetDevice.Name,
                a.Action,
                a.IsActive,
                a.ScheduledTime,
                a.LastExecutedAt,
                a.CreatedAt
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAutomation(int id)
        {
            var userId = GetUserId();

            var automation = await _context.Automations
            .Include(a => a.TargetDevice)
            .Include(a => a.SourceDevice)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Ο αυτοματισμός δεν βρέθηκε.");
            return Ok(new
            {
                automation.Id,
                automation.Name,
                automation.ConditionType,
                automation.ConditionValue,

                automation.SourceDeviceId,
                SourceDeviceName = automation.SourceDevice != null
                    ? automation.SourceDevice.Name
                    : null,

                automation.TargetDeviceId,
                TargetDeviceName = automation.TargetDevice.Name,

                automation.Action,
                automation.IsActive,
                automation.ScheduledTime,
                automation.LastExecutedAt,
                automation.CreatedAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAutomation(CreateAutomationRequest request)
        {
            var userId = GetUserId();

            if (request.ConditionType == ConditionType.TimeOfDay &&
            !request.ScheduledTime.HasValue)
            {
                return BadRequest("Πρέπει να οριστεί ώρα εκτέλεσης.");
            }

            if (request.ConditionType != ConditionType.TimeOfDay &&
                !request.SourceDeviceId.HasValue)
            {
                return BadRequest("Πρέπει να επιλεγεί συσκευή πηγής.");
            }

            Device? sourceDevice = null;

            if (request.SourceDeviceId.HasValue)
            {
                sourceDevice = await _context.Devices
                    .FirstOrDefaultAsync(d =>
                        d.Id == request.SourceDeviceId.Value &&
                        d.UserId == userId);

                if (sourceDevice == null)
                    return BadRequest("Η συσκευή πηγής δεν βρέθηκε.");
            }


            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == request.TargetDeviceId && d.UserId == userId);

            if (device == null)
                return BadRequest("Η συσκευή στόχου δεν βρέθηκε.");

            var automation = new AutomationRule
            {
                Name = request.Name,
                ConditionType = request.ConditionType,
                ConditionValue = request.ConditionValue,
                SourceDeviceId = request.ConditionType == ConditionType.TimeOfDay ? null
                               : request.SourceDeviceId,
                TargetDeviceId = request.TargetDeviceId,
                Action = request.Action,
                IsActive = true,
                ScheduledTime = request.ConditionType == ConditionType.TimeOfDay
                              ? request.ScheduledTime : null,
                LastExecutedAt = null,
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
                automation.UserId,
                automation.SourceDeviceId,
                automation.ScheduledTime,
                automation.LastExecutedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAutomation(int id, UpdateAutomationRequest request)
        {
            var userId = GetUserId();

            if (request.ConditionType == ConditionType.TimeOfDay &&
            !request.ScheduledTime.HasValue)
            {
                return BadRequest("Πρέπει να οριστεί ώρα εκτέλεσης.");
            }

            if (request.ConditionType != ConditionType.TimeOfDay &&
                !request.SourceDeviceId.HasValue)
            {
                return BadRequest("Πρέπει να επιλεγεί συσκευή πηγής.");
            }

            Device? sourceDevice = null;

            if (request.SourceDeviceId.HasValue)
            {
                sourceDevice = await _context.Devices
                    .FirstOrDefaultAsync(d =>
                        d.Id == request.SourceDeviceId.Value &&
                        d.UserId == userId);

                if (sourceDevice == null)
                    return BadRequest("Η συσκευή πηγής δεν βρέθηκε.");
            }

            var automation = await _context.Automations
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Ο αυτοματισμός δεν βρέθηκε.");

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == request.TargetDeviceId && d.UserId == userId);

            if (device == null)
                return BadRequest("Η συσκευή στόχου δεν βρέθηκε.");

            automation.Name = request.Name;
            automation.ConditionType = request.ConditionType;
            automation.ConditionValue = request.ConditionValue;
            automation.SourceDeviceId = request.ConditionType == ConditionType.TimeOfDay
                ? null
                : request.SourceDeviceId;
            automation.ScheduledTime = request.ConditionType == ConditionType.TimeOfDay
                ? request.ScheduledTime
                : null;
            automation.TargetDeviceId = request.TargetDeviceId;
            automation.LastExecutedAt = null;
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
                automation.IsActive,
                automation.SourceDeviceId,
                automation.ScheduledTime,
                automation.LastExecutedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAutomation(int id)
        {
            var userId = GetUserId();

            var automation = await _context.Automations
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Ο αυτοματισμός δεν βρέθηκε.");

            _context.Automations.Remove(automation);
            await _context.SaveChangesAsync();

            return Ok("Ο αυτοματισμός διαγράφηκε");
        }

        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteAutomation(int id, [FromQuery] double currentValue)
        {
            var userId = GetUserId();

            var automation = await _context.Automations
                .Include(a => a.TargetDevice)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (automation == null)
                return NotFound("Ο αυτοματισμός δεν βρέθηκε.");

            if (!automation.IsActive)
                return BadRequest("Ο αυτοματισμός είναι ανενεργός.");

            var conditionMet = automation.ConditionType switch
            {
                ConditionType.TemperatureGreaterThan => currentValue > automation.ConditionValue,
                ConditionType.TemperatureLessThan => currentValue < automation.ConditionValue,
                ConditionType.EnergyGreaterThan => currentValue > automation.ConditionValue,
                _ => false
            };

            if (!conditionMet)
                return Ok("Η συνθήκη δεν ικανοποιήθηκε.");

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
                message = "Ο αυτοματισμός εκτελέστηκε επιτυχώς.",
                deviceId = automation.TargetDevice.Id,
                newStatus = automation.TargetDevice.Status == DeviceStatus.On
                ? "Ενεργή"
                : "Ανενεργή"
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
