using Server.Netw;

class Program
{
    static void Main(string[] args)
    {
        var server = new TcpServer();
        server.Start(9010 );
    }
}