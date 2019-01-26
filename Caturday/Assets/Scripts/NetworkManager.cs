using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager
{
    private UdpClient _udpBroadcaster;
    private IPEndPoint _broadcastIpEndPoint;
    public NetworkManager()
    {
        InitializeBroadcast();
    }

    private void InitializeBroadcast()
    {
        _udpBroadcaster = new UdpClient();
        _broadcastIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 13947);

    }

    public void StartBroadcasting()
    {
        for (; ; )
        {
            //string message = "HELLO!";
            //Byte[] buffer = new Byte[1000];
            //buffer = ASCIIEncoding.ASCII.GetBytes(message);
            //_udpBroadcaster.Send(buffer, buffer.Length, _broadcastIpEndPoint);
        }
    }
}
