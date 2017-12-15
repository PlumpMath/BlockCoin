using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SocketTest
{

	public class Server
	{
		public const int PORT = 54545;
		public const string BROADCAST_IP = "255.255.255.255";

		public UdpClient RECEIVING_CLIENT;
		public UdpClient SENDING_CLIENT;

		public Server()
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
	}
}	