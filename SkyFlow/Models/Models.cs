namespace SkyFlow.Models
{
    // Inheritance  and polymorphism are used to model the different user roles 
    

   
    public abstract class User
    {
        // Encapsulation: backing fields are private; public properties expose them.
        private int    _userId;
        private string _username  = string.Empty;
        private string _password  = string.Empty;
        private string _role      = string.Empty;
        private string _email     = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName  = string.Empty;

        public int    UserId    { get => _userId;    set => _userId    = value; }
        public string Username  { get => _username;  set => _username  = value ?? string.Empty; }
        public string Password  { get => _password;  set => _password  = value ?? string.Empty; }
        public string Role      { get => _role;      set => _role      = value ?? string.Empty; }
        public string Email     { get => _email;     set => _email     = value ?? string.Empty; }
        public string FirstName { get => _firstName; set => _firstName = value ?? string.Empty; }
        public string LastName  { get => _lastName;  set => _lastName  = value ?? string.Empty; }
        public string FullName  => $"{_firstName} {_lastName}".Trim();

        
        public abstract void DisplayDashboard();
    }

    
    public class Admin : User
    {
        public override void DisplayDashboard()
        {
            Console.WriteLine("\n  ╔══════════════════════════════════════╗");
            Console.WriteLine("  ║       ADMIN DASHBOARD — SKYFLOW      ║");
            Console.WriteLine("  ╠══════════════════════════════════════╣");
            Console.WriteLine("  ║  1. Manage Flights                   ║");
            Console.WriteLine("  ║  2. View System Overview             ║");
            Console.WriteLine("  ║  3. Manage Staff                     ║");
            Console.WriteLine("  ║  4. View Audit Log                   ║");
            Console.WriteLine("  ║  0. Logout                           ║");
            Console.WriteLine("  ╚══════════════════════════════════════╝");
            Console.Write("\n  Select option: ");
        }
    }

    
    public class GateAgent : User
    {
        public override void DisplayDashboard()
        {
            Console.WriteLine("\n  ╔══════════════════════════════════════╗");
            Console.WriteLine("  ║     GATE AGENT DASHBOARD — SKYFLOW   ║");
            Console.WriteLine("  ╠══════════════════════════════════════╣");
            Console.WriteLine("  ║  1. View Flight Manifest             ║");
            Console.WriteLine("  ║  2. Passenger Check-In               ║");
            Console.WriteLine("  ║  3. Update Flight Status             ║");
            Console.WriteLine("  ║  4. View My Assigned Flights         ║");
            Console.WriteLine("  ║  0. Logout                           ║");
            Console.WriteLine("  ╚══════════════════════════════════════╝");
            Console.Write("\n  Select option: ");
        }
    }

    

    public class Flight
    {
        private string _flightNumber = string.Empty;
        private string _status       = "Scheduled";

        public int      FlightId          { get; set; }
        public string   FlightNumber      { get => _flightNumber; set => _flightNumber = value ?? string.Empty; }
        public string   Origin            { get; set; } = string.Empty;
        public string   Destination       { get; set; } = string.Empty;
        public DateTime DepartureTime     { get; set; }
        public DateTime ArrivalTime       { get; set; }
        public int      Capacity          { get; set; }
        public int      CurrentOccupancy  { get; set; }
        public int?     GateAgentId       { get; set; }

        
        public string Status { get => _status; private set => _status = value; }

        public bool IsFull => CurrentOccupancy >= Capacity;


        public void BoardFlight()
        {
            if (_status == "Departed")
                throw new InvalidOperationException("Flight has already departed.");
            _status = "Boarding";
        }

        public void DepartFlight()
        {
            if (_status == "Departed")
                throw new InvalidOperationException("Flight has already departed.");
            if (_status != "Boarding")
                throw new InvalidOperationException("Flight must be in Boarding status before departure.");
            _status = "Departed";
        }

        public void SetStatus(string status) => _status = status;   // Used only by repository hydration
    }

    //  Passenger & Booking

    public class Passenger
    {
        public int     PassengerId    { get; set; }
        public int?    UserId         { get; set; }
        public string  PassportNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth  { get; set; }
        public string  Nationality    { get; set; } = string.Empty;
        public string  ContactNumber  { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName  { get; set; }
        public string  FullName  => $"{FirstName} {LastName}".Trim();
    }

    public class Booking
    {
        private string _bookingStatus = "Booked";

        public int      BookingId     { get; set; }
        public int      FlightId      { get; set; }
        public int      PassengerId   { get; set; }
        public string   SeatNumber    { get; set; } = string.Empty;
        public DateTime BookingDate   { get; set; }
        public DateTime? CheckInTime  { get; set; }
        public DateTime? BoardingTime { get; set; }

        public string BookingStatus
        {
            get => _bookingStatus;
            private set => _bookingStatus = value;
        }

        // Denormalised display fields
        public string? PassengerName    { get; set; }
        public string? PassportNumber   { get; set; }
        public string? FlightNumber     { get; set; }

        public void CheckIn()
        {
            if (_bookingStatus == "CheckedIn" || _bookingStatus == "Boarded")
                throw new InvalidOperationException("Passenger is already checked in or boarded.");
            if (_bookingStatus == "Cancelled")
                throw new InvalidOperationException("Booking has been cancelled.");
            _bookingStatus = "CheckedIn";
            CheckInTime    = DateTime.Now;
        }

        public void Board()
        {
            if (_bookingStatus != "CheckedIn")
                throw new InvalidOperationException("Passenger must be checked in before boarding.");
            _bookingStatus = "Boarded";
            BoardingTime   = DateTime.Now;
        }

        public void SetStatus(string status) => _bookingStatus = status;  // Repository use only
    }

    //  Airport / Aircraft (supporting entities)

    public class Airport
    {
        public int    AirportId   { get; set; }
        public string AirportCode { get; set; } = string.Empty;
        public string AirportName { get; set; } = string.Empty;
        public string City        { get; set; } = string.Empty;
        public string Country     { get; set; } = string.Empty;
    }

    public class Aircraft
    {
        public int    AircraftId          { get; set; }
        public string AircraftType        { get; set; } = string.Empty;
        public string RegistrationNumber  { get; set; } = string.Empty;
        public string Manufacturer        { get; set; } = string.Empty;
        public int    Capacity            { get; set; }
        public string Status              { get; set; } = "Active";
    }
}
