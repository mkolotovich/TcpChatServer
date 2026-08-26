using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpChatServer;

public class ChatServer
{
    private TcpListener? _listener;
    private readonly List<ClientHandler> _clients = new();
    private bool _running = false;
    public void Start()
    {
        Console.Write("Введите порт для сервера (например, 12345): ");
        int port = int.Parse(Console.ReadLine()!);

        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _running = true;

        Console.WriteLine($"[SERVER] Сервер запущен на порту {port}");
        Console.WriteLine("[SERVER] Ожидание подключений...");

        // Запуск цикла принятия клиентов в отдельном потоке
        Task.Run(AcceptClientsLoop);
    }

    private async void AcceptClientsLoop()
    {
        while (_running)
        {
            try
            {
                TcpClient tcpClient = await _listener!.AcceptTcpClientAsync();
                var client = new ClientHandler(tcpClient, this, _clients);
                lock (_clients) _clients.Add(client);

                Console.WriteLine($"[SERVER] Подключился клиент: {tcpClient.Client.RemoteEndPoint}");
                client.Start();
            }
            catch (ObjectDisposedException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"[ОШИБКА ПРИЁМА] {ex.Message}");
            }
        }
    }

    public void Broadcast(string message, ClientHandler? except = null)
    {
        lock (_clients)
        {
            var disconnected = new List<ClientHandler>();
            foreach (var client in _clients)
            {
                if (client == except) continue;
                if (!client.Send(message))
                    disconnected.Add(client);
            }
            foreach (var c in disconnected)
                RemoveClient(c);
        }
    }

    public void RemoveClient(ClientHandler client)
    {
        lock (_clients)
        {
            _clients.Remove(client);
            if (!string.IsNullOrEmpty(client.Name))
                Broadcast($"exit{client.Name}");
            Console.WriteLine($"[SERVER] Клиент отключился: {client.Name}");
        }
    }

    public void Stop()
    {
        _running = false;
        _listener?.Stop();
        lock (_clients) _clients.Clear();
        Console.WriteLine("[SERVER] Сервер остановлен.");
    }
}