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
    public class CagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CagesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Cages
        public async Task<IActionResult> Index(string cagestring)
        {
            //var applicationDbContext = _context.Cage.Include(c => c.Location).Include(c => c.Order);
            ViewData["CurrentFilter"] = cagestring;

            var cages = from m in _context.Cage.Include(c => c.Location).Include(c => c.Order)
                             select m;

            if (!String.IsNullOrEmpty(cagestring))
            {
                int forID;
                if (Int32.TryParse(cagestring, out forID))
                {
                    cages = cages.Where(s => s.CageID.Equals(forID));
                    return View(await cages.OrderByDescending(s => s.CageID).ToListAsync());
                }
                else
                {
                    cages = cages.Where(s => s.Designation.Contains(cagestring)
                                       || s.Gender.Contains(cagestring)
                                       || s.Location.Name.Contains(cagestring)
                                       || s.Location.Room.Contains(cagestring)
                                       || s.Location.NormalizedStr.Contains(cagestring)
                                       || s.Order.SNNumber.Contains(cagestring)
                                       || s.Order.Vendor.Name.Contains(cagestring)
                                       || s.Order.CAT.Contains(cagestring));
                    return View(await cages.OrderByDescending(s => s.CageID).ToListAsync());
                }
            }
                return View(await cages.OrderByDescending(s => s.CageID).ToListAsync());

        }

        [Authorize(Roles = "Admin")]
        // GET: Cages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cage = await _context.Cage.SingleOrDefaultAsync(m => m.CageID == id);
            if (cage == null)
            {
                return NotFound();
            }

            return View(cage);
        }

        // GET: Cages/Create
        public IActionResult Create()
        {
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr");
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID");
            return View();
        }

        // POST: Cages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CageID,DOB,Designation,Gender,LocationID,OrderID,Species")] Cage cage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr", cage.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", cage.OrderID);
            return View(cage);
        }

        [Authorize(Roles = "Admin")]
        // GET: Cages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cage = await _context.Cage.SingleOrDefaultAsync(m => m.CageID == id);
            if (cage == null)
            {
                return NotFound();
            }
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr", cage.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", cage.OrderID);
            return View(cage);
        }

        // POST: Cages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CageID,DOB,Designation,Gender,LocationID,OrderID,Species")] Cage cage)
        {
            if (id != cage.CageID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CageExists(cage.CageID))
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
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr", cage.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", cage.OrderID);
            return View(cage);
        }

        // GET: Cages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cage = await _context.Cage.SingleOrDefaultAsync(m => m.CageID == id);
            if (cage == null)
            {
                return NotFound();
            }

            return View(cage);
        }

        // POST: Cages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cage = await _context.Cage.SingleOrDefaultAsync(m => m.CageID == id);
            _context.Cage.Remove(cage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CageExists(int id)
        {
            return _context.Cage.Any(e => e.CageID == id);
        }
    }
}
