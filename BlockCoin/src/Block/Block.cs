using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace BlockCoin
{
    public class Block
    {
        public int BlockNumber = 0;
        public string BlockHash;
        public List<Transaction> Transactions;

        public Block()
        {

        }

        public Block(List<Transaction> transactions)
        {
            BlockNumber = 0;
            Transactions = transactions;

            GenerateBlockHash();

        }

        public void GenerateBlockHash()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Block));
            using (StringWriter textWriter = new StringWriter())
            {
                xs.Serialize(textWriter, this);
                BlockHash = Hashing.ComputeHash(textWriter.ToString(), Supported_HA.SHA256, null);
            }
        }

       
    }

    
}
