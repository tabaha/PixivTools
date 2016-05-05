using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class Tags {

        private Dictionary<int, TagData> _TagsDict;

        public Tags() {
            _TagsDict = new Dictionary<int, TagData>();
        }

        public void Add(int tid, string title, string description) {
            _TagsDict.Add(tid, new TagData(tid, title, description));
        }

        public void Add(string title, string description) {
            var tid = _TagsDict.Count + 1;
            Add(tid, title, description);
        }

        public IEnumerable<TagData> GetAll() {
            return _TagsDict.Values;
        }

        public void Clear() {
            _TagsDict.Clear();
        }
    }
}
