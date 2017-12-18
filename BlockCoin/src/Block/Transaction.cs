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

        public string TransactionSig;
        public string TransactionSigXPubHashed;

        public Transaction()
        {

        }

        public Transaction(Wallet wallet, Key addressTo, int amount)
        {
            this.AddressForm = (Key)wallet.ComputePublicKey();
            this.AddressTo = addressTo;
            this.Amount = amount;
            this.TransactionDate = DateTime.Now;
            this.AddressFormFinalBalance = wallet.Balance;

            //calculated when the transaction is verified by the network
            this.AddressToFinalBalance = 0;

            //sign the transaction
            string xpub_key_hash = Hashing.ComputeHash(string.Format("{0}{1}", wallet.ComputePublicKey(), wallet.PrivateKey), Supported_HA.SHA256, null);
            TransactionSenderXPubKey = xpub_key_hash;

            //then hash the xpub with the data in the transaction
            string transaction_data = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                AddressForm._Key,
                AddressTo._Key,
                Amount,
                AddressFormFinalBalance,
                AddressToFinalBalance,
                TransactionSenderXPubKey,
                TransactionDate
                );
            TransactionSig = Hashing.ComputeHash(transaction_data, Supported_HA.SHA256, null);
            TransactionSigXPubHashed = Hashing.ComputeHash(string.Format("{0}{1}", TransactionSig, TransactionSenderXPubKey), Supported_HA.SHA256, null);
        }
        public bool Verify()
        {
            //gather the transaction data to see if it has been altered
            //then hash the xpub with the data in the transaction
            string transaction_data = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                AddressForm._Key,
                AddressTo._Key,
                Amount,
                AddressFormFinalBalance,
                AddressToFinalBalance,
                TransactionSenderXPubKey,
                TransactionDate
                );
            string unverified_signature = Hashing.ComputeHash(transaction_data, Supported_HA.SHA256, null);

            //Verify the current signature with the stored signature
            if (Hashing.Confirm(string.Format("{0}{1}", unverified_signature, TransactionSenderXPubKey), TransactionSigXPubHashed, Supported_HA.SHA256))
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return string.Format("{0} sent {1}{2} to {3} at {4}", 
                AddressForm, Amount, BlockCoin.TICKER_SYMBOL, AddressTo, TransactionDate);
        }
    }
}
