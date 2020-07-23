using System;
using UnityEngine;
using static RakNet_Enums;

public class RakNet_Packet
{
    /// <summary>
    /// Packet id
    /// </summary>
    public byte packet_id
    {
        get { return _packet_id; }
        private set { _packet_id = value; }
    }

    [SerializeField]
    private byte _packet_id;

    private RakNet_Peer peer;

    private bool _receiving;
    private bool _sending;

    public RakNet_Packet Unpack(RakNet_Peer peer, ArraySegment<byte> data)
    {
        this.peer = peer;
        packet_id = data.Array[0];
        _sz = peer.incomingBytes - 1;//-1 skip packet id sz
        _receiving = true;
        _sending = false;
        try
        {
            OnDeserialize();
        }
        catch (Exception ex)
        {
            throw new Exception(GetType().Name + " packet deserialize error! (" + ex.ToString() + ")");
        }
        return this;
    }

    /// <summary>
    /// Create empty packet with packet id
    /// </summary>
    /// <param name="packet_id"></param>
    public void CreatePacket(RakNet_Peer peer, byte packet_id)
    {
        this.peer = peer;
        this.packet_id = packet_id;
        this._sz = 0;
        _receiving = false;
        _sending = true;
        OnCreatePacket();
    }

    public override string ToString()
    {
        return "Packet class = " + GetType() + "  packet = " + packet_id + "  packet_size = " + Size();
    }


    private int _sz = 0;

    /// <summary>
    /// Total size (in bytes)
    /// </summary>
    public int Size()
    {
        return _sz;
    }

    /// <summary>
    /// Total size (in bits)
    /// </summary>
    /// <returns></returns>
    public int Bits()
    {
        return _sz * 8;
    }

    protected virtual void OnCreatePacket() { }

    /// <summary>
    /// Called on packet ready for reading data
    /// </summary>
    protected virtual void OnDeserialize() { }

    /// <summary>
    /// Called on packet end writing
    /// </summary>
    protected virtual void OnSerialize() { }

    /// <summary>
    /// Call before start write data in this packet
    /// </summary>
    public bool begin_write { get; private set; } = false;
    public void BeginWritePacket()
    {
        if (_receiving)
        {
            Debug.LogWarning(GetType()+ " unable to write packet! He is received!");
            return;
        }
        begin_write = true;
        peer?.BeginWrite();
        Write(packet_id);
    }

    /// <summary>
    /// Call after end write data in this packet
    /// </summary>
    public bool end_write { get; private set; } = false;
    public void EndWritePacket()
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        end_write = true;
        OnSerialize();
    }

    protected bool ReadBool()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return false;
        }
        return peer != null ? peer.ReadBool() : false;
    }

    protected void Write(bool s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 1;
    }

    protected string ReadString()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return string.Empty;
        }
        return peer?.ReadString();
    }
    protected void Write(string s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += s.Length * 2;//*2 - unicode
    }

    protected byte ReadByte()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return byte.MinValue;
        }
        return peer != null ? peer.ReadByte() : byte.MinValue;
    }
    protected void Write(byte s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 1;
    }

    protected byte[] ReadBytes()
    {
        byte[] array = new byte[1];

        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return array;
        }

        if (peer != null)
        {
            int array_sz = peer.ReadInt();
            array = new byte[array_sz];
            for(int i = 0; i < array_sz; i++)
            {
                array[i] = ReadByte();
            }
        }

        return array;
    }
    protected void Write(byte[] s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s.Length);

        for (int i = 0; i < s.Length; i++)
        {
            peer?.Write(s[i]);
        }
        _sz += s.Length;
    }

    protected sbyte ReadSByte()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return ReadSByte();
        }
        return 0;
    }

    protected void Write(sbyte s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 1;
    }

    protected float ReadFloat()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return peer.ReadFloat();
        }
        return 0;
    }

    protected void Write(float s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 4;
    }

    protected short ReadShort()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return peer.ReadShort();
        }
        return 0;
    }
    protected void Write(short s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 2;
    }

    protected ushort ReadUShort()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return peer.ReadUShort();
        }
        return 0;
    }
    protected void Write(ushort s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 2;
    }

    protected int ReadInt()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return peer.ReadInt();
        }
        return 0;
    }
    protected void Write(int s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 4;
    }

    protected uint ReadUInt()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return peer.ReadUInt();
        }
        return 0;
    }
    protected void Write(uint s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 4;
    }

    protected long ReadLong()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return peer.ReadLong();
        }
        return 0;
    }
    protected void Write(long s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 8;
    }

    protected ulong ReadULong()
    {
        if (_sending)
        {
            Debug.Log(GetType() + " unable to read data in packet! It is sent!");
            return 0;
        }
        if (peer != null)
        {
            return peer.ReadULong();
        }
        return 0;
    }
    protected void Write(ulong s)
    {
        if (_receiving)
        {
            Debug.Log(GetType() + " unable to write packet! He is received!");
            return;
        }
        peer?.Write(s);
        _sz += 8;
    }

    protected Color ReadColor()
    {
        if (peer != null)
        {
            return new Color(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }
        return Color.white;
    }
    protected void Write(Color s)
    {
        Write(s.r);
        Write(s.g);
        Write(s.b);
        Write(s.a);
    }

    protected Vector2 ReadVector2()
    {
        if (peer != null)
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }
        return Vector2.zero;
    }
    protected void Write(Vector2 s)
    {
        Write(s.x);
        Write(s.y);
    }

    protected Vector3 ReadVector3()
    {
        if (peer != null)
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }
        return Vector3.zero;
    }

    protected void Write(Vector3 s, ushort precision)
    {
        Write(s.x.SetFloat(precision));
        Write(s.y.SetFloat(precision));
        Write(s.z.SetFloat(precision));
    }

    protected void Write(Vector3 s, byte precision)
    {
        Write(s.x.SetFloat(precision));
        Write(s.y.SetFloat(precision));
        Write(s.z.SetFloat(precision));
    }

    protected Vector3 ReadVector3(ushort precision)
    {
        if (peer != null)
        {
            return new Vector3(ReadUShort().GetFloat(precision), ReadUShort().GetFloat(precision), ReadUShort().GetFloat(precision));
        }
        return Vector3.zero;
    }

    protected Vector3 ReadVector3(byte precision)
    {
        if (peer != null)
        {
            return new Vector3(ReadByte().GetFloat(precision), ReadByte().GetFloat(precision), ReadByte().GetFloat(precision));
        }
        return Vector3.zero;
    }

    protected void Write(Vector3 s)
    {
        Write(s.x);
        Write(s.y);
        Write(s.z);
    }

    protected Vector4 ReadVector4()
    {
        if (peer != null)
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }
        return Vector4.zero;
    }
    protected void Write(Vector4 s)
    {
        Write(s.x);
        Write(s.y);
        Write(s.z);
        Write(s.w);
    }

    protected Quaternion ReadQuaternion()
    {
        if (peer != null)
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }
        return Quaternion.identity;
    }
    protected void Write(Quaternion s)
    {
        Write(s.x);
        Write(s.y);
        Write(s.z);
        Write(s.w);
    }

    public void SendToServer(PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY, PacketReliability reliability = PacketReliability.RELIABLE, NetChannel channel = NetChannel.NET_EVENTS)
    {
        peer?.SendToServer(priority, reliability, channel);
    }

    public void SendToClient(ulong guid,PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY, PacketReliability reliability = PacketReliability.RELIABLE, NetChannel channel = NetChannel.NET_EVENTS)
    {
        peer?.SendToClient(guid,priority, reliability, channel);
    }
}
