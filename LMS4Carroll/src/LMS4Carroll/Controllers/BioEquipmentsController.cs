using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMS4Carroll.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using LMS4Carroll.Models;
using Microsoft.AspNetCore.Authorization;

namespace LMS4Carroll.Controllers
{
    [Authorize(Roles = "Admin,Handler,Student,BiologyUser")]
    public class BioEquipmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BioEquipmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BioEquipments
        public async Task<IActionResult> Index(string bioeqpmtString)
        {
            ViewData["CurrentFilter"] = bioeqpmtString;

            var equipments = from m in _context.BioEquipments.Include(c => c.Location).Include(c => c.Order)
                             select m;

            if (!String.IsNullOrEmpty(bioeqpmtString))
            {
                int forID;
                if (Int32.TryParse(bioeqpmtString, out forID))
                {
                    equipments = equipments.Where(s => s.BioEquipmentID.Equals(forID));
                    return View(await equipments.OrderByDescending(s => s.BioEquipmentID).ToListAsync());
                }
                else
                {
                    equipments = equipments.Where(s => s.EquipmentName.Contains(bioeqpmtString)
                                       || s.EquipmentModel.Contains(bioeqpmtString)
                                       || s.LocationID.Equals(forID)
                                       || s.SerialNumber.Contains(bioeqpmtString)
                                       || s.Location.NormalizedStr.Contains(bioeqpmtString)
                                       || s.OrderID.Equals(forID)
                                       || s.Type.Contains(bioeqpmtString)
                                       || s.Order.VendorID.Equals(forID)
                                       || s.Order.Vendor.Name.Contains(bioeqpmtString)
                                       || s.Location.Room.Contains(bioeqpmtString)
                                       || s.Location.NormalizedStr.Contains(bioeqpmtString));
                    return View(await equipments.OrderByDescending(s => s.BioEquipmentID).ToListAsync());
                }
            }

            // var applicationDbContext = _context.bioicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await equipments.OrderByDescending(s => s.BioEquipmentID).ToListAsync());
        }

        // GET: BioEquipments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bioEquipment = await _context.BioEquipments.SingleOrDefaultAsync(m => m.BioEquipmentID == id);
            if (bioEquipment == null)
            {
                return NotFound();
            }

            return View(bioEquipment);
        }

        // GET: BioEquipments/Create
        public IActionResult Create()
        {
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr");
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID");
            return View();
        }

        // POST: BioEquipments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BioEquipmentID,SerialNumber,InstalledDate,InspectionDate,EquipmentModel,EquipmentName,LocationID,OrderID,Type")] BioEquipment bioEquipment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bioEquipment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", bioEquipment.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", bioEquipment.OrderID);
            return View(bioEquipment);
        }

        // GET: BioEquipments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bioEquipment = await _context.BioEquipments.SingleOrDefaultAsync(m => m.BioEquipmentID == id);
            if (bioEquipment == null)
            {
                return NotFound();
            }
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", bioEquipment.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", bioEquipment.OrderID);
            return View(bioEquipment);
        }

        // POST: BioEquipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BioEquipmentID,SerialNumber,InstalledDate,InspectionDate,EquipmentModel,EquipmentName,LocationID,OrderID,Type")] BioEquipment bioEquipment)
        {
            if (id != bioEquipment.BioEquipmentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bioEquipment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BioEquipmentExists(bioEquipment.BioEquipmentID))
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
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", bioEquipment.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", bioEquipment.OrderID);
            return View(bioEquipment);
        }

        // GET: BioEquipments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bioEquipment = await _context.BioEquipments.SingleOrDefaultAsync(m => m.BioEquipmentID == id);
            if (bioEquipment == null)
            {
                return NotFound();
            }

            return View(bioEquipment);
        }

        // POST: BioEquipments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bioEquipment = await _context.BioEquipments.SingleOrDefaultAsync(m => m.BioEquipmentID == id);
            _context.BioEquipments.Remove(bioEquipment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool BioEquipmentExists(int id)
        {
            return _context.BioEquipments.Any(e => e.BioEquipmentID == id);
        }
    }
}
