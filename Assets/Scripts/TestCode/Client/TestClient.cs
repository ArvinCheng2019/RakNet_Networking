using System;
using UnityEngine;
using UnityEngine.UI;

public class TestClient : BaseNetworkClient
{

    packet_ClientData packet_ClientData = new packet_ClientData();

    public override void OnReceivedData(ArraySegment<byte> data)
    {
        PacketIDs packet_id = (PacketIDs)data.Array[0];

        switch (packet_id)
        {
            case PacketIDs.CLIENT_DATA_REQUEST:


                packet_ClientData.CreatePacket(client_peer, (byte)PacketIDs.CLIENT_DATA);
                packet_ClientData.BeginWritePacket();
                packet_ClientData.username = UserNameField.text;
                packet_ClientData.version = Application.version;
                packet_ClientData.EndWritePacket();
                packet_ClientData.SendToServer();

                break;

            case PacketIDs.CLIENT_DATA:
                packet_ClientData.Unpack(client_peer, data);
                Debug.Log("Client data processed by server :)  username="+packet_ClientData.username);
                break;
        }
    }

    public override void OnInit()
    {
        ConnectBtn.onClick.AddListener(() => 
        {
            int port = 0;
            if(int.TryParse(PortField.text,out int result))
            {
                port = result;
            }
            Connect(AddressField.text, port);
        }
        );

        DisconnectBtn.onClick.AddListener(() => 
        {
            Disconnect();
        }
        );
    }

    public Button ConnectBtn;
    public Button DisconnectBtn;
    public InputField AddressField;
    public InputField PortField;
    public InputField UserNameField;
}
