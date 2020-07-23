# RakNet For Unity3D
This is an updated version of the wrapper for the native library of the RakNet network engine.

## How to use it
In order to use The raknet network engine in your project, I recommend that you study the test client and server.

Download this repository and place the downloaded files in the Assets folder of your project.

If you have this error

![Alt-текст](https://i.imgur.com/hPcyZQu.png "Error on compiling")

Check the box in Player Settings -> Allow 'unsafe' Code
![Alt-текст](https://i.imgur.com/1kPPo5S.png "Fixing error")

Next, open the test scene, and click the button to establish a connection between the client and server and see the editor console. If you see messages that the client has joined then everything is fine. You can easily modify the code and try sending other data.

The base client and server already have all the functionality for receiving and sending data, you just need to inherit from the BaseClientNetwork and BaseServerNetwork classes

### I apologize in advance for the water in the text I try to explain as much as possible :)

### How to read received data?

The Base Client Network class has a virtual method OnReceivedData (ArraySegment<byte> data); which is called when the client receives data from the server.

The Base Server Network class has a similar method OnReceivedData(ulong connection_guid, ArraySegment<byte> data);

They differ in that the server side method has the 'connection_guid' argument, but more on that later.

When receiving data (both on the client side and on the server side), the first element of the array is the packet number, followed by other data that was written by the sender.

To convert bytes to types (int,float, string, etc.), you need to call Unpack() method for packet, specifying server_peer and the array itself with data. After that, the data will be converted to types and you can continue working with them.

##### Overrided method
![Alt-текст](https://i.imgur.com/ICp14Hg.png "Method")

**It is important to follow the sequence of reading the sent data, if suddenly there is no data in the sent buffer, a non-critical error occurs. You just don't get the value and that's it.**
###### Structure of the client data packet
```csharp
    public class packet_ClientData : RakNet_Packet //Any class can be converted to a packet, the main thing is just to inherit from the RakNet_Packet class
    {
        //deserialization method is called when the packet is unpacked (see the screenshot above), here we convert bytes to types (float, int, string, etc.)
        protected override void OnDeserialize()
        {
            username = ReadString();
            version = ReadString();
        }
        
        //serialization occurs when the Begin Write Packet method is called (more on this later)
        protected override void OnSerialize()
        {
            Write(username);
            Write(version);
        }

        public string username;
        public string version;
    }
```

You can read data directly without creating packet classes, just refer to the RakNet_Peer class that is specified in the client (client_peer) and in the server (server_peer). It has almost all the main types for writing and reading.

###### Example of reading data without the use RakNet_Packet

```csharp
    //reading on client side
    void ReadData()
    {
        byte packet_id = client_peer.ReadByte();//first read packet id
        if(packet_id == 255)//as an example
        {
            client_peer.ReadInt();//12345
            client_peer.ReadString();//hello world!
        }
    }
    
    //writing on server side
    void WriteData()
    {
        server_peer.BeginWrite();
        server_peer.Write((byte)255);//write packet id
        server_peer.Write((int)12345);
        server_peer.Write("Hello world!");
        server_peer.SendToClient(123456789012345);//sending to client by guid
    }
```

I recommend using classes for processing information because it is much more convenient :)


### How to write data?

```csharp
    //Creating a packet with an ID on client side
    packet_ClientData.CreatePacket(client_peer, (byte)PacketIDs.CLIENT_DATA);
    packet_ClientData.BeginWritePacket();
    packet_ClientData.username = UserNameField.text;
    packet_ClientData.version = Application.version;
    packet_ClientData.EndWritePacket();
    packet_ClientData.SendToServer();//sending packet to the server

    //Read packet on server side
    packet_ClientData.Unpack(server_peer, data);
    OnReceivedClientData(connection_guid, packet_ClientData);
```

### How to get ping?

```csharp
    //get client ping
    BaseNetworkClient.GetPingAverage();
    BaseNetworkClient.GetPingLast();
    BaseNetworkClient.GetPingLowest();
    
    client_peer.GetPingAverage();
    client_peer.GetPingLast();
    client_peer.GetPingLowest();
    
    //get client ping on server side
    ulong connection_guid = 1234567890123;//client guid
    
    server_peer.GetPingAverage(connection_guid);
    server_peer.GetPingLast(connection_guid);
    server_peer.GetPingLowest(connection_guid);

```
