using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockCoin
{
    [System.Serializable]
    public class Key
    {
        public string _Key = "";

        public Key()
        {
            _Key = string.Empty;
        }

        public Key(string key)
        {
            _Key = key;
        }

        public override string ToString()
        {
            return _Key.Substring(0, 12) + "...";
        }
    }
}
