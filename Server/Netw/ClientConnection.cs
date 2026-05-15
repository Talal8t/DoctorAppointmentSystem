
using Server.Controllers;
using Server.Models;
using Server.Services;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server.Netw
{
    public class ClientConnection
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public int UserId { get; private set; }

        public ClientConnection(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public async Task HandleClient()
        {
            var buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                        break;

                    var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var message = JsonSerializer.Deserialize<MessageModel>(json);

                    if (message == null)
                        continue;

                    // register user
                    if (UserId == 0)
                    {
                        UserId = message.SenderId;
                        ClientManager.Instance.AddClient(UserId, this);
                    }

                    MessageControllerz.Handle(message);
                }
            }
            catch
            {
                Console.WriteLine($"User {UserId} disconnected");
            }
            finally
            {
                ClientManager.Instance.RemoveClient(UserId);
                _client.Close();
            }
        }

        public async Task SendAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}