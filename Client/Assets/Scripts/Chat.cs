using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Chat : MonoBehaviour {

    [SerializeField]
    Text name;
    [SerializeField]
    Text message;
    [SerializeField]
    Text forPrint;
    [SerializeField]
    Scrollbar scroll;
    public static Text sendMessage;
    const int PORT = 11000;
    Client client;


    void Start()
    {
        Application.runInBackground = true;
        client = new Client(PORT, "192.168.1.101");
        client.Work();
        sendMessage = forPrint;
    }

    public void PrintMessage()
    {
        client.SendMessage(name.text + " : " + message.text);
        scroll.value = 0;
    }
}
