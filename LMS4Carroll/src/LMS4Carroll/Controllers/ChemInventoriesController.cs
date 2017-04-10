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
    public class ChemInventoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChemInventoriesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: ChemInventories
        public async Task<IActionResult> Index(string cheminventorystring)
        {
            //var applicationDbContext = _context.ChemInventory.Include(c => c.Chemical).Include(c => c.Location).Include(c => c.Order);
            ViewData["CurrentFilter"] = cheminventorystring;

            var inventory = from m in _context.ChemInventory.Include(c => c.Chemical).Include(c => c.Location).Include(c => c.Order)
                             select m;

            if (!String.IsNullOrEmpty(cheminventorystring))
            {
                int forID;
                if (Int32.TryParse(cheminventorystring, out forID))
                {
                    inventory = inventory.Where(s => s.BarcodeID.Equals(forID));
                    return View(await inventory.OrderByDescending(s => s.BarcodeID).ToListAsync());
                }
                else
                {
                    inventory = inventory.Where(s => s.Chemical.CAS.Contains(cheminventorystring)
                                       || s.Chemical.Formula.Contains(cheminventorystring)
                                       || s.Chemical.FormulaName.Contains(cheminventorystring)
                                       || s.Chemical.Hazard.Contains(cheminventorystring)
                                       || s.Chemical.State.Contains(cheminventorystring)
                                       || s.Chemical.Storage.Contains(cheminventorystring)
                                       || s.Order.Vendor.SNNumber.Contains(cheminventorystring)
                                       || s.Order.Vendor.Name.Contains(cheminventorystring)
                                       || s.Order.Vendor.CAT.Contains(cheminventorystring)
                                       || s.Location.Name.Contains(cheminventorystring)
                                       || s.Location.NormalizedStr.Contains(cheminventorystring)
                                       || s.Location.Room.Contains(cheminventorystring));
                    return View(await inventory.OrderByDescending(s => s.BarcodeID).ToListAsync());
                }
            }

            // var applicationDbContext = _context.bioicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await inventory.OrderByDescending(s => s.BarcodeID).ToListAsync());
            //return View(await applicationDbContext.ToListAsync());
        }

        // GET: ChemInventories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            if (chemInventory == null)
            {
                return NotFound();
            }

            return View(chemInventory);
        }

        // GET: ChemInventories/Create
        public IActionResult Create()
        {
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "ChemID");
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr");
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID");
            return View();
        }

        // POST: ChemInventories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BarcodeID,ChemID,ExpiryDate,LocationID,OrderID,QtyLeft,QtyLeftTeXT")] ChemInventory chemInventory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chemInventory);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "ChemID", chemInventory.ChemID);
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr", chemInventory.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemInventory.OrderID);
            return View(chemInventory);
        }

        // GET: ChemInventories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            if (chemInventory == null)
            {
                return NotFound();
            }
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "ChemID", chemInventory.ChemID);
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr", chemInventory.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemInventory.OrderID);
            return View(chemInventory);
        }

        // POST: ChemInventories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BarcodeID,ChemID,ExpiryDate,LocationID,OrderID,QtyLeft,QtyLeftTeXT")] ChemInventory chemInventory)
        {
            if (id != chemInventory.BarcodeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chemInventory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChemInventoryExists(chemInventory.BarcodeID))
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
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "ChemID", chemInventory.ChemID);
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr", chemInventory.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemInventory.OrderID);
            return View(chemInventory);
        }

        // GET: ChemInventories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            if (chemInventory == null)
            {
                return NotFound();
            }

            return View(chemInventory);
        }

        // POST: ChemInventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            _context.ChemInventory.Remove(chemInventory);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ChemInventoryExists(int id)
        {
            return _context.ChemInventory.Any(e => e.BarcodeID == id);
        }
    }
}
