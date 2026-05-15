using System.Net;
using System.Net.Sockets;

namespace Server.Netw
{
    public class TcpServer
    {
        private TcpListener _listener;

        public void Start(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            Console.WriteLine($"Server started on port {port}");

            while (true)
            {
                var client = _listener.AcceptTcpClient();

                Console.WriteLine("Client connected");

                var connection = new ClientConnection(client);

                Task.Run(() => connection.HandleClient());
            }
        }
    }
}