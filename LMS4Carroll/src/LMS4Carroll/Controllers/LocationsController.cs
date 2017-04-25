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
    [Authorize(Roles = "Admin,ChemUser,BiologyUser,Student")]
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            sp_Logging("1-Info", "View", "Successfuly viewed Locations list", "Success");
            return View(await _context.Locations.ToListAsync());
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations.SingleOrDefaultAsync(m => m.LocationID == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Locations/Create
        [Authorize(Roles = "Admin,Handler")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string addressstring,string namestring,string typestring,string roomstring,string storagestring)
        {
            //[Bind("LocationID,Address,Name,Room,Type")] Location location
            ViewData["Address"] = addressstring;
            ViewData["Name"] = namestring;
            ViewData["Type"] = typestring;
            ViewData["Room"] = roomstring;
            ViewData["StorageCode"] = storagestring;
            Location location = new Location();
            if (ModelState.IsValid)
            {
                location.Address = addressstring;
                location.Name = namestring;
                location.Type = typestring;
                location.Room = roomstring;
                location.NormalizedStr = namestring + "-" + roomstring;
                location.StorageCode = storagestring;
                _context.Add(location);
                await _context.SaveChangesAsync();
                sp_Logging("2-Change", "Create", "User created location: " + namestring + "-" + roomstring, "Success");
                return RedirectToAction("Index");
            }
            return View(location);
        }

        // GET: Locations/Edit/5
        //[Authorize(Roles = "Admin,Handler")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations.SingleOrDefaultAsync(m => m.LocationID == id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin,Handler")]
        public async Task<IActionResult> Edit(int id, [Bind("LocationID,Address,Name,StorageCode,Room,Type")] Location location)
        {
            if (id != location.LocationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string name = location.Name;
                    string room = location.Room;
                    location.NormalizedStr = name + "-" + room;
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                    sp_Logging("2-Change", "Edit", "User edited a location where ID= " + id.ToString(), "Success");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.LocationID))
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
            return View(location);
        }

        // GET: Locations/Delete/5
        //[Authorize(Roles = "Admin,Handler")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations.SingleOrDefaultAsync(m => m.LocationID == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin,Handler")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Locations.SingleOrDefaultAsync(m => m.LocationID == id);
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            sp_Logging("3-Remove", "Delete", "User removed a location where ID= " + id.ToString(), "Success");
            return RedirectToAction("Index");
        }

        private bool LocationExists(int? id)
        {
            return _context.Locations.Any(e => e.LocationID == id);
        }

        private void sp_Logging(string level, string logger, string message, string exception)
        {

            string CS = "Server = cscsql2.carrollu.edu; Database = CarrollChemistry; User ID = CarrollChemistry; Password = Carroll2016;";
            string user = User.Identity.Name;
            string app = "Carroll LMS";
            DateTime logged = DateTime.Now;
            string site = "Locations";
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
