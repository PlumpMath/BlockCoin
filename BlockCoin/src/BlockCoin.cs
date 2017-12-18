using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockCoin
{
    public class BlockCoin
    {
        public const string NAME = "BlockCoin";
        public const string TICKER_SYMBOL = "BCN";
        public const int MAX_SUPPLY = 1000000;
        public const string WALLET_PATH = @"wallet.dat";
        public const string BLOCKCHAIN_PATH = @"blockchain.dat";
    }
}
