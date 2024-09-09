﻿using IRepository;
using Microsoft.AspNetCore.Hosting;
using Models;
using Models.DTOs;
using Services;
using Utility;

namespace BLL
{
    public class FilesBLL
    {
        private static readonly string[] AllowedExtensions = { "any", ".xls", ".xlsx" };
        private static long MAX_UPLOAD_SIZE = 2_000_000; //bytes

        public static IEnumerable<FileMetadata> ListFiles(IFileMetadataRepository _fileMetadataRepository, long userId, string? page)
        {
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["user_id"] = userId;
            condition["deleted_at"] = null;
            var list = _fileMetadataRepository.GetAll(condition);
            return list;

        }

        public static BLLReturnEnum Upload(IFileMetadataRepository _fileMetadataRepository, IWebHostEnvironment _webHostEnvironment, long userId, UploadDTO uploadDto)
        {


            if (uploadDto.ExcelFile == null)
            {
                return BLLReturnEnum.File_FILE_IS_EMPTY;
            }
            //check file type

            var fileExtension = Path.GetExtension(uploadDto.ExcelFile.FileName).ToLower();

            if (!AllowedExtensions.First().Equals("any") && !AllowedExtensions.Contains(fileExtension))
            {
                return BLLReturnEnum.File_FILE_TYPE_NOT_ALLOWED;
            }


            if (uploadDto.ExcelFile.Length > MAX_UPLOAD_SIZE)
            {
                return BLLReturnEnum.File_FILE_SIZE_TOO_LARGE;
            }

            var fileNameUUID = Guid.NewGuid().ToString();
            var fileNameOriginal = uploadDto.FileName ?? Path.GetFileNameWithoutExtension(uploadDto.ExcelFile.FileName);
            string fileNameSystem = fileNameUUID + fileExtension;

            string baseFileDirectory = _webHostEnvironment.WebRootPath;
            string fileDirectory = Path.Combine(baseFileDirectory, "uploads");
            var filePathDisk = Path.Combine(fileDirectory, fileNameSystem);


            //save file
            using (var stream = new FileStream(filePathDisk, FileMode.Create))
            {
                uploadDto.ExcelFile.CopyToAsync(stream).Wait();
            }
            //add to database

            FileMetadata fileMetadata = new FileMetadata
            {
                CreatedAt = DateTime.Now,
                FileName = fileNameOriginal + fileExtension,
                FileNameSystem = fileNameSystem,
                UserId = userId,
                FilesizeBytes = uploadDto.ExcelFile.Length,
            };

            var fileId = _fileMetadataRepository.Add(fileMetadata);

            if (fileId != 0)
            {
                return BLLReturnEnum.ACTION_OK;
            }
            //delete file, since unable to add to db
            try
            {
                System.IO.File.Delete(filePathDisk);
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }

            return BLLReturnEnum.ACTION_ERROR;
        }


        public static BLLReturnEnum Update(IFileMetadataRepository _fileMetadataRepository, long userId, EditFileDTO editFileDto)
        {

            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["user_id"] = userId;
            condition["id"] = editFileDto.fileId;

            var existingFile = _fileMetadataRepository.Get(condition);

            if (existingFile == null)
            {
                return BLLReturnEnum.File_FILE_NOT_FOUND;

            }

            var fileExtension = Path.GetExtension(existingFile.FileNameSystem).ToLower();
            var fileNameUser = Path.GetFileNameWithoutExtension(editFileDto.FileName);
            existingFile.FileName = fileNameUser + fileExtension;
            existingFile.UpdatedAt = DateTime.Now;


            if (_fileMetadataRepository.Update(existingFile) != null)
            {
                return BLLReturnEnum.ACTION_OK;
            }


            return BLLReturnEnum.ACTION_ERROR;
        }

    }
}