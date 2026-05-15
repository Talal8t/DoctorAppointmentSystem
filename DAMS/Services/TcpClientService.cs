using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DAMS.Models;

namespace DAMS.Services
{
    public class TcpClientService
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        public event Action<Message> OnMessageReceived;

        private readonly string _serverIp = "127.0.0.1";
        private readonly int _serverPort = 9010;

        // ================= CONNECT =================
        public async Task Connect()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_serverIp, _serverPort);

            _stream = _client.GetStream();
            _cts = new CancellationTokenSource();

            _ = Task.Run(ReceiveMessages);
        }

        // ================= SEND MESSAGE =================
        public async Task SendMessage(Message msg)
        {
            if (_client == null || !_client.Connected)
                await Connect();

            string json = JsonConvert.SerializeObject(msg);
            byte[] data = Encoding.UTF8.GetBytes(json);

            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task SendMessageToRecipients(Message msg, IEnumerable<int> recipientUserIds)
        {
            foreach (var recipientUserId in recipientUserIds.Distinct())
            {
                var routedMessage = new Message
                {
                    MessageId = msg.MessageId,
                    ChatId = msg.ChatId,
                    SenderId = msg.SenderId,
                    ReceiverId = recipientUserId,
                    Content = msg.Content,
                    MessageType = "Private",
                    Status = msg.Status,
                    CreatedAt = msg.CreatedAt
                };

                await SendMessage(routedMessage);
            }
        }

        // ================= RECEIVE MESSAGE =================
        private async Task ReceiveMessages()
        {
            byte[] buffer = new byte[4096];

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0) continue;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var message = JsonConvert.DeserializeObject<Message>(json);

                    OnMessageReceived?.Invoke(message);
                }
                catch
                {
                    break; // connection closed
                }
            }
        }

        // ================= DISCONNECT =================
        public void Disconnect()
        {
            _cts?.Cancel();
            _stream?.Close();
            _client?.Close();
        }
    }
}
