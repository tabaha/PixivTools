using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class ItemData {
        public int PixivID { get; private set; }
        public int Page { get; private set; }
        private HashSet<int> _Tags;
        public string FavReason { get; set; }
        public string Note { get; set; }
        public string Path { get; set; }

        public ItemData(int pid, int page) {
            PixivID = pid;
            Page = page;
            _Tags = new HashSet<int>();
        }

        public ItemData(string path) {
            Path = path;
            PixivID = Pixiv.Tools.GetPixivIdFromPath(Path);
            Page = Pixiv.Tools.GetPageFromPath(Path);
            _Tags = new HashSet<int>();
        }

        public bool AddTag(int tid) {
            return _Tags.Add(tid);
        }

        public IEnumerable<int> GetTags() {
            return _Tags;
        }

        public bool ContainsTag(int tagID) {
            return _Tags.Contains(tagID);
        }
    }
}
