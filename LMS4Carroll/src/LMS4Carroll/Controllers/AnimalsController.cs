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
using NLog;
using System.Data;
using System.Data.SqlClient;

namespace LMS4Carroll.Controllers
{
    [Authorize(Roles = "Admin,AnimalUser,Student")]
    [ServiceFilter(typeof(LogFilter))]
    public class AnimalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        //private readonly NLog.ILogger _logger;

        public AnimalsController(ApplicationDbContext context)
        {
            _context = context;
            //_logger = LogManager.GetLogger("databaseLogger");
        }

        private void sp_Logging(string level, string logger, string message, string exception)
        {

            string CS = "Server = cscsql2.carrollu.edu; Database = CarrollChemistry; User ID = CarrollChemistry; Password = Carroll2016;";
            string user = User.Identity.Name;
            string app = "Carroll LMS";
            DateTime logged = DateTime.Now;
            string site = "Animal";
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

        // GET: Cages
        public async Task<IActionResult> Index(string cagestring)
        {
            //_logger.Info("Viewed an animal list - AnimalController");
            //var applicationDbContext = _context.Cage.Include(c => c.Location).Include(c => c.Order);
            sp_Logging("1-Info", "View", "Viewed list of animals", "Success");
            ViewData["CurrentFilter"] = cagestring;

            var cages = from m in _context.Animal.Include(c => c.Location).Include(c => c.Order)
                             select m;

            if (!String.IsNullOrEmpty(cagestring))
            {
                int forID;
                if (Int32.TryParse(cagestring, out forID))
                {
                    cages = cages.Where(s => s.AnimalID.Equals(forID));
                    return View(await cages.OrderByDescending(s => s.AnimalID).ToListAsync());
                }
                else
                {
                    cages = cages.Where(s => s.Designation.Contains(cagestring)
                                       || s.Gender.Contains(cagestring)
                                       || s.Location.Name.Contains(cagestring)
                                       || s.Location.Room.Contains(cagestring)
                                       || s.Location.NormalizedStr.Contains(cagestring)
                                       || s.Order.Invoice.Contains(cagestring)
                                       || s.Order.PO.Contains(cagestring)
                                       || s.Order.Vendor.Name.Contains(cagestring)
                                       || s.Order.CAT.Contains(cagestring));
                    return View(await cages.OrderByDescending(s => s.AnimalID).ToListAsync());
                }
            }
                return View(await cages.OrderByDescending(s => s.AnimalID).ToListAsync());

        }

        // GET: Cages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cage = await _context.Animal.SingleOrDefaultAsync(m => m.AnimalID == id);
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
        public async Task<IActionResult> Create(DateTime dobinput,string designationstring,string genderstring,int locationinput,int orderinput,string speciesstring, string namestring)
        {
            //_logger.Info("Attempted to add an animal - AnimalController");
            ViewData["DOB"] = dobinput;
            ViewData["Designation"] = designationstring;
            ViewData["Gender"] = genderstring;
            ViewData["Location"] = locationinput;
            ViewData["Order"] = orderinput;
            ViewData["Species"] = speciesstring;
            ViewData["Name"] = namestring;
            Animal cage = new Animal();
            //[Bind("CageID,DOB,Designation,Gender,LocationID,OrderID,Species")] Cage cage
            if (ModelState.IsValid)
            {
                var temp = _context.Locations.First(m => m.LocationID == locationinput);
                cage.DOB = dobinput;
                cage.Designation = designationstring;
                cage.Gender = genderstring;
                cage.LocationID = locationinput;
                cage.OrderID = orderinput;
                cage.Species = speciesstring;
                cage.Name = namestring;
                cage.NormalizedLocation = temp.NormalizedStr;
                _context.Add(cage);
                sp_Logging("2-Change", "Create", "User created an Animal where name=" + namestring, "Success");
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr", cage.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", cage.OrderID);
            return View(cage);
        }

        // GET: Cages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cage = await _context.Animal.SingleOrDefaultAsync(m => m.AnimalID == id);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DateTime dobinput, string designationstring, string genderstring, int locationinput, int orderinput, string speciesstring, string namestring)
        {
            Animal cage = await _context.Animal.FirstAsync(s => s.AnimalID == id);
            var temp = _context.Locations.First(m => m.LocationID == locationinput);
            cage.DOB = dobinput;
            cage.Designation = designationstring;
            cage.Gender = genderstring;
            cage.LocationID = locationinput;
            cage.OrderID = orderinput;
            cage.Species = speciesstring;
            cage.Name = namestring;
            cage.NormalizedLocation = temp.NormalizedStr;

            if (id != cage.AnimalID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    sp_Logging("2-Change", "Edit", "User edited an Animal where ID= " + id.ToString(), "Success");
                    _context.Update(cage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalExists(cage.AnimalID))
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

            sp_Logging("3-Remove", "Delete", "User removed an Animal where ID= " + id.ToString(), "Success");
            var cage = await _context.Animal.SingleOrDefaultAsync(m => m.AnimalID == id);
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
            var cage = await _context.Animal.SingleOrDefaultAsync(m => m.AnimalID == id);
            _context.Animal.Remove(cage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool AnimalExists(int id)
        {
            return _context.Animal.Any(e => e.AnimalID == id);
        }
    }
}
