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
    [Authorize(Roles = "Admin,ChemUser,BiologyUser,Student")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Courses
        public async Task<IActionResult> Index(string coursestring)
        {
            ViewData["CurrentFilter"] = coursestring;

            var courses = from m in _context.Course.Include(c => c.Location)
                             select m;

            if (!String.IsNullOrEmpty(coursestring))
            {
                int forID;
                if (Int32.TryParse(coursestring, out forID))
                {
                    courses = courses.Where(s => s.CourseID.Equals(forID));
                    return View(await courses.OrderByDescending(s => s.CourseID).ToListAsync());
                }
                else
                {
                    courses = courses.Where(s => s.Department.Contains(coursestring)
                                       || s.Handler.Contains(coursestring)
                                       || s.NormalizedStr.Contains(coursestring)
                                       || s.Location.Name.Contains(coursestring)
                                       || s.Location.Room.Contains(coursestring)
                                       || s.Location.NormalizedStr.Contains(coursestring)
                                       || s.Name.Contains(coursestring)
                                       || s.Number.Contains(coursestring));
                    return View(await courses.OrderByDescending(s => s.CourseID).ToListAsync());
                }
            }

            // var applicationDbContext = _context.bioicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await courses.OrderByDescending(s => s.CourseID).ToListAsync());
            //return View(await _context.Course.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.SingleOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewData["LocationName"] = new SelectList(_context.Locations.Distinct(), "LocationID", "NormalizedStr");

            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string deptstring,string handlerstring,
            string namestring, string numberstring, int locationinput)
        {
            //[Bind("CourseID,Department,Handler,Instructor,Name,Number")] Course course
            ViewData["Location"] = locationinput;
            ViewData["Name"] = namestring;
            ViewData["Number"] = numberstring;
            //ViewData["Instructor"] = instructorstring;
            ViewData["Handler"] = handlerstring;
            ViewData["Department"] = deptstring;
            Course course = new Course();
            if (ModelState.IsValid)
            {
                course.Department = deptstring;
                course.Handler = handlerstring;
                course.Name = namestring;
                course.Number = numberstring;
                course.LocationID = locationinput;
                course.NormalizedStr = deptstring + "-" + numberstring;               
                var temp = _context.Locations.First(m => m.LocationID == locationinput);
                course.NormalizedLocation = temp.NormalizedStr;
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", course.LocationID);

            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.SingleOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", course.LocationID);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string deptstring, string handlerstring,
            string namestring, string numberstring, int locationinput)
        {
            var course = await _context.Course.FirstAsync(m => m.CourseID == id);
            course.Department = deptstring;
            course.Handler = handlerstring;
            course.Name = namestring;
            course.Number = numberstring;
            course.LocationID = locationinput;
            course.NormalizedStr = deptstring + "-" + numberstring;
            var temp = _context.Locations.First(m => m.LocationID == locationinput);
            course.NormalizedLocation = temp.NormalizedStr;
            _context.Entry<Course>(course).State = EntityState.Modified;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseID))
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
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "NormalizedStr", course.LocationID);
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.SingleOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.SingleOrDefaultAsync(m => m.CourseID == id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.CourseID == id);
        }
    }
}
