using MessagePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class TcpNetworkMessage
{
    [Key(0)]
    public MessageType MessageType { get; set; }
    [Key(1)]
    public string ClientUuid { get; set; }
    
}
