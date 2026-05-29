using SkyFlow.Helpers;
using SkyFlow.Interfaces;
using SkyFlow.Models;
using SkyFlow.Repositories;

namespace SkyFlow.Services
{
    
    public class AdminService
    {
        private readonly IFlightRepository  _flightRepo;
        private readonly IUserRepository    _userRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly User               _currentUser;

        public AdminService(User currentUser)
        {
            _currentUser = currentUser;
            _flightRepo  = new FlightRepository();
            _userRepo    = new UserRepository();
            _bookingRepo = new BookingRepository();
        }

        //  Entry point — main menu loop
        

        public void Run()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                ConsoleHelper.WriteInfo($"Logged in as: {_currentUser.FullName} [{_currentUser.Role}]");
                _currentUser.DisplayDashboard();   // Polymorphic call

                string choice = Console.ReadLine()?.Trim() ?? "0";
                switch (choice)
                {
                    case "1": ManageFlights();    break;
                    case "2": SystemOverview();   break;
                    case "3": ManageStaff();      break;
                    case "4": ViewAuditLog();     break;
                    case "0": running = false;    break;
                    default:
                        ConsoleHelper.WriteError("Invalid selection. Please try again.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        //  1. Manage Flights

        private void ManageFlights()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("MANAGE FLIGHTS");
                Console.WriteLine("  1. View All Flights");
                Console.WriteLine("  2. Add New Flight");
                Console.WriteLine("  3. Update Flight Status");
                Console.WriteLine("  4. Delete Flight");
                Console.WriteLine("  0. Back");
                Console.Write("\n  Select option: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": ViewAllFlights();   break;
                    case "2": AddFlight();         break;
                    case "3": UpdateFlightStatus(); break;
                    case "4": DeleteFlight();      break;
                    case "0": back = true;         break;
                    default:
                        ConsoleHelper.WriteError("Invalid option.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private void ViewAllFlights()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("ALL FLIGHTS");
            var flights = _flightRepo.GetAll();
            TableRenderer.RenderFlights(flights);
            ConsoleHelper.Pause();
        }

        private void AddFlight()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("ADD NEW FLIGHT");

            string flightNumber = ConsoleHelper.ReadRequiredString("Flight Number (e.g. SF505)").ToUpper();
            string origin       = ConsoleHelper.ReadRequiredString("Origin Airport Code (e.g. JNB)").ToUpper();
            string destination  = ConsoleHelper.ReadRequiredString("Destination Airport Code (e.g. CPT)").ToUpper();
            DateTime departure  = ConsoleHelper.ReadDateTime("Departure Time");
            DateTime arrival    = ConsoleHelper.ReadDateTime("Arrival Time");
            int capacity        = ConsoleHelper.ReadInt("Aircraft Capacity (seats)", 1, 853);

            // Assign gate agent
            Console.WriteLine();
            var agents = _userRepo.GetByRole("GateAgent");
            ConsoleHelper.WriteInfo("Available Gate Agents:");
            TableRenderer.RenderUsers(agents);
            int agentId = ConsoleHelper.ReadInt("Assign Gate Agent ID (0 for none)", 0);

            var flight = new Flight
            {
                FlightNumber  = flightNumber,
                Origin        = origin,
                Destination   = destination,
                DepartureTime = departure,
                ArrivalTime   = arrival,
                Capacity      = capacity,
                GateAgentId   = agentId == 0 ? null : agentId,
                CurrentOccupancy = 0
            };
            flight.SetStatus("Scheduled");

            if (_flightRepo.Insert(flight))
                ConsoleHelper.WriteSuccess($"Flight {flightNumber} added successfully.");
            else
                ConsoleHelper.WriteError("Failed to add flight. Verify data and try again.");

            ConsoleHelper.Pause();
        }

        private void UpdateFlightStatus()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("UPDATE FLIGHT STATUS");

            var flights = _flightRepo.GetAll();
            TableRenderer.RenderFlights(flights);

            int flightId = ConsoleHelper.ReadInt("Enter Flight ID to update");
            var flight   = _flightRepo.GetById(flightId);

            if (flight == null)
            {
                ConsoleHelper.WriteError("Flight not found.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  Current status: {flight.Status}");
            Console.WriteLine("  Available statuses: Scheduled | Boarding | Departed | Cancelled | Delayed");
            string newStatus = ConsoleHelper.ReadRequiredString("Enter new status");

            // Encapsulation: use the method rather than setting Status directly
            try
            {
                if (newStatus.Equals("Boarding",  StringComparison.OrdinalIgnoreCase)) flight.BoardFlight();
                else if (newStatus.Equals("Departed", StringComparison.OrdinalIgnoreCase)) flight.DepartFlight();
                else flight.SetStatus(newStatus);
            }
            catch (InvalidOperationException ex)
            {
                ConsoleHelper.WriteError(ex.Message);
                ConsoleHelper.Pause();
                return;
            }

            if (_flightRepo.UpdateStatus(flightId, flight.Status))
                ConsoleHelper.WriteSuccess($"Flight {flight.FlightNumber} status updated to '{flight.Status}'.");
            else
                ConsoleHelper.WriteError("Update failed.");

            ConsoleHelper.Pause();
        }

        private void DeleteFlight()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("DELETE FLIGHT");

            var flights = _flightRepo.GetAll();
            TableRenderer.RenderFlights(flights);

            int flightId = ConsoleHelper.ReadInt("Enter Flight ID to delete");
            var flight   = _flightRepo.GetById(flightId);

            if (flight == null)
            {
                ConsoleHelper.WriteError("Flight not found.");
                ConsoleHelper.Pause();
                return;
            }

            ConsoleHelper.WriteWarning($"This will permanently delete flight {flight.FlightNumber}.");
            if (!ConsoleHelper.Confirm("Are you sure?"))
            {
                ConsoleHelper.WriteInfo("Deletion cancelled.");
                ConsoleHelper.Pause();
                return;
            }

            if (_flightRepo.Delete(flightId))
                ConsoleHelper.WriteSuccess("Flight deleted successfully.");
            else
                ConsoleHelper.WriteError("Deletion failed. Ensure no bookings are linked to this flight.");

            ConsoleHelper.Pause();
        }

        //  2. System Overview

        private void SystemOverview()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("SYSTEM OVERVIEW — ALL FLIGHTS & OCCUPANCY");

            var flights = _flightRepo.GetAll().ToList();
            TableRenderer.RenderFlights(flights);

            // Summary statistics
            int total    = flights.Count;
            int sched    = flights.Count(f => f.Status == "Scheduled");
            int boarding = flights.Count(f => f.Status == "Boarding");
            int departed = flights.Count(f => f.Status == "Departed");

            Console.WriteLine($"  Summary: {total} flights total — " +
                              $"{sched} Scheduled | {boarding} Boarding | {departed} Departed");

            ConsoleHelper.Pause();
        }

        //  3. Manage Staff

        private void ManageStaff()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("MANAGE STAFF");
                Console.WriteLine("  1. View All Staff");
                Console.WriteLine("  2. Add New Staff Member");
                Console.WriteLine("  3. Remove Staff Member");
                Console.WriteLine("  0. Back");
                Console.Write("\n  Select option: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": ViewAllStaff(); break;
                    case "2": AddStaff();     break;
                    case "3": RemoveStaff();  break;
                    case "0": back = true;    break;
                    default:
                        ConsoleHelper.WriteError("Invalid option.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private void ViewAllStaff()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("ALL STAFF MEMBERS");
            var users = _userRepo.GetAll();
            TableRenderer.RenderUsers(users);
            ConsoleHelper.Pause();
        }

        private void AddStaff()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("ADD NEW STAFF MEMBER");

            string username = ConsoleHelper.ReadRequiredString("Username");

            if (_userRepo.UsernameExists(username))
            {
                ConsoleHelper.WriteError("Username already exists. Please choose a different one.");
                ConsoleHelper.Pause();
                return;
            }

            string password  = ConsoleHelper.ReadPassword("Password");
            string firstName = ConsoleHelper.ReadRequiredString("First Name");
            string lastName  = ConsoleHelper.ReadRequiredString("Last Name");
            string email     = ConsoleHelper.ReadOptionalString("Email Address");

            Console.WriteLine("  Role options: Admin | GateAgent");
            string role = ConsoleHelper.ReadRequiredString("Role");
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("GateAgent", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.WriteError("Invalid role. Must be 'Admin' or 'GateAgent'.");
                ConsoleHelper.Pause();
                return;
            }

            User newUser = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? new Admin() : new GateAgent();
            newUser.Username  = username;
            newUser.Password  = password;
            newUser.FirstName = firstName;
            newUser.LastName  = lastName;
            newUser.Email     = email;
            newUser.Role      = role;

            if (_userRepo.Insert(newUser))
                ConsoleHelper.WriteSuccess($"Staff member '{username}' added successfully.");
            else
                ConsoleHelper.WriteError("Failed to add staff member.");

            ConsoleHelper.Pause();
        }

        private void RemoveStaff()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("REMOVE STAFF MEMBER");

            var users = _userRepo.GetAll();
            TableRenderer.RenderUsers(users);

            int userId = ConsoleHelper.ReadInt("Enter User ID to remove");

            if (userId == _currentUser.UserId)
            {
                ConsoleHelper.WriteError("You cannot remove your own account.");
                ConsoleHelper.Pause();
                return;
            }

            var user = _userRepo.GetById(userId);
            if (user == null)
            {
                ConsoleHelper.WriteError("User not found.");
                ConsoleHelper.Pause();
                return;
            }

            ConsoleHelper.WriteWarning($"This will remove '{user.Username}' ({user.FullName}).");
            if (!ConsoleHelper.Confirm("Confirm removal?"))
            {
                ConsoleHelper.WriteInfo("Removal cancelled.");
                ConsoleHelper.Pause();
                return;
            }

            if (_userRepo.Delete(userId))
                ConsoleHelper.WriteSuccess("Staff member removed successfully.");
            else
                ConsoleHelper.WriteError("Removal failed. The user may have linked records.");

            ConsoleHelper.Pause();
        }

        //  4.  Basic flight log view
        

        private void ViewAuditLog()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("AUDIT LOG — RECENT FLIGHT ACTIVITY");
            ConsoleHelper.WriteInfo("Fetching recent flight log entries...");

            try
            {
                using var conn = Data.DatabaseConnection.GetConnection();
                const string sql = @"
                    SELECT TOP 20
                        fl.LogId,
                        f.FlightNumber,
                        fl.Action,
                        u.Username AS PerformedBy,
                        fl.PerformedAt,
                        fl.Details
                    FROM FlightLog fl
                    JOIN Flight f ON fl.FlightId = f.FlightId
                    JOIN Users  u ON fl.PerformedBy = u.UserId
                    ORDER BY fl.PerformedAt DESC";

                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
                using var r   = cmd.ExecuteReader();

                var headers = new[] { "Log ID", "Flight", "Action", "Performed By", "At", "Details" };
                var rows    = new List<string[]>();

                while (r.Read())
                    rows.Add(new[]
                    {
                        r["LogId"].ToString()!,
                        r["FlightNumber"].ToString()!,
                        r["Action"].ToString()!,
                        r["PerformedBy"].ToString()!,
                        ((DateTime)r["PerformedAt"]).ToString("yyyy-MM-dd HH:mm"),
                        r["Details"].ToString()!
                    });

                TableRenderer.Render(headers, rows);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Could not load audit log: {ex.Message}");
            }

            ConsoleHelper.Pause();
        }
    }
}
