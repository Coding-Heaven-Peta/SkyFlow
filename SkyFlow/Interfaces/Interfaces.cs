using SkyFlow.Models;

namespace SkyFlow.Interfaces
{
    
    public interface IDataRepository<T>
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        bool Insert(T entity);
        bool Update(T entity);
        bool Delete(int id);
    }


    public interface IUserRepository : IDataRepository<User>
    {
        User? Authenticate(string username, string password);
        IEnumerable<User> GetByRole(string role);
        bool UsernameExists(string username);
    }

    public interface IFlightRepository : IDataRepository<Flight>
    {
        IEnumerable<Flight> GetByStatus(string status);
        IEnumerable<Flight> GetByGateAgent(int agentId);
        bool UpdateStatus(int flightId, string newStatus);
        bool UpdateOccupancy(int flightId, int delta);
    }

    public interface IPassengerRepository : IDataRepository<Passenger>
    {
        Passenger? GetByPassport(string passportNumber);
    }

    public interface IBookingRepository : IDataRepository<Booking>
    {
        IEnumerable<Booking> GetByFlight(int flightId);
        IEnumerable<Booking> GetByPassenger(int passengerId);
        bool UpdateStatus(int bookingId, string newStatus, DateTime? timeStamp = null);
        Booking? FindPassengerOnFlight(int flightId, string passportOrId);
    }
}
