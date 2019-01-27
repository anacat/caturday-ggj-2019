using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    None,
    ServerOn,
    Connected,
    Connecting,
    ConnectionRefused,
    ConnectionAccepted,
    ConnectingAuthentication,
    MovementStarted,
    MovementNotAllowed,
    AssetsPositions,
}
