using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockCoin
{
    [System.Serializable]
    public class Transaction
    {
        public Key AddressForm;
        public Key AddressTo;
        public int Amount;
        public int AddressFormFinalBalance;
        public int AddressToFinalBalance;
        public string TransactionSenderXPubKey;
        public DateTime TransactionDate;

        public string TransactionSignature;

        public Transaction()
        {

        }

        public Transaction(Wallet wallet, Key addressTo, int amount)
        {
            this.AddressForm = wallet.PublicKey;
            this.AddressTo = addressTo;
            this.Amount = amount;
            this.TransactionDate = DateTime.Now;
            this.AddressFormFinalBalance = wallet.Balance;
            
            //calculated when the transaction is veerified by the network
            this.AddressToFinalBalance = 0;

            //sign the transaction
            string xpub_key_hash = Hashing.ComputeHash(string.Format("{0}{1}", wallet.PublicKey, wallet.PrivateKey), Supported_HA.SHA256, null);
            TransactionSenderXPubKey = xpub_key_hash;
            Console.WriteLine(xpub_key_hash);
            //then hash the xpub with the data in the transaction
            string transaction_data = string.Format("{0}{1}{0}{1}{0}{1}{0}{1}",
                AddressForm._Key,
                AddressTo._Key,
                Amount,
                AddressFormFinalBalance,
                AddressToFinalBalance,
                TransactionSenderXPubKey,
                TransactionDate
                );

            TransactionSignature = Hashing.ComputeHash(transaction_data, Supported_HA.SHA256, null);
            Console.WriteLine(TransactionSignature);
        }

        public override string ToString()
        {
            return string.Format("{0} sent {1}{2} to {3} at {4}", 
                AddressForm, Amount, BlockCoin.TICKER_SYMBOL, AddressTo, TransactionDate);
        }
    }
}
