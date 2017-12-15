using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static void Main()
        {
            Server server = new Server();
            Console.WriteLine("send or recieve");
            string input = Console.ReadLine();

            if (input == "send")
            {
                while (true)
                {
                    server.Send(DateTime.Now.ToLongTimeString());
                }
            }
            else
            {
                while(true)
                {
                    Console.WriteLine(server.Receive());
                }
            }

        }
        /*   
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MAIN());
        }*/
    }
}
