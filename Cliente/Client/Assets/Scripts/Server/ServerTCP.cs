using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;

public class ServerTCP : MonoBehaviour
{
    private Socket socket;
    private Thread mainThread = null;

    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string serverText;

    public struct User
    {
        public string name;
        public Socket socket;
    }

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }

    void Update()
    {
        UItext.text = serverText;
    }

    public void startServer()
    {
        serverText = "Starting TDP Server...";

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 9050);
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
            newUser.name = "";
            newUser.socket = socket.Accept(); // Accept the socket

            IPEndPoint clientEndPoint = (IPEndPoint)newUser.socket.LocalEndPoint;
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
            recv = user.socket.Receive(data);
            if (recv == 0) 
                break;
            else
            {
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
                serverText += $"\nReceived: {receivedMessage}";

                // Send a ping back every time a message is received
                Thread answer = new Thread(() => Send(user));
                answer.Start();
            }
        }
    }

    void Send(User user)
    {
        string pingMessage = "ping";
        byte[] data = Encoding.ASCII.GetBytes(pingMessage);

        user.socket.Send(data);
        serverText += "\nSent: ping";
    }
}
