
public class RakNet_Enums
{
    //Packet priority
    public enum PacketPriority
    {
        IMMEDIATE_PRIORITY,
        HIGH_PRIORITY,
        MEDIUM_PRIORITY,
        LOW_PRIORITY
    }

    //Packet reliability
    public enum PacketReliability
    {
        //Unreliable packets are sent by straight UDP. They may arrive out of order, or not at all. This is best for data that is unimportant, or data that you send very frequently so even if some packets are missed newer packets will compensate.
        UNRELIABLE = 0,
        //Unreliable sequenced packets are the same as unreliable packets, except that only the newest packet is ever accepted. Older packets are ignored.
        UNRELIABLE_SEQUENCED = 1,
        //Reliable ordered packets are UDP packets monitored by a reliability layer to ensure they arrive at the destination and are ordered at the destination.
        RELIABLE_UNORDERED = 2,
        //Reliable packets are UDP packets monitored by a reliablilty layer to ensure they arrive at the destination.
        RELIABLE = 3,
        //Reliable sequenced packets are UDP packets monitored by a reliability layer to ensure they arrive at the destination and are sequenced at the destination.
        RELIABLE_SEQUENCED = 4
    }

    public enum RNSPerSecondMetrics
    {
        /// How many bytes per pushed via a call to send
        USER_MESSAGE_BYTES_PUSHED,

        /// How many user message bytes were sent via a call to send. This is less than or equal to USER_MESSAGE_BYTES_PUSHED.
        /// A message would be pushed, but not yet sent, due to congestion control
        USER_MESSAGE_BYTES_SENT,

        /// How many user message bytes were resent. A message is resent if it is marked as reliable, and either the message didn't arrive or the message ack didn't arrive.
        USER_MESSAGE_BYTES_RESENT,

        /// How many user message bytes were received, and returned to the user successfully.
        USER_MESSAGE_BYTES_RECEIVED_PROCESSED,

        /// How many user message bytes were received, but ignored due to data format errors. This will usually be 0.
        USER_MESSAGE_BYTES_RECEIVED_IGNORED,

        /// How many actual bytes were sent, including per-message and per-datagram overhead, and reliable message acks
        ACTUAL_BYTES_SENT,

        /// How many actual bytes were received, including overead and acks.
        ACTUAL_BYTES_RECEIVED,
        RNS_PER_SECOND_METRICS_COUNT
    }

    //net channel
    public enum NetChannel
    {
        LOCALPLAYER,
        PLAYERS_ENTITIES,
        PLAYER_EVENTS,
        OTHER_ENTITIES,
        NET_EVENTS,
        GAME_EVENTS,
        CHAT,
        RPC,
        OTHER
    }

    //Disconnection type
    public enum DisconnectionType
    {
        ConnectionLost,
        ConnectionClosed,
        Timeout,
        ServerIsFull,
        ByUser,
        Unknown
    }

    public enum RakDebugLevel
    {
        None,
        Low,
        Medium,
        Full
    }

    public enum PeerState
    {
        Unknown,
        RunAsServer,
        RunAsClient,
        ClientInitFailed,
        ServerInitFailed
    }

    //standart native packet ids
    public enum RakNetPacketID
    {
        NEW_INCOMING_CONNECTION = 19,
        CONNECTION_REQUEST_ACCEPTED = 16,
        CONNECTION_ATTEMPT_FAILED = 17,
        NO_FREE_INCOMING_CONNECTIONS = 20,
        DISCONNECTION_NOTIFICATION = 21,
        CONNECTION_LOST = 22
    }
}