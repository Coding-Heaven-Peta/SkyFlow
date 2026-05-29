namespace SkyFlow.Helpers
{
    // Renders SQL data
    public static class TableRenderer
    {
        // Generic Table
        public static void Render(string[] headers, List<string[]> rows)
        {
            if (headers == null || headers.Length == 0) return;

            // Calculate column widths: max of header length vs any cell in that column
            int[] widths = new int[headers.Length];
            for (int i = 0; i < headers.Length; i++)
                widths[i] = headers[i].Length;

            foreach (var row in rows)
                for (int i = 0; i < headers.Length && i < row.Length; i++)
                    if ((row[i]?.Length ?? 0) > widths[i])
                        widths[i] = row[i].Length;

            string separator = BuildSeparator(widths);

            Console.WriteLine("\n" + separator);
            Console.WriteLine(BuildRow(headers, widths));
            Console.WriteLine(separator);

            if (rows.Count == 0)
            {
                string empty = "| " + "No records found.".PadRight(separator.Length - 4) + " |";
                Console.WriteLine(empty);
            }
            else
            {
                foreach (var row in rows)
                    Console.WriteLine(BuildRow(row, widths));
            }

            Console.WriteLine(separator);
            Console.WriteLine($"  {rows.Count} record(s) found.\n");
        }

        private static string BuildSeparator(int[] widths)
        {
            var parts = widths.Select(w => new string('-', w + 2));
            return "+" + string.Join("+", parts) + "+";
        }

        private static string BuildRow(string[] cells, int[] widths)
        {
            var parts = new List<string>();
            for (int i = 0; i < widths.Length; i++)
            {
                string cell = (i < cells.Length ? cells[i] : string.Empty) ?? string.Empty;
                parts.Add(" " + cell.PadRight(widths[i]) + " ");
            }
            return "|" + string.Join("|", parts) + "|";
        }


        public static void RenderFlights(IEnumerable<Models.Flight> flights)
        {
            var headers = new[] { "ID", "Flight No", "Origin", "Destination", "Departure", "Capacity", "Occupancy", "Status" };
            var rows    = flights.Select(f => new[]
            {
                f.FlightId.ToString(),
                f.FlightNumber,
                f.Origin,
                f.Destination,
                f.DepartureTime.ToString("yyyy-MM-dd HH:mm"),
                f.Capacity.ToString(),
                f.CurrentOccupancy.ToString(),
                f.Status
            }).ToList();
            Render(headers, rows);
        }

        public static void RenderBookings(IEnumerable<Models.Booking> bookings)
        {
            var headers = new[] { "Booking ID", "Seat", "Passenger", "Passport", "Status", "Check-In Time" };
            var rows    = bookings.Select(b => new[]
            {
                b.BookingId.ToString(),
                b.SeatNumber,
                b.PassengerName ?? "—",
                b.PassportNumber ?? "—",
                b.BookingStatus,
                b.CheckInTime.HasValue ? b.CheckInTime.Value.ToString("HH:mm") : "—"
            }).ToList();
            Render(headers, rows);
        }

        public static void RenderUsers(IEnumerable<Models.User> users)
        {
            var headers = new[] { "ID", "Username", "Full Name", "Role", "Email" };
            var rows    = users.Select(u => new[]
            {
                u.UserId.ToString(),
                u.Username,
                u.FullName,
                u.Role,
                u.Email
            }).ToList();
            Render(headers, rows);
        }
    }
}
