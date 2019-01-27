using MessagePack;
using System;
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
    [Key(3)]
    public List<Tuple<int, Vector3, Vector3 >> AssetList { get; set; }
}
