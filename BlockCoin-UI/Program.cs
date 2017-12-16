using System;
using BlockCoin;
namespace BlockCoin_UI
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to BlockCoin");

            


            
            //BlockChain blockChain = IO.DeserializeBlockChain();
            
            Console.WriteLine("\r\n\r\n");

            Wallet wallet = IO.OpenCreateWallet();
            Console.WriteLine("Your public key : {0}", wallet.PublicKey._Key);
            Console.WriteLine("Your private key : {0}", wallet.PrivateKey._Key);
            Console.WriteLine("Your balance : {0}", wallet.Balance);

            Console.WriteLine("Push or Pull");
            string input = Console.ReadLine();

            if(input.ToLower() == "push")
            {
                //sending
                Console.WriteLine("Sending");
                //ill have to set the key manually 
                Key sendKey = new Key(@"SlkxqxefYjGNAdUVWnybBMk5kIPpHX8VAY+uRPqSD9WJhA4g/IA1IombKhNC0ernZejdXNfRVz4Zy3o7RlipD8BfySGD1luIwsPaVLB//FS6xEsQdhECwu4DmgyV6gTJOcVCS557YcrMxZneX83nPlofSwpV1DQzBtMXWl7eQco=");

                wallet.Send(sendKey, 1000);
               
            }
            else
            {
                //receiving
                Console.WriteLine("Searching");
                Transaction trRec = null;
                while (trRec == null)
                {
                    //check for network transactions
                    trRec = Network.GetNetwork().PullTransaction();

                }
                Console.WriteLine(trRec.ToString());
            }
            
            Console.ReadLine();
        }
    }
}
