

namespace Models
{
    public class ResponseModel<T>
    {
        public string Status { get; set; } = "error";

        public string? Error { get; set; }

        public string Message { get; set; }
        public T? Data { get; set; }

        public ResponseModel(string status, string message, T? data, string? error = "")
        {
            Error = error;
            Message = message;
            Status = status;
            Data = data;
        }
        public ResponseModel() { }
    }

}