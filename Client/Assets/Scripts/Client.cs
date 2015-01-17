using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections.Generic;

public class Client : MonoBehaviour {

    static TcpClient client;

    public Client(int Port, string connectIp)
    {
        client = new TcpClient();
        client.Connect(IPAddress.Parse(connectIp), Port);
    }

    public void Work()
    {
        Thread clientListener = new Thread(Reader);
        clientListener.Start();
    }

    public void SendMessage(string message)
    {
        message.Trim();
        byte[] Buffer = Encoding.ASCII.GetBytes((message).ToCharArray());
        client.GetStream().Write(Buffer, 0, Buffer.Length);
        Chat.sendMessage.text += message + "\n";
    }

    static void Reader()
    {
        while (true)
        {
            NetworkStream NS = client.GetStream();
            List<byte> Buffer = new List<byte>();
            while (NS.DataAvailable)
            {
                int ReadByte = NS.ReadByte();
                if (ReadByte > -1)
                {
                    Buffer.Add((byte)ReadByte);
                }
            }
            if (Buffer.Count > 0)
                Chat.sendMessage.text +=  Encoding.ASCII.GetString(Buffer.ToArray()) +"\n";
        }
    }

    ~Client()
    {
        if (client != null)
        {
            client.Close();
        }
    }
}
