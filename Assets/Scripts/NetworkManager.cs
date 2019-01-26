using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{
    
    private UdpClient _udpBroadcaster;
    private UdpClient _udpBroadcastClient;
    private IPEndPoint _broadcastIpEndPoint;
    private bool _isBroadcasting;
    private bool _isFindingServers;
    private BroadcastMessage _broadcastMessage;
    private byte[] _broadcastMessageBytes;
    private int _broadcastPort;
    public GameObject ParentMenu;
    public GameObject MenuItem;
    public GameObject MyIpAddress;
    private IPEndPoint _serverBroadcastEndPoint;
    private List<NetworkServer> _broadcastList;
    [HideInInspector]
    public bool IsServer;
    private TcpListener _tcpListener;
    private int _tcpPort;
    private bool _tcpServerIsRunning;
    [HideInInspector]
    public List<NetworkClient> networkClientList;

    private void Awake()
    {
        _broadcastPort = 13947;
        _tcpPort = 13948;
        _broadcastList = new List<NetworkServer>();
        networkClientList = new List<NetworkClient>();
    }


    #region Broadcast
    public void StartBroadcasting()
    {
        _isBroadcasting = true;
        _broadcastMessage = new BroadcastMessage()
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
            yield return new WaitForSeconds(1000);
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
        foreach (NetworkServer n in _broadcastList)
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
                BroadcastMessage networkMessage = MessagePackSerializer.Deserialize<BroadcastMessage>(data);
                if (_broadcastList.FirstOrDefault(c => c.NetworkMessage.BroadcasterUuid == networkMessage.BroadcasterUuid) == null)
                {
                    NetworkServer networkServer = new NetworkServer();
                    networkServer.NetworkMessage = networkMessage;
                    networkServer.InstantiatedButtonStartTime = DateTime.Now;
                    networkServer.InstantiatedButton = Instantiate(MenuItem, ParentMenu.transform);
                    networkServer.InstantiatedButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 180 - (90 * _broadcastList.Count - 10), 0);
                    _broadcastList.Add(networkServer);
                }
                Debug.Log(networkMessage);
            }
            yield return new WaitForSeconds(1000);
            // todo remvoe items if they timeout
        }
    }
    #endregion

    #region Tcp
    public void StartTcpServer()
    {
        _tcpServerIsRunning = true;
        _tcpListener = new TcpListener(IPAddress.Parse(GetLocalIPAddress()), _tcpPort);
        _tcpListener.Start();
    }

    private IEnumerator InitializeTcpServer()
    {
        while (_tcpServerIsRunning)
        {
            if (_tcpListener.Pending())
            {
                Task<TcpClient> tcpClientTask = _tcpListener.AcceptTcpClientAsync();
                Task.WaitAll(tcpClientTask);
                NetworkClient networkClient = new NetworkClient()
                {
                    Client = tcpClientTask.Result
                };

            }
            yield return new WaitForSeconds(1000);
        }
    }
    #endregion
}
