using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class ServerTCP : MonoBehaviour
{
    private Socket socket;
    private Thread mainThread = null;

    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string serverText;

    public GameObject activeUsersObj;
    private TextMeshProUGUI activeUsersGUI;

    public TMP_InputField inputNickname;
    public TMP_InputField inputIP;
    public TMP_InputField inputMessage;

    private string nickname = "Server";  // Default nickname for the server.
    private List<User> connectedUsers = new List<User>();  // List of connected clients

    public class User
    {
        public string name;
        public Socket socket;
    }

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
        activeUsersGUI = activeUsersObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UItext.text = serverText;
        activeUsersGUI.text = GetConnectedUsers();

    }

    // Function to update the list of connected users
    string GetConnectedUsers()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var user in connectedUsers)
        {
            sb.AppendLine(user.name);
            Debug.Log(sb.ToString() + "//" + user.name + "//" + user.socket);
        }
        return sb.ToString();
    }

    public void startServer()
    {
        nickname = inputNickname.text;  // Use nickname from input
        string ip = inputIP.text;  // Get IP from input
        if (string.IsNullOrEmpty(ip)) ip = "0.0.0.0";  // Default to any IP if not provided

        serverText = $"Starting TCP Server at {ip}...";

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), 9050);
        socket.Bind(localEndPoint);
        socket.Listen(10);

        mainThread = new Thread(CheckNewConnections);
        mainThread.Start();
    }

    void CheckNewConnections()
    {
        while (true)
        {
            User newUser = new User();
            newUser.name = ""; // This will be assigned when the client sends its nickname
            newUser.socket = socket.Accept();  // Accept incoming connection

            connectedUsers.Add(newUser);  // Add user to list of connected clients

            IPEndPoint clientEndPoint = (IPEndPoint)newUser.socket.RemoteEndPoint;
            serverText += $"\nConnected with {clientEndPoint.Address} at port {clientEndPoint.Port}";

            Thread newConnection = new Thread(() => Receive(newUser));
            newConnection.Start();
        }
    }

    void Receive(User user)
    {
        byte[] data = new byte[1024];
        int recv = 0;

        while (true)
        {
            try
            {
                recv = user.socket.Receive(data);
                if (recv == 0) break;
                else
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                    // Check if it's the first message (nickname setup)
                    if (string.IsNullOrEmpty(user.name))
                    {
                        user.name = receivedMessage;
                        serverText += $"\nUser {user.name} has joined.";
                    }
                    else
                    {
                        serverText += $"\n{user.name}: {receivedMessage}";
                        BroadcastMessage($"{user.name}: {receivedMessage}", user);
                    }
                }
            }
            catch
            {
                break;
            }
        }

        // Remove user from connected list when disconnected
        connectedUsers.Remove(user);
    }

    // Broadcast message to all connected users
    void BroadcastMessage(string message, User sender)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        foreach (var user in connectedUsers)
        {
            if (user.socket != sender.socket)  // Don't send the message back to the sender
            {
                user.socket.Send(data);
            }
        }
    }

    // Server can also send a message to all clients
    public void SendMessage()
    {
        string message = $"{nickname}: {inputMessage.text}";
        serverText += $"\nSent: {message}";
        BroadcastMessage(message, new User());  // Broadcasting server's message (new User() is a dummy sender)

        inputMessage.text = "";
    }
}
