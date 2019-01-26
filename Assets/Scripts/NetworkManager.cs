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
using System.IO;

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
    public GameObject TargetIpAddress;
    private IPEndPoint _serverBroadcastEndPoint;
    private List<NetworkServer> _broadcastList;
    [HideInInspector]
    public bool IsServer;
    private TcpListener _tcpListener;
    private int _tcpPort;
    private bool _tcpServerIsRunning;
    [HideInInspector]
    public List<NetworkClient> NetworkClientList;
    public TcpClient TcpNetworkClient;
    public TcpClient TcpNetworkServer;
    private Guid _ownGuid;

    private void Awake()
    {
        _ownGuid = Guid.NewGuid();
        _broadcastPort = 13947;
        _tcpPort = 13948;
        _broadcastList = new List<NetworkServer>();
        NetworkClientList = new List<NetworkClient>();
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
        _broadcastIpEndPoint = new IPEndPoint(IPAddress.Parse("239.0.0.254"), _broadcastPort);
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
        IPAddress ipAddress = IPAddress.Parse("239.0.0.254");
        _udpBroadcastClient.JoinMulticastGroup(ipAddress,IPAddress.Parse(GetLocalIPAddress()));
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
                else
                {
                    _broadcastList.FirstOrDefault(c => c.NetworkMessage.BroadcasterUuid == networkMessage.BroadcasterUuid).InstantiatedButtonStartTime = DateTime.Now;
                }
            }
            for (int x = _broadcastList.Count - 1; x >= 0; x--)
            {
                if (_broadcastList[x].InstantiatedButtonStartTime.AddSeconds(5) < DateTime.Now)
                {
                    Destroy(_broadcastList[x].InstantiatedButton);
                    _broadcastList.RemoveAt(x);
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
    #endregion

    #region Tcp Server
    public void StartTcpServer()
    {
        _tcpServerIsRunning = true;
        _tcpListener = new TcpListener(IPAddress.Parse(GetLocalIPAddress()), _tcpPort);
        _tcpListener.Start();
        StartCoroutine(InitializeTcpServer());
        StartCoroutine(ReceiveTcpClientMessage());
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
                    Client = tcpClientTask.Result,
                    NetworkStream = tcpClientTask.Result.GetStream()
                };
                NetworkClientList.Add(networkClient);
            }
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator ReceiveTcpClientMessage()
    {
        while (_tcpServerIsRunning)
        {
            foreach(NetworkClient c in NetworkClientList)
            {
                if(c.Client.Available > 0)
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[0x1000];
                    do {
                        Task<int> readBytes = c.NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                        Task.WaitAll(readBytes);
                        ms.Write(buffer, 0, readBytes.Result);
                    }
                    while (c.NetworkStream.DataAvailable);

                    TcpNetworkMessage tcpNetworkMessage = MessagePackSerializer.Deserialize<TcpNetworkMessage>(ms.ToArray());
                    Debug.Log(tcpNetworkMessage.MessageType);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator SendTcpClientMessage(TcpNetworkMessage tcpNetworkMessage)
    {
        //TcpNetworkServer
        yield return null;
    }

    public IEnumerator SendTcpClientMessageToAll()
    {
        yield return null;
    }
    #endregion

    #region Tcp Client

    public void ConnectToTcpServer(BroadcastMessage broadcastMessage)
    {
        TcpNetworkClient = new TcpClient();
        StartCoroutine(InitializeTcpClient(broadcastMessage.BroadcasterIpAddress));
    }

    public void ConnectToTcpServer()
    {
        TcpNetworkClient = new TcpClient();
        StartCoroutine(InitializeTcpClient(TargetIpAddress.GetComponent<InputField>().text));
    }

    private IEnumerator InitializeTcpClient(string ipAddress)
    {
        Task.WaitAll(TcpNetworkClient.ConnectAsync(IPAddress.Parse(ipAddress), _tcpPort));
        if (TcpNetworkClient.Connected)
        {
            TcpNetworkMessage tcpNetworkMessage = new TcpNetworkMessage()
            {
                ClientUuid = _ownGuid.ToString(),
                MessageType = MessageType.JoinGame
            };
            SendTcpServerMessage(tcpNetworkMessage);
        }
        yield return null;
    }

    public void SendTcpServerMessage(TcpNetworkMessage tcpNetworkMessage)
    {
        byte[] bytesToSend = MessagePackSerializer.Serialize(tcpNetworkMessage);
        Task.WaitAll(TcpNetworkClient.GetStream().WriteAsync(bytesToSend,0, bytesToSend.Length));
    }
    #endregion
}
