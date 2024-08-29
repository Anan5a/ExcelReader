namespace ExcelReader.Services
{
    public class ErrorConsole
    {
        public static void console(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {message}");
            Console.ResetColor();

        }
    }
}
