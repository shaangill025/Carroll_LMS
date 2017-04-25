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
using System.Data.SqlClient;

namespace LMS4Carroll.Controllers
{
    [Authorize (Roles = "Admin,ChemUser,BiologyUser,Student")]
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
            sp_Logging("1-Info", "View", "Successfuly viewed Chemical Log list", "Success");
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
                                       || s.ChemInventory.Location.Name.Contains(chemlogstring)
                                       || s.ChemInventory.Location.Room.Contains(chemlogstring)
                                       || s.ChemInventory.Location.NormalizedStr.Contains(chemlogstring)
                                       || s.ChemInventory.BarcodeID.Equals(forID)
                                       || s.ChemInventory.Order.Vendor.Name.Contains(chemlogstring)
                                       || s.ChemInventory.Order.Invoice.Contains(chemlogstring)
                                       || s.ChemInventory.Order.PO.Contains(chemlogstring)
                                       || s.ChemInventory.Order.CAT.Contains(chemlogstring));
                    return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());

                }
            }

            // var applicationDbContext = _context.bioicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await logs.OrderByDescending(s => s.LogID).ToListAsync());
            //return View(await applicationDbContext.ToListAsync());
        }

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
            ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "NormalizedStr");
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
                    sp_Logging("2-Change", "Create", "User created a Log entry where Barcode=" + barcodeinput, "Success");
                    return RedirectToAction("Index");
                }
                ViewData["BarcodeID"] = new SelectList(_context.ChemInventory, "BarcodeID", "BarcodeID", chemLog.BarcodeID);
                ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "NormalizedStr", chemLog.CourseID);
                return View(chemLog);
            }
            else
            {
                return View("CheckBarcode");
            }      
           
        }

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
            ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "NormalizedStr", chemLog.CourseID);
            return View(chemLog);
        }

        // POST: ChemLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int barcodeinput, int courseinput, float qtyusedinput)
        {
       
            if (_context.ChemInventory.Count(M => M.BarcodeID == barcodeinput) >= 1)
            {
                ChemLog chemLog = await _context.ChemLog.FirstAsync(m => m.LogID == id);
                ChemInventory temp = _context.ChemInventory.FirstOrDefault(s => s.BarcodeID == barcodeinput);
                float tempValue = temp.QtyLeft;
                temp.QtyLeft = tempValue - qtyusedinput;
                _context.Entry<ChemInventory>(temp).State = EntityState.Modified;
                _context.SaveChanges();
                chemLog.BarcodeID = barcodeinput;
                chemLog.CourseID = courseinput;
                chemLog.QtyUsed = qtyusedinput;

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
                        sp_Logging("2-Change", "Edit", "User edited a Log entry where ID= " + id.ToString(), "Success");

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
                ViewData["CourseID"] = new SelectList(_context.Course, "CourseID", "NormalizedStr", chemLog.CourseID);
                return View(chemLog);
            }
            else
            {
                return View("CheckBarcode");
            }           
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
            var used = chemLog.QtyUsed;
            var barcode = chemLog.BarcodeID;
            var chemInv = _context.ChemInventory.First(m => m.BarcodeID == barcode);
            var tempQty = chemInv.QtyLeft;
            chemInv.QtyLeft = tempQty + used;
            _context.Update(chemInv);
            _context.ChemLog.Remove(chemLog);
            await _context.SaveChangesAsync();
            sp_Logging("3-Remove", "Edit", "User deleted a Log entry where ID= " + id.ToString(), "Success");
            return RedirectToAction("Index");
        }

        private bool ChemLogExists(int id)
        {
            return _context.ChemLog.Any(e => e.LogID == id);
        }

        private void sp_Logging(string level, string logger, string message, string exception)
        {

            string CS = "Server = cscsql2.carrollu.edu; Database = CarrollChemistry; User ID = CarrollChemistry; Password = Carroll2016;";
            string user = User.Identity.Name;
            string app = "Carroll LMS";
            DateTime logged = DateTime.Now;
            string site = "Chemical Log";
            string query = "insert into dbo.Log([User], [Application], [Logged], [Level], [Message], [Logger], [CallSite], [Exception]) values(@User, @Application, @Logged, @Level, @Message,@Logger, @Callsite, @Exception)";
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@User", user);
                cmd.Parameters.AddWithValue("@Application", app);
                cmd.Parameters.AddWithValue("@Logged", logged);
                cmd.Parameters.AddWithValue("@Level", level);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@Logger", logger);
                cmd.Parameters.AddWithValue("@Callsite", site);
                cmd.Parameters.AddWithValue("@Exception", exception);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }
    }
}
