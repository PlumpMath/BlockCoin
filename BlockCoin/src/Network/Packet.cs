using System;


namespace BlockCoin
{
    [System.Serializable]
    public class Packet
    {     
        public PacketType PacketType;
        public byte[] Data;
        public DateTime TimeStamp;

        public Packet(PacketType pktType, byte[] data)
        {
            PacketType = pktType;
            Data = data;
            TimeStamp = DateTime.Now;
        }
    }

    [System.Serializable]
    public enum PacketType
    {
        Ping,
        ASCII_Text,
        BlockChain_Request,
        Transaction

    }
}
