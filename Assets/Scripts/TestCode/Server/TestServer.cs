using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RakNet_Enums;

public class TestServer : BaseNetworkServer
{
    /// <summary>
    /// Method is called when data is received from the client
    /// </summary>
    /// connection - client's native network id
    /// data - received data from client
    /// data[0] - packet id
    public override void OnReceivedData(ulong connection_guid, ArraySegment<byte> data)
    {
        PacketIDs packet_id = (PacketIDs)data.Array[0];

        switch (packet_id)
        {
            case PacketIDs.CLIENT_DATA:

                packet_ClientData.Unpack(server_peer, data);
                OnReceivedClientData(connection_guid, packet_ClientData);


                server_peer.GetPingAverage(connection_guid);
                server_peer.GetPingLast(connection_guid);
                server_peer.GetPingLowest(connection_guid);


                break;
        }
    }

    packet_ClientData packet_ClientData = new packet_ClientData();


    public List<ClientData> clients = new List<ClientData>();

    void AddClient(ulong connection_guid,string username,string version)
    {
        clients.Add(
        new ClientData()
        {
            connection_guid = connection_guid,
            username = username,
            version = version
        }
        );//we consider the client to be verified, think about it yourself
        Debug.Log(username + " connected!");
    }

    void RemoveClient(ulong connection_guid, DisconnectionType disconnectionType = DisconnectionType.ByUser)
    {
        for(int i = 0; i < clients.Count; i++)
        {
            if(clients[i].connection_guid == connection_guid)
            {
                Debug.Log(clients[i].username+" disconnected! (Disconnect type = "+disconnectionType+")");
                clients.RemoveAt(i);
                break;
            }
        }
    }

    public void OnReceivedClientData(ulong connection_guid, packet_ClientData packet)
    {
        AddClient(connection_guid,packet.username, packet.version);

        packet.CreatePacket(server_peer, (byte)PacketIDs.CLIENT_DATA);
        packet.BeginWritePacket();
        packet.EndWritePacket();
        packet.SendToClient(connection_guid);
    }

    public override void OnConnected(ulong connection)
    {
        SendRPC((byte)PacketIDs.CLIENT_DATA_REQUEST, connection);//we request information from the client
    }

    public override void OnDisconnected(ulong connection_guid, DisconnectionType type)
    {
        RemoveClient(connection_guid, type);
    }

    public Button StartServerBtn;
    public Button StopServerBtn;

    public override void OnInit()
    {
        StartServerBtn.onClick.AddListener(() => 
        {
            StartServerBtn.gameObject.SetActive(false);
            StopServerBtn.gameObject.SetActive(true);
            StartServer();
        }
        );

        StopServerBtn.onClick.AddListener(() =>
        {
            StartServerBtn.gameObject.SetActive(true);
            StopServerBtn.gameObject.SetActive(false);
            StopServer();
        }
        );
    }
}
