using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class ServerUDP : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread mainThread = null;
    private IPEndPoint remoteEndPoint;

    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string serverText;

    public GameObject activeUsersObj;
    private TextMeshProUGUI activeUsersGUI;

    public TMP_InputField inputNickname;
    public TMP_InputField inputIP;
    public TMP_InputField inputMessage;

    private string nickname = "Server";  // Default nickname for the server.
    private Dictionary<IPEndPoint, string> connectedUsers = new Dictionary<IPEndPoint, string>();  // List of connected clients

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
            sb.AppendLine(user.Value);  // Value is the username
            Debug.Log(sb.ToString());
        }
        return sb.ToString();
    }

    public void startServer()
    {
        nickname = inputNickname.text;  // Use nickname from input
        string ip = inputIP.text;  // Get IP from input
        if (string.IsNullOrEmpty(ip)) ip = "0.0.0.0";  // Default to any IP if not provided

        serverText = $"Starting UDP Server at {ip}...";

        udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), 9050));

        mainThread = new Thread(CheckNewConnections);
        mainThread.Start();
    }

    void CheckNewConnections()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);  // Receive data
                string receivedMessage = Encoding.ASCII.GetString(data);

                if (connectedUsers.ContainsKey(remoteEndPoint))
                {
                    // Existing user sending a message
                    string username = connectedUsers[remoteEndPoint];
                    serverText += $"\n{username}: {receivedMessage}";
                    BroadcastMessage($"{username}: {receivedMessage}", remoteEndPoint);
                }
                else
                {
                    // New user joining the server
                    connectedUsers[remoteEndPoint] = receivedMessage;
                    serverText += $"\nUser {receivedMessage} has joined from {remoteEndPoint.Address}:{remoteEndPoint.Port}";
                }
            }
            catch
            {
                serverText += "\nError receiving data.";
            }
        }
    }

    // Broadcast message to all connected users
    void BroadcastMessage(string message, IPEndPoint senderEndPoint)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        foreach (var user in connectedUsers)
        {
            if (!user.Key.Equals(senderEndPoint))  // Don't send the message back to the sender
            {
                udpClient.Send(data, data.Length, user.Key);
            }
        }
    }

    // Server can also send a message to all clients
    public void SendMessage()
    {
        string message = $"{nickname}: {inputMessage.text}";
        serverText += $"\nSent: {message}";
        BroadcastMessage(message, null);  // Broadcasting server's message (null as sender)

        inputMessage.text = "";
    }
}
