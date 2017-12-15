using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BlockCoin
{
    [System.Serializable]
    [XmlRoot("BlockChain")]
    public class BlockChain
    {
        [XmlArrayAttribute("Blocks")]
        public List<Block> Blocks = null;

        public void AddBlock(Block block)
        {
            if (Blocks != null)
                Blocks.Add(block);
            else
                Blocks = new List<Block>() { block };
        }


    }
}
