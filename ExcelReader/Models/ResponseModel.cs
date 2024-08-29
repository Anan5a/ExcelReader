namespace ExcelReader.Models
{
    public class ResponseModel
    {
        public string Status { get; set; } = "error";

        public string? Error { get; set; }

        public string Message { get; set; }

        public ResponseModel(string status = "",  string message = "", string? error)
        {
            Error = error;
            Message = message;
            Status = status;
        }
    }

}
