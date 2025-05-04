using Microsoft.Data.SqlClient;
using CW_7_s31753.Models;
using CW_7_s31753.Database; 

namespace CW_7_s31753.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly DbConnection _dbConnection;
        
        public TripRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Trip>> GetAllTripsAsync()
        {
            var trips = new List<Trip>();
            
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = @"
                SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                       c.IdCountry, c.Name as CountryName
                FROM Trip t
                LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
                ORDER BY t.DateFrom DESC";
            
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            var tripDict = new Dictionary<int, Trip>();
            
            while (await reader.ReadAsync())
            {
                var tripId = reader.GetInt32(0);
                if (!tripDict.TryGetValue(tripId, out var trip))
                {
                    trip = new Trip
                    {
                        Id = tripId,
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        StartDate = reader.GetDateTime(3),
                        EndDate = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5)
                    };
                    tripDict.Add(tripId, trip);
                }
                
                if (!reader.IsDBNull(6))
                {
                    trip.Countries.Add(new Country
                    {
                        Id = reader.GetInt32(6),
                        Name = reader.GetString(7)
                    });
                }
            }
            
            return tripDict.Values;
        }

        public async Task<IEnumerable<ClientTrip>> GetClientTripsAsync(int clientId)
        {
            var clientTrips = new List<ClientTrip>();
            
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = @"
                SELECT ct.IdClient, ct.IdTrip, ct.RegisteredAt, ct.PaymentDate,
                       t.Name, t.DateFrom, t.DateTo
                FROM Client_Trip ct
                JOIN Trip t ON ct.IdTrip = t.IdTrip
                WHERE ct.IdClient = @ClientId
                ORDER BY t.DateFrom DESC";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClientId", clientId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                clientTrips.Add(new ClientTrip
                {
                    ClientId = reader.GetInt32(0),
                    TripId = reader.GetInt32(1),
                    RegisteredAt = reader.GetDateTime(2),
                    PaymentDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    TripName = reader.GetString(4),
                    TripStartDate = reader.GetDateTime(5),
                    TripEndDate = reader.GetDateTime(6)
                });
            }
            
            return clientTrips;
        }

        public async Task<int> AddClientAsync(Client client)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = @"
                INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                OUTPUT INSERTED.IdClient
                VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", client.FirstName);
            command.Parameters.AddWithValue("@LastName", client.LastName);
            command.Parameters.AddWithValue("@Email", client.Email);
            command.Parameters.AddWithValue("@Telephone", client.Telephone);
            command.Parameters.AddWithValue("@Pesel", client.Pesel);
            
            return (int)await command.ExecuteScalarAsync();
        }

        public async Task<bool> AssignClientToTripAsync(int clientId, int tripId, DateTime? paymentDate)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = @"
                INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                VALUES (@ClientId, @TripId, @RegisteredAt, @PaymentDate)";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@TripId", tripId);
            command.Parameters.AddWithValue("@RegisteredAt", DateTime.Now);
            command.Parameters.AddWithValue("@PaymentDate", paymentDate ?? (object)DBNull.Value);
            
            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> RemoveClientFromTripAsync(int clientId, int tripId)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = @"
                DELETE FROM Client_Trip
                WHERE IdClient = @ClientId AND IdTrip = @TripId";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@TripId", tripId);
            
            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> ClientExistsAsync(int clientId)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = "SELECT 1 FROM Client WHERE IdClient = @ClientId";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClientId", clientId);
            
            return await command.ExecuteScalarAsync() != null;
        }

        public async Task<bool> TripExistsAsync(int tripId)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = "SELECT 1 FROM Trip WHERE IdTrip = @TripId";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TripId", tripId);
            
            return await command.ExecuteScalarAsync() != null;
        }

        public async Task<int> GetTripParticipantsCountAsync(int tripId)
        {
            using var connection = _dbConnection.GetConnection();
            await connection.OpenAsync();
            
            var query = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TripId", tripId);
            
            return (int)await command.ExecuteScalarAsync();
        }
    }
}