using System;
using System.Text;
using UnityEngine;
using static RakNet_Enums;

//Subscribe!   My Youtube channel https://www.youtube.com/channel/UCPQ04Xpbbw2uGc1gsZtO3HQ

//ATTENTION! WRITTEN EVERYTHING FOR MYSELF! IF YOU DO NOT LIKE THE IMPLEMENTATION YOU CAN WRITE YOUR OWN


/// <summary>
/// Base network client class
/// </summary>
public class BaseNetworkClient : MonoBehaviour,ITickUpdate
{
    public RakNet_Peer client_peer;
    public QueryProtocol Query { get; private set; } = new QueryProtocol(QueryProtocolMode.Client);
    public bool IsConnected { get; private set; } = false;
    public bool IsConnecting { get; private set; } = false;
    public int Timeout { get; private set; } = 999;
    public string Address => client_peer != null ? client_peer.address : "-";
    public int Port => client_peer != null ? client_peer.port : -1;
    private string disconnect_reason = string.Empty;

    private void Awake()
    {
        BehaviourManager.RegisterBehaviour(this);
        Query.Initialize();
        OnInit();
    }

    public void RequestQuery(string address,int port)
    {
        Query.RequestQuery(address, port+1);
    }

    public void Connect(string address = "127.0.0.1", int port = 7777, int retries = 10, int timeout = 15)
    {
        if (client_peer == null)
        {
            Timeout = timeout;
            disconnect_reason = string.Empty;
            client_peer = RakNet_Peer.Connect(address, port, retries, timeout);
            IsConnecting = true;
            OnConnecting(address, port);
            OnConnectingEvent?.Invoke(address, port);
        }
        else
        {
            Debug.LogWarning("[Client] Is already running...");
        }
    }

    public void Disconnect(DisconnectionType disconnectionType = DisconnectionType.Unknown)
    {
        if (client_peer != null)
        {
            client_peer.Shutdown();
            IsConnected = IsConnecting = false;
            if (disconnectionType != DisconnectionType.Unknown)
            {
                OnDisconnected(client_peer.address, client_peer.port, disconnectionType, disconnect_reason);
                OnDisconnectedEvent?.Invoke(client_peer.address, client_peer.port, disconnectionType, disconnect_reason);
            }
            else if (disconnectionType == DisconnectionType.Unknown)
            {
                OnDisconnected(client_peer.address, client_peer.port, DisconnectionType.ByUser, string.Empty);
                OnDisconnectedEvent?.Invoke(client_peer.address, client_peer.port, DisconnectionType.ByUser, string.Empty);
            }
            if (RakNet_Peer.debugLevel >= RakDebugLevel.Low)
            {
                Debug.Log("[Client] Disconnected... (" + disconnectionType + ")");
            }
            client_peer = null;
        }
    }

    public void TickUpdate(float dt)
    {
        if (client_peer != null)
        {
            //while there is data in the buffer, read it
            while (client_peer != null && client_peer.HasReceived(out ArraySegment<byte> data))
            {
                ProcessPacket(data);
            }
        }
        ClientUpdate();
        Query?.Update();
    }

    private void ProcessPacket(ArraySegment<byte> data)
    {
        bool default_id = data.Array[0] < 134;

        if(data.Array[0] == 134)
        {
            string text = Encoding.UTF8.GetString(data.Array, 1, data.Count - 1);
            if (RakNet_Peer.debugLevel >= RakDebugLevel.Low)
            {
                Debug.Log("[Client] Disconnect reason " + (text.Length > 0 ? text : "empty"));
            }
            disconnect_reason = text;
            return;
        }

        if (default_id)
        {
            switch ((RakNetPacketID)data.Array[0])
            {
                case RakNetPacketID.CONNECTION_REQUEST_ACCEPTED:
                    IsConnecting = false;
                    IsConnected = true;
                    Debug.Log("[Client] Connected to " + client_peer.address + ":" + client_peer.port);
                    OnConnected(client_peer.address, client_peer.port);
                    OnConnectedEvent?.Invoke(client_peer.address, client_peer.port);
                    break;

                case RakNetPacketID.NO_FREE_INCOMING_CONNECTIONS:
                    Disconnect(DisconnectionType.ServerIsFull);
                    break;

                case RakNetPacketID.CONNECTION_ATTEMPT_FAILED:
                    Disconnect(DisconnectionType.Timeout);
                    break;

                case RakNetPacketID.CONNECTION_LOST:
                    Disconnect(DisconnectionType.ConnectionLost);
                    break;

                case RakNetPacketID.DISCONNECTION_NOTIFICATION:
                    Disconnect(DisconnectionType.ConnectionClosed);
                    break;
            }
        }
        else
        {
            OnReceivedData(data);
        }
    }

    /// <summary>
    /// Send data to server
    /// </summary>
    public void SendData
        (
        byte packet_id,
        byte[] data,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        if (IsConnected)
        {
            client_peer?.BeginWrite(packet_id);
            client_peer?.WriteBytes(data);
            client_peer?.SendToServer(priority, reliability, channel);
        }
        else
        {
            if (RakNet_Peer.debugLevel >= RakDebugLevel.Low)
            {
                Debug.LogError("[Client] Is not connected!");
            }
        }
    }

    /// <summary>
    /// Send rpc to server
    /// </summary>
    public void SendRPC
        (
        byte packet_id,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        if (IsConnected)
        {
            client_peer?.BeginWrite();
            client_peer?.Write(packet_id);
            client_peer?.SendToServer(priority, reliability, channel);
        }
        else
        {
            if (RakNet_Peer.debugLevel >= RakDebugLevel.Low)
            {
                Debug.LogError("[Client] Is not connected!");
            }
        }
    }

    /// <summary>
    /// Send data to server
    /// </summary>
    public void SendData
        (
        byte[] data,
        PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY,
        PacketReliability reliability = PacketReliability.RELIABLE,
        NetChannel channel = NetChannel.NET_EVENTS
        )
    {
        if (IsConnected)
        {
            client_peer?.BeginWrite();
            client_peer?.WriteBytes(data);
            client_peer?.SendToServer(priority, reliability, channel);
        }
        else
        {
            if (RakNet_Peer.debugLevel >= RakDebugLevel.Low)
            {
                Debug.LogError("[Client] Is not connected!");
            }
        }
    }

    public string GetNetworkStats()
    {
        return client_peer != null ? client_peer.GetNetStatsString() : "-";
    }
    public int GetPingAverage() { return client_peer != null ? client_peer.GetPingAverage() : 0; }
    public int GetPingLast() { return client_peer != null ? client_peer.GetPingAverage() : 0; }
    public int GetPingLowest() { return client_peer != null ? client_peer.GetPingAverage() : 0; }
    public ulong GetNetStat(RNSPerSecondMetrics metrics) { return client_peer != null ? client_peer.GetNetStat(metrics) : 0; }
    public ulong GetNetStatLastSecond(RNSPerSecondMetrics metrics) { return client_peer != null ? client_peer.GetNetStatLastSecond(metrics) : 0; }
    public float GetNetStatOut() { return client_peer != null ? client_peer.GetNetStat(RNSPerSecondMetrics.ACTUAL_BYTES_SENT) : 0; }
	public float GetNetStatIn() { return client_peer != null ? client_peer.GetNetStat(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED) : 0; }
    public float GetNetStatOutPerSec() { return client_peer != null ? client_peer.GetNetStatLastSecond(RNSPerSecondMetrics.ACTUAL_BYTES_SENT) : 0; }
    public float GetNetStatInPerSec() { return client_peer != null ? client_peer.GetNetStatLastSecond(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED) : 0; }
    public float GetLoss() { return client_peer != null ? Mathf.Clamp(client_peer.GetNetStats().packetlossLastSecond * 1000f,0,100) : 0; }
    private void OnDestroy(){ BehaviourManager.UnregisterBehaviour(this); client_peer?.Shutdown(); }


    public virtual void OnInit() { }
    public virtual void ClientUpdate() { }
    /// <summary>
    /// Event on connecting (address,port)
    /// </summary>
    public Action<string, int> OnConnectingEvent;
    public virtual void OnConnecting(string address, int port) { }
    public virtual void OnConnected(string address, int port) { }
    /// <summary>
    /// Event on connected (address,port)
    /// </summary>
    public Action<string, int> OnConnectedEvent;
    public virtual void OnDisconnected(string address, int port, DisconnectionType type, string reason) { }
    /// <summary>
    /// Event on disconnected (address,port,disconnectiontype,reason)
    /// </summary>
    public Action<string, int, DisconnectionType, string> OnDisconnectedEvent;
    public virtual void OnReceivedData(ArraySegment<byte> data) { }
}
