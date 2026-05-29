using Microsoft.Data.SqlClient;
using SkyFlow.Data;
using SkyFlow.Interfaces;
using SkyFlow.Models;

namespace SkyFlow.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private static Flight HydrateFlight(SqlDataReader r)
        {
            var f = new Flight
            {
                FlightId         = (int)r["FlightId"],
                FlightNumber     = r["FlightNumber"].ToString()  ?? string.Empty,
                Origin           = r["Origin"].ToString()        ?? string.Empty,
                Destination      = r["Destination"].ToString()   ?? string.Empty,
                DepartureTime    = (DateTime)r["DepartureTime"],
                ArrivalTime      = r["ArrivalTime"] == DBNull.Value ? DateTime.MinValue : (DateTime)r["ArrivalTime"],
                Capacity         = (int)r["Capacity"],
                CurrentOccupancy = (int)r["CurrentOccupancy"],
                GateAgentId      = r["GateAgentId"] == DBNull.Value ? null : (int?)r["GateAgentId"],
            };
            f.SetStatus(r["Status"].ToString() ?? "Scheduled");
            return f;
        }

        public IEnumerable<Flight> GetAll()
        {
            var list = new List<Flight>();
            const string sql = "SELECT * FROM Flight ORDER BY DepartureTime";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            using var r    = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateFlight(r));
            return list;
        }

        public Flight? GetById(int id)
        {
            const string sql = "SELECT * FROM Flight WHERE FlightId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? HydrateFlight(r) : null;
        }

        public bool Insert(Flight f)
        {
            const string sql = @"
                INSERT INTO Flight
                    (FlightNumber, Origin, Destination, DepartureTime, ArrivalTime, Capacity, Status, CurrentOccupancy, GateAgentId)
                VALUES
                    (@FlightNumber, @Origin, @Destination, @DepartureTime, @ArrivalTime, @Capacity, @Status, @CurrentOccupancy, @GateAgentId)";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FlightNumber",    f.FlightNumber);
            cmd.Parameters.AddWithValue("@Origin",          f.Origin);
            cmd.Parameters.AddWithValue("@Destination",     f.Destination);
            cmd.Parameters.AddWithValue("@DepartureTime",   f.DepartureTime);
            cmd.Parameters.AddWithValue("@ArrivalTime", f.ArrivalTime == DateTime.MinValue ? DBNull.Value : f.ArrivalTime);
            cmd.Parameters.AddWithValue("@Capacity",        f.Capacity);
            cmd.Parameters.AddWithValue("@Status",          f.Status);
            cmd.Parameters.AddWithValue("@CurrentOccupancy",f.CurrentOccupancy);
            cmd.Parameters.AddWithValue("@GateAgentId",     (object?)f.GateAgentId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Flight f)
        {
            const string sql = @"
                UPDATE Flight
                SET FlightNumber=@FlightNumber, Origin=@Origin, Destination=@Destination,
                    DepartureTime=@DepartureTime, Capacity=@Capacity, Status=@Status,
                    GateAgentId=@GateAgentId
                WHERE FlightId=@FlightId";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FlightNumber",  f.FlightNumber);
            cmd.Parameters.AddWithValue("@Origin",        f.Origin);
            cmd.Parameters.AddWithValue("@Destination",   f.Destination);
            cmd.Parameters.AddWithValue("@DepartureTime", f.DepartureTime);
            cmd.Parameters.AddWithValue("@Capacity",      f.Capacity);
            cmd.Parameters.AddWithValue("@Status",        f.Status);
            cmd.Parameters.AddWithValue("@GateAgentId",   (object?)f.GateAgentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FlightId",      f.FlightId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Flight WHERE FlightId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public IEnumerable<Flight> GetByStatus(string status)
        {
            var list = new List<Flight>();
            const string sql = "SELECT * FROM Flight WHERE Status = @Status ORDER BY DepartureTime";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", status);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateFlight(r));
            return list;
        }

        public IEnumerable<Flight> GetByGateAgent(int agentId)
        {
            var list = new List<Flight>();
            const string sql = "SELECT * FROM Flight WHERE GateAgentId = @AgentId ORDER BY DepartureTime";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AgentId", agentId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateFlight(r));
            return list;
        }

        public bool UpdateStatus(int flightId, string newStatus)
        {
            const string sql = "UPDATE Flight SET Status = @Status WHERE FlightId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", newStatus);
            cmd.Parameters.AddWithValue("@Id",     flightId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateOccupancy(int flightId, int delta)
        {
            const string sql = "UPDATE Flight SET CurrentOccupancy = CurrentOccupancy + @Delta WHERE FlightId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Delta", delta);
            cmd.Parameters.AddWithValue("@Id",    flightId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
