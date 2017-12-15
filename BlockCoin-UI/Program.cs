using System;
using BlockCoin;
namespace BlockCoin_UI
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to BlockCoin");

            //pull in the blockchain from local and on the network
            BlockChain blockChain = IO.DeserializeBlockChain();

            foreach (Block block in blockChain.Blocks)
            {
                Console.WriteLine(block.BlockNumber);
                Console.WriteLine(block.Transactions.Count);
                foreach (Transaction transaction in block.Transactions)
                {
                    Console.WriteLine(transaction.ToString());
                }
            }

            Console.WriteLine("\r\n\r\n");


            Wallet wallet = IO.OpenCreateWallet();
            Console.WriteLine("Your public key : {0}", wallet.PublicKey._Key);
            Console.WriteLine("Your private key : {0}", wallet.PrivateKey._Key);
            Console.WriteLine("Your balance : {0}", wallet.Balance);


            Transaction tr = new Transaction(wallet.PublicKey, wallet.PublicKey, 0);

            Network network = new Network();
            network.PushTransaction(tr);
            network.PushTransaction(tr); network.PushTransaction(tr);
            network.PushTransaction(tr);
            network.PushTransaction(tr);

            Transaction trRec = null;

            while(trRec == null)
            {
                trRec = network.PullTransaction();
            }
            Console.WriteLine("Transaction Found");
            Console.WriteLine(trRec.AddressForm);

            Console.ReadLine();
        }
    }
}
