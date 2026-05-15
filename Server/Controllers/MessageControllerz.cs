using Server.CoreFunc;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace Server.Controllers
{
    internal class MessageControllerz
    {
        public static void Handle(MessageModel message)
        {
            Console.WriteLine($"Message received from {message.SenderId}");

            Task.Run(async () =>
            {
                await MessageRouter.Route(message);
            });
        }
    }
}
