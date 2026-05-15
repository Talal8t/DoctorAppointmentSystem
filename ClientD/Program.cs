using ClientD.Models;
using ClientD.Networking;

namespace ClientD
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var client = new TCPClientService();

            Console.WriteLine("=== DAMS CLIENT START ===");

            
            Console.Write("Enter Your UserId: ");
            int userId = int.Parse(Console.ReadLine());

            
            await client.Connect("127.0.0.1", 9010);

            Console.WriteLine("Connected successfully!");

            
            await client.SendMessage(new MessageModel
            {
                SenderId = userId,
                Message = "User Connected",
                Type = "Private",
                Timestamp = DateTime.Now
            });

            
            while (true)
            {
                Console.WriteLine("\n====================");
                Console.WriteLine("1. Private Message");
                Console.WriteLine("2. Broadcast (Selected Users)");
                Console.WriteLine("3. Admin Broadcast");
                Console.WriteLine("4. Exit");
                Console.Write("Choose option: ");

                var option = Console.ReadLine();

                if (option == "1")
                {
                    Console.Write("Receiver Id: ");
                    int receiverId = int.Parse(Console.ReadLine());

                    Console.Write("Message: ");
                    string msg = Console.ReadLine();

                    await client.SendMessage(new MessageModel
                    {
                        SenderId = userId,
                        ReceiverId = receiverId,
                        Message = msg,
                        Type = "Private",
                        Timestamp = DateTime.Now
                    });
                }
                else if (option == "2")
                {
                    Console.Write("Receiver IDs (comma separated): ");
                    var input = Console.ReadLine();

                    var ids = input.Split(',')
                                   .Select(int.Parse)
                                   .ToList();

                    Console.Write("Message: ");
                    string msg = Console.ReadLine();

                    await client.SendMessage(new MessageModel
                    {
                        SenderId = userId,
                        ReceiverIds = ids,
                        Message = msg,
                        Type = "Broadcast",
                        Timestamp = DateTime.Now
                    });
                }
                else if (option == "3")
                {
                    Console.Write("Message: ");
                    string msg = Console.ReadLine();

                    await client.SendMessage(new MessageModel
                    {
                        SenderId = userId,
                        Message = msg,
                        Type = "AdminBroadcast",
                        Timestamp = DateTime.Now
                    });
                }
                else if (option == "4")
                {
                    Console.WriteLine("Exiting...");
                    break;
                }
            }
        }
    }
}