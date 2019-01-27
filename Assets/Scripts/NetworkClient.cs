using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkClient
{
    public TcpClient TcpClient {get; set;}
    public NetworkStream NetworkStream { get; set; }
    public UdpClient UdpClient { get; set; }
    public string IpAddress { get; set; }
    public string ClientUuid { get; set; }
    public int CatId { get; set; }
}
