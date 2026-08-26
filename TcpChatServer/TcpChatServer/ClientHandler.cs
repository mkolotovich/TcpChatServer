using System.Net.Sockets;
using System.Text;

namespace TcpChatServer;

public class ClientHandler
{
    private readonly TcpClient _tcpClient;
    private readonly ChatServer _server;
    private NetworkStream _stream;
    private readonly List<ClientHandler> _clients;
    public string? Name { get; private set; }

    public ClientHandler(TcpClient tcpClient, ChatServer server, List<ClientHandler> clients)
    {
        _tcpClient = tcpClient;
        _server = server;
        _stream = tcpClient.GetStream();
        _clients = clients;
    }

    public void Start()
    {
        Task.Run(ProcessClientLoop);
    }

    private async void ProcessClientLoop()
    {
        var buffer = new byte[8192];
        var sb = new StringBuilder();

        try
        {
            while (true)
            {
                int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytes == 0) break;

                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytes));

                string fullMessage = sb.ToString();
                int eol = fullMessage.IndexOf('\n');
                while (eol >= 0)
                {
                    string line = fullMessage.Substring(0, eol).Trim();
                    fullMessage = fullMessage.Substring(eol + 1);
                    if (!string.IsNullOrEmpty(line))
                        HandleCommand(line);
                    eol = fullMessage.IndexOf('\n');
                }
                sb.Clear();
                sb.Append(fullMessage);
            }
        }
        catch { /* клиент отключился */ }
        finally
        {
            _server.RemoveClient(this);
            _tcpClient.Close();
        }
    }

    private void HandleCommand(string command)
    {
        if (command.StartsWith("name"))
        {
            Name = command.Substring(4);
            var clientsContainName = _clients.FindAll(client => client.Name == Name);
            if (clientsContainName.Count <= 1)
            {
                _server.Broadcast($"new{Name}", this);
                Console.WriteLine($"[SERVER] Пользователь {Name} представился");
            } 
            else
            {
                Send($"exit Вы");
                Console.WriteLine($"[SERVER] Пользователь c именем {Name} уже существует!");
                throw new Exception("Клиент уже существует!");
            }
        }
        else if (command.StartsWith("message"))
        {
            string text = command.Substring(7);
            _server.Broadcast($"message{Name}: {text}", this);
        }
        else if (command == "quit")
        {
            throw new Exception("Клиент запросил выход");
        }
    }

    public bool Send(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            _stream.Write(data, 0, data.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }
}