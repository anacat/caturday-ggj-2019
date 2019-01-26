using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkClient
{
    public TcpClient Client {get; set;}
    public NetworkStream NetworkStream { get; set; }
    public string ClientUuid { get; set; }
}
