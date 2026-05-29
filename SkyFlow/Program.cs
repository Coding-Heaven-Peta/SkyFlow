using SkyFlow.Data;
using SkyFlow.Helpers;
using SkyFlow.Models;
using SkyFlow.Services;

namespace SkyFlow
{
    // Application entry point. Handles initial database connections
    internal class Program
    {
        static void Main(string[] args)
        {
            //   Verify database connectivity 
            Console.Clear();
            ConsoleHelper.PrintBanner();
            Console.WriteLine("\n  Connecting to SkyFlowDB...");

            if (!DatabaseConnection.TestConnection())
            {
                ConsoleHelper.WriteError("Cannot connect to the database.");
                ConsoleHelper.WriteError("Ensure SQL Server LocalDB is running and SkyFlowDB exists.");
                ConsoleHelper.WriteError("Run the SkyFlow_Database.sql script to initialise the database.");
                ConsoleHelper.Pause();
                return;
            }

            ConsoleHelper.WriteSuccess("Database connection established.");
            System.Threading.Thread.Sleep(800);

            //   Authentication loop 
            bool sessionActive = true;
            while (sessionActive)
            {
                var authService = new AuthService();
                User? user = authService.Login();

                if (user == null)
                {
                    // Maximum login attempts exhausted
                    sessionActive = false;
                    break;
                }

                //  Role-based dispatch 
                if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminService = new AdminService(user);
                    adminService.Run();
                }
                else if (user.Role.Equals("GateAgent", StringComparison.OrdinalIgnoreCase))
                {
                    var agentService = new GateAgentService(user);
                    agentService.Run();
                }
                else
                {
                    ConsoleHelper.WriteError($"Unknown role '{user.Role}'. Contact your administrator.");
                    ConsoleHelper.Pause();
                }

                // After logout, prompt for re-login or exit
                Console.Clear();
                ConsoleHelper.PrintBanner();
                Console.WriteLine("\n  You have been logged out.\n");
                if (!ConsoleHelper.Confirm("Return to login screen?"))
                    sessionActive = false;
            }

            Console.Clear();
            ConsoleHelper.PrintBanner();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Thank you for using SkyFlow Terminal Manager.");
            Console.WriteLine("  Safe travels.\n");
            Console.ResetColor();
        }
    }
}
