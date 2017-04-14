using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LMS4Carroll.Data;
using LMS4Carroll.Models;
using ZXing;
using Microsoft.AspNetCore.Authorization;

namespace LMS4Carroll.Controllers
{
    [Authorize(Roles = "Admin,Handler,Student,ChemUser")]
    public class ChemEquipmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChemEquipmentsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: ChemEquipments
        public async Task<IActionResult> Index(string chemeqpmtString)
        {
            ViewData["CurrentFilter"] = chemeqpmtString;

            var equipments = from m in _context.ChemicalEquipments.Include(c => c.Location).Include(c => c.Order)
            select m;

            if (!String.IsNullOrEmpty(chemeqpmtString))
            {
                int forID;
                if (Int32.TryParse(chemeqpmtString, out forID))
                {
                    equipments = equipments.Where(s => s.ChemEquipmentID.Equals(forID));
                    return View(await equipments.OrderByDescending(s => s.ChemEquipmentID).ToListAsync());
                }
                else
                {
                    equipments = equipments.Where(s => s.EquipmentName.Contains(chemeqpmtString)
                                       || s.EquipmentModel.Contains(chemeqpmtString)
                                       || s.SerialNumber.Contains(chemeqpmtString)
                                       || s.Location.NormalizedStr.Contains(chemeqpmtString)
                                       || s.LocationID.Equals(forID)
                                       || s.OrderID.Equals(forID)
                                       || s.Type.Contains(chemeqpmtString)
                                       || s.Order.VendorID.Equals(forID)
                                       || s.Order.Vendor.Name.Contains(chemeqpmtString)
                                       || s.Location.NormalizedStr.Contains(chemeqpmtString)
                                       || s.Location.Room.Contains(chemeqpmtString));
                    return View(await equipments.OrderByDescending(s => s.ChemEquipmentID).ToListAsync());
                }
            }

           // var applicationDbContext = _context.ChemicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await equipments.OrderByDescending(s => s.ChemEquipmentID).ToListAsync());
        }

        // GET: ChemEquipments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemEquipment = await _context.ChemicalEquipments.SingleOrDefaultAsync(m => m.ChemEquipmentID == id);
            if (chemEquipment == null)
            {
                return NotFound();
            }

            return View(chemEquipment);
        }

        // GET: ChemEquipments/Create
        public IActionResult Create()
        {
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr");
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID");
            return View();
        }

        // POST: ChemEquipments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChemEquipmentID,SerialNumber,InstalledDate,InspectionDate,EquipmentModel,EquipmentName,LocationID,OrderID,Type")] ChemEquipment chemEquipment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chemEquipment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", chemEquipment.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemEquipment.OrderID);
            return View(chemEquipment);
        }

        // GET: ChemEquipments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemEquipment = await _context.ChemicalEquipments.SingleOrDefaultAsync(m => m.ChemEquipmentID == id);
            if (chemEquipment == null)
            {
                return NotFound();
            }
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", chemEquipment.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemEquipment.OrderID);
            return View(chemEquipment);
        }

        // POST: ChemEquipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChemEquipmentID,SerialNumber,InstalledDate,InspectionDate,EquipmentModel,EquipmentName,LocationID,OrderID,Type")] ChemEquipment chemEquipment)
        {
            if (id != chemEquipment.ChemEquipmentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chemEquipment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChemEquipmentExists(chemEquipment.ChemEquipmentID))
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
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", chemEquipment.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemEquipment.OrderID);
            return View(chemEquipment);
        }

        // GET: ChemEquipments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemEquipment = await _context.ChemicalEquipments.SingleOrDefaultAsync(m => m.ChemEquipmentID == id);
            if (chemEquipment == null)
            {
                return NotFound();
            }

            return View(chemEquipment);
        }

        // POST: ChemEquipments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chemEquipment = await _context.ChemicalEquipments.SingleOrDefaultAsync(m => m.ChemEquipmentID == id);
            _context.ChemicalEquipments.Remove(chemEquipment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ChemEquipmentExists(int id)
        {
            return _context.ChemicalEquipments.Any(e => e.ChemEquipmentID == id);
        }
    }
}
