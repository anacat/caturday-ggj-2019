using MessagePack;

[MessagePackObject]
public class BroadcastMessage
{
    [Key(0)]
    public MessageType MessageType { get; set; }
    [Key(1)]
    public string BroadcasterIpAddress { get; set; }
    [Key(2)]
    public string BroadcasterUuid { get; set; }
}
