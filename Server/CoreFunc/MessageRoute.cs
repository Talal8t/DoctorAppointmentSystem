using Server.Models;
using Server.Services;

namespace Server.CoreFunc
{
    public class MessageRouter
    {
        public static async Task Route(MessageModel message)
        {
            switch (message.MessageType)
            {
                case "Private":
                    await SendPrivate(message);
                    break;

                case "Broadcast":
                    await SendBroadcast(message);
                    break;

                case "AdminBroadcast":
                    await SendToAll(message);
                    break;
            }
        }

        private static async Task SendPrivate(MessageModel message)
        {
            if (message.ReceiverId == null)
                return;

            var client = ClientManager.Instance.GetClient(message.ReceiverId.Value);

            if (client != null)
                await client.SendAsync(message);
        }

        private static async Task SendBroadcast(MessageModel message)
        {
            var clients = ClientManager.Instance.GetAllClients();

            foreach (var client in clients)
            {
                await client.SendAsync(message);
            }
        }

        private static async Task SendToAll(MessageModel message)
        {
            var clients = ClientManager.Instance.GetAllClients();

            foreach (var client in clients)
            {
                await client.SendAsync(message);
            }
        }
    }
}