using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;

public class NetworkMessageManager : MonoBehaviour
{
    public void ProcessTcpNetworkMessage(TcpNetworkMessage message, TcpClient tcpClient)
    {
        switch (message.MessageType)
        {
            case MessageType.ConnectionRefused:
                break;
            case MessageType.Connecting:
                // validate if there is room left. If not, reply ConnectionRefused
                /*TcpNetworkMessage connectingMessage = new TcpNetworkMessage()
                {
                    MessageType = MessageType.ConnectionRefused,
                    ClientUuid = GameManager.Instance.NetworkManager.OwnGuid.ToString()
                };*/
                TcpNetworkMessage connectingMessage = new TcpNetworkMessage()
                {
                    MessageType = MessageType.ConnectionAccepted,
                    ClientUuid = GameManager.Instance.NetworkManager.OwnGuid.ToString(),
                    AssetList = new List<System.Tuple<int, Vector3, Vector3>>() {new System.Tuple<int, Vector3, Vector3>(0, Vector3.back, Vector3.down) } 
                };
                GameManager.Instance.NetworkManager.NetworkClientList.FirstOrDefault(
                    c => c.IpAddress == ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()).ClientUuid = connectingMessage.ClientUuid;
                GameManager.Instance.NetworkManager.SendTcpServerMessage(connectingMessage);
                break;
            case MessageType.ConnectionAccepted:

                break;
        }
        NetworkManager.ExecuteOnMainThread.Enqueue(() => { StartCoroutine(ProcessTcpMessageMainThread(message, tcpClient)); });
    }

    private IEnumerator ProcessTcpMessageMainThread(TcpNetworkMessage message, TcpClient tcpClient)
    {
        Debug.Log(message.MessageType);
        yield return null;
    }

    public void ProcessUdpNetworkMessage(UdpNetworkMessage message, IPEndPoint iPEndPoint)
    {
        NetworkManager.ExecuteOnMainThread.Enqueue(() => { StartCoroutine(ProcessUdpNetworkMessageMainThrad(message, iPEndPoint)); });
    }

    private IEnumerator ProcessUdpNetworkMessageMainThrad(UdpNetworkMessage message, IPEndPoint iPEndPoint)
    {
        Debug.Log(message.MessageType);
        yield return null;
    }
}
