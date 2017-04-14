using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LMS4Carroll.Data;
using LMS4Carroll.Models;
using Microsoft.AspNetCore.Authorization;

namespace LMS4Carroll.Controllers
{
    [Authorize(Roles = "Admin,Handler,Student")]
    public class CageLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CageLogsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: CageLogs
        public async Task<IActionResult> Index(string cagelogstring)
        {
            //var applicationDbContext = _context.CageLog.Include(c => c.Cage);
            ViewData["CurrentFilter"] = cagelogstring;

            var logs = from m in _context.CageLog.Include(c => c.Cage)
                       select m;

            if (!String.IsNullOrEmpty(cagelogstring))
            {
                int forID;
                if (Int32.TryParse(cagelogstring, out forID))
                {
                    logs = logs.Where(s => s.LogID.Equals(forID));
                    return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());
                }
                else
                {
                    logs = logs.Where(s => s.Cage.Designation.Contains(cagelogstring)
                                       || s.Cage.Species.Contains(cagelogstring)
                                       || s.Cage.Gender.Contains(cagelogstring)
                                       || s.Cage.Order.SNNumber.Contains(cagelogstring)
                                       || s.Cage.Order.Vendor.Name.Contains(cagelogstring)
                                       || s.Cage.Order.CAT.Contains(cagelogstring)
                                       || s.Cage.Location.Name.Contains(cagelogstring)
                                       || s.Cage.Location.NormalizedStr.Contains(cagelogstring)
                                       || s.Cage.Location.Room.Contains(cagelogstring));
                    return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());
                }
            }
            return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());
        }

        // GET: CageLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cageLog = await _context.CageLog.SingleOrDefaultAsync(m => m.LogID == id);
            if (cageLog == null)
            {
                return NotFound();
            }

            return View(cageLog);
        }

        // GET: CageLogs/Create
        public IActionResult Create()
        {
            ViewData["CageID"] = new SelectList(_context.Cage, "CageID", "CageID");
            return View();
        }

        // POST: CageLogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LogID,CageID,Clean,Food,FoodComments,Social,SocialComments,WashComments,Washed")] CageLog cageLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cageLog);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["CageID"] = new SelectList(_context.Cage, "CageID", "CageID", cageLog.CageID);
            return View(cageLog);
        }

        // GET: CageLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cageLog = await _context.CageLog.SingleOrDefaultAsync(m => m.LogID == id);
            if (cageLog == null)
            {
                return NotFound();
            }
            ViewData["CageID"] = new SelectList(_context.Cage, "CageID", "CageID", cageLog.CageID);
            return View(cageLog);
        }

        // POST: CageLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogID,CageID,Clean,DatetimeCreated,Food,FoodComments,Social,SocialComments,WashComments,Washed")] CageLog cageLog)
        {
            if (id != cageLog.LogID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cageLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CageLogExists(cageLog.LogID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["CageID"] = new SelectList(_context.Cage, "CageID", "CageID", cageLog.CageID);
            return View(cageLog);
        }

        // GET: CageLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cageLog = await _context.CageLog.SingleOrDefaultAsync(m => m.LogID == id);
            if (cageLog == null)
            {
                return NotFound();
            }

            return View(cageLog);
        }

        // POST: CageLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cageLog = await _context.CageLog.SingleOrDefaultAsync(m => m.LogID == id);
            _context.CageLog.Remove(cageLog);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CageLogExists(int id)
        {
            return _context.CageLog.Any(e => e.LogID == id);
        }
    }
}
