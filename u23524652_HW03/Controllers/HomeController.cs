using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using u23524652_HW03.Models;
using Ganss.Xss;
using System.Diagnostics;


namespace u23524652_HW03.Controllers
{
    public class HomeController : Controller
    {
        private LibraryEntities db = new LibraryEntities();

        public async Task<ActionResult> CombinedIndex(int studentPage = 1, int bookPage = 1)
        {
            int pageSize = 10;

            // Fetch students and books from the database
            List<students> students = await db.students.ToListAsync();
            List<books> books = await db.books.Include(p=>p.authors).Include(m=>m.types).ToListAsync();

            // Create a list to hold books with their availability status
            var bookAvailabilityList = books.Select(book =>
            {
                // Determine if the book is available based on the new criteria
                bool isAvailable = true;

                // Check if there are any borrow entries for the book
                var borrowEntries = db.borrows.Where(b => b.bookId == book.bookId).ToList();

                if (borrowEntries.Count == 0)
                {
                    // No borrow entries means the book is available
                    isAvailable = true;
                }
                else
                {
                    
                    var latestBorrow = borrowEntries.OrderByDescending(b => b.takenDate).First();

                    
                    isAvailable = latestBorrow.broughtDate != null;
                }

                return new BookAvailabilityViewModel
                {
                    BookId = book.bookId,
                    Name = book.name,
                    PageCount = book.pagecount,
                    Point = book.point,
                    IsAvailable = isAvailable,
                    Author=book.authors.name + " " + book.authors.surname,
                    Type=book.types.name
                   
                };
            }).ToList();

            // Pagination for students
            PagedList<students> studentList = new PagedList<students>
            {
                Items = students.Skip((studentPage - 1) * pageSize).Take(pageSize).ToList(),
                PageNumber = studentPage,
                PageSize = pageSize,
                TotalItems = students.Count
            };

            // Pagination for books
            PagedList<BookAvailabilityViewModel> bookList = new PagedList<BookAvailabilityViewModel>
            {
                Items = bookAvailabilityList.Skip((bookPage - 1) * pageSize).Take(pageSize).ToList(),
                PageNumber = bookPage,
                PageSize = pageSize,
                TotalItems = bookAvailabilityList.Count()
            };

            // Create the view model
            var model = new LibraryViewModel
            {
                Students = studentList,
                Books = bookList
            };

            return View(model);
        }

        public async Task<ActionResult> CombinedDetails(int authorPage = 1, int typePage = 1, int borrowPage = 1)
        {
            int pageSize = 10;

            // Fetch students and books from the database
            List<authors> authors = await db.authors.ToListAsync();
            List<types> types = await db.types.ToListAsync();
            List<borrows> borrows = await db.borrows.Include(s => s.students).Include(p => p.books).ToListAsync();

            PagedList<authors> authorList = new PagedList<authors>
            {
                Items = authors.Skip((authorPage - 1) * pageSize).Take(pageSize).ToList(),
                PageNumber = authorPage,
                PageSize = pageSize,
                TotalItems = authors.Count
            };
            PagedList<types> typeList = new PagedList<types>
            {
                Items = types.Skip((typePage - 1) * pageSize).Take(pageSize).ToList(),
                PageNumber = typePage,
                PageSize = pageSize,
                TotalItems = types.Count
            };
            PagedList<borrows> borrowList = new PagedList<borrows>
            {
                Items = borrows.Skip((borrowPage - 1) * pageSize).Take(pageSize).ToList(),
                PageNumber = borrowPage,
                PageSize = pageSize,
                TotalItems = borrows.Count
            };

            var model = new LibraryViewModel
            {
                Authors = authorList,
                Types = typeList,
                Borrows = borrowList
            };
            return View(model);
        }
        public async Task<ActionResult> DurationAnalysis()
        {
            var report = await GetDurationAnalysisReportAsync(); 

            // Get all CSV files from the Reports directory
            string directoryPath = Server.MapPath("~/Reports");
            var reportFiles = Directory.GetFiles(directoryPath, "*.csv")
                .Select(Path.GetFileName)
                .ToList();

            // Create the view model
            var viewModel = new DurationAnalysisViewModel
            {
                ReportFiles = reportFiles,
                Report = report
            };

            return View(viewModel); // Pass the view model to the view
        }

        private async Task<DurationAnalysisReport> GetDurationAnalysisReportAsync()
        {
            var report = new DurationAnalysisReport();

            // Calculate overall average borrow duration
            report.AverageBorrowDuration = (double)await db.borrows
                .GroupBy(b => 1)
                .Select(g => g.Average(b => (DbFunctions.DiffDays(b.takenDate, b.broughtDate ?? DateTime.Now))))
                .FirstOrDefaultAsync();

            // Get average borrow duration for each book
            report.BorrowDetails = await db.borrows
                .Include(b => b.books) 
                .GroupBy(b => b.bookId) // Grouping by BookId to calculate average duration per book
                .Select(g => new BookBorrowDetail
                {
                    BookName = g.Select(b => b.books.name).FirstOrDefault(), // Get the book name
                    AverageBorrowDuration = (double)g.Average(b => DbFunctions.DiffDays(b.takenDate, b.broughtDate ?? DateTime.Now))
                })
                .ToListAsync();

            return report;
        }
        public async Task<ActionResult> SaveReport(string fileName)
        {
            var report = await GetDurationAnalysisReportAsync(); 

            // Set the file extension
            string fileExtension = ".csv";

            // Prepare the file name and path
            string directoryPath = Server.MapPath("~/Reports");

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, $"{fileName}{fileExtension}");

            // Generate the file content
            string fileContent = GenerateFileContent(report); 

            // Save the content to the file
            System.IO.File.WriteAllText(filePath, fileContent);

            // Redirect back to the DurationAnalysis view
            return RedirectToAction("DurationAnalysis");
        }



        private string GenerateFileContent(DurationAnalysisReport report)
        {
            
            
                var csvContent = new System.Text.StringBuilder();
                csvContent.AppendLine("Book Name,Average Borrow Duration (Days)");

                foreach (var detail in report.BorrowDetails)
                {
                    csvContent.AppendLine($"{detail.BookName},{detail.AverageBorrowDuration}");
                }

                return csvContent.ToString();
              
        
            }

        public ActionResult DownloadReport(string fileName)
        {
            string directoryPath = Server.MapPath("~/Reports");
            string filePath = Path.Combine(directoryPath, fileName);

            // Ensure the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            return File(filePath, "text/csv", fileName); 
        }

        [HttpPost]
        public ActionResult DeleteReport(string fileName)
        {
            string directoryPath = Server.MapPath("~/Reports");
            string filePath = Path.Combine(directoryPath, fileName);

            // Ensure the file exists
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath); 
            }

            return RedirectToAction("DurationAnalysis"); 
        }

        [ValidateInput(false)]
        public ActionResult AddDescription(string fileName, string description)
        {
            string directoryPath = Server.MapPath("~/Reports");
            string filePath = Path.Combine(directoryPath, $"{fileName}");

            Debug.WriteLine($"File Path: {filePath}");

            if (!System.IO.File.Exists(filePath))
            {

                return RedirectToAction("DurationAnalysis");
            }

            try
            {
                var sanitizer = new HtmlSanitizer();
                string sanitizedDescription = sanitizer.Sanitize(description);
                string formattedDescription = $"Description: {sanitizedDescription.Replace(",", ";")}";

                System.IO.File.AppendAllText(filePath, $"\n{formattedDescription}");
              
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error appending to file: {ex.Message}");
            }

            return RedirectToAction("DurationAnalysis");
        }


    }
}

  


