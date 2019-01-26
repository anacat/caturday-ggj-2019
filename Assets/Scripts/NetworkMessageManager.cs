using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkMessageManager : MonoBehaviour
{
    public void ProcessTcpNetworkMessage(TcpNetworkMessage message, TcpClient tcpClient)
    {
        Debug.Log(message.MessageType);
    }

    public void ProcessUdpNetworkMessage(UdpNetworkMessage message)
    {

    }
}
