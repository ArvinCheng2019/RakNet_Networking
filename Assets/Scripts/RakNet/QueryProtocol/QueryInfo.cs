[System.Serializable]
public class QueryInfo
{
    public string hostname = string.Empty;
    public byte players = 0;
    public byte maxPlayers = 0;
    public string scene = string.Empty;
    public string version = string.Empty;
    public short ping = 0;
    public QueryInfo() { }

    public QueryInfo(string hostname,byte players,byte maxPlayers,string scene,string version,short ping)
    {
        this.hostname = hostname;
        this.players = players;
        this.maxPlayers = maxPlayers;
        this.scene = scene;
        this.version = version;
        this.ping = ping;
    }
}
