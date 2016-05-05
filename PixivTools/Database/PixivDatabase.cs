using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace PixivTools.Database {

    class PixivDatabase {

        private static string _TempLibraryLocation = @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\";
        private static string _LocalLibraryLocation = @"D:\pictures\japan_related\pixiv\";
        private static string _UgoiraLocation = @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\.ugoira\";
        private static string _RejectedLocation = @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\.rejected\";

        private string _Path;
        public Items Items { get; private set; }
        public Editions Editions { get; private set; }
        public ItemEditions ItemEditions { get; private set; }
        public Tags Tags { get; private set; }
        public Favorites Favorites { get; private set; }
        public Notes Notes { get; private set; }

        public List<ItemData> AcceptedThisSession { get; private set; }

        public static readonly IEnumerable<string> CreateTablesQuery = new List<string> {
            "CREATE TABLE Item (PixivID INT NOT NULL, Page INT NOT NULL, PRIMARY KEY(PixivID, Page))",
            "CREATE TABLE Edition (EditionID INT NOT NULL UNIQUE, Date TEXT, PRIMARY KEY(EditionID))",
            "CREATE TABLE ItemEdition (PixivID INT NOT NULL, EditionID INT NOT NULL, FOREIGN KEY (PixivID) REFERENCES Item(PixivID), FOREIGN KEY (EditionID) REFERENCES Edition(EditionID), PRIMARY KEY(PixivID, EditionID))",
            "CREATE TABLE Tag (TagID INT NOT NULL UNIQUE, Title TEXT NOT NULL, Description TEXT, PRIMARY KEY(TagID))",
            "CREATE TABLE ItemTag (PixivID INT NOT NULL, Page INT NOT NULL, TagID INT NOT NULL, FOREIGN KEY(PixivID, Page) REFERENCES Item(PixivID, Page), FOREIGN KEY(TagID) REFERENCES Tag(TagID), PRIMARY KEY(PixivID, Page, TagID))",
            "CREATE TABLE Favorite (PixivID INT NOT NULL, Page INT NOT NULL, Reason TEXT, FOREIGN KEY(PixivID, Page) REFERENCES Item(PixivID, Page), PRIMARY KEY(PixivID, Page))",
            "CREATE TABLE ItemNote(PixivID INT NOT NULL, Page INT NOT NULL, Text TEXT, PRIMARY KEY(PixivID,Page), FOREIGN KEY(PixivID, Page) REFERENCES Item(PixivID, Page))",
            "CREATE TABLE ItemArtist (PixivID INT NOT NULL, ArtistID INT, PRIMARY KEY (PixivID), FOREIGN KEY(PixivID) REFERENCES Item(PixivID))"
        };



        public PixivDatabase(string path) {
            _Path = path;
            Items = new Items();
            Editions = new Editions();
            ItemEditions = new ItemEditions();
            Tags = new Tags();
            Favorites = new Favorites();
            AcceptedThisSession = new List<ItemData>();
        }

        public void LoadDatabase() {

            if(!File.Exists(_Path)) {
                CreateNewDB();
            }

            Items.Clear();
            Editions.Clear();
            ItemEditions.Clear();
            Tags.Clear();
            Favorites.Clear();

            using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + _Path)) {

                int pixivID, page, editionID, tagID;
                string date, title, description, reason;

                sql_conn.Open();
                string sql_query = "SELECT * FROM Item";
                SQLiteCommand sql_comm = new SQLiteCommand(sql_query, sql_conn);
                SQLiteDataReader reader = sql_comm.ExecuteReader();

                
                while (reader.Read()) {
                    pixivID = (int)reader["PixivID"];
                    page = (int)reader["Page"];
                    Items.Add(pixivID, page);
                }
                reader.Close();

                sql_query = "SELECT * FROM Edition";
                sql_comm = new SQLiteCommand(sql_query, sql_conn);
                reader = sql_comm.ExecuteReader();

                while (reader.Read()) {
                    editionID = (int)reader["EditionID"];
                    date = reader["Date"].ToString();
                    Editions.Add(editionID, date);
                }
                reader.Close();

                sql_query = "SELECT MAX(EditionID) FROM Edition";
                sql_comm = new SQLiteCommand(sql_query, sql_conn);
                var maxEd = int.Parse(sql_comm.ExecuteScalar().ToString());
                Editions.HighestEdition = maxEd;

                sql_query = "SELECT * FROM ItemEdition";
                sql_comm = new SQLiteCommand(sql_query, sql_conn);
                reader = sql_comm.ExecuteReader();
                while (reader.Read()) {
                    pixivID = (int)reader["PixivID"];
                    editionID = (int)reader["EditionID"];
                    ItemEditions.Add(pixivID, editionID);
                }
                reader.Close();

                sql_query = "SELECT * FROM Tag";
                sql_comm = new SQLiteCommand(sql_query, sql_conn);
                reader = sql_comm.ExecuteReader();
                while (reader.Read()) {
                    tagID = (int)reader["TagID"];
                    title = reader["Title"].ToString();
                    description = reader["Description"].ToString();
                    Tags.Add(tagID, title, description);
                }
                reader.Close();

                sql_query = "SELECT * FROM ItemTag";
                sql_comm = new SQLiteCommand(sql_query, sql_conn);
                reader = sql_comm.ExecuteReader();
                while (reader.Read()) {
                    pixivID = (int)reader["PixivID"];
                    page = (int)reader["Page"];
                    tagID = (int)reader["TagID"];
                    Items.AddTagToItem(pixivID, page, tagID);
                }
                reader.Close();

                sql_query = "SELECT * FROM Favorite";
                sql_comm = new SQLiteCommand(sql_query, sql_conn);
                reader = sql_comm.ExecuteReader();
                while (reader.Read()) {
                    pixivID = (int)reader["PixivID"];
                    page = (int)reader["Page"];
                    reason = reader["Reason"].ToString();
                    Favorites.Add(Items.GetItem(pixivID, page), reason);
                }
                reader.Close();

                sql_conn.Close();
            }
        }

        public void CreateNewDB() {
            SQLiteConnection.CreateFile(_Path);
            using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + _Path)) {
                sql_conn.Open();
                var sql_comm = new SQLiteCommand(sql_conn);
                foreach(var queryText in CreateTablesQuery) {
                    sql_comm.CommandText = queryText;
                    sql_comm.ExecuteNonQuery();
                }
                sql_conn.Close();
            }
        }

        public void FillDatabaseWithOldData() {
            using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + _Path)) {
                sql_conn.Open();

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter PixivID = new SQLiteParameter();
                        SQLiteParameter Page = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [Item] VALUES(?,?)";
                        sql_comm.Parameters.Add(PixivID);
                        sql_comm.Parameters.Add(Page);

                        foreach (var item in Items.GetAll()) {
                            PixivID.Value = item.PixivID;
                            Page.Value = item.Page;
                            sql_comm.ExecuteNonQuery();
                        }

                    }
                    sql_tran.Commit();
                }

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter EditionID = new SQLiteParameter();
                        SQLiteParameter Date = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [Edition] VALUES(?,?)";
                        sql_comm.Parameters.Add(EditionID);
                        sql_comm.Parameters.Add(Date);

                        foreach (var edition in Editions.GetAll()) {
                            EditionID.Value = edition.Key;
                            Date.Value = edition.Value;
                            sql_comm.ExecuteNonQuery();
                        }

                    }
                    sql_tran.Commit();
                }

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter PixivID = new SQLiteParameter();
                        SQLiteParameter EditionID = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [ItemEdition] VALUES(?,?)";
                        sql_comm.Parameters.Add(PixivID);
                        sql_comm.Parameters.Add(EditionID);

                        foreach (var itemedition in ItemEditions.GetAll()) {
                            PixivID.Value = itemedition.Key;
                            EditionID.Value = itemedition.Value;
                            sql_comm.ExecuteNonQuery();
                        }

                    }
                    sql_tran.Commit();
                }


                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter TagID = new SQLiteParameter();
                        SQLiteParameter Title = new SQLiteParameter();
                        SQLiteParameter Description = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [Tag] VALUES(?,?,?)";
                        sql_comm.Parameters.Add(TagID);
                        sql_comm.Parameters.Add(Title);
                        sql_comm.Parameters.Add(Description);

                        foreach (var tag in Tags.GetAll()) {
                            TagID.Value = tag.ID;
                            Title.Value = tag.Title;
                            Description.Value = tag.Description;
                            sql_comm.ExecuteNonQuery();
                        }

                    }
                    sql_tran.Commit();
                }

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter PixivID = new SQLiteParameter();
                        SQLiteParameter Page = new SQLiteParameter();
                        SQLiteParameter TagID = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [ItemTag] VALUES(?,?,?)";
                        sql_comm.Parameters.Add(PixivID);
                        sql_comm.Parameters.Add(Page);
                        sql_comm.Parameters.Add(TagID);

                        foreach (var item in Items.GetAll()) {
                            PixivID.Value = item.PixivID;
                            Page.Value = item.Page;
                            foreach (var tag in item.GetTags()) {
                                TagID.Value = tag;
                                sql_comm.ExecuteNonQuery();
                            }
                            
                        }

                    }
                    sql_tran.Commit();
                }

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter PixivID = new SQLiteParameter();
                        SQLiteParameter Page = new SQLiteParameter();
                        SQLiteParameter Reason = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [Favorite] VALUES(?,?,?)";
                        sql_comm.Parameters.Add(PixivID);
                        sql_comm.Parameters.Add(Page);
                        sql_comm.Parameters.Add(Reason);

                        foreach (var fav in Favorites.GetAll()) {
                            PixivID.Value = fav.PixivID;
                            Page.Value = fav.Page;
                            Reason.Value = fav.FavReason;
                            sql_comm.ExecuteNonQuery();
                        }

                    }
                    sql_tran.Commit();
                }



                sql_conn.Close();
            }
        }

        public void WriteDBToTextFiles(string path) {
            using (StreamWriter sw = new StreamWriter(path + "NewDBItems.txt")) {
                sw.WriteLine("PixivID\tPage\tTagList");
                foreach (var item in Items.GetAll()) {
                    sw.Write(item.PixivID + "\t" + item.Page + "\t");
                    foreach(var tag in item.GetTags()) {
                        sw.Write(tag + ",");
                    }
                    sw.WriteLine();
                }
            }

            using (StreamWriter sw = new StreamWriter(path + "NewEditions.txt")) {
                sw.WriteLine("EditionID\tDate");
                foreach (var edition in Editions.GetAll()) {
                    sw.WriteLine(edition.Key + "\t" + edition.Value);
                }
            }
        }

        public ICollection<int> GetPageNumbersFromID(int pixivID) {
            return Items.GetPageNumbersFromID(pixivID);
        }

        public void AddNewImages(List<ItemData> images) {


            using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + _Path)) {
                sql_conn.Open();

                var itemsAdded = 0;

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter pixivID = new SQLiteParameter();
                        SQLiteParameter page = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [Item] VALUES(?,?)";
                        sql_comm.Parameters.Add(pixivID);
                        sql_comm.Parameters.Add(page);

                        
                        foreach (var image in images) {
                            if (!Items.ContainsItem(image.PixivID, image.Page)) {
                                pixivID.Value = image.PixivID;
                                page.Value = image.Page;
                                sql_comm.ExecuteNonQuery();
                                Items.Add(image.PixivID, image.Page);
                                itemsAdded++;
                            }
                        }
                    }
                    sql_tran.Commit();
                }


                if (itemsAdded > 0) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter editionID = new SQLiteParameter();
                        SQLiteParameter Date = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [Edition] VALUES(?,?)";
                        sql_comm.Parameters.Add(editionID);
                        sql_comm.Parameters.Add(Date);

                        editionID.Value = Editions.HighestEdition + 1;
                        Date.Value = DateTime.Now.ToShortDateString();
                        sql_comm.ExecuteNonQuery();
                        Editions.IncrementEdition();
                        Editions.Add(Editions.HighestEdition);
                    }

                    using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                        using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                            SQLiteParameter pixivID = new SQLiteParameter();
                            SQLiteParameter editionID = new SQLiteParameter();
                            sql_comm.CommandText = "INSERT INTO [ItemEdition] VALUES(?,?)";
                            sql_comm.Parameters.Add(pixivID);
                            sql_comm.Parameters.Add(editionID);
                            editionID.Value = Editions.HighestEdition;

                            foreach (var image in images) {
                                if (!ItemEditions.ContainsPixivID(image.PixivID)) {
                                    pixivID.Value = image.PixivID;
                                    sql_comm.ExecuteNonQuery();
                                    ItemEditions.Add(image.PixivID, Editions.HighestEdition);
                                }
                            }
                        }
                        sql_tran.Commit();
                    }
                }

                var favWorks = new List<ItemData>();

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter pixivID = new SQLiteParameter();
                        SQLiteParameter page = new SQLiteParameter();
                        SQLiteParameter tagID = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [ItemTag] VALUES(?,?,?)";
                        sql_comm.Parameters.Add(pixivID);
                        sql_comm.Parameters.Add(page);
                        sql_comm.Parameters.Add(tagID);


                        foreach (var image in images) {
                            pixivID.Value = image.PixivID;
                            page.Value = image.Page;
                            var item = Items.GetItem(image.PixivID, image.Page);
                            foreach(var tag in image.GetTags()) {
                                if(!item.ContainsTag(tag)) {
                                    tagID.Value = tag;
                                    sql_comm.ExecuteNonQuery();
                                    item.AddTag(tag);
                                    if(tag == 7) {
                                        favWorks.Add(item);
                                        item.FavReason = image.FavReason;
                                    }
                                }
                            }
                        }
                    }
                    sql_tran.Commit();
                }

                using (SQLiteTransaction sql_tran = sql_conn.BeginTransaction()) {
                    using (SQLiteCommand sql_comm = new SQLiteCommand(sql_conn)) {
                        SQLiteParameter pixivID = new SQLiteParameter();
                        SQLiteParameter page = new SQLiteParameter();
                        SQLiteParameter reason = new SQLiteParameter();
                        sql_comm.CommandText = "INSERT INTO [Favorite] VALUES(?,?,?)";
                        sql_comm.Parameters.Add(pixivID);
                        sql_comm.Parameters.Add(page);
                        sql_comm.Parameters.Add(reason);


                        foreach (var item in favWorks) {
                            pixivID.Value = item.PixivID;
                            page.Value = item.Page;
                            reason.Value = item.FavReason;
                            sql_comm.ExecuteNonQuery();
                            Favorites.Add(item);
                        }
                    }
                    sql_tran.Commit();
                }

                sql_conn.Close();
            }

            foreach(var image in images) {
                AcceptedThisSession.Add(image);
            }
        }


        public void CreateFolders() {

        }

        public void OrganizeImagesByArtistID() {
            var moveFromUnknown = false;
            var kek = true;
            var dict = new Dictionary<int, int>();

            var hashset = new HashSet<string>();

            using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\db.sqlite")) {


                sql_conn.Open();
                string sql_query = "SELECT image_id, member_id FROM pixiv_master_image";
                SQLiteCommand sql_comm = new SQLiteCommand(sql_query, sql_conn);
                SQLiteDataReader reader = sql_comm.ExecuteReader();
                int wid, aid;
                while (reader.Read()) {
                    wid = int.Parse(reader["image_id"].ToString());
                    aid = int.Parse(reader["member_id"].ToString());
                    dict.Add(wid, aid);
                }
                reader.Close();

                sql_conn.Close();

            }

            if(kek) {
                using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\db.sqlite")) {


                    sql_conn.Open();
                    string sql_query = "SELECT member_id FROM pixiv_master_image";
                    SQLiteCommand sql_comm = new SQLiteCommand(sql_query, sql_conn);
                    SQLiteDataReader reader = sql_comm.ExecuteReader();
                    string aid;
                    while (reader.Read()) {
                        aid = reader["member_id"].ToString();
                        hashset.Add(aid);
                    }
                    reader.Close();

                    sql_conn.Close();

                }
                var followedHash = new HashSet<string>();
                using(StreamReader reader = new StreamReader(@"C: \Users\tabaha\Desktop\pixivutil_fetchnew\f.txt")) {
                    using(StreamWriter writer = new StreamWriter("notfollowed.txt")) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            followedHash.Add(line);
                        }
                        foreach (var artist in hashset) {
                            if (!followedHash.Contains(artist)) {
                                writer.WriteLine(@"<a href=http://www.pixiv.net/member.php?id=" + artist + ">" + artist + "</a></br>");
                            }
                        }
                    }        
                }
                

            }

            if (moveFromUnknown) {
                var imagesUnknownArtists = new List<string>();
                var listFiles = new List<string>();
                var count = 0;
                foreach (var image in Directory.GetFiles(_LocalLibraryLocation + @"unknown_artist\")) {
                    var wid = Pixiv.Tools.GetPixivIdFromPath(image);
                    if (dict.ContainsKey(wid)) {
                        var aid = dict[wid];
                        if (!Directory.Exists(_LocalLibraryLocation + aid)) {
                            Directory.CreateDirectory(_LocalLibraryLocation + aid);
                            count++;
                        }
                        Pixiv.Tools.MoveToDir(image, _LocalLibraryLocation + aid + @"\");
                        listFiles.Add(image);
                    }
                }
                System.Windows.Forms.MessageBox.Show("created " + count + " directories");

                using (StreamWriter writer = new StreamWriter(@"C:\Users\tabaha\Desktop\unknown.txt")) {
                    foreach (var image in listFiles) {
                        writer.WriteLine(image);
                    }
                    writer.WriteLine(listFiles.Count);
                }
            }

            /*
            //first run only
            foreach(var aid in dict.Values) {
                Directory.CreateDirectory(_LocalLibraryLocation + aid + @"\");
            }
            Directory.CreateDirectory(_LocalLibraryLocation + @"unknown_artist\");

            var count = 0;
            var totalFiles = 0;

            foreach(var file in Directory.GetFiles(_LocalLibraryLocation)) {
                var id = Pixiv.Tools.GetPixivIdFromPath(file);
                if (dict.ContainsKey(id)) {
                    count++;
                    Pixiv.Tools.MoveToDir(file, _LocalLibraryLocation + dict[id] + @"\");
                }
                else {
                    Pixiv.Tools.MoveToDir(file, _LocalLibraryLocation + @"unknown_artist\");
                }
            }
            System.Windows.Forms.MessageBox.Show("Done!\ntotal: " + totalFiles + " --- found: " + count);*/
        }
    }
}
