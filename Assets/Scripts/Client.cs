using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;
    public string ip = "127.0.0.1"; //로컬 호스트
    public int port = 26950;
    public int myId = 0;
    public string userName;
    public TCP tcp;
    public UDP udp;
    private bool isConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.Log("Instanve already exists, destroying this obj");
            Destroy(this.gameObject);
        }
    }
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        isConnected = true;
        tcp.Connect();
    }
    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            //서버 쪽에서 클라이언트의 endPoint를 저장해두기 위해서 그냥 빈 패킷 한번 보내기.
            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                //서버에서 누가 보냈는지 알게 하려고 아이디를 맨 앞에 추가
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP : {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }
                HandleData(_data);
            }
            catch (Exception _ex)
            {
                Disconnect();
            }

        }
        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });
            }
        }
        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }
    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;

        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }
        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);
            if (!socket.Connected)
            {
                Debug.Log("Failed to Connect");
                return;
            }
            Debug.Log("ConnectedToServer");

            stream = socket.GetStream();

            receivedData = new Packet();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP : {_ex}");
            }
        }
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }
                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);
                receivedData.Reset(HandleData(_data));

                //TODO: _data 처리
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiveing TCP data : {_ex} ");
                Disconnect();
            }
        }
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;
            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {

                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });
                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }
            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    //나중에 이부분 Reflection 이용하도록 바꾸면 굳이 매번 패킷 만들때마다 일일이 딕셔네리 안써도 될듯.
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.Welcome, ClientHandle.Welcome},
            { (int)ServerPackets.SpawnPlayer, ClientHandle.SpawnPlayer},
            { (int)ServerPackets.PlayerPosition, ClientHandle.PlayerPosition},
            { (int)ServerPackets.PlayerRotation, ClientHandle.PlayerRotation},
            { (int)ServerPackets.DisconncetPlayer, ClientHandle.DisconncetPlayer},
            //{ (int)ServerPackets.UDPTest, ClientHandle.UDPTest}
        };
        Debug.Log("Initialize packets");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server");
        }
    }
}
