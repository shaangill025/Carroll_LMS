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
    [Authorize (Roles = "Admin,Handler,Student")]
    public class ChemLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChemLogsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: ChemLogs
        public async Task<IActionResult> Index(string chemlogstring)
        {
            //var applicationDbContext = _context.ChemLog.Include(c => c.ChemInventory).Include(c => c.Course);
            ViewData["CurrentFilter"] = chemlogstring;

            var logs = from m in _context.ChemLog.Include(c => c.ChemInventory).Include(c => c.Course)
                       select m;

            if (!String.IsNullOrEmpty(chemlogstring))
            {
                int forID;
                if (Int32.TryParse(chemlogstring, out forID))
                {
                    logs = logs.Where(s => s.LogID.Equals(forID));
                    return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());
                }
                else
                {
                    logs = logs.Where(s => s.Course.Department.Contains(chemlogstring)
                                       || s.Course.Handler.Contains(chemlogstring)
                                       || s.Course.Instructor.Contains(chemlogstring)
                                       || s.Course.Name.Contains(chemlogstring)
                                       || s.Course.Number.Contains(chemlogstring)
                                       || s.Course.CourseID.Equals(forID)
                                       || s.Course.Location.Name.Contains(chemlogstring)
                                       || s.Course.Location.Room.Contains(chemlogstring)
                                       || s.Course.Location.Type.Contains(chemlogstring)
                                       || s.ChemInventory.BarcodeID.Equals(forID)
                                       || s.ChemInventory.ChemID.Equals(forID)
                                       || s.ChemInventory.Chemical.CAS.Contains(chemlogstring)
                                       || s.ChemInventory.Chemical.Formula.Contains(chemlogstring)
                                       || s.ChemInventory.Chemical.FormulaName.Contains(chemlogstring)
                                       || s.ChemInventory.Chemical.Hazard.Contains(chemlogstring)
                                       || s.ChemInventory.Chemical.State.Contains(chemlogstring)
                                       || s.ChemInventory.Chemical.Storage.Contains(chemlogstring)
                                       || s.ChemInventory.Location.Name.Contains(chemlogstring)
                                       || s.ChemInventory.Location.Room.Contains(chemlogstring)
                                       || s.ChemInventory.Location.NormalizedStr.Contains(chemlogstring)
                                       || s.ChemInventory.BarcodeID.Equals(forID)
                                       || s.ChemInventory.Order.Vendor.Name.Contains(chemlogstring)
                                       || s.ChemInventory.Order.Vendor.SNNumber.Contains(chemlogstring)
                                       || s.ChemInventory.Order.Vendor.CAT.Contains(chemlogstring));
                    return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());

                }
            }

            // var applicationDbContext = _context.bioicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());
            //return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        // GET: ChemLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemLog = await _context.ChemLog.SingleOrDefaultAsync(m => m.LogID == id);
            if (chemLog == null)
            {
                return NotFound();
            }

            return View(chemLog);
        }

        // GET: ChemLogs/Create
        public IActionResult Create()
        {
            ViewData["BarcodeID"] = new SelectList(_context.ChemInventory, "BarcodeID", "BarcodeID");
            ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "CourseID");
            return View();
        }

        // POST: ChemLogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int barcodeinput, int courseinput, float qtyusedinput)
        {
            ViewData["Barcode"] = barcodeinput;
            ViewData["Barcode"] = courseinput;
            ViewData["Qty"] = qtyusedinput;
            if (_context.ChemInventory.Count(M => M.BarcodeID == barcodeinput) >= 1) {
                ChemLog chemLog = new ChemLog();
                ChemInventory temp = _context.ChemInventory.FirstOrDefault(s => s.BarcodeID == barcodeinput);
                float tempValue = temp.QtyLeft;
                temp.QtyLeft = tempValue - qtyusedinput;
                _context.Entry<ChemInventory>(temp).State = EntityState.Modified;
                _context.SaveChanges();
                //chemLog.DatetimeCreated = DateTime.UtcNow;
                if (ModelState.IsValid)
                {
                    chemLog.BarcodeID = barcodeinput;
                    chemLog.CourseID = courseinput;
                    chemLog.QtyUsed = qtyusedinput;
                    _context.Add(chemLog);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewData["BarcodeID"] = new SelectList(_context.ChemInventory, "BarcodeID", "BarcodeID", chemLog.BarcodeID);
                ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "CourseID", chemLog.CourseID);
                return View(chemLog);
            }
            else
            {
                return View("CheckBarcode");
            }      
           
        }

        [Authorize( Roles = "Admin")]
        // GET: ChemLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemLog = await _context.ChemLog.SingleOrDefaultAsync(m => m.LogID == id);
            if (chemLog == null)
            {
                return NotFound();
            }
            ViewData["Barcode"] = new SelectList(_context.ChemInventory, "BarcodeID", "BarcodeID", chemLog.BarcodeID);
            ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "CourseID", chemLog.CourseID);
            return View(chemLog);
        }

        // POST: ChemLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogID,Barcode,CourseID")] ChemLog chemLog)
        {
            if (id != chemLog.LogID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chemLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChemLogExists(chemLog.LogID))
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
            ViewData["Barcode"] = new SelectList(_context.ChemInventory, "BarcodeID", "BarcodeID", chemLog.BarcodeID);
            ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "CourseID", chemLog.CourseID);
            return View(chemLog);
        }

        // GET: ChemLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemLog = await _context.ChemLog.SingleOrDefaultAsync(m => m.LogID == id);
            if (chemLog == null)
            {
                return NotFound();
            }

            return View(chemLog);
        }

        // POST: ChemLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chemLog = await _context.ChemLog.SingleOrDefaultAsync(m => m.LogID == id);
            _context.ChemLog.Remove(chemLog);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ChemLogExists(int id)
        {
            return _context.ChemLog.Any(e => e.LogID == id);
        }
    }
}
