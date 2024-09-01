namespace ExcelReader.Services
{
    public class ErrorConsole
    {
        public static void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now.ToString()}: Error: {message}");
            Console.ResetColor();

        }
    }
}
