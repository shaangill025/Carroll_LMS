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
            return RedirectToAction("Index");
        }

        private bool FileDetailExists(int id)
        {
            return _context.FileDetails.Any(e => e.FileDetailID == id);
        }
    }
}
