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
        public DateTime TransactionDate;

        public Transaction()
        {

        }

        public Transaction(Key addressForm, Key addressTo, int amount)
        {
            this.AddressForm = addressForm;
            this.AddressTo = addressTo;
            this.Amount = amount;
            this.TransactionDate = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0} sent {1}{2} to {3} at {4}", 
                AddressForm, Amount, BlockCoin.TICKER_SYMBOL, AddressTo, TransactionDate);
        }
    }
}
