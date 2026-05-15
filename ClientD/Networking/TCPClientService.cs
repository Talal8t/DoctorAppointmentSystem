using ClientD.Models;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ClientD.Networking
{
    public class TCPClientService
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public async Task Connect(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);

            _stream = _client.GetStream();

            Console.WriteLine("Connected to server");

            Task.Run(ReceiveMessages);
        }

        public async Task SendMessage(MessageModel message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[4096];

            while (true)
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var message = JsonSerializer.Deserialize<MessageModel>(json);

                if (message != null)
                {
                    Console.WriteLine($"\n📩 From {message.SenderId}: {message.Message}");
                }
            }
        }
    }
}