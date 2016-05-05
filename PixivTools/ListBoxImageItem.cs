using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PixivTools {
    public class ListBoxImageItem {

        public string Filename { get; private set; }
        public int ID { get; private set; }
        public int Page { get; private set; }

        public ListBoxImageItem(string path) {
            Filename = path;
            ID = Pixiv.Tools.GetPixivIdFromPath(path);
            Page = Pixiv.Tools.GetPageFromFilename(path);
        }

        public override string ToString() {
            return ID + "_" + Page;
        }
    }
}
