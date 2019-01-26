using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    #region Broadcast
    private UdpClient _udpBroadcaster;
    private UdpClient _udpBroadcastClient;
    private IPEndPoint _broadcastIpEndPoint;
    private bool _isBroadcasting;
    private bool _isFindingServers;
    private NetworkMessage _broadcastMessage;
    private byte[] _broadcastMessageBytes;
    private float _broadcastSleepTime;
    private int _broadcastPort;
    public GameObject ParentMenu;
    public GameObject MenuItem;
    public GameObject MyIpAddress;
    private IPEndPoint _serverBroadcastEndPoint;
    private List<NetworkClient> _broadcastList;
    

    private void Awake()
    {
        _broadcastSleepTime = 1f;
        _broadcastPort = 13947;
        _broadcastList = new List<NetworkClient>();
    }

    public void StartBroadcasting()
    {
        _isBroadcasting = true;
        _broadcastMessage = new NetworkMessage()
        {
            BroadcasterIpAddress = GetLocalIPAddress(),
            BroadcasterUuid = Guid.NewGuid().ToString(),
            MessageType = MessageType.HelloWorld
        };
        _broadcastMessageBytes = MessagePackSerializer.Serialize(_broadcastMessage);
        _udpBroadcaster = new UdpClient();
        _broadcastIpEndPoint = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);
        StartCoroutine(InitializeBroadcastServer());
    }

    public void StopBroadcasting()
    {
        _isBroadcasting = false;
    }

    private string GetLocalIPAddress()
    {
        string localIP = string.Empty;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString();
        }
        MyIpAddress.GetComponent<Text>().text = localIP;
        return localIP;
    }

    private IEnumerator InitializeBroadcastServer()
    {
        while (_isBroadcasting)
        {
            _udpBroadcaster.Send(_broadcastMessageBytes, _broadcastMessageBytes.Length, _broadcastIpEndPoint);
            yield return new WaitForSeconds(_broadcastSleepTime);
        }
    }

    public void StartBroadcastClient()
    {
        _isFindingServers = true;
        _udpBroadcastClient = new UdpClient();

        _udpBroadcastClient.ExclusiveAddressUse = false;
        _serverBroadcastEndPoint = new IPEndPoint(IPAddress.Any, _broadcastPort);

        _udpBroadcastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpBroadcastClient.ExclusiveAddressUse = false;

        _udpBroadcastClient.Client.Bind(_serverBroadcastEndPoint);
        StartCoroutine(InitializeBroadcastClient());
    }

    public void StopBroadcastClient()
    {
        foreach (NetworkClient n in _broadcastList)
        {
            Destroy(n.InstantiatedButton);
        }
        _broadcastList.Clear();
        _isFindingServers = false;
    }

    private IEnumerator InitializeBroadcastClient()
    {
        while (_isFindingServers)
        {
            if(_udpBroadcastClient.Available > 0)
            {
                byte[] data = _udpBroadcastClient.Receive(ref _serverBroadcastEndPoint);
                NetworkMessage networkMessage = MessagePackSerializer.Deserialize<NetworkMessage>(data);
                if (_broadcastList.FirstOrDefault(c => c.NetworkMessage.BroadcasterUuid == networkMessage.BroadcasterUuid) == null)
                {
                    NetworkClient networkClient = new NetworkClient();
                    networkClient.NetworkMessage = networkMessage;
                    networkClient.InstantiatedButtonStartTime = DateTime.Now;
                    networkClient.InstantiatedButton = Instantiate(MenuItem, ParentMenu.transform);
                    networkClient.InstantiatedButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 180 - (90 * _broadcastList.Count - 10), 0);
                    _broadcastList.Add(networkClient);
                }
                Debug.Log(networkMessage);
            }
            yield return new WaitForSeconds(_broadcastSleepTime);
        }
    }
    #endregion
}
