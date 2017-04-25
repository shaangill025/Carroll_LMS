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
    public class ChemInventoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChemInventoriesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: ChemInventories
        public async Task<IActionResult> Index(string cheminventorystring)
        {
            //var applicationDbContext = _context.ChemInventory.Include(c => c.Chemical).Include(c => c.Location).Include(c => c.Order);
            ViewData["CurrentFilter"] = cheminventorystring;
            sp_Logging("1-Info", "View", "Successfuly viewed Chemical Inventory list", "Success");
            var inventory = from m in _context.ChemInventory.Include(c => c.Chemical).Include(c => c.Location).Include(c => c.Order)
                             select m;

            if (!String.IsNullOrEmpty(cheminventorystring))
            {
                int forID;
                if (Int32.TryParse(cheminventorystring, out forID))
                {
                    inventory = inventory.Where(s => s.BarcodeID.Equals(forID));
                    return View(await inventory.OrderByDescending(s => s.BarcodeID).ToListAsync());
                }
                else
                {
                    inventory = inventory.Where(s => s.Chemical.CAS.Contains(cheminventorystring)
                                       || s.Chemical.Formula.Contains(cheminventorystring)
                                       || s.Chemical.FormulaName.Contains(cheminventorystring)
                                       || s.Department.Equals(cheminventorystring)
                                       || s.Chemical.Hazard.Contains(cheminventorystring)
                                       || s.Chemical.State.Contains(cheminventorystring)
                                       || s.Order.Invoice.Contains(cheminventorystring)
                                       || s.Order.PO.Contains(cheminventorystring)
                                       || s.Order.Vendor.Name.Contains(cheminventorystring)
                                       || s.Order.CAT.Contains(cheminventorystring)
                                       || s.Location.Name.Contains(cheminventorystring)
                                       || s.Location.NormalizedStr.Contains(cheminventorystring)
                                       || s.Location.Room.Contains(cheminventorystring));
                    return View(await inventory.OrderByDescending(s => s.BarcodeID).ToListAsync());
                }
            }

            // var applicationDbContext = _context.bioicalEquipments.Include(c => c.Location).Include(c => c.Order);
            return View(await inventory.OrderByDescending(s => s.BarcodeID).ToListAsync());
            //return View(await applicationDbContext.ToListAsync());
        }

        // GET: ChemInventories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            if (chemInventory == null)
            {
                return NotFound();
            }

            return View(chemInventory);
        }

        // GET: ChemInventories/Create
        public IActionResult Create()
        {
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "FormulaName");
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "StorageCode");
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID");
            return View();
        }

        // POST: ChemInventories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? formulainput, DateTime dateinput, int? storageinput, int? orderinput, float qtyinput, string unitstring, string deptstring)
        {
            //[Bind("BarcodeID,OrderID,LocationID,ChemID,Units,QtyLeft,ExpiryDate")] ChemInventory chemInventory
            //int formulainput,DateTime dateinput,int storageinput,int orderinput,float qtyinput,string unitstring
            //int formulainput, DateTime dateinput, int storageinput, int orderinput, float qtyinput, string unitstring
            
            ViewData["Formula"] = formulainput;
            ViewData["ExpiryDate"] = dateinput;
            ViewData["StorageCode"] = storageinput;
            ViewData["Order"] = orderinput;
            ViewData["Qty"] = qtyinput;
            ViewData["Unit"] = unitstring;
            ViewData["Department"] = deptstring;
            ChemInventory chemInventory = null;

            if (ModelState.IsValid)
            {
                //var chemID = _context.Chemical.Where(p => p.Formula == FormulaString).Select(p => p.ChemID);
                //var Chem = _context.Chemical.Where(p => p.Formula == FormulaString);
                //chemInventory.ChemID = await chemID;
                chemInventory = new ChemInventory();
                chemInventory.ChemID = formulainput;
                chemInventory.LocationID = storageinput;
                chemInventory.ExpiryDate = dateinput;
                chemInventory.OrderID = orderinput;
                chemInventory.QtyLeft = qtyinput;
                chemInventory.Units = unitstring;
                chemInventory.Department = deptstring;
                var temp = _context.Locations.First(m => m.LocationID == storageinput);
                chemInventory.NormalizedLocation = temp.StorageCode;
                
                _context.Add(chemInventory);
                await _context.SaveChangesAsync();
                sp_Logging("2-Change", "Create", "User created a chemical inventory item where ChemID=" + formulainput + ", OrderID=" + formulainput, "Success");
                return RedirectToAction("Index");
            }
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "FormulaName", chemInventory.ChemID);
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "StorageCode", chemInventory.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemInventory.OrderID);
            return View(chemInventory);
        }

        // GET: ChemInventories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            if (chemInventory == null)
            {
                return NotFound();
            }
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "FormulaName", chemInventory.ChemID);
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "StorageCode", chemInventory.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemInventory.OrderID);
            return View(chemInventory);
        }

        // POST: ChemInventories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int? formulainput, DateTime dateinput, int? storageinput, int? orderinput, float qtyinput, string unitstring, string deptstring)
        {
            //[Bind("BarcodeID,OrderID,LocationID,ChemID,Units,QtyLeft,ExpiryDate")] ChemInventory chemInventory
            ChemInventory chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(p => p.BarcodeID == id);

            if (id != chemInventory.BarcodeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    chemInventory.ChemID = formulainput;
                    chemInventory.LocationID = storageinput;
                    chemInventory.ExpiryDate = dateinput;
                    chemInventory.OrderID = orderinput;
                    chemInventory.QtyLeft = qtyinput;
                    chemInventory.Units = unitstring;
                    chemInventory.Department = deptstring;
                    var temp = _context.Locations.First(m => m.LocationID == storageinput);
                    chemInventory.NormalizedLocation = temp.StorageCode;
                    _context.Update(chemInventory);
                    await _context.SaveChangesAsync();
                    sp_Logging("2-Change", "Edit", "User edited a Chemical inventory item where ID= " + id.ToString(), "Success");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChemInventoryExists(chemInventory.BarcodeID))
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
            ViewData["ChemID"] = new SelectList(_context.Chemical, "ChemID", "FormulaName", chemInventory.ChemID);
            ViewData["LocationName"] = new SelectList(_context.Locations, "LocationID", "StorageCode", chemInventory.LocationID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", chemInventory.OrderID);
            return View(chemInventory);
        }

        // GET: ChemInventories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            if (chemInventory == null)
            {
                return NotFound();
            }

            return View(chemInventory);
        }

        // POST: ChemInventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chemInventory = await _context.ChemInventory.SingleOrDefaultAsync(m => m.BarcodeID == id);
            _context.ChemInventory.Remove(chemInventory);
            await _context.SaveChangesAsync();
            sp_Logging("3-Remove", "Delete", "User deleted a Chemical inventory item where ID=" + id.ToString(), "Success");
            return RedirectToAction("Index");
        }

        private bool ChemInventoryExists(int id)
        {
            return _context.ChemInventory.Any(e => e.BarcodeID == id);
        }

        private void sp_Logging(string level, string logger, string message, string exception)
        {

            string CS = "Server = cscsql2.carrollu.edu; Database = CarrollChemistry; User ID = CarrollChemistry; Password = Carroll2016;";
            string user = User.Identity.Name;
            string app = "Carroll LMS";
            DateTime logged = DateTime.Now;
            string site = "Chemical Inventory";
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
