using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class Favorites {
        private HashSet<ItemData> _FavsHashset;

        public Favorites() {
            _FavsHashset = new HashSet<ItemData>();
        }

        public void Add(ItemData itemData) {
            _FavsHashset.Add(itemData);
        }

        public void Add(ItemData itemData, string reason) {
            _FavsHashset.Add(itemData);
            itemData.FavReason = reason;
        }

        public IEnumerable<ItemData> GetAll() {
            return _FavsHashset;
        }

        public void Clear() {
            _FavsHashset.Clear();
        }
    }
}
