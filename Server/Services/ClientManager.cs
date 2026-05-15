using Server.Netw;
using System.Collections.Concurrent;

namespace Server.Services
{
    public class ClientManager
    {
        private static readonly Lazy<ClientManager> _instance =
            new(() => new ClientManager());

        public static ClientManager Instance => _instance.Value;

        private readonly ConcurrentDictionary<int, ClientConnection> _clients
            = new();

        public void AddClient(int userId, ClientConnection connection)
        {
            _clients[userId] = connection;
            Console.WriteLine($"User {userId} connected");
        }

        public void RemoveClient(int userId)
        {
            _clients.TryRemove(userId, out _);
            Console.WriteLine($"User {userId} disconnected");
        }

        public ClientConnection? GetClient(int userId)
        {
            _clients.TryGetValue(userId, out var client);
            return client;
        }

        public List<ClientConnection> GetAllClients()
        {
            return _clients.Values.ToList();
        }
    }
}