using System.Net.Sockets;
using System.Text;

namespace TcpChatClient;

public class ChatClient
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private string? _name;

    public async Task ConnectAsync()
    {
        Console.Write("Введите IP сервера (например, 127.0.0.1): ");
        string host = Console.ReadLine()!;
        Console.Write("Введите порт сервера (по умолчанию 12345): ");
        string portStr = Console.ReadLine()!;
        int port = string.IsNullOrWhiteSpace(portStr) ? 12345 : int.Parse(portStr);

        _client = new TcpClient();
        await _client.ConnectAsync(host, port);
        _stream = _client.GetStream();

        Console.Write("Введите ваше имя: ");
        _name = Console.ReadLine()!;
        Send($"name{_name}");

        Console.WriteLine("[CLIENT] Подключено. Вводите сообщения (или /exit для выхода)\n");

        // Запуск приёма сообщений от сервера
        _ = Task.Run(ReceiveLoop);

        // Основной цикл ввода
        while (true)
        {
            string? input = Console.ReadLine();
            if (input == null || input == "/exit")
            {
                Send("quit");
                break;
            }
            if (!string.IsNullOrWhiteSpace(input))
                Send($"message{input}");
        }

        _client.Close();
    }

    private void Send(string text)
    {
        if (_stream == null) return;
        byte[] data = Encoding.UTF8.GetBytes(text + "\n");
        try
        {
            _stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            _client.Close();
        }
    }

    private async void ReceiveLoop()
    {
        var buffer = new byte[8192];
        try
        {
            while (true)
            {
                int bytes = await _stream!.ReadAsync(buffer, 0, buffer.Length);
                if (bytes == 0) break;
                string message = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                if (message.StartsWith("new"))
                    Console.WriteLine($"*** {message.Substring(3)} вошёл в чат ***");
                else if (message.StartsWith("exit"))
                    Console.WriteLine($"*** {message.Substring(4)} вышел из чата ***");
                else if (message.StartsWith("message"))
                    Console.WriteLine(message.Substring(7));
            }
        }
        catch { }
    }
}
