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
using Shared.Models;

namespace LogProducer.Controllers
{
    public class LogController : Controller
    {
        private readonly IDistributedLogService _logService;

        public LogController(IDistributedLogService logService)
        {
            _logService = logService;
        }

        // GET: Log
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var filter = new LogSearchFilter
            {
                UserId = userId
            };
            return View(await _logService.GetLogsAsync(filter));
        }

        // GET: Log/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Log/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Log/Delete/5
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

        // POST: Log/Delete/5
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
