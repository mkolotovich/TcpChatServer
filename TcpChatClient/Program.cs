namespace TcpChatClient;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== TCP Чат — Клиент ===\n");

        var client = new ChatClient();
        await client.ConnectAsync();
    }
}
