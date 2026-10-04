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
    public class DeviceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeviceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyDevices()
        {
            var userId = GetUserId();

            var devices = await _context.Devices
                .Where(d => d.UserId == userId)
                .ToListAsync();

            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevice(int id)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (device == null)
                return NotFound("Η συσκευή δεν βρέθηκε.");

            return Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDevice(CreateDeviceRequest request)
        {
            var userId = GetUserId();

            var device = new Device
            {
                Name = request.Name,
                Type = request.Type,
                Location = request.Location,
                Status = DeviceStatus.Off,
                IsOnline = true,
                PowerConsumption = request.PowerConsumption,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return Ok(device);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(int id, UpdateDeviceRequest request)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (device == null)
                return NotFound("Η συσκευή δεν βρέθηκε.");

            device.Name = request.Name;
            device.Type = request.Type;
            device.Location = request.Location;
            device.Status = request.Status;
            device.IsOnline = request.IsOnline;
            device.PowerConsumption = request.PowerConsumption;

            await _context.SaveChangesAsync();

            return Ok(device);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var userId = GetUserId();

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (device == null)
                return NotFound("Η συσκευή δεν βρέθηκε.");

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return Ok("Η συσκευή διαγράφηκε");
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