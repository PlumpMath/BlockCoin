using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BlockCoin
{

	public class Network
	{
		public const int PORT = 54545;
		public const string BROADCAST_IP = "255.255.255.255";

		public UdpClient RECEIVING_CLIENT;
		public UdpClient SENDING_CLIENT;

		public Network()
		{
			InitalizeReceiver();
			InitalizeSender();
		}

		public void InitalizeReceiver()
		{
			RECEIVING_CLIENT = new UdpClient();
			RECEIVING_CLIENT.ExclusiveAddressUse = false;
			RECEIVING_CLIENT.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			RECEIVING_CLIENT.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
		}

		public void InitalizeSender()
		{
			SENDING_CLIENT = new UdpClient();
			SENDING_CLIENT.ExclusiveAddressUse = false;

			SENDING_CLIENT.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			SENDING_CLIENT.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
		}


		public void Send(string message)
		{
			if (!string.IsNullOrEmpty(message))

			{
				byte[] data = Encoding.ASCII.GetBytes(message);
				SENDING_CLIENT.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(BROADCAST_IP), PORT));
			}
		}

		public string Receive()
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);

			byte[] data = RECEIVING_CLIENT.Receive(ref endPoint);

			return Encoding.ASCII.GetString(data);
		}

		public void PushTransaction(Transaction transaction)
		{
            //convert the object to bytes and push to the network
            using (var ms = new System.IO.MemoryStream())
            {
                byte[] data;
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(ms, transaction);
                data = ms.ToArray();
                SENDING_CLIENT.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(BROADCAST_IP), PORT));
            }
        }

        public Transaction PullTransaction()
        {
            //convert the bytes to an object (pulled byte from the network)
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
            byte[] data = RECEIVING_CLIENT.Receive(ref endPoint);
            using (var ms = new System.IO.MemoryStream(data))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (Transaction)bf.Deserialize(ms);
            }
                
        }
    }
}	