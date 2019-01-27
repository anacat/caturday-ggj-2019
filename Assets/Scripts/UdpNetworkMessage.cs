using MessagePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class UdpNetworkMessage
{
    [Key(0)]
    public MessageType MessageType { get; set; }
}
