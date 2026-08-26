using System.Text;

namespace TcpChatServer;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=== TCP Чат — Сервер ===\n");

        var server = new ChatServer();
        server.Start();

        Console.WriteLine("Нажмите любую клавишу для остановки сервера...");
        Console.ReadKey();
        server.Stop();
    }
}
