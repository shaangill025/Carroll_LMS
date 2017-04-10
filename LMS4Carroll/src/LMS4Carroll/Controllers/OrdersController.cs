using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LMS4Carroll.Data;
using LMS4Carroll.Models;
using System.Data.SqlTypes;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace LMS4Carroll.Controllers
{
    [Authorize(Roles = "Admin,Handler")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Orders
        public async Task<IActionResult> Index(string orderString)
        {
            ViewData["CurrentFilter"] = orderString;

            var orders = from m in _context.Orders.Include(c => c.Vendor)
                             select m;

            if (!String.IsNullOrEmpty(orderString))
            {
                /*SqlDateTime dateCompare = Convert.ToSqlDateTime(orderString);
                CultureInfo myCItrad = new CultureInfo("bg-BG");
                DateTime parsedDate = DateTime.ParseExact(
                   orderString,
                   "dd/MM/yyyy hh:mm:ss",
                   myCItrad);
                */
                DateTime dt;
                string dateTime;
                if (!orderString.Contains(":")) {
                    dateTime = orderString + " 00:00:00.0000000";
                }
                else
                {
                    dateTime = orderString + "00.0000000";
                }

                int forID;
                if (Int32.TryParse(orderString, out forID)&&!orderString.Contains("-"))
                {
                    orders = orders.Where(p => p.OrderID.Equals(forID));
                    return View(await orders.OrderByDescending(s => s.OrderID).ToListAsync());
                }
                else if (DateTime.TryParseExact(dateTime, "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    orders = orders.Where(p => p.Recievedate.Equals(dt)
                                || p.Orderdate.Equals(dt));
         
                    return View(await orders.OrderByDescending(s => s.OrderID).ToListAsync());
                }
                else
                {
                    orders = orders.Where(p => p.Status.Contains(orderString)
                                || p.OrderID.Equals(forID)
                                || p.Status.Contains(orderString)
                                || p.Type.Contains(orderString)
                                || p.Vendor.Name.Contains(orderString));
                    return View(await orders.OrderByDescending(s => s.OrderID).ToListAsync());
                }
            }
            //var applicationDbContext = _context.Orders.Include(o => o.Vendor);
            return View(await orders.OrderByDescending(s => s.OrderID).ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.SingleOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["VendorID"] = new SelectList(_context.Vendors, "VendorID", "Name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderID,Orderdate,Recievedate,Status,Type,VendorID")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["VendorID"] = new SelectList(_context.Vendors, "VendorID", "Name", order.VendorID);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.SingleOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["VendorID"] = new SelectList(_context.Vendors, "VendorID", "Address", order.VendorID);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,Orderdate,Recievedate,Status,Type,VendorID")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
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
            ViewData["VendorID"] = new SelectList(_context.Vendors, "VendorID", "Address", order.VendorID);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.SingleOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.SingleOrDefaultAsync(m => m.OrderID == id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}
