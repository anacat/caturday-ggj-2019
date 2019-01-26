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
using System.Threading;

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
    public NetworkStream TcpNetworkClientStream;
    private Guid _ownGuid;
    private bool _tcpClientIsRunning;
    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    private void Awake()
    {
        _ownGuid = Guid.NewGuid();
        _broadcastPort = 13947;
        _tcpPort = 13948;
        _broadcastList = new List<NetworkServer>();
        NetworkClientList = new List<NetworkClient>();
    }

    public virtual void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
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
        Thread t = new Thread(InitializeBroadcastServer);
        t.Start();
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

    private void InitializeBroadcastServer()
    {
        while (_isBroadcasting)
        {
            _udpBroadcaster.Send(_broadcastMessageBytes, _broadcastMessageBytes.Length, _broadcastIpEndPoint);
            Thread.Sleep(1000);
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
        Thread t = new Thread(InitializeBroadcastClient);
        t.Start();
    }

    public void StopBroadcastClient()
    {
        foreach (NetworkServer n in _broadcastList)
        {
            ExecuteOnMainThread.Enqueue(() => { StartCoroutine(DestroyButtom(n.InstantiatedButton)); });
        }
        _broadcastList.Clear();
        _isFindingServers = false;
    }

    private IEnumerator DestroyButtom(GameObject button)
    {
        Destroy(button);
        yield return null;
    }

    private void InitializeBroadcastClient()
    {
        while (_isFindingServers)
        {
            if(_udpBroadcastClient.Available > 0)
            {
                byte[] data = _udpBroadcastClient.Receive(ref _serverBroadcastEndPoint);
                BroadcastMessage networkMessage = MessagePackSerializer.Deserialize<BroadcastMessage>(data);
                if (_broadcastList.FirstOrDefault(c => c.NetworkMessage.BroadcasterUuid == networkMessage.BroadcasterUuid) == null)
                {
                    ExecuteOnMainThread.Enqueue(() => { StartCoroutine(AddItem(networkMessage)); });
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
            Thread.Sleep(1000);
        }
    }

    private IEnumerator AddItem(BroadcastMessage networkMessage)
    {
        NetworkServer networkServer = new NetworkServer();
        networkServer.NetworkMessage = networkMessage;
        networkServer.InstantiatedButtonStartTime = DateTime.Now;
        networkServer.InstantiatedButton = Instantiate(MenuItem, ParentMenu.transform);
        networkServer.InstantiatedButton.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.MenuManager.JoinIpGame(networkMessage));
        networkServer.InstantiatedButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 180 - (90 * _broadcastList.Count - 10), 0);

        _broadcastList.Add(networkServer);
        yield return null;
    }
    #endregion

    #region Tcp Server
    public void StartTcpServer()
    {
        _tcpServerIsRunning = true;
        _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, _tcpPort));
        _tcpListener.Start(50);
        Thread t1 = new Thread(InitializeTcpServer);
        t1.Start();
        Thread t2 = new Thread(ReceiveTcpClientMessage);
        t2.Start();
    }

    public void StopTcpServer()
    {
        _tcpServerIsRunning = false;
    }

    private async void InitializeTcpServer()
    {
        while (_tcpServerIsRunning)
        {
            if (_tcpListener.Pending())
            {
                TcpClient tcpClientTask = await _tcpListener.AcceptTcpClientAsync();
                NetworkClient networkClient = new NetworkClient();

                networkClient.Client = tcpClientTask;
                networkClient.NetworkStream = tcpClientTask.GetStream();

                NetworkClientList.Add(networkClient);
                TcpNetworkMessage tcpNetworkMessage = new TcpNetworkMessage()
                {
                    ClientUuid = _ownGuid.ToString(),
                    MessageType = MessageType.HelloFromServer
                };
                Thread t = new Thread(new ParameterizedThreadStart(SendTcpClientMessage));
                object[] objectToSend = { tcpNetworkMessage, networkClient.NetworkStream };
                t.Start(objectToSend);
            }
            Thread.Sleep(1000);
        }
    }

    private void ReceiveTcpClientMessage()
    {
        ProcessReceiveTcpClientMessage();
    }

    private async void ProcessReceiveTcpClientMessage()
    {
        while (_tcpServerIsRunning)
        {
            foreach (NetworkClient c in NetworkClientList)
            {
                if (c.Client.Available > 0)
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[0x1000];
                    do
                    {
                        int readBytes = await c.NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, readBytes);
                    }
                    while (c.NetworkStream.DataAvailable);

                    TcpNetworkMessage tcpNetworkMessage = MessagePackSerializer.Deserialize<TcpNetworkMessage>(ms.ToArray());
                    GameManager.Instance.NetworkMessageManager.ProcessTcpNetworkMessage(tcpNetworkMessage, c.Client);
                }
            }
            Thread.Sleep(100);
        }
    }

    public void SendTcpClientMessage(object value)
    {
        TcpNetworkMessage tcpNetworkMessage = ((object[])value)[0] as TcpNetworkMessage;
        NetworkStream networkStream = ((object[])value)[1] as NetworkStream;
        byte[] bytesToSend = MessagePackSerializer.Serialize(tcpNetworkMessage);
        networkStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
    }



    public async Task SendTcpClientMessageToAll(TcpNetworkMessage tcpNetworkMessage)
    {
        for(int x = 0; x < NetworkClientList.Count; x++)
        {
            byte[] bytesToSend = MessagePackSerializer.Serialize(tcpNetworkMessage);
            await NetworkClientList[x].NetworkStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
        }
    }
    #endregion

    #region Tcp Client
    public void ConnectToTcpServer(BroadcastMessage broadcastMessage)
    {
        TcpNetworkClient = new TcpClient();
        Thread t = new Thread(new ParameterizedThreadStart(InitializeTcpClient));
        t.Start(broadcastMessage.BroadcasterIpAddress);
    }

    public void ConnectToTcpServer()
    {
        TcpNetworkClient = new TcpClient();
        Thread t = new Thread(new ParameterizedThreadStart(InitializeTcpClient));
        t.Start(TargetIpAddress.GetComponent<InputField>().text);
    }

    private void InitializeTcpClient(object ipAddress)
    {
        _tcpClientIsRunning = true;
        ProcessInitializeTcpClient(ipAddress.ToString());
    }

    private async void ProcessInitializeTcpClient(string ipAddress)
    {
        await TcpNetworkClient.ConnectAsync(IPAddress.Parse(ipAddress), _tcpPort);
        if (TcpNetworkClient.Connected)
        {
            TcpNetworkMessage tcpNetworkMessage = new TcpNetworkMessage()
            {
                ClientUuid = _ownGuid.ToString(),
                MessageType = MessageType.JoinGame
            };
            TcpNetworkClientStream = TcpNetworkClient.GetStream();
            Thread t1 = new Thread(ReceiveTcpServerMessage);
            t1.Start();
            Thread t2 = new Thread(new ParameterizedThreadStart(SendTcpServerMessage));
            t2.Start(tcpNetworkMessage);
        }
        else
        {
            _tcpClientIsRunning = false;
        }
    }

    public void SendTcpServerMessage(object tcpNetworkMessage)
    {
        ProcessSendTcpServerMessage(tcpNetworkMessage as TcpNetworkMessage);
    }

    private async void ProcessSendTcpServerMessage(TcpNetworkMessage tcpNetworkMessage)
    {
        byte[] bytesToSend = MessagePackSerializer.Serialize(tcpNetworkMessage);
        await TcpNetworkClientStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
    }

    public void ReceiveTcpServerMessage()
    {
        ProcessReceiveTcpServerMessages();
    }

    private async void ProcessReceiveTcpServerMessages()
    {
        while (_tcpClientIsRunning)
        {
            if (TcpNetworkClient.Available > 0)
            {
                MemoryStream ms = new MemoryStream();
                byte[] buffer = new byte[0x1000];
                do
                {
                    int readBytes = await TcpNetworkClientStream.ReadAsync(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readBytes);
                }
                while (TcpNetworkClientStream.DataAvailable);

                TcpNetworkMessage tcpNetworkMessage = MessagePackSerializer.Deserialize<TcpNetworkMessage>(ms.ToArray());
                GameManager.Instance.NetworkMessageManager.ProcessTcpNetworkMessage(tcpNetworkMessage, TcpNetworkClient);
            }
            Thread.Sleep(100);
        }
    }

    #endregion

    #region Udp Server
    public IEnumerator SendUdpMessage()
    {
        yield return null;
    }
    #endregion

    #region Udp Client

    #endregion
}
