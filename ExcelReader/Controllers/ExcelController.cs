using ClosedXML.Extensions;
using DataAccess.IRepository;
using ExcelReader.Models;
using ExcelReader.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;

namespace ExcelReader.Controllers
{
    /*

    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : Controller
    {
        private readonly string[] AllowedExtensions = { ".xls", ".xlsx" };
        private long MAX_UPLOAD_SIZE = 1_000_000;


        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserRepository _userRepository;


        public ExcelController(IWebHostEnvironment webHostEnvironment, IUserRepository userRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;

        }
        [HttpGet]
        public async Task<IEnumerable<User>> Index()
        {
            var users = _userRepository.GetAll();

            return users != null ? users : Enumerable.Empty<User>();
        }


        [HttpPost]
        public async Task<ActionResult<User>> Upload([FromForm] Upload uploadData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (uploadData.excelFile == null)
            {
                return BadRequest();
            }
            //check file type

            var fileExtension = Path.GetExtension(uploadData.excelFile.FileName).ToLower();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return BadRequest();
            }


            if (uploadData.excelFile.Length > MAX_UPLOAD_SIZE)
            {
                return BadRequest();
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadData.excelFile.FileName);

            string baseFilePath = _webHostEnvironment.WebRootPath;
            string filePathDb = Path.Combine("uploads", fileName);
            var filePath = Path.Combine(baseFilePath, filePathDb);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadData.excelFile.CopyToAsync(stream);
            }

            //read excel

            var excelData = ExcelService2.ReadExcelFile(filePath);

            if (excelData == null)
            {
                return BadRequest(new { message = "No valid data in excel" });
            }
            IList<User> users = new List<User>();

            if (excelData.Rows[0].ItemArray.Length != 3)
            {
                //invalid file data
                return BadRequest(new { message = "Invalid file data" });
            }
            long skippedRows = 0;
            long totalRows = 0;
            long failedCount = 0;

            foreach (DataRow row in excelData.Rows)
            {
                //skip empty rows or label row
                if (string.IsNullOrEmpty(row[0].ToString()) || string.IsNullOrEmpty(row[1].ToString()) || string.IsNullOrEmpty(row[2].ToString()))
                {
                    skippedRows++;
                    totalRows++;
                    continue;
                }
                var user = new User { UUID = row[0].ToString(), Email = row[2].ToString(), Name = row[1].ToString(), CreatedAt = DateTime.Now };
                totalRows++;
                var id = _userRepository.Add(user);
                if (id == 0)
                {
                    //failed to add
                    failedCount++;

                }
                //user.Id = id;
                //users.Add(user);
            }

            //delete the uploaded file
            System.IO.File.Delete(filePath);
            //TODO: add db ops
            return Ok(new
            {
                //users,
                totalRows,
                skippedRows,
                failedCount
            });
        }

        [HttpPost]
        [Route("export")]
        public async Task<ActionResult> Export()
        {
            //export all data from database table to a excel file
            var allEntry = _userRepository.GetAll();
            IList<string> columns = new List<string> { "UUID", "Name", "Email", "CreatedAt" };


            var filtered = allEntry
                .Select(user => new User(
                    user.UUID,
                    user.Name,
                    user.Email,
                    user.CreatedAt
                ));
            var fileName = Guid.NewGuid().ToString() + ".xlsx";
            string baseFilePath = _webHostEnvironment.WebRootPath;
            string filePath2 = Path.Combine("exports", fileName);
            var filePath = Path.Combine(baseFilePath, filePath2);

            var wb = ExcelService2.WriteExcelFile(columns, filtered, filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Return the file as a downloadable response
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report_all_users.xlsx");
        }
    }
    */
}
