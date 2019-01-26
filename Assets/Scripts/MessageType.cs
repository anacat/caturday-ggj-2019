using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    None,
    ServerOn,
    Connected,
    ConnectionRefused,
    ConnectionAccepted,
    ConnectingAuthentication,
    MovementStarted,
    MovementNotAllowed,
    AssetsPositions,

    HelloWorld,
    JoinGame,
    HelloFromServer,
}
