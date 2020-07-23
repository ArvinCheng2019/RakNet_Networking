using Native;
using System;
using System.Runtime.InteropServices;
using System.Security;
using static RakNet_Enums;

[SuppressUnmanagedCodeSecurity]
public class RakNet_Native
{
    [Serializable]
    public struct RakNetStats
    {
        /// <summary>
        /// ActualBytesPerSecondSent - [6]
        /// ActualBytesPerSecondReceived - [7]
        /// MessageBytesPerSecondSent - [2]
        /// MessageBytesPerSecondPushed - [1]
        /// MessageBytesPerSecondReturned - [4]
        /// </summary>
        public unsafe fixed ulong valueOverLastSecond[7];

        /// <summary>
        /// TotalBytesSent - [6]
        /// TotalBytesReceived - [7]
        /// TotalMessageBytesSent - [0]
        /// TotalMessageBytesPushed - [1]
        /// TotalMesageBytesReturned - [4]
        /// </summary>
        public unsafe fixed ulong runningTotal[7];

        public ulong connectionStartTime;

        public byte isLimitedByCongestionControl;

        public ulong BPSLimitByCongestionControl;

        public byte isLimitedByOutgoingBandwidthLimit;

        public ulong BPSLimitByOutgoingBandwidthLimit;

        public unsafe fixed uint messageInSendBuffer[4];

        public unsafe fixed double bytesInSendBuffer[4];

        public uint messagesInResendBuffer;

        public ulong bytesInResendBuffer;

        public float packetlossLastSecond;

        public float packetlossTotal;
    }

#if UNITY_64
    public const string DLL_NAME = "RakNet_x64";
#elif !UNITY_64
    public const string DLL_NAME = "RakNet_x86";
#endif

    [DllImport(DLL_NAME, EntryPoint = "NET_Create")]
    private static extern IntPtr _NET_Create();

    public static IntPtr NET_Create()
    {
        IntPtr ptr = IntPtr.Zero;

        try
        {
            ptr = _NET_Create();
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return ptr;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_Close")]
    private static extern void _NET_Close(IntPtr nw);

    public static void NET_Close(IntPtr ptr)
    {
        try
        {
            _NET_Close(ptr);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_StartClient")]
    private static extern int _NET_StartClient(IntPtr nw, string hostName, int port, int retries, int retryDelay, int timeout);

    public static int NET_StartClient(IntPtr nw, string hostName, int port, int retries, int retryDelay, int timeout)
    {
        int result = int.MinValue;
        try
        {
            result = _NET_StartClient(nw, hostName, port, retries, retryDelay, timeout);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_StartServer")]
    private static extern int _NET_StartServer(IntPtr nw, string ip, int port, int maxConnections);

    public static int NET_StartServer(IntPtr nw, string ip, int port, int maxConnections)
    {
        int result = int.MinValue;
        try
        {
            result = _NET_StartServer(nw, ip, port, maxConnections);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_LastStartupError")]
    private static extern IntPtr _NET_LastStartupError(IntPtr nw);

    public static IntPtr NET_LastStartupError(IntPtr nw)
    {
        IntPtr result = IntPtr.Zero;
        try
        {
            result = _NET_LastStartupError(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_Receive")]
    private static extern bool _NET_Receive(IntPtr nw);

    public static bool NET_Receive(IntPtr nw)
    {
        bool result = false;
        try
        {
            result = _NET_Receive(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_GUID")]
    private static extern ulong _NETRCV_GUID(IntPtr nw);

    public static ulong NETRCV_GUID(IntPtr nw)
    {
        ulong result = 0;
        try
        {
            result = _NETRCV_GUID(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_Address")]
    private static extern uint _NETRCV_Address(IntPtr nw);

    public static uint NETRCV_Address(IntPtr nw)
    {
        uint result = 0;
        try
        {
            result = _NETRCV_Address(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_Port")]
    private static extern uint _NETRCV_Port(IntPtr nw);

    public static uint NETRCV_Port(IntPtr nw)
    {
        uint result = 0;
        try
        {
            result = _NETRCV_Port(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_RawData")]
    private static extern IntPtr _NETRCV_RawData(IntPtr nw);

    public static IntPtr NETRCV_RawData(IntPtr nw)
    {
        IntPtr result = IntPtr.Zero;
        try
        {
            result = _NETRCV_RawData(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_LengthBits")]
    private static extern int _NETRCV_LengthBits(IntPtr nw);

    public static int NETRCV_LengthBits(IntPtr nw)
    {
        int result = 0;
        try
        {
            result = _NETRCV_LengthBits(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_UnreadBits")]
    private static extern int _NETRCV_UnreadBits(IntPtr nw);

    public static int NETRCV_UnreadBits(IntPtr nw)
    {
        int result = 0;
        try
        {
            result = _NETRCV_UnreadBits(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_ReadBytes")]
    private unsafe static extern bool _NETRCV_ReadBytes(IntPtr nw, byte* data, int length);

    public static unsafe bool NETRCV_ReadBytes(IntPtr nw, byte* data, int length)
    {
        bool result = false;
        try
        {
            result = _NETRCV_ReadBytes(nw, data, length);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETRCV_SetReadPointer")]
    private static extern void _NETRCV_SetReadPointer(IntPtr nw, int bitsOffset);

    public static void NETRCV_SetReadPointer(IntPtr nw, int bitsOffset)
    {
        try
        {
            _NETRCV_SetReadPointer(nw, bitsOffset);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_Start")]
    private static extern void _NETSND_Start(IntPtr nw);

    public static void NETSND_Start(IntPtr nw)
    {
        try
        {
            _NETSND_Start(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_WriteBytes")]
    private unsafe static extern void _NETSND_WriteBytes(IntPtr nw, byte* data, int length);

    public unsafe static void NETSND_WriteBytes(IntPtr nw, byte* data, int length)
    {
        try
        {
            _NETSND_WriteBytes(nw, data, length);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_Size")]
    private static extern uint _NETSND_Size(IntPtr nw);

    public unsafe static uint NETSND_Size(IntPtr nw)
    {
        uint result = 0;
        try
        {
            result = _NETSND_Size(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_Broadcast")]
    private static extern uint _NETSND_Broadcast(IntPtr nw, int priority, int reliability, int channel);

    public static uint NETSND_Broadcast(IntPtr nw, int priority, int reliability, int channel)
    {
        uint result = 0;
        try
        {
            result = _NETSND_Broadcast(nw, priority, reliability, channel);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_Send")]
    private static extern uint _NETSND_Send(IntPtr nw, ulong connectionID, int priority, int reliability, int channel);

    public static uint NETSND_Send(IntPtr nw, ulong connectionID, int priority, int reliability, int channel)
    {
        uint result = 0;
        try
        {
            result = _NETSND_Send(nw, connectionID, priority, reliability, channel);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_CloseConnection")]
    private static extern void _NET_CloseConnection(IntPtr nw, ulong connectionID);

    public static void NET_CloseConnection(IntPtr nw, ulong connectionID)
    {
        try
        {
            _NET_CloseConnection(nw, connectionID);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_GetAddress")]
    private static extern IntPtr _NET_GetAddress(IntPtr nw, ulong connectionID);

    public static IntPtr NET_GetAddress(IntPtr nw, ulong connectionID)
    {
        IntPtr result = IntPtr.Zero;
        try
        {
            result = _NET_GetAddress(nw, connectionID);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_GetStatisticsString")]
    private static extern IntPtr _NET_GetStatisticsString(IntPtr nw, ulong connectionID);

    public static IntPtr NET_GetStatisticsString(IntPtr nw, ulong connectionID)
    {
        IntPtr result = IntPtr.Zero;
        try
        {
            result = _NET_GetStatisticsString(nw, connectionID);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_GetStatistics")]
    private static extern bool _NET_GetStatistics(IntPtr nw, ulong connectionID, ref RakNetStats data, int dataLength);

    public static bool NET_GetStatistics(IntPtr nw, ulong connectionID, ref RakNetStats data)
    {
        bool result = false;
        try
        {
            result = _NET_GetStatistics(nw, connectionID, ref data, Marshal.SizeOf(data));
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_GetAveragePing")]
    private static extern int _NET_GetAveragePing(IntPtr nw, ulong connectionID);

    public static int NET_GetAveragePing(IntPtr nw, ulong connectionID)
    {
        int result = 0;
        try
        {
            result = _NET_GetAveragePing(nw, connectionID);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME+ " (NET_GetAveragePing)", MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_GetLastPing")]
    private static extern int _NET_GetLastPing(IntPtr nw, ulong connectionID);

    public static int NET_GetLastPing(IntPtr nw, ulong connectionID)
    {
        int result = 0;
        try
        {
            result = _NET_GetLastPing(nw, connectionID);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME + " (NET_GetLastPing)", MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_GetLowestPing")]
    private static extern int _NET_GetLowestPing(IntPtr nw, ulong connectionID);

    public static int NET_GetLowestPing(IntPtr nw, ulong connectionID)
    {
        int result = 0;
        try
        {
            result = _NET_GetLowestPing(nw, connectionID);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME + " (NET_GetLowestPing)", MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_SendMessage")]
    private unsafe static extern void _NET_SendMessage(IntPtr nw, byte* data, int length, uint adr, ushort port);

    public unsafe static void NET_SendMessage(IntPtr nw, byte* data, int length, uint adr, ushort port)
    {
        try
        {
            _NET_SendMessage(nw, data, length, adr, port);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_WriteCompressedInt32")]
    private static extern void _NETSND_WriteCompressedInt32(IntPtr nw, int val);

    public unsafe static void NETSND_WriteCompressedInt32(IntPtr nw, int val)
    {
        try
        {
            _NETSND_WriteCompressedInt32(nw, val);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_WriteCompressedInt64")]
    private static extern void _NETSND_WriteCompressedInt64(IntPtr nw, long val);

    public unsafe static void NETSND_WriteCompressedInt64(IntPtr nw, long val)
    {
        try
        {
            _NETSND_WriteCompressedInt64(nw, val);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_WriteCompressedFloat")]
    private static extern void _NETSND_WriteCompressedFloat(IntPtr nw, float val);

    public unsafe static void NETSND_WriteCompressedFloat(IntPtr nw, float val)
    {
        try
        {
            _NETSND_WriteCompressedFloat(nw, val);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_ReadCompressedInt32")]
    private static extern int _NETSND_ReadCompressedInt32(IntPtr nw);

    public unsafe static int NETSND_ReadCompressedInt32(IntPtr nw)
    {
        int result = 0;
        try
        {
            result = _NETSND_ReadCompressedInt32(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_ReadCompressedInt64")]
    private static extern long _NETSND_ReadCompressedInt64(IntPtr nw);

    public unsafe static long NETSND_ReadCompressedInt64(IntPtr nw, long val)
    {
        long result = 0;
        try
        {
            result = _NETSND_ReadCompressedInt64(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NETSND_ReadCompressedFloat")]
    private static extern float _NETSND_ReadCompressedFloat(IntPtr nw);

    public unsafe static float NETSND_ReadCompressedFloat(IntPtr nw)
    {
        float result = 0;
        try
        {
            result = _NETSND_ReadCompressedFloat(nw);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }

        return result;
    }

    [DllImport(DLL_NAME, EntryPoint = "NET_LimitBandwidth")]
    private static extern void _NET_LimitBandwidth(IntPtr nw, uint maxBitsPerSecond);

    public unsafe static void NET_LimitBandwidth(IntPtr nw, uint maxBytesPerSecond)
    {
        try
        {
            _NET_LimitBandwidth(nw, maxBytesPerSecond*8);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }

    public unsafe static void NET_LimitBandwidth(IntPtr nw, int maxBytesPerSecond)
    {
        try
        {
            _NET_LimitBandwidth(nw, (uint)maxBytesPerSecond / 8);
        }
        catch
        {
            MessageBox.Show("Fatal Error!", "Native error in " + DLL_NAME, MessageBox.Type.ICON_ERROR);
        }
    }
}
