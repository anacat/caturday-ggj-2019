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
    public TcpClient TcpNetworkClient;
    public TcpClient TcpNetworkServer;
    private Guid _ownGuid;

    private void Awake()
    {
        _ownGuid = Guid.NewGuid();
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
            BroadcasterUuid = _ownGuid.ToString(),
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
            yield return new WaitForSeconds(1);
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
                    networkServer.InstantiatedButton.GetComponent<Button>().onClick.AddListener(() => ConnectToTcpServer(networkMessage));
                    networkServer.InstantiatedButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 180 - (90 * _broadcastList.Count - 10), 0);
                    _broadcastList.Add(networkServer);
                }
                Debug.Log(networkMessage);
            }
            yield return new WaitForSeconds(1);
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

    public void StopTcpServer()
    {
        _tcpServerIsRunning = false;
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
            yield return new WaitForSeconds(1);
        }
    }

    public void ConnectToTcpServer(BroadcastMessage broadcastMessage)
    {
        TcpNetworkClient = new TcpClient();
        StartCoroutine(InitializeTcpClient(broadcastMessage));
    }

    private IEnumerator InitializeTcpClient(BroadcastMessage broadcastMessage)
    {
        Task.WaitAll(TcpNetworkClient.ConnectAsync(IPAddress.Parse(broadcastMessage.BroadcasterIpAddress), _tcpPort));
        if (TcpNetworkClient.Connected)
        {
            TcpNetworkMessage tcpNetworkMessage = new TcpNetworkMessage()
            {
                ClientUuid = _ownGuid.ToString(),
                MessageType = MessageType.JoinGame
            };
            SendTcpMessage(tcpNetworkMessage);
        }
        yield return null;
    }

    public IEnumerator SendTcpMessage(TcpNetworkMessage tcpNetworkMessage)
    {
        //TcpNetworkServer
        yield return null;
    }

    public IEnumerator SendTcpMessageToAll()
    {
        yield return null;
    }
    #endregion
}
