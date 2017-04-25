using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LMS4Carroll.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;
using LMS4Carroll.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMS4Carroll.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        //private SearchViewModel searchVM;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
            //searchVM = new SearchViewModel();
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Dashboard/Search
        public IActionResult Search()
        {
            return View();
        }

        // POST: Dashboard/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string searchstring, string entitystring)
        {
            ViewData["Search"] = searchstring;
            ViewData["Entity"] = entitystring;
            switch (entitystring)
            {
                case "ChemEqpmt":
                    ChemEquipment chemModel = new ChemEquipment();
                    int ChemEqpmtInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                    
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out ChemEqpmtInt))
                        {
                            var temp = await _context.ChemicalEquipments.Where(s => s.ChemEquipmentID.Equals(ChemEqpmtInt)).SingleAsync();
                            chemModel = temp;
                            ViewData["Result"] = chemModel;
                            ViewData["Type"] = "ChemEqpmt";
                            return View();
                        }
                    }
                    
                    //@ViewData["Type"] = "ChemEqpmt";
                    break;
                case "BioEqpmt":
                    BioEquipment bioModel = new BioEquipment();
                    int BioEqpmtInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                    
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out BioEqpmtInt))
                        {
                            var temp = await _context.BioEquipments.Where(s => s.BioEquipmentID.Equals(BioEqpmtInt)).SingleAsync();
                            bioModel = temp;
                            ViewData["Result"] = bioModel;
                            ViewData["Type"] = "BioEqpmt";
                            return View();
                        }
                    }
                    //@ViewData["Result"] = tempbeqpmt;
                    //@ViewData["Type"] = "BioEqpmt";
                    break;
                case "Animal":
                    Animal animalModel = new Animal();
                    int AnimalInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                   
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out AnimalInt))
                        {
                            var temp = await _context.Animal.Where(s => s.AnimalID.Equals(AnimalInt)).SingleAsync();
                            animalModel = temp;
                            ViewData["Result"] = animalModel;
                            ViewData["Type"] = "Animal";
                            return View();
                        }
                    }
                    //@ViewData["Result"] = tempAnimal;
                    //@ViewData["Type"] = "Animal";
                    break;
                case "Order":
                    Order orderModel = new Order();
                    int OrderInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                    
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out OrderInt))
                        {
                            var temp = await _context.Orders.Where(s => s.OrderID.Equals(OrderInt)).SingleAsync();
                            orderModel = temp;
                            ViewData["Result"] = orderModel;
                            ViewData["Type"] = "Order";
                            return View();
                        }
                    }
                    //@ViewData["Result"] = tempOrder;
                    //@ViewData["Type"] = "Order";
                    break;
                case "Vendor":
                    Vendor vendorModel = new Vendor();
                    int VendorInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                   
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out VendorInt))
                        {
                            var temp = await _context.Vendors.Where(s => s.VendorID.Equals(VendorInt)).SingleAsync();
                            vendorModel = temp;
                            ViewData["Result"] = vendorModel;
                            ViewData["Type"] = "Vendor";
                            return View();
                        }
                    }
                    //@ViewData["Result"] = tempVendor;
                    //@ViewData["Type"] = "Vendor";
                    break;
                case "Location":
                    Location locationModel = new Location();
                    int LocInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                    
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out LocInt))
                        {
                            var temp = await _context.Locations.Where(s => s.LocationID.Equals(LocInt)).SingleAsync();
                            locationModel = temp;
                            ViewData["Result"] = locationModel;
                            ViewData["Type"] = "Location";
                            return View();
                        }
                    }
                    // @ViewData["Result"] = tempLoc;
                    //@ViewData["Type"] = "Location";
                    break;
                case "Courses":
                    Course courseModel = new Course();
                    int CourseInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                    
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out CourseInt))
                        {
                            var temp = await _context.Course.Where(s => s.CourseID.Equals(CourseInt)).SingleAsync();
                            courseModel = temp;
                            ViewData["Result"] = courseModel;
                            ViewData["Type"] = "Courses";
                            return View();
                        }
                    }

                    break;
                    //@ViewData["Result"] = tempCourse;
                    //@ViewData["Type"] = "Courses";
                case "Chemical":
                    Chemical chemicalModel = new Chemical();
                    int ChemInt = 0;
                    //Int32.TryParse(searchstring, out ChemInt);
                    
                    if (!String.IsNullOrEmpty(searchstring))
                    {
                        if (Int32.TryParse(searchstring, out ChemInt))
                        {
                            var temp = await _context.Chemical.Where(s => s.ChemID.Equals(ChemInt)).SingleAsync();
                            chemicalModel = temp;
                            ViewData["Result"] = chemicalModel;
                            ViewData["Type"] = "Chemical";
                            return View();
                        }
                    }                  
                    break;
                    ///@ViewData["Result"] = "ChemID: " + tempChem.ChemID;
                    //return View(await tempChem.ToListAsync());
                default:
                    ViewData["Result"] = "default";
                    return View();
            }
            return View();
        }
    }
}