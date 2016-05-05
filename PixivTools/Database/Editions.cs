using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class Editions {

        private Dictionary<int, string> _EditDict;
        public int HighestEdition { get; set; }

        public Editions() {
            _EditDict = new Dictionary<int, string>();    
        }

        public Editions(int highestEdition) {
            _EditDict = new Dictionary<int, string>();
            HighestEdition = highestEdition;
        }

        public void Clear() {
            _EditDict.Clear();
            HighestEdition = 0;
        }

        public void Add(int eid, string text) {
            _EditDict.Add(eid, text);
        }

        public void Add(int eid) {
            if (_EditDict.ContainsKey(eid)) return;
            _EditDict.Add(eid, null);
        }

        public void IncrementEdition() {
            HighestEdition++;
        }

        public IReadOnlyDictionary<int, string> GetAll() {
            return _EditDict;
        }
    }
}
