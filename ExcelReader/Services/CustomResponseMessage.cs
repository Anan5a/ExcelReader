namespace ExcelReader.Services
{
    public class CustomResponseMessage
    {
        public static object NotFound()
        {
            return new
            {
                status = "error",
                error = "Not Found",
                message = "The resource was not found"
            };
        }

        public static object Unauthorized()
        {
            return new
            {
                status = "error",
                error = "Unauthorized",
                message = "You are not authorized to access this resource"
            };
        }

        public static object InternalServerError()
        {
            return new
            {
                status = "error",
                error = "Internal Server Error",
                message = "An unexpected error occurred while processing the request"
            };
        }
        public static object ErrorCustom(string error, string message)
        {
            return new
            {
                status = "error",
                error = $"{error}",
                message = $"{message}"
            };
        }

        public static object OkCustom(string message)
        {
            return new
            {
                status="ok",
                message = $"{message}"
            };
        }

    }
}
