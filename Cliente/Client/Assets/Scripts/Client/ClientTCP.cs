using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;

public class ClientTCP : MonoBehaviour
{
    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string clientText;

    public TMP_InputField inputNickname;
    public TMP_InputField inputIP;
    public TMP_InputField inputMessage;

    private string nickname = "Client";  // Default nickname for the client
    private Socket server;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UItext.text = clientText;
    }

    public void StartClient()
    {
        nickname = inputNickname.text;  // Use nickname from input
        string ip = inputIP.text;  // Get IP from input
        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";  // Default to localhost if not provided

        Thread connect = new Thread(() => Connect(ip));
        connect.Start();
    }

    void Connect(string ip)
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), 9050);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Connect(ipep);

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();

        clientText += $"\nConnected to server at {ip}";
    }

    public void SendMessage()
    {
        string message = inputMessage.text;  // Get the message from input field
        if (server != null && !string.IsNullOrEmpty(message))
        {
            string fullMessage = $"{nickname}: {message}";
            byte[] data = Encoding.ASCII.GetBytes(fullMessage);
            server.Send(data);

            clientText += $"\nSent: {fullMessage}";
        }
    }

    void Receive()
    {
        byte[] data = new byte[1024];

        while (true)
        {
            try
            {
                int recv = server.Receive(data);
                if (recv > 0)
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
                    clientText += $"\nReceived: {receivedMessage}";
                }
            }
            catch
            {
                break;
            }
        }
    }
}
