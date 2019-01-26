using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkClient : ScriptableObject
{
    public TcpClient Client {get; set;}
}
