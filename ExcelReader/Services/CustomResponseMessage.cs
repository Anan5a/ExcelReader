using ExcelReader.Models;
using Microsoft.OpenApi.Any;

namespace ExcelReader.Services
{
    public class CustomResponseMessage
    {
        public static object NotFound()
        {
            return new ResponseModel<string?>
            {
                Status = "error",
                Error = "Not Found",
                Message = "The resource was not found"
            };
        }

        public static object Unauthorized()
        {
            return new ResponseModel<string?>
            {
                Status = "error",
                Error = "Unauthorized",
                Message = "You are not authorized to access this resource"
            };
        }

        public static object InternalServerError()
        {
            return new ResponseModel<string?>
            {
                Status = "error",
                Error = "Internal Server Error",
                Message = "An unexpected error occurred while processing the request"
            };
        }
        public static object ErrorCustom(string error, string message)
        {
            return new ResponseModel<string?>
            {
                Status = "error",
                Error = $"{error}",
                Message = $"{message}"
            };
        }

        public static object OkCustom<T>(string message, T data)
        {
            return new ResponseModel<T>
            {
                Status = "ok",
                Message = $"{message}",
                Data = data
            };
        }

    }
}
