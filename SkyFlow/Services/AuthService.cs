using SkyFlow.Helpers;
using SkyFlow.Interfaces;
using SkyFlow.Models;
using SkyFlow.Repositories;

namespace SkyFlow.Services
{
    //Handles User Authentication and related operations.
    public class AuthService
    {
        private readonly IUserRepository _userRepo;

        public AuthService()
        {
            _userRepo = new UserRepository();
        }

       // Login screen and authentication logic with 3 attempts given.
        public User? Login()
        {
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Console.Clear();
                ConsoleHelper.PrintBanner();
                Console.WriteLine("\n  SkyFlow > Welcome to SkyFlow Terminal Manager");
                Console.WriteLine($"  SkyFlow > Please enter your credentials (Attempt {attempt}/{maxAttempts})\n");

                string username = ConsoleHelper.ReadRequiredString("Username");
                string password = ConsoleHelper.ReadPassword("Password");

                User? user = _userRepo.Authenticate(username, password);

                if (user != null)
                {
                    Console.WriteLine();
                    ConsoleHelper.WriteSuccess($"Authentication successful. Role: {user.Role}");
                    System.Threading.Thread.Sleep(1200);
                    return user;
                }

                ConsoleHelper.WriteError("Invalid username or password.");
                if (attempt < maxAttempts)
                {
                    ConsoleHelper.WriteWarning($"{maxAttempts - attempt} attempt(s) remaining.");
                    System.Threading.Thread.Sleep(800);
                }
            }

            ConsoleHelper.WriteError("Maximum login attempts exceeded. The system will now exit.");
            System.Threading.Thread.Sleep(2000);
            return null;
        }
    }
}
