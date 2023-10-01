using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class ClientHandle
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Server Message via TCP : {_msg}");
        Client.instance.myId = _myId;

        //서버에게 welcom 패킷 받았다고 알림.
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }
    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();
        Debug.Log($"Server Message via UDP : {_msg}");

        //서버에게 UDPTest 패킷 받았다고 알림.
        ClientSend.UDPTestReceived();
    }
}
