using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LogProducer.Data;
using LogProducer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Shared.Models;

namespace LogProducer.Controllers
{
    [Authorize]
    public class LogController : Controller
    {
        private readonly IDistributedLogService _logService;

        public LogController(IDistributedLogService logService)
        {
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var filter = new LogSearchFilter();
            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                filter.UserId = userId;
            }
            return View(await _logService.GetLogsAsync(filter));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Application,Level,Message,Timestamp")] LogMessage logMessage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            logMessage.UserId = userId;
            logMessage.Timestamp = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                await _logService.LogAsync(logMessage);
                return RedirectToAction(nameof(Index));
            }
            return View(logMessage);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logMessage = (await _logService.GetLogsAsync(new LogSearchFilter())).FirstOrDefault(m => m.Id == id);
            if (logMessage == null)
            {
                return NotFound();
            }

            return View(logMessage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var filter = new LogSearchFilter
            {
                Id = id
            };
            await _logService.DeleteLogsAsync(filter);

            return RedirectToAction(nameof(Index));
        }
    }
}
