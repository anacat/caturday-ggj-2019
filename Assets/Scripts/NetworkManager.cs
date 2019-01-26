using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private UdpClient _udpBroadcaster;
    private IPEndPoint _broadcastIpEndPoint;
    private bool _isBroadcasting;
    private NetworkMessage _broadcastMessage;
    private byte[] _broadcastMessageBytes;
    private float _broadcastSleepTime;
    private int _broadcastPort;

    public void InitializeBroadcast()
    {
        _broadcastSleepTime = 1f;
        _broadcastPort = 13947;
        _broadcastMessage = new NetworkMessage()
        {
            BroadcasterIpAddress = GetLocalIPAddress(),
            BroadcasterUuid = Guid.NewGuid().ToString(),
            MessageType = MessageType.HelloWorld
        };

        _broadcastMessageBytes = MessagePackSerializer.Serialize(_broadcastMessage);
        _udpBroadcaster = new UdpClient();
        _broadcastIpEndPoint = new IPEndPoint(IPAddress.Broadcast, _broadcastPort);
        StartCoroutine(StartBroadcasting());
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
        return localIP;
    }

    private IEnumerator StartBroadcasting()
    {
        for (; ; )
        {
            if (_isBroadcasting)
            {
                _udpBroadcaster.Send(_broadcastMessageBytes, _broadcastMessageBytes.Length, _broadcastIpEndPoint);
            }
            yield return new WaitForSeconds(_broadcastSleepTime);
        }
    }

    public void SetBroadcasting(bool value)
    {
        _isBroadcasting = value;
    }
}
