using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.WelcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(Client.instance.userName);

            SendTCPData(_packet);
        }
    }

    public static void UDPTestReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.UDPTestReceived))
        {
            _packet.Write($"Message from '{Client.instance.userName}' via UDP : Received UDPTest Packet.");

            SendUDPData(_packet);
        }
    }

    public static void PlayerMovement(float horizontal, float vertical)
    {
        using (Packet _packet = new Packet((int)ClientPackets.PlayerMovement))
        {
            _packet.Write(horizontal);
            _packet.Write(vertical);
            _packet.Write(GameManager.playersDic[Client.instance.myId].transform.rotation);

            //패킷 손실이 발생할 수 있지만.. 플레이어 위치는 꾸준히 실시간으로 계속 보내고 있어서 괜찮다.
            //그니까 Tcp보다 빠른 udp로 선택 
            SendUDPData(_packet);
        }
    }
    #endregion
}
