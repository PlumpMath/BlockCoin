using System;
using System.IO;
using System.Xml.Serialization;

namespace BlockCoin
{
    [System.Serializable]
    public class Wallet
    {
        public Key PublicKey;
        public Key PrivateKey;
        public int Balance;

        public static Wallet ImportWallet()
        {
            try
            {
                Wallet wallet;
                using (var sr = new StreamReader(BlockCoin.WALLET_PATH))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Wallet));
                    wallet = (Wallet)xs.Deserialize(sr);
                }
                return wallet;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public static bool ExportWallet(Wallet wallet)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Wallet));
                TextWriter tw = new StreamWriter(BlockCoin.WALLET_PATH);
                xs.Serialize(tw, wallet);
                tw.Close();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
      
        public Wallet()
        {
            //create a private/public key pair and assign
            Random rand = new Random(DateTime.Now.Millisecond);
            PrivateKey = new Key(Hashing.ComputeHash(string.Format("{0}{1}",DateTime.Now.Millisecond, rand.NextDouble()), Supported_HA.SHA256, null));
            PublicKey = new Key(Hashing.Encrypt(PrivateKey._Key, PrivateKey._Key));
            Balance = 0;

        }

        public void Send(Key publicKey, int amount)
        {
            if(Balance >= amount)
            {
                //generate a transaction and pass it to the network to verif
                Transaction transaction = new Transaction(PublicKey, publicKey, amount);
            }

        }

    }
}
