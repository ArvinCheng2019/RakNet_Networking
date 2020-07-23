using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using static RakNet_Enums;
using static RakNet_Native;

/// <summary>
/// A universal peer for creating a connection from a client to a server or creating a server receiving a connection from clients...
/// 
/// [Warning! To explicitly close the connection on a busy port, it is recommended to call the Shutdown() method, otherwise you will not be able to use the specified port until the program is closed]
/// </summary>
public class RakNet_Peer
{

    public RakNet_Peer(IntPtr ptr)
    {
        this.ptr = ptr;
        guid = 0;
        address = string.Empty;
        port = 0;
        maxConnections = 0;
        state = PeerState.Unknown;
        runningTime = 0;
        receive_buffer = new byte[1];
        received_bytes = 0;
    }

    public static RakDebugLevel debugLevel = RakDebugLevel.None;
    public IntPtr ptr { get; private set; }
    public ulong guid { get; private set; }
    public string address { get; private set; }
    public int port { get; private set; }
    public int maxConnections { get; private set; }
    public PeerState state { get; private set; }
    public float runningTime;

    /// <summary>
    /// Create peer and start server
    /// </summary>
    /// <returns></returns>
    public static RakNet_Peer CreateServer(string address, int port, int maxConnections)
    {
        RakNet_Peer peer = new RakNet_Peer(NET_Create());

        if (NET_StartServer(peer.ptr, address, port, maxConnections) == 0)
        {
            peer.guid = ulong.MaxValue;
            peer.address = address;
            peer.port = port;
            peer.maxConnections = maxConnections;
            peer.state = PeerState.RunAsServer;
            peer.runningTime = 0;

            //set the bandwidth so that it is enough for all connections
            //peer.SetBandwidth(MAX_BANDWIDTH*(uint)maxConnections);
            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.Log("[Peer] ["+peer.state+"] Server started on port " + port + " (max connections: " + maxConnections + ")");
            }
            return peer;
        }
        string text = Marshal.PtrToStringAnsi(NET_LastStartupError(peer.ptr));
        if (debugLevel >= RakDebugLevel.Low)
        {
            Debug.LogError("[Peer] ["+ peer.state + "] Couldn't create server on port " + port + " (" + text + ")");
        }
        peer.state = PeerState.ServerInitFailed;
        peer.Shutdown();
        return null;
    }

    /// <summary>
    /// Create peer and connect to server
    /// </summary>
    public static RakNet_Peer Connect(string address, int port, int retries, int timeout)
    {
        IntPtr ptr = NET_Create();
        RakNet_Peer peer = new RakNet_Peer(ptr);

        int state = NET_StartClient(ptr, address, port, retries, 1400, timeout * 1000);

        if (state == 0)
        {
            peer.address = address;
            peer.port = port;
            peer.state = PeerState.RunAsClient;
            peer.runningTime = 0;

            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.Log("[Peer] ["+ peer.state + "] Connecting to " + address + ":" + port + " with " + retries + " retry count [Timeout: " + timeout + "]");
            }

            return peer;
        }

        string text = Marshal.PtrToStringAnsi(NET_LastStartupError(peer.ptr));
        if (debugLevel >= RakDebugLevel.Low)
        {
            Debug.LogWarning("[Peer] ["+ peer.state + "] Couldn't connect to server " + address + ":" + port + " (" + text + ")");
        }
        peer.state = PeerState.ClientInitFailed;
        peer.Shutdown();
        return null;
    }

    /// <summary>
    /// Shutdown this peer
    /// </summary>
    public void Shutdown()
    {
        if (ptr != IntPtr.Zero)
        {
            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.Log("[Peer] ["+state+"] Shutting down...");
            }
            state = PeerState.Unknown;
            NET_Close(ptr);
            ptr = IntPtr.Zero;
            Marshal.FreeHGlobal(ptr);
        }
    }

    public void SetBandwidth(uint bytesPerSecond)
    {
        if (ptr != IntPtr.Zero) 
        {
            NET_LimitBandwidth(ptr, bytesPerSecond);
        }
    }

    public int GetPingAverage(ulong guid)
    {
        return ptr != IntPtr.Zero ? NET_GetAveragePing(ptr, guid) : 0;
    }
    public int GetPingLast(ulong guid)
    {
        return ptr != IntPtr.Zero ? NET_GetLastPing(ptr, guid) : 0;
    }
    public int GetPingLowest(ulong guid)
    {
        return ptr != IntPtr.Zero ? NET_GetLowestPing(ptr, guid) : 0;
    }
    public int GetPingAverage() { return GetPingAverage(guid); }
    public int GetPingLast() { return GetPingAverage(guid); }
    public int GetPingLowest() { return GetPingAverage(guid); }

    public ulong incomingGUID() => ptr != IntPtr.Zero ? NETRCV_GUID(ptr) : 0;
    public uint incomingPort() => ptr != IntPtr.Zero ? NETRCV_Port(ptr) : 0;
    public int incomingBits() => ptr != IntPtr.Zero ? NETRCV_LengthBits(ptr) : 0;
    public int incomingBitsUnread() => ptr != IntPtr.Zero ? NETRCV_UnreadBits(ptr) : 0;
    public int incomingBytes => incomingBits() / 8;
    public int incomingBytesUnread => incomingBitsUnread() / 8;

    public string GetAddress(ulong guid)
    {
        if (ptr != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(NET_GetAddress(ptr, guid));
        }
        return string.Empty;
    }
    public RakNetStats GetNetStats()
    {
        RakNetStats stats = new RakNetStats();
        if (ptr != IntPtr.Zero && NET_GetStatistics(ptr, guid, ref stats))
        {
            return stats;
        }

        return stats;
    }
    public string GetNetStatsString()
    {
        string stats = "NetStats: -";
        if (ptr != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(NET_GetStatisticsString(ptr, guid));
        }

        return stats;
    }
    public RakNetStats GetNetStats(ulong guid)
    {
        RakNetStats stats = new RakNetStats();
        if (ptr != IntPtr.Zero && NET_GetStatistics(ptr, guid, ref stats))
        {
            return stats;
        }

        return stats;
    }

    public unsafe ulong ReceivedBytes(ulong guid)
    {
        RakNetStats stats = new RakNetStats();
        if (ptr != IntPtr.Zero && NET_GetStatistics(ptr, guid, ref stats))
        {
            return stats.runningTotal[(int)RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED];
        }

        return 0;
    }

    public unsafe ulong SendedBytes(ulong guid)
    {
        RakNetStats stats = new RakNetStats();
        if (ptr != IntPtr.Zero && NET_GetStatistics(ptr, guid, ref stats))
        {
            return stats.runningTotal[(int)RNSPerSecondMetrics.ACTUAL_BYTES_SENT];
        }

        return 0;
    }

    public string GetNetStatsString(ulong guid)
    {
        string stats = "NetStats: -";
        if (ptr != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(NET_GetStatisticsString(ptr, guid));
        }

        return stats;
    }
    public unsafe ulong GetNetStatLastSecond(RNSPerSecondMetrics metrics)
    {
        if(ptr != IntPtr.Zero)
        {
            RakNetStats stats = GetNetStats();
            ulong value = stats.valueOverLastSecond[(int)metrics];
            return value;
        }

        return 0;
    }
    public unsafe ulong GetNetStat(RNSPerSecondMetrics metrics)
    {
        if (ptr != IntPtr.Zero)
        {
            RakNetStats stats = GetNetStats();
            ulong value = stats.runningTotal[(int)metrics];
            return value;
        }

        return 0;
    }
    public void CloseConnection(ulong guid)
    {
        if (ptr != IntPtr.Zero && state == PeerState.RunAsClient)
        {
            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.LogWarning("[Peer] ["+state+"] Closing connections is not possible on the client!");
            }
            return;
        }
        if (ptr != IntPtr.Zero)
        {
            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.Log("[Peer] ["+state+"] Kicked " + guid + " (address: " + GetAddress(guid) + ")");
            }
            NET_CloseConnection(ptr, guid);
        }
        else
        {
            if(debugLevel >= RakDebugLevel.Low)
            {
                Debug.LogError("[Peer] ["+state+"] Is not active!");
            }
        }
    }
    public void CloseConnection(ulong guid,string text)
    {
        if (ptr != IntPtr.Zero && state == PeerState.RunAsClient)
        {
            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.LogWarning("[Peer] [" + state + "] Closing connections is not possible on the client!");
            }
            return;
        }
        if (ptr != IntPtr.Zero)
        {
            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.Log("[Peer] [" + state + "] Kicked " + guid + " (address: " + GetAddress(guid) + ")");
            }
            BeginWrite();
            Write((byte)134);
            WriteBytes(Encoding.UTF8.GetBytes(text));
            SendToClient(guid, PacketPriority.IMMEDIATE_PRIORITY, PacketReliability.RELIABLE, NetChannel.NET_EVENTS);
            NET_CloseConnection(ptr, guid);
        }
        else
        {
            if (debugLevel >= RakDebugLevel.Low)
            {
                Debug.LogError("[Peer] [" + state + "] Is not active!");
            }
        }
    }




    //IO PAGE
    private static byte[] receive_buffer;//reserve 1 byte for reading packet id
    public int received_bytes { get; private set; }
    private static readonly MemoryStream memoryStream = new MemoryStream();

    public void SendToServer(PacketPriority priority, PacketReliability reliability, NetChannel channel)
    {
        if (ptr != IntPtr.Zero)
            NETSND_Send(ptr, guid, (int)priority, (int)reliability, (int)channel);
    }

    public void SendToServer(PacketPriority priority, PacketReliability reliability)
    {
        if (ptr != IntPtr.Zero)
            NETSND_Send(ptr, guid, (int)priority, (int)reliability, (int)NetChannel.NET_EVENTS);
    }

    public void SendToServer(PacketPriority priority)
    {
        if (ptr != IntPtr.Zero)
            NETSND_Send(ptr, guid, (int)priority, (int)PacketReliability.RELIABLE, (int)NetChannel.NET_EVENTS);
    }

    public void SendToServer()
    {
        if (ptr != IntPtr.Zero)
            NETSND_Send(ptr, guid, (int)PacketPriority.IMMEDIATE_PRIORITY, (int)PacketReliability.RELIABLE, (int)NetChannel.NET_EVENTS);
    }

    public void SendToClient(ulong guid, PacketPriority priority, PacketReliability reliability, NetChannel channel)
    {
        if (ptr != IntPtr.Zero)
        {
            NETSND_Send(ptr, guid, (int)priority, (int)reliability, (int)channel);
        }
    }

    public void SendToClient(ulong guid, PacketPriority priority, PacketReliability reliability)
    {
        if (ptr != IntPtr.Zero)
            NETSND_Send(ptr, guid, (int)priority, (int)reliability, (int)NetChannel.NET_EVENTS);
    }

    public void SendToClient(ulong guid, PacketPriority priority)
    {
        if (ptr != IntPtr.Zero)
            NETSND_Send(ptr, guid, (int)priority, (int)PacketReliability.RELIABLE, (int)NetChannel.NET_EVENTS);
    }

    public void SendToClient(ulong guid)
    {
        if (ptr != IntPtr.Zero)
            NETSND_Send(ptr, guid, (int)PacketPriority.IMMEDIATE_PRIORITY, (int)PacketReliability.RELIABLE, (int)NetChannel.NET_EVENTS);
    }

    public unsafe void SendBytesToServer(byte[] bytes, PacketPriority priority, PacketReliability reliability, NetChannel channel)
    {
        if (ptr != IntPtr.Zero)
        {
            BeginWrite();
            WriteBytes(bytes);
            NETSND_Send(ptr, guid, (int)priority, (int)reliability, (int)channel);
            if (debugLevel == RakDebugLevel.Medium)
            {
                Debug.Log("[Peer] [" + state + "] Sending data [packet_id=" + bytes[0] + "] [sz=" + bytes.Length + "]");
            }
            if (debugLevel == RakDebugLevel.Full)
            {
                Debug.Log("[Peer] [" + state + "] Sending data to server [packet_id=" + (RakNetPacketID)bytes[0] + "] [sz=" + bytes.Length + "]");
            }
        }
    }

    public unsafe void SendBytesToServer(byte[] bytes, PacketPriority priority, PacketReliability reliability)
    {
        SendBytesToServer(bytes, priority, reliability, NetChannel.NET_EVENTS);
    }

    public unsafe void SendBytesToServer(byte[] bytes, PacketPriority priority)
    {
        SendBytesToServer(bytes, priority, PacketReliability.RELIABLE, NetChannel.NET_EVENTS);
    }

    public unsafe void SendBytesToServer(byte[] bytes)
    {
        SendBytesToServer(bytes, PacketPriority.IMMEDIATE_PRIORITY, PacketReliability.RELIABLE, NetChannel.NET_EVENTS);
    }

    public unsafe void SendBytesToClient(ulong guid, byte[] bytes, PacketPriority priority, PacketReliability reliability, NetChannel channel)
    {
        if (ptr != IntPtr.Zero)
        {
            BeginWrite();
            WriteBytes(bytes);
            NETSND_Send(ptr, guid, (int)priority, (int)reliability, (int)channel);
            if (debugLevel == RakDebugLevel.Medium)
            {
                Debug.Log("[Peer] [" + state + "] Sending data to client [packet_id=" + bytes[0] + "] [sz=" + bytes.Length + "]");
            }
            if (debugLevel == RakDebugLevel.Full)
            {
                Debug.Log("[Peer] [" + state + "] Sending data to client " + guid + " [packet_id=" + (RakNetPacketID)bytes[0] + "] [sz=" + bytes.Length + "]");
            }
        }
    }

    public unsafe void SendBytesToClient(ulong guid, byte[] bytes, PacketPriority priority, PacketReliability reliability)
    {
        SendBytesToClient(guid, bytes, priority, reliability, NetChannel.NET_EVENTS);
    }

    public unsafe void SendBytesToClient(ulong guid, byte[] bytes, PacketPriority priority)
    {
        SendBytesToClient(guid, bytes, priority, PacketReliability.RELIABLE, NetChannel.NET_EVENTS);
    }

    public unsafe void SendBytesToClient(ulong guid, byte[] bytes)
    {
        SendBytesToClient(guid, bytes, PacketPriority.IMMEDIATE_PRIORITY, PacketReliability.RELIABLE, NetChannel.NET_EVENTS);
    }

    /// <summary>
    /// Call before writing packet
    /// </summary>
    public void BeginWrite(byte packet_id)
    {
        if (ptr != IntPtr.Zero)
        {
            NETSND_Start(ptr);
            Write(packet_id);
        }
    }

    /// <summary>
    /// Call before writing packet
    /// </summary>
    public void BeginWrite()
    {
        if (ptr != IntPtr.Zero)
        {
            NETSND_Start(ptr);
        }
    }

    /// <summary>
    /// Write bytes
    /// </summary>
    public unsafe void WriteBytes(byte* data, int size)
    {
        if (ptr != IntPtr.Zero && size > 0 && data != null)
        {
            NETSND_WriteBytes(ptr, data, size);
        }
    }

    /// <summary>
    /// Write bytes
    /// </summary>
    public unsafe void WriteBytes(byte[] bytes)
    {
        fixed (byte* data = &((bytes != null && bytes.Length != 0) ? ref bytes[0] : ref *(byte*)null))
        {
            WriteBytes(data, bytes.Length);
        }
    }

    /// <summary>
    /// Write bytes array
    /// </summary>
    public void WriteBytesArray(byte[] bytes)
    {
        Write(bytes.Length);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Read bytes array
    /// </summary>
    /// <returns></returns>
    public byte[] ReadBytesArray()
    {
        byte[] arr = new byte[ReadInt()];

        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = ReadByte();
        }

        return arr;
    }

    byte[] sending_buffer = new byte[1];

    /// <summary>
    /// Write bool
    /// </summary>
    public unsafe void Write(bool value)
    {
        byte _t = (value ? (byte)1 : (byte)0);
        Write(_t);
    }

    /// <summary>
    /// Write byte
    /// </summary>
    public unsafe void Write(byte value)
    {
        fixed (byte* b = sending_buffer)
        {
            *((byte*)b) = value;
            WriteBytes(b, 1);
        }
    }

    /// <summary>
    /// Write sbyte
    /// </summary>
    public unsafe void Write(sbyte value)
    {
        Write((byte)value);
    }

    /// <summary>
    /// Write short
    /// </summary>
    public unsafe void Write(short value)
    {
        fixed (byte* b = sending_buffer)
        {
            *((short*)b) = value;
            WriteBytes(b, 2);
        }
    }

    /// <summary>
    /// Write ushort
    /// </summary>
    public unsafe void Write(ushort value)
    {
        Write((short)value);
    }

    /// <summary>
    /// Write int
    /// </summary>
    public unsafe void Write(int value)
    {
        fixed (byte* b = sending_buffer)
        {
            *((int*)b) = value;
            WriteBytes(b, 4);
        }
    }

    /// <summary>
    /// Write uint
    /// </summary>
    public unsafe void Write(uint value)
    {
        Write((int)value);
    }

    /// <summary>
    /// Write long
    /// </summary>
    public unsafe void Write(long value)
    {
        fixed (byte* b = sending_buffer)
        {
            *((long*)b) = value;
            WriteBytes(b, 8);
        }
    }

    /// <summary>
    /// Write ulong
    /// </summary>
    public unsafe void Write(ulong value)
    {
        Write((long)value);
    }

    /// <summary>
    /// Write float
    /// </summary>
    public unsafe void Write(float value)
    {
        UIntFloat conversion = new UIntFloat()
        {
            floatValue = value
        };
        Write(conversion.intValue);
    }


    /// <summary>
    /// Write double
    /// </summary>
    public unsafe void Write(double value)
    {
        UIntDouble converter = new UIntDouble
        {
            doubleValue = value
        };
        Write(converter.longValue);
    }

    /// <summary>
    /// Write char
    /// </summary>
    public unsafe void Write(char value)
    {
        Write((byte)value);
    }

    private unsafe byte* Read(int size, byte* data)
    {
        if (incomingBytesUnread < size)
            return (byte*)0;

        if (size > receive_buffer.Length)
        {
            if (debugLevel >= RakDebugLevel.Medium)
            {
                Debug.LogError("[Peer] Receive buffer overflow! Overriding receive buffer for " + size + " bytes");
            }
            receive_buffer = new byte[size];
        }

        if (NETRCV_ReadBytes(ptr, data, size))
            return data;

        if (debugLevel >= RakDebugLevel.Full)
        {
            Debug.Log("[Client] Buffer is empty!");
        }
        return null;
    }

    public bool ReadBool()
    {
        return ReadByte() == 1 ? true : false;
    }

    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    public unsafe long ReadLong()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return *(long*)Read(8, data);
        }
    }

    public unsafe int ReadInt()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return *(int*)Read(4, data);
        }
    }

    public unsafe short ReadShort()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return *(short*)Read(2, data);
        }
    }

    public unsafe ulong ReadULong()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return (ulong)*(long*)Read(8, data);
        }
    }

    public unsafe uint ReadUInt()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return *(uint*)Read(4, data);
        }
    }

    public unsafe ushort ReadUShort()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return *(ushort*)Read(2, data);
        }
    }

    public unsafe float ReadFloat()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return *(float*)Read(4, data);
        }
    }

    public unsafe float ReadFloat(bool compressed)
    {
        if (compressed)
            return NETSND_ReadCompressedFloat(ptr);
        fixed (byte* data = &receive_buffer[0])
        {
            return *(float*)Read(4, data);
        }
    }

    public unsafe double ReadDouble()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            return *(double*)Read(8, data);
        }
    }

    public unsafe byte ReadByte()
    {
        fixed (byte* data = &receive_buffer[0])
        {
            try
            {
                return *Read(1, data);
            }
            catch
            {
                if (debugLevel >= RakDebugLevel.Medium)
                {
                    Debug.LogError("[Peer] Can't read byte!");
                }
                return 0;
            }
        }
    }

    public unsafe string ReadString()
    {
        return Encoding.UTF8.GetString(ReadBytesArray());
    }

    public unsafe void Write(string value)
    {
        byte[] arr = Encoding.UTF8.GetBytes(value);
        WriteBytesArray(arr);
    }

    private unsafe byte[] ReadReceivedData()
    {
        received_bytes = incomingBytesUnread;

        if (received_bytes >= receive_buffer.Length)
        {
            if (debugLevel >= RakDebugLevel.Medium)
            {
                Debug.LogError("[Peer] Receive buffer overflow! Overriding receive buffer for " + received_bytes + " bytes");
            }
            receive_buffer = new byte[received_bytes];
        }

        if (ptr != IntPtr.Zero)
        {
            fixed (byte* data = receive_buffer)
            {
                if (!NETRCV_ReadBytes(ptr, data, received_bytes))
                {
                    if (debugLevel >= RakDebugLevel.Full)
                    {
                        Debug.Log("[Peer] Packet is empty!");
                    }
                    return receive_buffer;
                }
                else
                {
                    NETRCV_SetReadPointer(ptr, 8);//skip 8 bits (packet id)
                }
                return receive_buffer;
            }
        }

        return receive_buffer;
    }

    /// <summary>
    /// If true, then the network stream has unread data
    /// </summary>
    public bool HasReceived(out ArraySegment<byte> data)
    {
        bool result = false;
        if (ptr != IntPtr.Zero)
        {
            runningTime += Time.time;
            result = NET_Receive(ptr);
            if (result)
            {
                data = new ArraySegment<byte>(ReadReceivedData(),0, received_bytes);
                if (debugLevel >= RakDebugLevel.Medium)
                {
                    Debug.Log("[Peer] [" + state + "] Packet received from " + GetAddress(incomingGUID()) + " with packet=" + data.Array[0] + " sz=" + received_bytes + " bytes");
                }
                if (data.Array[0] == 16)
                {
                    if (guid == 0)
                    {
                        guid = incomingGUID();
                    }
                }

                if (debugLevel >= RakDebugLevel.Medium)
                {
                    if (data.Array[0] < 134)
                    {
                        if (!Enum.IsDefined(typeof(RakNetPacketID), (RakNetPacketID)data.Array[0]))
                        {
                            Debug.LogWarning("[Peer] [" + state + "] Undefined native packet id [" + data.Array[0] + "]!!!  ( Check RakNet_Enums )");
                        }
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// If true, then the network stream has unread data
    /// </summary>
    public bool HasReceived(out ulong guid, out ArraySegment<byte> data)
    {
        data = new ArraySegment<byte>();
        guid = 0;
        bool result = false;
        if (ptr != IntPtr.Zero)
        {
            runningTime += Time.time;
            result = NET_Receive(ptr);
            if (result)
            {
                guid = incomingGUID();
                data = new ArraySegment<byte>(ReadReceivedData());
                if (debugLevel >= RakDebugLevel.Medium)
                {
                    Debug.Log("[Peer] [" + state + "] Packet received from " + GetAddress(guid) + " with packet=" + data.Array[0] + " sz=" + data.Count + " bytes");
                }
                if (debugLevel >= RakDebugLevel.Medium)
                {
                    if (data.Array[0] < 134)
                    {
                        if (!Enum.IsDefined(typeof(RakNetPacketID), (RakNetPacketID)data.Array[0]))
                        {
                            Debug.LogWarning("[Peer] [" + state + "] Undefined native net packet id [" + data.Array[0] + "]!!!  ( Check id enums )");
                        }
                    }
                }
            }
        }

        return result;
    }
}
