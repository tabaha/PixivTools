using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class Items {
        private Dictionary<int, Dictionary<int, ItemData>> _ItemsDict;

        public Items() {
            _ItemsDict = new Dictionary<int, Dictionary<int, ItemData>>();
        }

        public void Clear() {
            _ItemsDict.Clear();
        }

        public void Add(int pixivID, int page) {
            if(!_ItemsDict.ContainsKey(pixivID)) {
                _ItemsDict.Add(pixivID, new Dictionary<int, ItemData>());
            }
            _ItemsDict[pixivID].Add(page, new ItemData(pixivID, page));
        }

        public bool ContainsID(int pixivID) {
            return _ItemsDict.ContainsKey(pixivID);
        }

        public bool ContainsItem(int pixivID, int page) {
            return _ItemsDict.ContainsKey(pixivID) && _ItemsDict[pixivID].ContainsKey(page);
        }

        public IEnumerable<int> GetIDs() {
            return _ItemsDict.Keys;
        }

        public ICollection<int> GetPageNumbersFromID(int pixivID) {
            if (ContainsID(pixivID)) {
                return _ItemsDict[pixivID].Keys;
            }
            else return new HashSet<int>();
        }

        public ItemData GetItem(int pixivID, int page) {
            if (ContainsItem(pixivID, page)) {
                return _ItemsDict[pixivID][page];
            }
            else return null;
        }

        public int CountAllWorks() {
            var count = 0;
            foreach(var hi in _ItemsDict.Values) {
                count += hi.Count;
            }
            return count;
        }

        public IEnumerable<ItemData> GetAll() {
            var result = new List<ItemData>();
            foreach(var vals in _ItemsDict.Values) {
                result.AddRange(vals.Values);
            }
            return result; 
        }

        public void AddTagToItem(int pid, int page, int tid) {
            GetItem(pid, page).AddTag(tid);
        }
    }
}
