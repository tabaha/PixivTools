using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class ItemEditions {

        private Dictionary<int, int> _ItEdDict;

        public ItemEditions() {
            _ItEdDict = new Dictionary<int, int>();
        }

        public bool Add(int pixivID, int editionID) {
            if (_ItEdDict.ContainsKey(pixivID)) return false;
            _ItEdDict.Add(pixivID, editionID);
            return true;
        }

        public bool ContainsPixivID(int pixivID) {
            return _ItEdDict.ContainsKey(pixivID);
        }

        public IReadOnlyDictionary<int,int> GetAll() {
            return _ItEdDict;
        }

        public void Clear() {
            _ItEdDict.Clear();
        }
    }
}
