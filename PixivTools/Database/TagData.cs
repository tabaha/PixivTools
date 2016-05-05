using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivTools.Database {
    class TagData {

        public int ID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        public TagData(int id, string title, string description) {
            ID = id;
            Title = title;
            Description = description;
        }
    }
}
