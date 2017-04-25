using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LMS4Carroll.Data;
using LMS4Carroll.Models;
//using LMS4Carroll.Services;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;

namespace LMS4Carroll.Controllers
{
    public class FileDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FileDetailsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: FileDetails
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FileDetails.Include(f => f.Order);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        // GET: FileDetails/Create
        public IActionResult Create()
        {
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID");
            return View();
        }

        // POST: FileDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, string contenttype, string filename, string filetype, int orderid)
        {
            //,[Bind("FileDetailID,Content,ContentType,FileName,FileType,OrderID")] FileDetail fileDetail
            //FileDetail fileDetail = new FileDetail(files, contenttype, filename, filetype, orderid);
            //List<IFormFile> files, string contenttype, string filename, string filetype, int  orderid
            byte[] fileBytes = null;

            if (file.Length > 0)
                {
                    using (var fileStream = file.OpenReadStream())
                    using (var ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
       
            }

            FileDetail fileDetail = new FileDetail();
            fileDetail.File = fileBytes;
            fileDetail.ContentType = contenttype;
            fileDetail.FileName = filename;
            fileDetail.FileType = filetype;
            fileDetail.OrderID = orderid;

            if (ModelState.IsValid)
            {
                _context.Add(fileDetail);
                await _context.SaveChangesAsync();
                sp_Logging("2-Change", "Create", "User uploaded a File where Filename=" + filename, "Success");
                return RedirectToAction("Index");
            }
            ViewData["OrderID"] = new SelectList(_context.Orders, "OrderID", "OrderID", fileDetail.OrderID);
            return View(fileDetail);
        }
       
        public async Task<FileResult> Download(int? id)
        {
            var fileDetail = await _context.FileDetails.SingleOrDefaultAsync(m => m.FileDetailID == id);
            byte[] fileBytes = fileDetail.File;
            string fileName = fileDetail.FileName;
            string fileExt = fileDetail.FileType;
            sp_Logging("2-Download", "Download", "User downloaded a file where ID=" + id.ToString(), "Success");
            return File(fileBytes, "application/x-msdownload", fileName + "." + fileExt);
        }

        // GET: FileDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileDetail = await _context.FileDetails.SingleOrDefaultAsync(m => m.FileDetailID == id);
            if (fileDetail == null)
            {
                return NotFound();
            }

            return View(fileDetail);
        }

        // POST: FileDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fileDetail = await _context.FileDetails.SingleOrDefaultAsync(m => m.FileDetailID == id);
            _context.FileDetails.Remove(fileDetail);
            await _context.SaveChangesAsync();
            sp_Logging("3-Remove", "Delete", "User deleted an Invoice where ID=" + id.ToString(), "Success");
            return RedirectToAction("Index");
        }

        private bool FileDetailExists(int id)
        {
            return _context.FileDetails.Any(e => e.FileDetailID == id);
        }

        private void sp_Logging(string level, string logger, string message, string exception)
        {

            string CS = "Server = cscsql2.carrollu.edu; Database = CarrollChemistry; User ID = CarrollChemistry; Password = Carroll2016;";
            string user = User.Identity.Name;
            string app = "Carroll LMS";
            DateTime logged = DateTime.Now;
            string site = "FileDB";
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
