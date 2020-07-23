using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public enum QueryProtocolMode
{
    Client,
    Server,
    Unknown
}

[Serializable]
public class QueryProtocol
{
    public static bool QueryDebug = false;
    public QueryProtocolMode mode = QueryProtocolMode.Unknown;
    public QueryProtocol(QueryProtocolMode mode)
    {
        this.mode = mode;
    }

    public bool Initialized { get; private set; } = false;

    public void Initialize()
    {
        Initialize(-1);
    }

    public void Initialize(int port)
    {
        if (!Initialized)
        {
            if (mode == QueryProtocolMode.Client)
            {
                udp = new UdpClient();
                if (QueryDebug)
                {
                    Debug.Log("[Query] Initialized...");
                }
                Initialized = true;
                thread = new Thread(Loop);
                thread.IsBackground = true;
                thread.Start();
            }
            else if (mode == QueryProtocolMode.Server)
            {
                if (port != -1)
                {
                    udp = new UdpClient(port);
                    if (QueryDebug)
                    {
                        Debug.Log("[Query] Initialized on port "+port);
                    }
                }
                else
                {
                    udp = new UdpClient(27020);
                    if (QueryDebug)
                    {
                        Debug.Log("[Query] Initialized on port 27020");
                    }
                }
                Initialized = true;
                thread = new Thread(Loop);
                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
    private Thread thread = null;
    private UdpClient udp = null;
    private bool requested = true;
    private bool _request_event = false;
    public Action<QueryInfo> OnReceivedQuery;
    public Action OnQueringFailed;
    private float _query_timer = 0;
    public QueryInfo info { get; private set; } = new QueryInfo();
    float time_at_last_request = 0;
    /// <summary>
    /// Requesting query from address:port
    /// </summary>
    public void RequestQuery(string address, int port)
    {
        if (mode == QueryProtocolMode.Client)
        {
            try
            {
                if (udp != null)
                {
                    byte[] data = new byte[13] { 0x51, 0x75, 0x65, 0x72, 0x79, 0x50, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C };//-QueryProtocol
                    udp.Send(data, data.Length, address, port);
                    time_at_last_request = Time.realtimeSinceStartup;
                    if (QueryDebug)
                    {
                        Debug.Log("[Query] Querying server information from "+address+":"+port);
                    }
                    _query_timer = 5;
                    requested = false;
                }
            }
            catch { }
        }
        else
        {
            Debug.LogWarning("[Query] Requesting query available only on client");
        }
    }

    private void SendQuery(IPEndPoint point)
    {
        if (mode == QueryProtocolMode.Server)
        {
            try
            {
                if (udp != null)
                {
                    using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                    {
                        writer.Write(new byte[13] { 0x51, 0x75, 0x65, 0x72, 0x79, 0x50, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C });
                        writer.Write(info.hostname);
                        writer.Write(info.players);
                        writer.Write(info.maxPlayers);
                        writer.Write(info.scene);
                        writer.Write(info.version);
                        byte[] data = (writer.BaseStream as MemoryStream).ToArray();
                        udp.Send(data, data.Length, point);
                        if (QueryDebug)
                        {
                            Debug.Log("[Query] Sending server information to " + point.Address + ":" + point.Port+ " [sz=" + data.Length + "]");
                        }
                    }
                }
            }
            catch { }
        }
        else
        {
            Debug.LogWarning("[Query] Requesting query available only on server");
        }
    }

    public void Update()
    {
        if(_query_timer <= 0)
        {
            _query_timer = 0;
            if (!requested)
            {
                OnQueringFailed?.Invoke();
            }
        }
        else if(_query_timer > 0)
        {
            _query_timer -= Time.deltaTime;
        }

        if (_request_event)
        {
            info.ping = (short)((Time.realtimeSinceStartup - time_at_last_request)*1000);
            OnReceivedQuery?.Invoke(info);
            _request_event = false;
        }
    }

    public void Loop()
    {
        while (Initialized)
        {

            if (mode == QueryProtocolMode.Client && udp != null)
            {
                IPEndPoint remote = null;

                try
                {
                    byte[] data = udp.Receive(ref remote);

                    if (data.Length > 0 &&
                        data[0] == 0x51 &&
                        data[1] == 0x75 &&
                        data[2] == 0x65 &&
                        data[3] == 0x72 &&
                        data[4] == 0x79 &&
                        data[5] == 0x50 &&
                        data[6] == 0x72 &&
                        data[7] == 0x6F &&
                        data[8] == 0x74 &&
                        data[9] == 0x6f &&
                        data[10] == 0x63 &&
                        data[11] == 0x6f &&
                        data[12] == 0x6c
                        )
                    {
                        using (BinaryReader _ = new BinaryReader(new MemoryStream(data)))
                        {
                            _.BaseStream.Position = 13;//skip 13 bytes (skip header)
                            info = new QueryInfo(_.ReadString(), _.ReadByte(), _.ReadByte(),_.ReadString(), _.ReadString(),0);
                            if (QueryDebug)
                            {
                                Debug.Log("[Query] Received information from the requested server...");
                            }
                            _request_event = true;
                            requested = true;
                        }
                    }
                }
                catch { }
            }
            else if (mode == QueryProtocolMode.Server && udp != null)
            {
                IPEndPoint received_from = null;

                try
                {
                    byte[] received_data = udp.Receive(ref received_from);
                    //0x51, 0x75, 0x65, 0x72, 0x79, 0x50, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C - Query Protocol
                    //ignore data if size > 13 bytes
                    if (received_data.Length > 0 && received_data.Length <= 13 &&
                        received_data[0] == 0x51 &&
                        received_data[1] == 0x75 &&
                        received_data[2] == 0x65 &&
                        received_data[3] == 0x72 &&
                        received_data[4] == 0x79 &&
                        received_data[5] == 0x50 &&
                        received_data[6] == 0x72 &&
                        received_data[7] == 0x6F &&
                        received_data[8] == 0x74 &&
                        received_data[9] == 0x6f &&
                        received_data[10] == 0x63 &&
                        received_data[11] == 0x6f &&
                        received_data[12] == 0x6c
                        )
                    {
                        if (QueryDebug)
                        {
                            Debug.Log("[Query] Request data from " + received_from.Address + ":" + received_from.Port);
                        }
                        SendQuery(received_from);
                    }
                }
                catch { }
            }
            Thread.Sleep(5);
        }
    }

    public void Shutdown()
    {
        Initialized = false;
        udp?.Close();
        thread?.Join();
        thread?.Abort();
    }
}
