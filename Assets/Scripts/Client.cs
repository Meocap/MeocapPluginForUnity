using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class Client : MonoBehaviour {
    public string serverIp = "127.0.0.1";
    public int port = 8888;
    public bool connectOnLoad = true;

    private Socket clientSocket;
    private int dataSize = 4096;
	private byte[] data = new byte[4096];


	/// <summary>
	/// connect to server
	/// </summary>
	/// <returns></returns>
	public void Connect() {
		clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		clientSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIp), port));
	}

    /// <summary>
    /// disconnect and close the socket
    /// </summary>
    /// <returns></returns>
    public void Disconnect() {
		clientSocket.Shutdown(SocketShutdown.Both);
		clientSocket.Close();
	}

    /// <summary>
    /// Receive a string that end with a specific character from the server. Note that the char must 
    /// occur at the end rather than in the middle of the string.
    /// </summary>
    /// <param name="end">The character that means the end of the received string.</param>
    /// <returns>The received string before (without) the specific character.</returns>
    public string Receive(char end) {
        string message = "";
        do {
            int length = clientSocket.Receive(data);
            message += Encoding.UTF8.GetString(data, 0, length);
        } while (message[message.Length - 1] != end);
        //Debug.Log("Recv " + message);
        return message.Substring(0, message.Length - 1);
    }

	/// <summary>
	/// Receive a string with a specified buffer size.
	/// </summary>
	/// <param name="size">The buffer size for the received data.</param>
	/// <returns>The received short string.</returns>
	public string Receive(int size = 4096) {
		if (dataSize != size) {
			data = new byte[size];
			dataSize = size;
		}
		int length = clientSocket.Receive(data);
		string message = Encoding.UTF8.GetString(data, 0, length);
		//Debug.Log("Recv " + message);
		return message;
	}

	/// <summary>
	/// Send a string to the server.
	/// </summary>
	public void Send(string message) {
        byte[] data = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(data);
        //Debug.Log("Send " + message);
    }

	void Start() {
        if (connectOnLoad) Connect();
	}

	void OnDestroy() {
        if (connectOnLoad) Disconnect();
    }
}