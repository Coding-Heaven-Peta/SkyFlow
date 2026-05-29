using Microsoft.Data.SqlClient;
using SkyFlow.Data;
using SkyFlow.Interfaces;
using SkyFlow.Models;

namespace SkyFlow.Repositories
{
    //  Passenger Repository

    public class PassengerRepository : IPassengerRepository
    {
        private static Passenger HydratePassenger(SqlDataReader r)
        {
            return new Passenger
            {
                PassengerId    = (int)r["PassengerId"],
                UserId         = r["UserId"]          == DBNull.Value ? null : (int?)r["UserId"],
                PassportNumber = r["PassportNumber"].ToString()  ?? string.Empty,
                DateOfBirth    = r["DateOfBirth"]     == DBNull.Value ? null : (DateTime?)r["DateOfBirth"],
                Nationality    = r["Nationality"]     == DBNull.Value ? string.Empty : r["Nationality"].ToString()!,
                ContactNumber  = r["ContactNumber"]   == DBNull.Value ? string.Empty : r["ContactNumber"].ToString()!,
            };
        }

        public IEnumerable<Passenger> GetAll()
        {
            var list = new List<Passenger>();
            const string sql = "SELECT * FROM Passenger ORDER BY PassportNumber";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            using var r    = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydratePassenger(r));
            return list;
        }

        public Passenger? GetById(int id)
        {
            const string sql = "SELECT * FROM Passenger WHERE PassengerId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? HydratePassenger(r) : null;
        }

        public Passenger? GetByPassport(string passportNumber)
        {
            const string sql = "SELECT * FROM Passenger WHERE PassportNumber = @Passport";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Passport", passportNumber);
            using var r = cmd.ExecuteReader();
            return r.Read() ? HydratePassenger(r) : null;
        }

        public bool Insert(Passenger p)
        {
            const string sql = @"
                INSERT INTO Passenger (UserId, PassportNumber, DateOfBirth, Nationality, ContactNumber)
                VALUES (@UserId, @Passport, @DOB, @Nationality, @Contact)";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId",      (object?)p.UserId         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Passport",    p.PassportNumber);
            cmd.Parameters.AddWithValue("@DOB",         (object?)p.DateOfBirth    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nationality", (object?)p.Nationality    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Contact",     (object?)p.ContactNumber  ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Passenger p)
        {
            const string sql = @"
                UPDATE Passenger
                SET Nationality=@Nationality, ContactNumber=@Contact
                WHERE PassengerId=@Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Nationality", (object?)p.Nationality   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Contact",     (object?)p.ContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Id",          p.PassengerId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Passenger WHERE PassengerId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    
    //  Booking Repository

    public class BookingRepository : IBookingRepository
    {
        private static Booking HydrateBooking(SqlDataReader r)
        {
            var b = new Booking
            {
                BookingId    = (int)r["BookingId"],
                FlightId     = (int)r["FlightId"],
                PassengerId  = (int)r["PassengerId"],
                SeatNumber   = r["SeatNumber"]  == DBNull.Value ? string.Empty : r["SeatNumber"].ToString()!,
                BookingDate  = r["BookingDate"] == DBNull.Value ? DateTime.Now  : (DateTime)r["BookingDate"],
                CheckInTime  = r["CheckInTime"] == DBNull.Value ? null           : (DateTime?)r["CheckInTime"],
                BoardingTime = r["BoardingTime"]== DBNull.Value ? null           : (DateTime?)r["BoardingTime"],
            };
            b.SetStatus(r["BookingStatus"] == DBNull.Value ? "Booked" : r["BookingStatus"].ToString()!);

            // Denormalised fields from JOIN queries
            if (HasColumn(r, "PassengerName"))
                b.PassengerName = r["PassengerName"] == DBNull.Value ? null : r["PassengerName"].ToString();
            if (HasColumn(r, "PassportNumber"))
                b.PassportNumber = r["PassportNumber"] == DBNull.Value ? null : r["PassportNumber"].ToString();
            if (HasColumn(r, "FlightNumber"))
                b.FlightNumber = r["FlightNumber"] == DBNull.Value ? null : r["FlightNumber"].ToString();

            return b;
        }

        private static bool HasColumn(SqlDataReader r, string name)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (r.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public IEnumerable<Booking> GetAll()
        {
            var list = new List<Booking>();
            const string sql = "SELECT * FROM Booking ORDER BY BookingDate DESC";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            using var r    = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateBooking(r));
            return list;
        }

        public Booking? GetById(int id)
        {
            const string sql = "SELECT * FROM Booking WHERE BookingId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? HydrateBooking(r) : null;
        }

        public IEnumerable<Booking> GetByFlight(int flightId)
        {
            var list = new List<Booking>();
            const string sql = @"
                SELECT b.*, p.PassportNumber,
                       ISNULL(u.FirstName + ' ' + u.LastName, 'Unknown Passenger') AS PassengerName,
                       f.FlightNumber
                FROM   Booking b
                JOIN   Passenger p ON b.PassengerId = p.PassengerId
                LEFT JOIN Users u  ON p.UserId = u.UserId
                JOIN   Flight f    ON b.FlightId = f.FlightId
                WHERE  b.FlightId = @FlightId
                ORDER  BY b.SeatNumber";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FlightId", flightId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateBooking(r));
            return list;
        }

        public IEnumerable<Booking> GetByPassenger(int passengerId)
        {
            var list = new List<Booking>();
            const string sql = @"
                SELECT b.*, p.PassportNumber, f.FlightNumber,
                       ISNULL(u.FirstName + ' ' + u.LastName, 'Unknown Passenger') AS PassengerName
                FROM   Booking b
                JOIN   Passenger p ON b.PassengerId = p.PassengerId
                LEFT JOIN Users u  ON p.UserId = u.UserId
                JOIN   Flight f    ON b.FlightId = f.FlightId
                WHERE  b.PassengerId = @PassengerId";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PassengerId", passengerId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateBooking(r));
            return list;
        }

        public Booking? FindPassengerOnFlight(int flightId, string passportOrId)
        {
            const string sql = @"
                SELECT b.*, p.PassportNumber,
                       ISNULL(u.FirstName + ' ' + u.LastName, 'Unknown Passenger') AS PassengerName,
                       f.FlightNumber
                FROM   Booking b
                JOIN   Passenger p ON b.PassengerId = p.PassengerId
                LEFT JOIN Users u  ON p.UserId = u.UserId
                JOIN   Flight f    ON b.FlightId = f.FlightId
                WHERE  b.FlightId = @FlightId
                  AND  (p.PassportNumber = @Search OR CAST(p.PassengerId AS NVARCHAR) = @Search)";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FlightId", flightId);
            cmd.Parameters.AddWithValue("@Search",   passportOrId);
            using var r = cmd.ExecuteReader();
            return r.Read() ? HydrateBooking(r) : null;
        }

        public bool Insert(Booking b)
        {
            const string sql = @"
                INSERT INTO Booking (FlightId, PassengerId, SeatNumber, BookingStatus, BookingDate)
                VALUES (@FlightId, @PassengerId, @Seat, @Status, @Date)";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FlightId",    b.FlightId);
            cmd.Parameters.AddWithValue("@PassengerId", b.PassengerId);
            cmd.Parameters.AddWithValue("@Seat",        (object?)b.SeatNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status",      b.BookingStatus);
            cmd.Parameters.AddWithValue("@Date",        b.BookingDate);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Booking b)
        {
            const string sql = @"
                UPDATE Booking
                SET BookingStatus=@Status, CheckInTime=@CheckIn, BoardingTime=@Boarding
                WHERE BookingId=@Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status",   b.BookingStatus);
            cmd.Parameters.AddWithValue("@CheckIn",  (object?)b.CheckInTime  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Boarding", (object?)b.BoardingTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Id",       b.BookingId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateStatus(int bookingId, string newStatus, DateTime? timeStamp = null)
        {
            string sql = newStatus == "CheckedIn"
                ? "UPDATE Booking SET BookingStatus=@Status, CheckInTime=@Stamp  WHERE BookingId=@Id"
                : newStatus == "Boarded"
                    ? "UPDATE Booking SET BookingStatus=@Status, BoardingTime=@Stamp WHERE BookingId=@Id"
                    : "UPDATE Booking SET BookingStatus=@Status WHERE BookingId=@Id";

            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", newStatus);
            if (newStatus is "CheckedIn" or "Boarded")
                cmd.Parameters.AddWithValue("@Stamp", (object?)timeStamp ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", bookingId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Booking WHERE BookingId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
