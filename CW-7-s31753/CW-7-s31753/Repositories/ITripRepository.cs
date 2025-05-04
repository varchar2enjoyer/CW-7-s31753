using CW_7_s31753.Models; 

namespace CW_7_s31753.Repositories
{
    public interface ITripRepository
    {
        Task<IEnumerable<Trip>> GetAllTripsAsync();
        Task<IEnumerable<ClientTrip>> GetClientTripsAsync(int clientId);
        Task<int> AddClientAsync(Client client);
        Task<bool> AssignClientToTripAsync(int clientId, int tripId, DateTime? paymentDate);
        Task<bool> RemoveClientFromTripAsync(int clientId, int tripId);
        Task<bool> ClientExistsAsync(int clientId);
        Task<bool> TripExistsAsync(int tripId);
        Task<int> GetTripParticipantsCountAsync(int tripId);
    }
}