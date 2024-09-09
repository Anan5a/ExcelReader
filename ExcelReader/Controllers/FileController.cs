using BLL;
using IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;
using Services;
using System.Net;
using System.Security.Claims;
using Utility;

namespace ExcelReader.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,user,super_admin")]
    public class FileController : Controller
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserRepository _userRepository;
        private readonly IFileMetadataRepository _fileMetadataRepository;
        private readonly IConfiguration _configuration;


        public FileController(
            IWebHostEnvironment webHostEnvironment,
            IUserRepository userRepository,
            IFileMetadataRepository fileMetadataRepository,
            IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _fileMetadataRepository = fileMetadataRepository;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("list")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<IEnumerable<FileMetadata>>>> ListFiles([FromQuery] string? page)
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            var list = FilesBLL.ListFiles(_fileMetadataRepository, userId, null);

            return Ok(CustomResponseMessage.OkCustom<IEnumerable<FileMetadata>>("Query successful.", list));

        }



        [HttpPost]
        [Route("upload")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<ulong?>>> Upload([FromForm] UploadDTO uploadDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var uploadState = FilesBLL.Upload(_fileMetadataRepository, _webHostEnvironment, userId, uploadDto);

            switch (uploadState)
            {
                case BLLReturnEnum.File_FILE_IS_EMPTY:
                    return BadRequest(CustomResponseMessage.ErrorCustom("file error", "No file"));

                case BLLReturnEnum.File_FILE_TYPE_NOT_ALLOWED:
                    return BadRequest(CustomResponseMessage.ErrorCustom("file error", "The file type is not allowed"));

                case BLLReturnEnum.File_FILE_SIZE_TOO_LARGE:
                    return BadRequest(CustomResponseMessage.ErrorCustom("file error", "The file exceeds maximum allowed size"));

                case BLLReturnEnum.ACTION_OK:
                    return Ok(CustomResponseMessage.OkCustom<ulong?>("File uploaded.", null));

                default:
                    return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Failed to upload file, try again later"));
            }
        }



        [HttpPost]
        [Route("update")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<ulong?>>> Update([FromBody] EditFileDTO editFileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            //check file type
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            var updateState = FilesBLL.Update(_fileMetadataRepository, userId, editFileDto);
            switch (updateState)
            {
                case BLLReturnEnum.File_FILE_NOT_FOUND:
                    return NotFound(CustomResponseMessage.ErrorCustom("no file", "File not found by the user"));

                case BLLReturnEnum.ACTION_OK:
                    return Ok(CustomResponseMessage.OkCustom<string>("File updated.", "File name was changed successfully."));

                default:
                    return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Failed to update file, try again later"));
            }
        }





        [HttpPost]
        [Route("download-link")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<string?>>> ExportLink([FromBody] ExportRequestDTO exportRequestDTO)
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["user_id"] = userId;
            condition["id"] = exportRequestDTO.FileId;

            var fileToDownload = _fileMetadataRepository.Get(condition);
            if (fileToDownload == null)
            {
                return NotFound(CustomResponseMessage.ErrorCustom("Not Found", "No file was found for the user"));
            }
            string baseFileDirectory = _webHostEnvironment.WebRootPath;
            string fileDirectory = Path.Combine(baseFileDirectory, "uploads");
            var filePathDisk = Path.Combine(fileDirectory, fileToDownload.FileNameSystem);
            ///////// Generate a signature /////////
            var validTill = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
            var signatureSrc = $"{fileToDownload.FileNameSystem}+{fileToDownload.FileName}+{validTill}";

            var signature = DownloadUrlSecurityService.SignData(signatureSrc, _configuration["FileDownloadSecurity:RSAPrivateKeyB64"]);

            var downloadLink = $"/download/{fileToDownload.FileNameSystem}?filename={WebUtility.UrlEncode(fileToDownload.FileName)}&expire={validTill}&signature={signature}";

            return Ok(CustomResponseMessage.OkCustom<string>("Link ok", downloadLink));


        }


        [HttpGet]
        [Route("/download/{fileId}")]
        [AllowAnonymous]

        public async Task<ActionResult> DownloadFile(
            string fileId,
            [FromQuery] string? filename,
            [FromQuery] string? expire,
            [FromQuery] string? signature)
        {
            //verify all required parameters are present

            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(expire) || string.IsNullOrEmpty(signature))
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid parameters?"));

            }

            //verify signature

            var validTill = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
            var signatureSrc = $"{fileId}+{filename}+{expire}";

            if (!DownloadUrlSecurityService.VerifySignature(signatureSrc, signature, _configuration["FileDownloadSecurity:RSAPublicKeyB64"]))
            {
                return Unauthorized(CustomResponseMessage.Unauthorized());
            }



            string baseFileDirectory = _webHostEnvironment.WebRootPath;
            string fileDirectory = Path.Combine(baseFileDirectory, "uploads");
            var filePathDisk = Path.Combine(fileDirectory, fileId);

            if (!System.IO.File.Exists(filePathDisk))
            {
                return NotFound(CustomResponseMessage.ErrorCustom("Not Found", "No file was found in the system"));
            }



            var fileStream = new FileStream(filePathDisk, FileMode.Open, FileAccess.Read);
            return File(fileStream, "application/octet-stream", filename);
        }


        [HttpDelete]
        [Route("delete/{fileId}")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<string?>>> DeleteFile(long fileId)
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["user_id"] = userId;
            condition["id"] = fileId;

            var fileToDelete = _fileMetadataRepository.Get(condition);

            if (fileToDelete == null)
            {
                return NotFound(CustomResponseMessage.ErrorCustom("Not Found", "No file was found for the user"));
            }
            string baseFileDirectory = _webHostEnvironment.WebRootPath;
            string fileDirectory = Path.Combine(baseFileDirectory, "uploads");
            var filePathDisk = Path.Combine(fileDirectory, fileToDelete.FileNameSystem);
            ///////// Generate a signature /////////

            fileToDelete.DeletedAt = DateTime.Now;

            if (_fileMetadataRepository.Update(fileToDelete) == null)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("bad request", "Unable to delete file, try later."));
            }

            System.IO.File.Delete(filePathDisk);


            return Ok(CustomResponseMessage.OkCustom<string>("delete ok", "File deleted from server."));


        }
    }

}
