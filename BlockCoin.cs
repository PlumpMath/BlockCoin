using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlockCoin
{

	public class Network
	{
		private static Network m_Network = null;
		private const int PORT = 1723;
		private const string BROADCAST_IP = "127.0.0.1";

		TcpListener listener;
		TcpClient client;

		public static Network GetNetwork()
		{
			if (m_Network == null)
			{
				m_Network = new Network();
				return m_Network;

			}

			//else
			return m_Network;
		}

		public Network()
		{
			


		}


		public async void PushTransaction(Transaction transaction)
		{
            //convert the object to bytes and push to the network
            client = new TcpClient();
            using (var ms = new System.IO.MemoryStream())
			{

				
				try
				{
					await client.ConnectAsync(IPAddress.Parse(BROADCAST_IP), PORT);
				}
				catch(Exception ex)
				{
					Console.WriteLine("Push Connection Error: " + ex.Message);
                    client = null;
					return;
				}

				NetworkStream ns = client.GetStream();

				byte[] transactionData;
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				bf.Serialize(ms, transaction);
				transactionData = ms.ToArray();
				//build the packet
				using (var ms2 = new System.IO.MemoryStream())
				{
					Packet transactionPacket = new Packet(PacketType.Transaction, transactionData);
					byte[] packetData;
					System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf2 = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
					bf2.Serialize(ms2, transactionPacket);
					packetData = ms2.ToArray();
					int packetLength = packetData.Length;
					//send packet length as a 4 byte int first before send the serialized packet
					await ns.WriteAsync(BitConverter.GetBytes(packetLength), 0, 4);

					await ns.WriteAsync(packetData, 0, packetLength);
				}
			}
			
		}




		public async Task<Transaction> _PullTransactionAsync()
		{
			listener = new TcpListener(IPAddress.Parse(BROADCAST_IP),PORT);
			listener.Start();
			try
			{
				//convert the bytes to an object (pulled byte from the network)
				
				client = await listener.AcceptTcpClientAsync();

				NetworkStream ns = client.GetStream();

				byte[] packetLength = new byte[4];
				await ns.ReadAsync(packetLength, 0, 4); // int32


				int packetLengthBytes = BitConverter.ToInt32(packetLength, 0);
				byte[] packetData = new byte[packetLengthBytes];

				await ns.ReadAsync(packetData, 0, packetLengthBytes);
				client.Close();
				using (var ms = new System.IO.MemoryStream(packetData))
				{
					BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
					Packet pkt = (Packet)bf.Deserialize(ms);

					using (var pktMs = new System.IO.MemoryStream(pkt.Data))
					{
						BinaryFormatter bf2 = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
						if (pkt.PacketType == PacketType.Transaction)
						{
							return (Transaction)bf2.Deserialize(pktMs);
						}
					}

				}
				
				return null;
				
			}
			catch (Exception ex)
			{

				Console.WriteLine("Pull async: " + ex.Message);       
				return null;
			}
			
		}

		public Transaction PullTransaction()
		{
			Task<Transaction> task = _PullTransactionAsync();
			task.Wait();
			return task.Result;
			
		}
