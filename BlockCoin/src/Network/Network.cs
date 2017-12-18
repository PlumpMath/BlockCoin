using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace BlockCoin
{

	public class Network
	{
		private const int PORT = 1723;
		private const string BROADCAST_IP = "127.0.0.1";
		private static Network m_Network = null;

		private Thread m_PullingThread;

		public List<Transaction> PendingTransactions = new List<Transaction>(); 

		public static Network GetNetwork(bool startPuller = false)
		{
			if (m_Network == null)
			{
				m_Network = new Network(startPuller);

				return m_Network;

			}

			//else
			return m_Network;
		}

		public Network(bool startPuller)
		{
			//initiate network puller
			if (startPuller)
			{
				m_PullingThread = new Thread(_StartNetworkPuller);
				m_PullingThread.Start();
			}

		}

		/*
		public void Send(string message)
		{
			if (!string.IsNullOrEmpty(message))

			{
				using (var ms = new System.IO.MemoryStream())
				{
					byte[] data;
					Packet pkt = new Packet(PacketType.ASCII_Text, Encoding.ASCII.GetBytes(message));
					System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
					bf.Serialize(ms, pkt);
					data = ms.ToArray();
					SENDING_CLIENT.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(BROADCAST_IP), PORT));
				}
			}
		}

		public string Receive()
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);

			byte[] data = RECEIVING_CLIENT.Receive(ref endPoint);

			using (var ms = new System.IO.MemoryStream(data))
			{
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				Packet pkt = (Packet)bf.Deserialize(ms);

				try
				{
					return Encoding.ASCII.GetString(pkt.Data);
				}
				catch(Exception)
				{
					return "";
				}
			}

		}
		*/

		private async void _StartNetworkPuller()
		{
			while(true)
			{
				Console.WriteLine("Checking: {0}" , DateTime.Now);
				TcpListener listener = TcpListener.Create(PORT);
				listener.Start();

				TcpClient client = await listener.AcceptTcpClientAsync();
				NetworkStream ns = client.GetStream();

				byte[] packetLength = new byte[4];
				await ns.ReadAsync(packetLength, 0, 4); // int32


				int packetLengthBytes = BitConverter.ToInt32(packetLength, 0);
				byte[] packetData = new byte[packetLengthBytes];

				await ns.ReadAsync(packetData, 0, packetLengthBytes);

				using (var ms = new System.IO.MemoryStream(packetData))
				{
					BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
					Packet pkt = (Packet)bf.Deserialize(ms);

					using (var pktMs = new System.IO.MemoryStream(pkt.Data))
					{
						BinaryFormatter bf2 = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						if (pkt.PacketType == PacketType.Transaction)
						{
							//must stop the listener after each
							listener.Stop();
							PendingTransactions.Add((Transaction)bf2.Deserialize(pktMs));
	        			}
					}

				}
			}
		} 

		public async void _PushTransactionAsync(Transaction transaction)
		{
			//convert the object to bytes and push to the network
			TcpClient client = new TcpClient();
			using (var ms = new System.IO.MemoryStream())
			{

				
				try
				{
					await client.ConnectAsync(IPAddress.Parse(BROADCAST_IP), PORT);
				}
				catch(Exception ex)
				{
					Console.WriteLine("Push Connection Error: " + ex.Message);
					return;
				}

				NetworkStream ns = client.GetStream();

				byte[] transactionData;
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(ms, transaction);
				transactionData = ms.ToArray();
				//build the packet
				using (var ms2 = new System.IO.MemoryStream())
				{
					Packet transactionPacket = new Packet(PacketType.Transaction, transactionData);
					byte[] packetData;
					BinaryFormatter bf2 = new BinaryFormatter();
					bf2.Serialize(ms2, transactionPacket);
					packetData = ms2.ToArray();
					int packetLength = packetData.Length;
					//send packet length as a 4 byte int first before send the serialized packet
					await ns.WriteAsync(BitConverter.GetBytes(packetLength), 0, 4);

					await ns.WriteAsync(packetData, 0, packetLength);
				}
			}
			client.Close();
		}

		public void PushTransaction(Transaction transaction)
		{
			Task.Run(() => _PushTransactionAsync(transaction));
		}

		
		
	}
}	