public class packet_ClientData : RakNet_Packet
{
    protected override void OnDeserialize()
    {
        username = ReadString();
        version = ReadString();
    }

    protected override void OnSerialize()
    {
        Write(username);
        Write(version);
    }

    public string username;
    public string version;
}
