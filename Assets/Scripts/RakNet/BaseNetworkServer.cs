using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static RakNet_Enums;

public class ConnectionInfo
{
    public ulong guid;
    public string address;
    private int old_packets_received;
    public int packets_received;

    public int PacketsPerSec()
    {
        int diff = packets_received - old_packets_received;
        old_packets_received = packets_received;
        return diff;
    }

    public ConnectionInfo(ulong guid,string address)
    {
        this.guid = guid;
        this.address = address;
        this.old_packets_received = 0;
        this.packets_received = 0;
    }
}

//Subscribe!   My Youtube channel https://www.youtube.com/channel/UCPQ04Xpbbw2uGc1gsZtO3HQ

//ATTENTION! WRITTEN EVERYTHING FOR MYSELF! IF YOU DO NOT LIKE THE IMPLEMENTATION YOU CAN WRITE YOUR OWN


/// <summary>
/// Base network server class
/// </summary>
public class BaseNetworkServer : MonoBehaviour, ITickUpdate
{
    public RakNet_Peer server_peer;
    public QueryProtocol Query { get; private set; } = new QueryProtocol(QueryProtocolMode.Server);
    public bool IsStarted => server_peer != null ? server_peer.state == PeerState.RunAsServer : false;
    public string Address => server_peer != null ? server_peer.address : "-";
    public int Port => server_peer != null ? server_peer.port : -1;
    public List<ConnectionInfo> connections = new List<ConnectionInfo>();


    private void Awake()
    {
        BehaviourManager.RegisterBehaviour(this);
        OnInit();
    }

    public bool StartServer(string bind_to_address = "127.0.0.1", int port = 7777, int maxPlayers = 32)
    {
        OnPreServerInit();
        if (server_peer == null)
        {
            server_peer = RakNet_Peer.CreateServer(bind_to_address, port, maxPlayers);

            if (server_peer != null)
            {
                Query.info.maxPlayers = (byte)maxPlayers;
                Query.info.version = Application.version;
                try
                {
                    Query.Initialize(port + 1);
                }
                catch
                {
                    Debug.LogError("Query initialized failure! Please port " + (port + 1));
                }
                OnServerInitSuccess(bind_to_address, port);
                Debug.Log("Server started -> " + bind_to_address + ":" + port + "    Max Players: " + maxPlayers + "\nGame/Engine version -> " + Application.version + "/" + Application.unityVersion);
                return true;
            }
            else
            {
                OnServerInitFailed();
                return false;
            }
        }
        else
        {
            Debug.LogWarning("[Server] Is already running...");
            return false;
        }
    }

    public void StopServer()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            Kick(connections[i].guid, "Server shutting down!");
        }
        if (server_peer != null)
        {
            OnServerShutdown();
            server_peer.Shutdown();
            server_peer = null;
        }

        Query.Shutdown();
    }

    public void TickUpdate(float dt)
    {
        if (server_peer != null)
        {
            //while there is data in the buffer, read it
            while (server_peer != null && server_peer.HasReceived(out ulong guid, out ArraySegment<byte> data))
            {
                ProcessPacket(guid, data);
            }
        }
        ServerUpdate();
        if(_checkConnectionsTimer < Time.fixedTime)
        {
            _checkConnectionsTimer = Time.fixedTime + 1f;
            CheckConnections();
        }
    }

    float _checkConnectionsTimer = 0;

    public const int MAX_PACKETS_PER_SECOND = 100;

    void CheckConnections()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            int packets_per_sec = connections[i].PacketsPerSec();

            if(packets_per_sec >= MAX_PACKETS_PER_SECOND)
            {
                Kick(connections[i].guid, "Maximum packets exceeded!");
            }
        }
    }


    private void ProcessPacket(ulong guid, ArraySegment<byte> data)
    {
        bool default_id = data.Array[0] < 134;
        if (default_id)
        {
            switch ((RakNetPacketID)data.Array[0])
            {
                case RakNetPacketID.NEW_INCOMING_CONNECTION:
                    connections.Add(new ConnectionInfo(guid, server_peer.GetAddress(guid)));
                    OnConnected(guid);
                    break;

                case RakNetPacketID.DISCONNECTION_NOTIFICATION:
                    RemoveConnectionByGUID(guid, out ConnectionInfo guid1);
                    OnDisconnected(guid1.guid, DisconnectionType.ConnectionClosed);
                    break;

                case RakNetPacketID.CONNECTION_LOST:
                    RemoveConnectionByGUID(guid, out ConnectionInfo guid2);
                    OnDisconnected(guid2.guid, DisconnectionType.Timeout);
                    break;
            }
        }
        else
        {
            if (GetConnectionByGUID(guid, out int i, out ConnectionInfo connection))
            {
                OnReceivedData(connection.guid, data);
                connection.packets_received++;
            }
        }
    }

    public bool GetConnectionByGUID(ulong guid, out int index, out ConnectionInfo connection)
    {
        connection = null;
        index = -1;
        for (int i = 0; i < connections.Count; i++)
        {
            if (guid == connections[i].guid)
            {
                index = i;
                connection = connections[i];
                return true;
            }
        }
        return false;
    }

    private void RemoveConnectionByGUID(ulong guid, out ConnectionInfo connection)
    {
        if (GetConnectionByGUID(guid, out int i, out connection))
        {
            connections.RemoveAt(i);
        }
    }

    /// <summary>
    /// Send data to client (with packet_id && payload)
    /// </summary>
    public void SendData
        (
        byte packet_id,
        byte[] data,
        ulong guid = 0,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite(packet_id);
        server_peer?.WriteBytes(data);
        if (guid != 0)
        {
            server_peer?.SendToClient(guid, priority, reliability, channel);
        }
    }

    /// <summary>
    /// Send rpc to client
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        ulong guid,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.Write(packet_id);
        server_peer?.SendToClient(guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send rpc to client
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        ulong guid,
        byte data1,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.Write(packet_id);
        server_peer?.Write(data1);
        server_peer?.SendToClient(guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send rpc to client
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        ulong guid,
        short data1,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.Write(packet_id);
        server_peer?.Write(data1);
        server_peer?.SendToClient(guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send rpc to client
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        ulong guid,
        short data1,
        float data2,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.Write(packet_id);
        server_peer?.Write(data1);
        server_peer?.Write(data2);
        server_peer?.SendToClient(guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send rpc to client
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        ulong guid,
        uint data1,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.Write(packet_id);
        server_peer?.Write(data1);
        server_peer?.SendToClient(guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send rpc to client
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        ulong guid,
        short data1,
        byte data2,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.Write(packet_id);
        server_peer?.Write(data1);
        server_peer?.Write(data2);
        server_peer?.SendToClient(guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send rpc to client
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        ulong guid,
        uint data1,
        byte data2,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.Write(packet_id);
        server_peer?.Write(data1);
        server_peer?.Write(data2);
        server_peer?.SendToClient(guid, priority, reliability, channel);
    }


    /// <summary>
    /// Send RPC to all clients
    /// </summary>
    public void SendRPCToAll
        (
        byte packet_id,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {

        for (int i = 0; i < connections.Count; i++)
        {
            server_peer?.BeginWrite();
            server_peer?.Write(packet_id);
            server_peer?.SendToClient(connections[i].guid, priority, reliability, channel);
        }
    }

    /// <summary>
    /// Send RPC to all clients
    /// </summary>
    public void SendRPCToAll
        (
        byte packet_id,
        short data1,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {

        for (int i = 0; i < connections.Count; i++)
        {
            server_peer?.BeginWrite();
            server_peer?.Write(packet_id);
            server_peer?.Write(data1);
            server_peer?.SendToClient(connections[i].guid, priority, reliability, channel);
        }
    }

    /// <summary>
    /// Send data to client (with payload)
    /// </summary>
    public void SendData
        (
        byte[] data,
        ulong guid = 0,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        server_peer?.BeginWrite();
        server_peer?.WriteBytes(data);
        if (guid != 0)
        {
            server_peer?.SendToClient(guid, priority, reliability, channel);
        }
    }

    /// <summary>
    /// Send data to all clients (with payload)
    /// </summary>
    public void SendDataToAll
        (
        byte[] data,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        if (server_peer.state != PeerState.RunAsServer)
            return;

        for (int i = 0; i < connections.Count; i++)
        {
            server_peer?.BeginWrite();
            server_peer?.WriteBytes(data);
            server_peer?.SendToClient(connections[i].guid, priority, reliability, channel);
        }
    }

    /// <summary>
    /// Send packet to target
    /// </summary>
    public void SendPacket
        (
        RakNet_Packet packet,
        ulong guid,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        packet.SendToClient(guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send packet to all from list
    /// </summary>
    public void SendPacketToAllFromListJob
        (
        ulong[] list,
        RakNet_Packet packet,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        if (server_peer.state != PeerState.RunAsServer)
            return;


        if (!packet.begin_write || !packet.end_write)
        {
            Debug.LogError("Can't send packet! Check Begin & End packet");
            return;
        }

        _SendPacketToAllJob a = new _SendPacketToAllJob();
        a.peer = (ulong)server_peer.ptr;
        a.p = priority;
        a.r = reliability;
        a.c = channel;
        a.array = new NativeArray<ulong>(list, Allocator.TempJob);

        JobHandle jobHandle = a.Schedule(list.Length, 64);

        jobHandle.Complete();

        a.array.Dispose();
    }

    /// <summary>
    /// Send packet to all from list and ignore guid
    /// </summary>
    public void SendPacketToAllFromListJob
        (
        ulong ignore_guid,
        ulong[] list,
        RakNet_Packet packet,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        if (server_peer.state != PeerState.RunAsServer)
            return;


        if (!packet.begin_write || !packet.end_write)
        {
            Debug.LogError("Can't send packet! Check Begin & End packet");
            return;
        }

        _SendPacketToAllJob a = new _SendPacketToAllJob();
        a.peer = (ulong)server_peer.ptr;
        a.p = priority;
        a.r = reliability;
        a.c = channel;
        a.array = new NativeArray<ulong>(list, Allocator.TempJob);

        JobHandle jobHandle = a.Schedule(list.Length, 64);

        jobHandle.Complete();

        a.array.Dispose();
    }

    /// <summary>
    /// Send packet to all
    /// </summary>
    public void SendPacketToAll
        (
        RakNet_Packet packet,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        if (server_peer.state != PeerState.RunAsServer)
            return;


        if (!packet.begin_write || !packet.end_write)
        {
            Debug.LogError("Can't send packet! Check Begin & End packet");
            return;
        }


        for (int i = 0; i < connections.Count; i++)
        {
            server_peer.SendToClient(connections[i].guid, priority, reliability, channel);
        }
    }


    struct _SendPacketToAllJob : IJobParallelFor
    {
        public NativeArray<ulong> array;
        public ulong peer;
        public PacketPriority p;
        public PacketReliability r;
        public NetChannel c;

        public void Execute(int index)
        {
            if (array[index] != 0)
            {
                RakNet_Native.NETSND_Send((IntPtr)peer, array[index], (int)p, (int)r, (int)c);
            }
        }
    }


    public void Kick(ulong guid)
    {
        if (IsStarted)
        {
            server_peer?.CloseConnection(guid);
        }
    }
    public void Kick(ulong guid, string text)
    {
        if (IsStarted)
        {
            server_peer?.CloseConnection(guid, text);
        }
    }

    public string GetNetworkStats(ulong guid)
    {
        return server_peer != null ? server_peer.GetNetStatsString(guid) : "-";
    }

    private void OnDestroy()
    {
        BehaviourManager.UnregisterBehaviour(this);
        server_peer?.Shutdown();
    }

    private void OnApplicationQuit()
    {
        server_peer?.Shutdown();
    }

    public virtual void OnInit() { }
    public virtual void ServerUpdate() { }
    public virtual void OnPreServerInit() { }
    public virtual void OnServerInitSuccess(string address, int port) { }
    public Action event_OnServerInitSuccess;
    public virtual void OnServerInitFailed() { }
    public Action event_OnServerInitFailed;
    public virtual void OnServerShutdown() { }
    public virtual void OnConnected(ulong connection) { }
    public virtual void OnDisconnected(ulong connection, DisconnectionType type) { }
    public virtual void OnReceivedData(ulong connection, ArraySegment<byte> data) { }
}
