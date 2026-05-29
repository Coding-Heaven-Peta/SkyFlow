using Microsoft.Data.SqlClient;
using SkyFlow.Data;
using SkyFlow.Interfaces;
using SkyFlow.Models;

namespace SkyFlow.Repositories
{
    
    public class UserRepository : IUserRepository
    {
        //  Hydration helper 

        private static User HydrateUser(SqlDataReader r)
        {
            string role = r["Role"].ToString() ?? "GateAgent";
            User user = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? new Admin()
                : new GateAgent();

            user.UserId    = (int)r["UserId"];
            user.Username  = r["Username"].ToString()  ?? string.Empty;
            user.Password  = r["PasswordHash"].ToString() ?? string.Empty;
            user.Role      = role;
            user.Email     = r["Email"]    == DBNull.Value ? string.Empty : r["Email"].ToString()!;
            user.FirstName = r["FirstName"] == DBNull.Value ? string.Empty : r["FirstName"].ToString()!;
            user.LastName  = r["LastName"]  == DBNull.Value ? string.Empty : r["LastName"].ToString()!;
            return user;
        }

        //  IDataRepository<User> 

        public IEnumerable<User> GetAll()
        {
            var list = new List<User>();
            const string sql = "SELECT * FROM Users ORDER BY LastName";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            using var r    = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateUser(r));
            return list;
        }

        public User? GetById(int id)
        {
            const string sql = "SELECT * FROM Users WHERE UserId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? HydrateUser(r) : null;
        }

        public bool Insert(User entity)
        {
            const string sql = @"
                INSERT INTO Users (Username, PasswordHash, Role, Email, FirstName, LastName)
                VALUES (@Username, @PasswordHash, @Role, @Email, @FirstName, @LastName)";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username",     entity.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", entity.Password);
            cmd.Parameters.AddWithValue("@Role",         entity.Role);
            cmd.Parameters.AddWithValue("@Email",        (object?)entity.Email     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstName",    (object?)entity.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastName",     (object?)entity.LastName  ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(User entity)
        {
            const string sql = @"
                UPDATE Users
                SET Email = @Email, FirstName = @FirstName, LastName = @LastName, Role = @Role
                WHERE UserId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email",     (object?)entity.Email     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstName", (object?)entity.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastName",  (object?)entity.LastName  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Role",      entity.Role);
            cmd.Parameters.AddWithValue("@Id",        entity.UserId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Users WHERE UserId = @Id";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        //  IUserRepository extras 

        public User? Authenticate(string username, string password)
        {
            const string sql = "SELECT * FROM Users WHERE Username = @Username AND PasswordHash = @Password";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Password", password);
            using var r = cmd.ExecuteReader();
            return r.Read() ? HydrateUser(r) : null;
        }

        public IEnumerable<User> GetByRole(string role)
        {
            var list = new List<User>();
            const string sql = "SELECT * FROM Users WHERE Role = @Role ORDER BY LastName";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Role", role);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(HydrateUser(r));
            return list;
        }

        public bool UsernameExists(string username)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            using var conn = DatabaseConnection.GetConnection();
            using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            return (int)cmd.ExecuteScalar()! > 0;
        }
    }
}