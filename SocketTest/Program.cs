using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
namespace SocketTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static void Main()
        {
            Console.WriteLine("Server or Client");

            string input = Console.ReadLine();

            if (input == "server")
            {
                Console.WriteLine("Server Started");
                TcpListener serverSocket = new TcpListener(8888);
                int requestCount = 0;
                TcpClient clientSocket = default(TcpClient);
                serverSocket.Start();
                Console.WriteLine(" >> Server Started");
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine(" >> Accept connection from client");
                requestCount = 0;

                while ((true))
                {
                    try
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = clientSocket.GetStream();
                        byte[] bytesFrom = new byte[10025];
                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                        Console.WriteLine(" >> Data from client - " + dataFromClient);
                        string serverResponse = "Last Message from client" + dataFromClient;
                        Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(" >> " + serverResponse);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine(" >> exit");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Client Started");
                TcpClient clientSocket = new TcpClient();
                clientSocket.Connect("127.0.0.1", 8888);

                while(Console.ReadLine() != "exit")
                {
                    if(Console.ReadLine() == "s")
                    {
                        NetworkStream serverStream = clientSocket.GetStream();
                        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(DateTime.Now.ToShortDateString() + "$");
                        serverStream.Write(outStream, 0, outStream.Length);
                        serverStream.Flush();

                        byte[] inStream = new byte[10025];
                        serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                        string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                        Console.WriteLine(returndata);
                    }
                }
            }
        }

        /*[STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MAIN());
        }*/
    }
}
