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
    public class ChemicalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChemicalsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Chemicals
        public async Task<IActionResult> Index(string chemstring)
        {
            ViewData["CurrentFilter"] = chemstring;
            sp_Logging("1-Info", "View", "Successfuly viewed Chemicals list", "Success");
            var chemicals = from m in _context.Chemical
                             select m;

            if (!String.IsNullOrEmpty(chemstring))
            {
                int forID;
                if (Int32.TryParse(chemstring, out forID))
                {
                    chemicals = chemicals.Where(s => s.ChemID.Equals(forID));
                    return View(await chemicals.OrderByDescending(s => s.ChemID).ToListAsync());
                }
                else
                {
                    chemicals = chemicals.Where(s => s.CAS.Contains(chemstring)
                                       || s.Formula.Contains(chemstring)
                                       || s.FormulaName.Contains(chemstring)
                                       || s.FormulaWeight.Contains(chemstring)
                                       || s.CAT.Contains(chemstring)
                                       || s.CAS.Contains(chemstring)
                                       || s.Hazard.Contains(chemstring)
                                       || s.State.Contains(chemstring));
                    return View(await chemicals.OrderByDescending(s => s.ChemID).ToListAsync());
                }
            }

            // var applicationDbContext = _context.bioicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await chemicals.OrderByDescending(s => s.ChemID).ToListAsync());
            //return View(await _context.Chemical.ToListAsync());
        }

        // GET: Chemicals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemical = await _context.Chemical.SingleOrDefaultAsync(m => m.ChemID == id);
            if (chemical == null)
            {
                return NotFound();
            }

            return View(chemical);
        }

        // GET: Chemicals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chemicals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChemID,CAS,CAT,Formula,FormulaName,FormulaWeight,Hazard,SDS,State")] Chemical chemical)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chemical);
                await _context.SaveChangesAsync();
                sp_Logging("2-Change", "Create", "User created a chemical", "Success");
                return RedirectToAction("Index");
            }
            return View(chemical);
        }

        // GET: Chemicals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemical = await _context.Chemical.SingleOrDefaultAsync(m => m.ChemID == id);
            if (chemical == null)
            {
                return NotFound();
            }
            return View(chemical);
        }

        // POST: Chemicals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChemID,CAS,CAT,Formula,FormulaName,FormulaWeight,Hazard,SDS,State")] Chemical chemical)
        {
            if (id != chemical.ChemID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chemical);
                    await _context.SaveChangesAsync();
                    sp_Logging("2-Change", "Edit", "User edited a Chemical where ID= " + id.ToString(), "Success");
                }
                catch (DbUpdateConcurrencyException)
                {                   
                    throw;
                }
                return RedirectToAction("Index");
            }
            return View(chemical);
        }

        // GET: Chemicals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemical = await _context.Chemical.SingleOrDefaultAsync(m => m.ChemID == id);
            if (chemical == null)
            {
                return NotFound();
            }

            return View(chemical);
        }

        // POST: Chemicals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chemical = await _context.Chemical.SingleOrDefaultAsync(m => m.ChemID == id);
            _context.Chemical.Remove(chemical);
            await _context.SaveChangesAsync();
            sp_Logging("3-Remove", "Delete", "User deleted a Chemical where ID=" + id.ToString(), "Success");
            return RedirectToAction("Index");
        }

        private bool ChemicalExists(int? id)
        {
            return _context.Chemical.Any(e => e.ChemID == id);
        }

        private void sp_Logging(string level, string logger, string message, string exception)
        {

            string CS = "Server = cscsql2.carrollu.edu; Database = CarrollChemistry; User ID = CarrollChemistry; Password = Carroll2016;";
            string user = User.Identity.Name;
            string app = "Carroll LMS";
            DateTime logged = DateTime.Now;
            string site = "Chemicals";
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
