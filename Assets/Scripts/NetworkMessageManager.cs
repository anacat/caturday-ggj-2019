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
            /*case MessageType.Connecting:
                
                break;
            case MessageType.ConnectionAccepted:
                //NetworkManager.ExecuteOnMainThread.Enqueue(() => { StartCoroutine(ProcessTcpMessageMainThread(message, tcpClient)); });
                break;*/
        }
        NetworkManager.ExecuteOnMainThread.Enqueue(() => { StartCoroutine(ProcessTcpMessageMainThread(message, tcpClient)); });
    }

    private IEnumerator ProcessTcpMessageMainThread(TcpNetworkMessage message, TcpClient tcpClient)
    {
        switch (message.MessageType)
        {
            case MessageType.Connecting:
                if (GameManager.Instance.NetworkManager.NetworkClientList.Count <= GameManager.Instance.RegisterAssets.Cats.Count)
                {
                    List<Tuple<Vector3, Quaternion>> assets = new List<Tuple<Vector3, Quaternion>>();
                    int count = GameManager.Instance.RegisterAssets.AssetList.Count;
                    for (int x = 0; x < count; x++)
                    {
                        Tuple<Vector3, Quaternion> n =
                            new Tuple<Vector3, Quaternion>(
                                GameManager.Instance.RegisterAssets.AssetList[x].transform.position,
                                GameManager.Instance.RegisterAssets.AssetList[x].transform.rotation);
                        assets.Add(n);
                    }
                    Tuple<Vector3, Quaternion> player = new Tuple<Vector3, Quaternion>(
                        GameManager.Instance.RegisterAssets.Player.transform.position,
                        GameManager.Instance.RegisterAssets.Player.transform.rotation);
                    List<Tuple<Vector3, Quaternion>> cats = new List<Tuple<Vector3, Quaternion>>();
                    count = GameManager.Instance.RegisterAssets.Cats.Count;
                    for (int x = 0; x < count; x++)
                    {
                        Tuple<Vector3, Quaternion> n =
                            new Tuple<Vector3, Quaternion>(
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
                        DesignatedCat = GameManager.Instance.NetworkManager.NetworkClientList.Count - 1
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
                GameManager.Instance.RegisterAssets.Player.transform.position = message.Player.Item1;
                GameManager.Instance.RegisterAssets.Player.transform.rotation = message.Player.Item2;
                int assetsCount = GameManager.Instance.RegisterAssets.Cats.Count;
                for (int x = 0; x < assetsCount; x++)
                {
                    GameManager.Instance.RegisterAssets.Cats[x].transform.position = message.CatList[x].Item1;
                    GameManager.Instance.RegisterAssets.Cats[x].transform.rotation = message.CatList[x].Item2;
                }
                assetsCount = GameManager.Instance.RegisterAssets.AssetList.Count;
                for (int x = 0; x < assetsCount; x++)
                {
                    GameManager.Instance.RegisterAssets.AssetList[x].transform.position = message.AssetList[x].Item1;
                    GameManager.Instance.RegisterAssets.AssetList[x].transform.rotation = message.AssetList[x].Item2;
                }
                GameManager.Instance.NetworkManager.IsReady = true;
                GameManager.Instance.MenuManager.Game.SetActive(true);
                break;
        }
        Debug.Log(message.MessageType);
        yield return null;
    }

    public void ProcessUdpNetworkMessage(UdpNetworkMessage message, IPEndPoint iPEndPoint)
    {
        if (!GameManager.Instance.NetworkManager.IsReady)
        {
            return;
        }

        NetworkManager.ExecuteOnMainThread.Enqueue(() => { StartCoroutine(ProcessUdpNetworkMessageMainThrad(message, iPEndPoint)); });
    }

    private IEnumerator ProcessUdpNetworkMessageMainThrad(UdpNetworkMessage message, IPEndPoint iPEndPoint)
    {
        Debug.Log(message.MessageType);
        yield return null;
    }
}
