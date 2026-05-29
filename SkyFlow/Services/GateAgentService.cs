using SkyFlow.Helpers;
using SkyFlow.Interfaces;
using SkyFlow.Models;
using SkyFlow.Repositories;

namespace SkyFlow.Services
{
    //Encapsulates all gate agent functionalities: viewing manifests, checking in passengers, updating flight statuses, and viewing assigned flights.
    public class GateAgentService
    {
        private readonly IFlightRepository  _flightRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly User               _currentUser;

        public GateAgentService(User currentUser)
        {
            _currentUser = currentUser;
            _flightRepo  = new FlightRepository();
            _bookingRepo = new BookingRepository();
        }

        //  Entry point

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
                    case "1": ViewFlightManifest();     break;
                    case "2": PassengerCheckIn();       break;
                    case "3": UpdateFlightStatus();     break;
                    case "4": ViewMyAssignedFlights();  break;
                    case "0": running = false;          break;
                    default:
                        ConsoleHelper.WriteError("Invalid selection. Please try again.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        //  1. View Flight Manifest

        private void ViewFlightManifest()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("FLIGHT MANIFEST");

            var flights = _flightRepo.GetAll().ToList();
            if (!flights.Any())
            {
                ConsoleHelper.WriteWarning("No flights available.");
                ConsoleHelper.Pause();
                return;
            }

            TableRenderer.RenderFlights(flights);
            int flightId = ConsoleHelper.ReadInt("Enter Flight ID to view manifest");

            var flight = _flightRepo.GetById(flightId);
            if (flight == null)
            {
                ConsoleHelper.WriteError("Flight not found.");
                ConsoleHelper.Pause();
                return;
            }

            Console.Clear();
            ConsoleHelper.PrintSectionHeader($"MANIFEST — {flight.FlightNumber}: {flight.Origin} → {flight.Destination}");
            ConsoleHelper.WriteInfo($"Departure: {flight.DepartureTime:yyyy-MM-dd HH:mm}  |  Status: {flight.Status}  |  Occupancy: {flight.CurrentOccupancy}/{flight.Capacity}");

            var bookings = _bookingRepo.GetByFlight(flightId);
            TableRenderer.RenderBookings(bookings);
            ConsoleHelper.Pause();
        }

        //  2. Passenger Check-In

        private void PassengerCheckIn()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("PASSENGER CHECK-IN");

            // Select flight
            var flights = _flightRepo.GetAll()
                .Where(f => f.Status is "Scheduled" or "Boarding")
                .ToList();

            if (!flights.Any())
            {
                ConsoleHelper.WriteWarning("No active flights available for check-in.");
                ConsoleHelper.Pause();
                return;
            }

            TableRenderer.RenderFlights(flights);
            int flightId = ConsoleHelper.ReadInt("Select Flight ID");
            var flight   = _flightRepo.GetById(flightId);

            if (flight == null)
            {
                ConsoleHelper.WriteError("Flight not found.");
                ConsoleHelper.Pause();
                return;
            }

            // Business rule: cannot check in if flight has departed
            if (flight.Status == "Departed")
            {
                ConsoleHelper.WriteError("This flight has already departed. Check-in is closed.");
                ConsoleHelper.Pause();
                return;
            }

            // Business rule: cannot check in if flight is full
            if (flight.IsFull)
            {
                ConsoleHelper.WriteError($"Flight {flight.FlightNumber} is at full capacity ({flight.Capacity} seats). No further check-ins permitted.");
                ConsoleHelper.Pause();
                return;
            }

            string search = ConsoleHelper.ReadRequiredString("Enter Passenger ID or Passport Number");
            var booking   = _bookingRepo.FindPassengerOnFlight(flightId, search);

            if (booking == null)
            {
                ConsoleHelper.WriteError("Passenger not found on this flight. Verify the ID or passport number.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  Passenger found: {booking.PassengerName} (Seat {booking.SeatNumber})");
            Console.WriteLine($"  Passport:        {booking.PassportNumber}");
            Console.WriteLine($"  Current status:  {booking.BookingStatus}");

            if (booking.BookingStatus == "CheckedIn")
            {
                ConsoleHelper.WriteWarning("Passenger is already checked in.");
                ConsoleHelper.Pause();
                return;
            }

            if (booking.BookingStatus == "Boarded")
            {
                ConsoleHelper.WriteWarning("Passenger has already boarded.");
                ConsoleHelper.Pause();
                return;
            }

            if (!ConsoleHelper.Confirm("Update status to CheckedIn?"))
            {
                ConsoleHelper.WriteInfo("Check-in cancelled.");
                ConsoleHelper.Pause();
                return;
            }

            try
            {
                booking.CheckIn();   // Encapsulated state transition
                if (_bookingRepo.UpdateStatus(booking.BookingId, booking.BookingStatus, booking.CheckInTime))
                    ConsoleHelper.WriteSuccess($"✔ Status updated to CheckedIn for {booking.PassengerName}.");
                else
                    ConsoleHelper.WriteError("Database update failed.");
            }
            catch (InvalidOperationException ex)
            {
                ConsoleHelper.WriteError(ex.Message);
            }

            ConsoleHelper.Pause();
        }

        //  3. Update Flight Status 

        private void UpdateFlightStatus()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("UPDATE FLIGHT STATUS");

            var myFlights = _flightRepo.GetByGateAgent(_currentUser.UserId).ToList();
            if (!myFlights.Any())
            {
                ConsoleHelper.WriteWarning("You have no flights assigned to you.");
                ConsoleHelper.Pause();
                return;
            }

            TableRenderer.RenderFlights(myFlights);
            int flightId = ConsoleHelper.ReadInt("Enter Flight ID to update");
            var flight   = _flightRepo.GetById(flightId);

            if (flight == null || flight.GateAgentId != _currentUser.UserId)
            {
                ConsoleHelper.WriteError("Flight not found or you are not assigned to it.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine($"\n  Flight:         {flight.FlightNumber}");
            Console.WriteLine($"  Current Status: {flight.Status}");
            Console.WriteLine("  New status options: Boarding | Departed");
            string newStatus = ConsoleHelper.ReadRequiredString("Enter new status");

            // Business rule validation
            if (newStatus.Equals("Departed", StringComparison.OrdinalIgnoreCase) && flight.Status != "Boarding")
            {
                ConsoleHelper.WriteError("Flight must be in 'Boarding' status before it can depart.");
                ConsoleHelper.Pause();
                return;
            }

            try
            {
                if (newStatus.Equals("Boarding",  StringComparison.OrdinalIgnoreCase)) flight.BoardFlight();
                else if (newStatus.Equals("Departed", StringComparison.OrdinalIgnoreCase)) flight.DepartFlight();
                else { ConsoleHelper.WriteError("Invalid status."); ConsoleHelper.Pause(); return; }
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

        //  4. View My Assigned Flights

        private void ViewMyAssignedFlights()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader($"ASSIGNED FLIGHTS — {_currentUser.FullName}");
            var flights = _flightRepo.GetByGateAgent(_currentUser.UserId);
            TableRenderer.RenderFlights(flights);
            ConsoleHelper.Pause();
        }
    }
}
