using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkMessageManager : MonoBehaviour
{
    public void ProcessTcpNetworkMessage(TcpNetworkMessage message, TcpClient tcpClient)
    {
        NetworkManager.ExecuteOnMainThread.Enqueue(() => { StartCoroutine(ProcessTcpMessageMainThread(message, tcpClient)); });
        
    }

    private IEnumerator ProcessTcpMessageMainThread(TcpNetworkMessage message, TcpClient tcpClient)
    {
        Debug.Log(message.MessageType);
        yield return null;
    }

    public void ProcessUdpNetworkMessage(UdpNetworkMessage message)
    {

    }
}
