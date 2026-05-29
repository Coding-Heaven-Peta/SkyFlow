namespace SkyFlow.Helpers
{
    // All console instances are in this file
    public static class ConsoleHelper
    {

        public static void WriteSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✔ " + msg);
            Console.ResetColor();
        }

        public static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✖ " + msg);
            Console.ResetColor();
        }

        public static void WriteWarning(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚠ " + msg);
            Console.ResetColor();
        }

        public static void WriteInfo(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ℹ " + msg);
            Console.ResetColor();
        }


        public static void PrintBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ╔════════════════════════════════════════════════════════════╗
  ║                                                            ║
  ║    ███████╗██╗  ██╗██╗   ██╗███████╗██╗     ██╗    ██╗   ║
  ║    ██╔════╝██║ ██╔╝╚██╗ ██╔╝██╔════╝██║     ██║    ██║   ║
  ║    ███████╗█████╔╝  ╚████╔╝ █████╗  ██║     ██║ █╗ ██║   ║
  ║    ╚════██║██╔═██╗   ╚██╔╝  ██╔══╝  ██║     ██║███╗██║   ║
  ║    ███████║██║  ██╗   ██║   ██║     ███████╗╚███╔███╔╝   ║
  ║    ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝     ╚══════╝ ╚══╝╚══╝   ║
  ║                                                            ║
  ║   ──────────────────────────────────────────────────────   ║
  ║                                                            ║
  ║            ✈   S K Y F L O W   T E R M I N A L           ║
  ║                  Airport Management System                 ║
  ║                                                            ║
  ║   ──────────────────────────────────────────────────────   ║
  ╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public static void PrintSectionHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ┌─────────────────────────────────────────────┐");
            Console.WriteLine($"  │  {title.PadRight(43)}│");
            Console.WriteLine("  └─────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        
        public static string ReadRequiredString(string prompt)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                string? value = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(value)) return value;
                WriteError("This field is required. Please try again.");
            }
        }

        public static string ReadOptionalString(string prompt)
        {
            Console.Write($"  {prompt}: ");
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                string? input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;
                WriteError($"Please enter a valid whole number between {min} and {max}.");
            }
        }

        public static DateTime ReadDateTime(string prompt)
        {
            while (true)
            {
                Console.Write($"  {prompt} (yyyy-MM-dd HH:mm): ");
                string? input = Console.ReadLine()?.Trim();
                if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime dt))
                    return dt;
                WriteError("Invalid date/time format. Example: 2025-09-15 14:30");
            }
        }

        // Reads a masked password without echoing characters.
        public static string ReadPassword(string prompt)
        {
            Console.Write($"  {prompt}: ");
            var pwd = new System.Text.StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
                {
                    pwd.Remove(pwd.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    pwd.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return pwd.ToString();
        }

        //Prompts for Y/N confirmation;
        public static bool Confirm(string prompt)
        {
            while (true)
            {
                Console.Write($"  {prompt} (Y/N): ");
                string? input = Console.ReadLine()?.Trim().ToUpper();
                if (input == "Y") return true;
                if (input == "N") return false;
                WriteError("Please enter Y or N.");
            }
        }

        public static void Pause()
        {
            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
