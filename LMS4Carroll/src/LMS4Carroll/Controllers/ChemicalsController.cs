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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChemicalExists(chemical.ChemID))
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
            return RedirectToAction("Index");
        }

        private bool ChemicalExists(int id)
        {
            return _context.Chemical.Any(e => e.ChemID == id);
        }
    }
}
