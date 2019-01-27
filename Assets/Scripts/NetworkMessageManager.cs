using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;
using System;

public class NetworkMessageManager : MonoBehaviour
{
    public void ProcessTcpNetworkMessage(TcpNetworkMessage message, TcpClient tcpClient)
    {
        switch (message.MessageType)
        {
            case MessageType.ConnectionRefused:
                break;
            case MessageType.Connecting:
                if(GameManager.Instance.NetworkManager.NetworkClientList.Count<= GameManager.Instance.RegisterAssets.Cats.Count)
                {
                    List<Tuple<int, Vector3, Quaternion>> assets = new List<Tuple<int, Vector3, Quaternion>>();
                    int count = GameManager.Instance.RegisterAssets.AssetList.Count;
                    for (int x = 0; x < count; x++)
                    {
                        Tuple<int, Vector3, Quaternion> n =
                            new Tuple<int, Vector3, Quaternion>(
                                x,
                                GameManager.Instance.RegisterAssets.AssetList[x].transform.position,
                                GameManager.Instance.RegisterAssets.AssetList[x].transform.rotation);
                        assets.Add(n);
                    }
                    Tuple<Vector3, Quaternion> player = new Tuple<Vector3, Quaternion>(
                        GameManager.Instance.RegisterAssets.Player.transform.position,
                        GameManager.Instance.RegisterAssets.Player.transform.rotation);
                    List<Tuple<int, Vector3, Quaternion>> cats = new List<Tuple<int, Vector3, Quaternion>>();
                    count = GameManager.Instance.RegisterAssets.Cats.Count;
                    for (int x = 0; x < count; x++)
                    {
                        Tuple<int, Vector3, Quaternion> n =
                            new Tuple<int, Vector3, Quaternion>(
                                x,
                                GameManager.Instance.RegisterAssets.Cats[x].transform.position,
                                GameManager.Instance.RegisterAssets.Cats[x].transform.rotation);
                        cats.Add(n);
                    }
                    TcpNetworkMessage connectingMessage = new TcpNetworkMessage()
                    {
                        MessageType = MessageType.ConnectionAccepted,
                        ClientUuid = GameManager.Instance.NetworkManager.OwnGuid.ToString(),
                        AssetList = assets,
                        Player = player,
                        CatList = cats,
                        DesignatedCat = GameManager.Instance.NetworkManager.NetworkClientList.Count-1
                    };
                    GameManager.Instance.NetworkManager.NetworkClientList.FirstOrDefault(
                        c => c.IpAddress == ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()).ClientUuid = connectingMessage.ClientUuid;
                    GameManager.Instance.NetworkManager.SendTcpServerMessage(connectingMessage);
                }
                else
                {
                    TcpNetworkMessage connectingMessage = new TcpNetworkMessage()
                    {
                        MessageType = MessageType.ConnectionRefused,
                        ClientUuid = GameManager.Instance.NetworkManager.OwnGuid.ToString(),
                    };
                    GameManager.Instance.NetworkManager.SendTcpServerMessage(connectingMessage);
                }
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
