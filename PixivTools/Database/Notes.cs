using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class Notes {
        private HashSet<ItemData> _NotesHashSet;

        public Notes() {
            _NotesHashSet = new HashSet<ItemData>();
        }

        public void Add(ItemData itemData) {
            _NotesHashSet.Add(itemData);
        }

        public void Add(ItemData itemData, string note) {
            _NotesHashSet.Add(itemData);
            itemData.Note = note;
        }

        public IEnumerable<ItemData> GetAll() {
            return _NotesHashSet;
        }

        public void Clear() {
            _NotesHashSet.Clear();
        }
    }
}
