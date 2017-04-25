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
    [Authorize(Roles = "Admin,AnimalUser,Student")]
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
            sp_Logging("1-Info", "View", "Successfuly viewed Cage log list", "Success");

            var logs = from m in _context.CageLog.Include(c => c.Animal)
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
                    logs = logs.Where(s => s.Animal.Designation.Contains(cagelogstring)
                                       || s.Animal.Species.Contains(cagelogstring)
                                       || s.Animal.Gender.Contains(cagelogstring)
                                       || s.Animal.Order.Invoice.Contains(cagelogstring)
                                       || s.Animal.Order.PO.Contains(cagelogstring)
                                       || s.Animal.Order.Vendor.Name.Contains(cagelogstring)
                                       || s.Animal.Order.CAT.Contains(cagelogstring)
                                       || s.Animal.Location.Name.Contains(cagelogstring)
                                       || s.Animal.Location.NormalizedStr.Contains(cagelogstring)
                                       || s.Animal.Location.Room.Contains(cagelogstring));
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
            ViewData["Animals"] = new SelectList(_context.Animal, "AnimalID", "Name");
            return View();
        }

        // POST: CageLogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LogID,AnimalID,Clean,Food,FoodComments,Social,SocialComments,WashComments,Washed")] CageLog cageLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cageLog);
                await _context.SaveChangesAsync();
                sp_Logging("2-Change", "Create", "User created a Cage Log entry where AnimalID=" + cageLog.AnimalID, "Success");
                return RedirectToAction("Index");
            }
            ViewData["Animals"] = new SelectList(_context.Animal, "AnimalID", "Name", cageLog.AnimalID);
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
            ViewData["Animals"] = new SelectList(_context.Animal, "AnimalID", "Name", cageLog.AnimalID);
            return View(cageLog);
        }

        // POST: CageLogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogID,AnimalID,Clean,DatetimeCreated,Food,FoodComments,Social,SocialComments,WashComments,Washed")] CageLog cageLog)
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
                    sp_Logging("2-Change", "Edit", "User edited a Cage Log entry where ID= " + id.ToString(), "Success");

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
            ViewData["Animals"] = new SelectList(_context.Animal, "AnimalID", "Name", cageLog.AnimalID);
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
            sp_Logging("3-Remove", "Delete", "User removed a Cage Log entry where ID= " + id.ToString(), "Success");
            return RedirectToAction("Index");
        }

        private bool CageLogExists(int id)
        {
            return _context.CageLog.Any(e => e.LogID == id);
        }

        private void sp_Logging(string level, string logger, string message, string exception)
        {

            string CS = "Server = cscsql2.carrollu.edu; Database = CarrollChemistry; User ID = CarrollChemistry; Password = Carroll2016;";
            string user = User.Identity.Name;
            string app = "Carroll LMS";
            DateTime logged = DateTime.Now;
            string site = "Cage Log";
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
