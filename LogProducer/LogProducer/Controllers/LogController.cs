using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
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

            var logs = await _logService.SearchLogsAsync(filter);
            logs ??= [];

            return View(logs);
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

            var searchResult = await _logService.SearchLogsAsync(new LogSearchFilter { Id = id });
            var logMessage = searchResult?[0];
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
