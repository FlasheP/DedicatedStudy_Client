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
    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _userName = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _userName, _position, _rotation);
    }
    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.playersDic[_id].transform.position = _position;
    }
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.playersDic[_id].transform.rotation = _rotation;
    }
    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();
        Debug.Log($"Server Message via UDP : {_msg}");

        //서버에게 UDPTest 패킷 받았다고 알림.
        ClientSend.UDPTestReceived();
    }
}
