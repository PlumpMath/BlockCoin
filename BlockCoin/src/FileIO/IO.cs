using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace BlockCoin
{
    public class IO
    {
        // opening/creating a wallet file
        public static Wallet OpenCreateWallet()
        {
            Wallet wallet;
            //open existing wallet
            if (File.Exists(BlockCoin.WALLET_PATH))
            {
                //read in existing wallet
                wallet = Wallet.ImportWallet();
                if (wallet != null)
                    return wallet;
            }

            //generate a new wallet
            wallet = new Wallet();
            Wallet.ExportWallet(wallet);
            return wallet;
        }

        // used for creating and adding the genesis block(chain)
        public static bool SerializeBlockChain(BlockChain bc)
        {
            try
            {
                if (File.Exists(BlockCoin.BLOCKCHAIN_PATH))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(BlockChain));
                    TextWriter tw = new StreamWriter(BlockCoin.BLOCKCHAIN_PATH, true);
                    xs.Serialize(tw, bc);
                    tw.Close();
                }
                else
                {
                    //download block chain
                    throw new FileNotFoundException();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        // used for creating and adding the genesis block(chain)
        public static BlockChain DeserializeBlockChain()
        {
            try
            {
                if (File.Exists(BlockCoin.BLOCKCHAIN_PATH))
                {
                    using (var sr = new StreamReader(BlockCoin.BLOCKCHAIN_PATH))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(BlockChain));
                        return (BlockChain)xs.Deserialize(sr);
                    }
                   
                   
                }
                else
                {
                    //download block chain
                    throw new FileNotFoundException();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        // adding subsequent blocks
        public static bool SerializeBlock(Block block)
        {
            try
            {
                if (File.Exists(BlockCoin.BLOCKCHAIN_PATH))
                {
                    XmlDocument BlockChain = new XmlDocument();
                    BlockChain.Load(BlockCoin.BLOCKCHAIN_PATH);
                    XmlNode blockChainRoot = BlockChain.GetElementsByTagName("Blocks")[0];

                    var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                    var serializer = new XmlSerializer(typeof(Block));
                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = true;

                    XmlNode xmlDocFrag = BlockChain.CreateDocumentFragment();

                    using (var stream = new StringWriter())
                    {
                        using (var writer = XmlWriter.Create(stream, settings))
                        {
                            serializer.Serialize(writer, block, emptyNamepsaces);
                            xmlDocFrag.InnerXml = stream.ToString();
                        }
                    }

                    blockChainRoot.AppendChild(xmlDocFrag);

                    BlockChain.Save(BlockCoin.BLOCKCHAIN_PATH);
                    

                }
                else
                {
                    //download block chain
                    throw new FileNotFoundException();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
