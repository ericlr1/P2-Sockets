using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;

public class ClientUDP : MonoBehaviour
{
    public GameObject UItextObj;
    private TextMeshProUGUI UItext;
    private string clientText;

    public TMP_InputField inputNickname;
    public TMP_InputField inputIP;
    public TMP_InputField inputMessage;

    private string nickname = "Client";  // Default nickname for the client
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;

    private Thread receiveThread;

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

        // Initialize UDP client and server endpoint
        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), 9050);

        // Send nickname as the first message to the server
        SendNickname();

        // Start a thread to listen for incoming messages
        receiveThread = new Thread(Receive);
        receiveThread.Start();

        clientText += $"\nConnected to server at {ip}";
    }

    void SendNickname()
    {
        byte[] data = Encoding.ASCII.GetBytes(nickname);
        udpClient.Send(data, data.Length, serverEndPoint);
        clientText += $"\nSent nickname: {nickname}";
    }

    public void SendMessage()
    {
        // Send message input from the user to the server
        string message = inputMessage.text;
        byte[] data = Encoding.ASCII.GetBytes(message);

        udpClient.Send(data, data.Length, serverEndPoint);
        clientText += $"\nSent: {message}";
        inputMessage.text = "";  // Clear input field
    }

    void Receive()
    {
        IPEndPoint fromEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                // Listen for incoming data from the server
                byte[] data = udpClient.Receive(ref fromEndPoint);
                string receivedMessage = Encoding.ASCII.GetString(data);

                clientText += $"\nReceived: {receivedMessage}";
            }
            catch
            {
                clientText += "\nError receiving data.";
                break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        // Stop receiving when the application closes
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }

        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
