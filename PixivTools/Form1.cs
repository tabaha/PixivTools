using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;
using System.Data.SQLite;


namespace PixivTools {
    public partial class Form1 : Form {


        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lp1, string lp2);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //this is a constant indicating the window that we want to send a text message
        const int WM_SETTEXT = 0X000C;

        private List<string> WorksToDownload;
        private List<int> WorksToRetag;

        private Database.PixivDatabase PixivDatabase;
        private Database.TempGallery TGallery;
        
        private bool[] _TagFlags;
        private string[] _TagNames = { "SFW", "NSFW", "SUPER NSFW", "LOLI", "BESTIALITY", "BELLY", "FUTA", "FAV" };

        private int AcceptedCount;
        private int RejectedCount;

        public Form1() {
            InitializeComponent();



            AcceptedCount = 0;
            RejectedCount = 0;

            WorksToDownload = new List<string>();
            WorksToRetag = new List<int>();

            PixivDatabase = new Database.PixivDatabase(@"D:\pixiv.sqlite");
            PixivDatabase.LoadDatabase();
            TGallery = new Database.TempGallery(@"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\");


            //tag shit
            _TagFlags = new bool[8];
            for(int i = 0; i < 8; i++) {
                _TagFlags[i] = false;
            }
            _TagFlags[1] = true;
            //
            UpdateTempGalleryTagsLabel();

        }

        private void runPixivUtil2Button_Click(object sender, EventArgs e) {
            Process ppp = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo("PixivUtil2.exe");
            startInfo.WorkingDirectory = textBox8.Text;
            ppp.StartInfo = startInfo;
            ppp.Start();
        }

        private void findByIdButton_Click(object sender, EventArgs e) {
            var idString = Pixiv.Tools.GetPixivIdStringFromFilename(Clipboard.GetText());
            if (idString == "" || idString == null) return;
            textBox6.Clear();
            var id = int.Parse(idString);
            var dbResults = PixivDatabase.GetPageNumbersFromID(id);
            if (dbResults.Count == 0) {
                textBox6.AppendText("ID=" + id + " NOT FOUND\n");
                textBox6.AppendText("DB: ID not found\n");
                WorksToDownload.Add(idString);
                textBox10.AppendText(idString + " ");
            }
            else {
                textBox6.AppendText("ID=" + id + " found: " + dbResults.Count + " pages.\n");
                WorksToRetag.Add(id);
                taggingIDsListBox.Items.Add(id);
                textBox6.AppendText("\n");
                foreach (var page in dbResults) {
                    textBox6.AppendText("DB: page:" + page + "\n");
                }
                textBox6.AppendText("---------------");
            }

        }

        private void SendMessageToPixivDownloader(string message) {
            IntPtr handle = FindWindow("ConsoleWindowClass", "PixivDownloader 20151019 ");
            if (!handle.Equals(IntPtr.Zero)) {
                if (SetForegroundWindow(handle)) {
                    SendKeys.Send(message);
                }
            }
        }

        private void updateShortButton_Click(object sender, EventArgs e) {
            SendMessageToPixivDownloader(Pixiv.Tools.ShortNewIllustFromBookmarkUpdate);
        }

        private void updateSafeButton_Click(object sender, EventArgs e) {
            SendMessageToPixivDownloader(Pixiv.Tools.SafeNewIllustFromBookmarkUpdate);
        }

        private void updateLongButton_Click(object sender, EventArgs e) {
            SendMessageToPixivDownloader(Pixiv.Tools.LongNewIllustFromBookmarkUpdate);
        }

        private void updateAllButton_Click(object sender, EventArgs e) {
            SendMessageToPixivDownloader(Pixiv.Tools.AllNewIllustFromBookmarkUpdate);
        }

        private void reloadLocalPixivLibrartButton_Click(object sender, EventArgs e) {
            int minimumID;
            int maximumID;
            bool useMinimum = int.TryParse(minimumIDTextBox.Text, out minimumID);
            bool useMaximum = int.TryParse(maximumIDTextBox.Text, out maximumID);
            
            if(useMinimum && useMaximum) {
                TGallery.Load(minimumID, maximumID);
            }
            else if(useMinimum) {
                TGallery.Load(minimumID, int.MaxValue);
            }
            else if(useMaximum) {
                TGallery.Load(0, maximumID);
            }
            else {
                TGallery.Load(0, int.MaxValue);
            }
            
            tempLibraryPicBox.ImageLocation = TGallery.CurrentImage.Path;            
        }

        private void previousImageTemGalleryButton_Click(object sender, EventArgs e) {
            //nope with queues
            tempLibraryPicBox.ImageLocation = TGallery.PreviousImage();
        }

        private void nextImageTemGalleryButton_Click(object sender, EventArgs e) {
            //tempLibraryPicBox.ImageLocation = WorksDownloaded.Dequeue();
            tempLibraryPicBox.ImageLocation = TGallery.NextImage(true);
        }

        private void openDLDirectoryButton_Click(object sender, EventArgs e) {
            Process.Start(textBox7.Text);
        }

        private void openPixivDirectory_Click(object sender, EventArgs e) {
            Process.Start(textBox2.Text);
        }

        private void rejectTempLibraryButton_Click(object sender, EventArgs e) {
            tempLibraryPicBox.ImageLocation = TGallery.RejectImage();
            RejectedCount++;
        }
 
        private void new_updateIDListButton_Click(object sender, EventArgs e) {
            PixivDatabase.AddNewImages(TGallery.AcceptedImages);
            foreach (var image in TGallery.AcceptedImages) {
                Pixiv.Tools.MoveToDir(image.Path, @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\checked\");
            }
            TGallery.MoveRejectedImages();
            textBox10.AppendText("Files moved to checked/\n");
            TGallery.AcceptedImages.Clear();
            textBox10.AppendText("Accepted List cleared\n");
            using(StreamWriter sw = new StreamWriter(@"D:\pixivtools_stats.txt", true)) {
                var result = AcceptedCount + RejectedCount;
                sw.WriteLine(result + "\t" + AcceptedCount + "\t" + RejectedCount);
            }
            AcceptedCount = 0;
            RejectedCount = 0;
            textBox10.AppendText("Report written\n");
            labelAccepted.Text = "0";
        }

        private void tempLibraryPicBox_LoadCompleted(object sender, AsyncCompletedEventArgs e) {
            label9.Text = TGallery.CurrentImage.PixivID + "_" + TGallery.CurrentImage.Page;
            label10.Text = "(" + TGallery.NumPages + ") " + tempLibraryPicBox.Image.Width + "x" + tempLibraryPicBox.Image.Height;
            label12.Text = "Left: " + TGallery.GetImageCount();
        }

        private void sourceToolStripMenuItem_Click(object sender, EventArgs e) {
            Clipboard.SetText(Pixiv.Tools.GetPixivWorkLink(TGallery.CurrentImage.PixivID));
        }

        private void moveOnlyButton_Click(object sender, EventArgs e) {
            textBox6.AppendText("not moving atm\n");
        }

        // 0 sfw
        // 1 nsfw
        // 2 snsfw
        // 3 loli
        // 4 best
        // 5 belly
        // 6 futa
        // 7 fav
        // 8
        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.L) {
                _TagFlags[3] = !_TagFlags[3];
            }
            else if(e.KeyCode == Keys.S) {
                _TagFlags[0] = !_TagFlags[0];
                _TagFlags[1] = false;
                _TagFlags[2] = false;
            }
            else if(e.KeyCode == Keys.N) {
                _TagFlags[1] = !_TagFlags[1];
                _TagFlags[0] = false;
                _TagFlags[2] = false;
            }
            else if (e.KeyCode == Keys.M) {
                _TagFlags[2] = !_TagFlags[2];
                _TagFlags[0] = false;
                _TagFlags[1] = false;
            }
            else if (e.KeyCode == Keys.H) {
                _TagFlags[4] = !_TagFlags[4];
            }
            else if (e.KeyCode == Keys.B) {
                _TagFlags[5] = !_TagFlags[5];
            }
            else if(e.KeyCode == Keys.F) {
                _TagFlags[6] = !_TagFlags[6];
            }
            else if(e.KeyCode == Keys.V) {
                _TagFlags[7] = !_TagFlags[7];
            }

            UpdateTempGalleryTagsLabel();


        }

        private void tempLibraryAcceptButton_Click(object sender, EventArgs e) {
            string reason = null;
            if (_TagFlags[7]) {     
                using(var tform = new FavoriteReasonPopupForm()) {
                    if (tform.ShowDialog() == DialogResult.OK) {
                        reason = tform.GetReason();
                    }
                }
            }

            var tags = new List<int>();
            for(int i = 0; i < 8; i++) {
                if(_TagFlags[i]) {
                    tags.Add(i);
                }
            }

            string note = null;

            tempLibraryPicBox.ImageLocation = TGallery.AcceptImage(tags, reason, note);
            _TagFlags[7] = false;
            labelAccepted.Text = TGallery.AcceptedImages.Count.ToString();
            AcceptedCount++;
        }

        private void tagsSourceToolStripMenuItem_Click(object sender, EventArgs e) {
            Clipboard.SetText(tempLibraryTagsLabel.Text + Pixiv.Tools.GetPixivWorkLink(TGallery.CurrentImage.PixivID));
        }

        private void copyToWallpaperFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            Pixiv.Tools.CopyToDir(TGallery.CurrentImage.Path, @"D:\pictures\wallpapers\");
        }

        private void taggingIDsListBox_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void deleteRejectedButton_Click(object sender, EventArgs e) {
            TGallery.DeleteRejected();
        }

        private void organizeWorksButton_Click(object sender, EventArgs e) {
            /*using(StreamWriter writer = new StreamWriter("todl.txt")) {
                var i = 0;
                foreach(var image in Directory.GetFiles(@"D:\pictures\japan_related\pixiv\unknown_artist")) {
                    if(i == 200) {
                        writer.WriteLine();
                        writer.WriteLine();
                        i = 0;
                    }
                    writer.Write(Pixiv.Tools.GetPixivIdFromPath(image) + " ");
                    i++;
                }
            }*/
            PixivDatabase.OrganizeImagesByArtistID();
        }

        private void loadArtistsIDsButton_Click(object sender, EventArgs e) {
            artistIDListBox.Items.Clear();
            foreach(var folder in Directory.GetDirectories(@"D:\pictures\japan_related\pixiv\")) {
                artistIDListBox.Items.Add(Path.GetFileName(folder));
            }
        }

        private void artistIDListBox_SelectedIndexChanged(object sender, EventArgs e) {
            listBox1.Items.Clear();
            foreach(var image in Directory.GetFiles(@"D:\pictures\japan_related\pixiv\" + artistIDListBox.SelectedItem)) {
                listBox1.Items.Add(new ListBoxImageItem(image));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            pixivLibraryPictureBox.ImageLocation = ((ListBoxImageItem)listBox1.SelectedItem).Filename;
        }

        private void tempLibraryPicBox_MouseDown(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left) {
                string[] fname = { TGallery.CurrentImage.Path };
                DoDragDrop(new DataObject(DataFormats.FileDrop, fname), DragDropEffects.Copy);
            }
        }

        private void labelAccepted_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                List<string> paths = new List<string>();
                foreach(var image in TGallery.AcceptedImages) {
                    paths.Add(image.Path);
                }
                DoDragDrop(new DataObject(DataFormats.FileDrop, paths.ToArray()), DragDropEffects.Copy);
            }
        }

        private void UpdateTempGalleryTagsLabel() {
            var lbl = "";
            for (int i = 0; i < 8; i++) {
                if (_TagFlags[i]) lbl += _TagNames[i] + " ";
            }
            tempLibraryTagsLabel.Text = lbl;
        }

        private void copyAcceptedLinksToolStripMenuItem_Click(object sender, EventArgs e) {
            var links = "";
            foreach(var image in TGallery.AcceptedImages) {
                links += Pixiv.Tools.GetPixivWorkLink(image.PixivID) + " Page " + image.Page + Environment.NewLine;
            }
            Clipboard.SetText(links);
        }

        private void copyAcceptedLinksTagsToolStripMenuItem_Click(object sender, EventArgs e) {
            var links = "";
            foreach (var image in TGallery.AcceptedImages) {
                var tags = "";
                foreach(var tag in image.GetTags()) {
                    tags += _TagNames[tag] + " ";
                }
                links += tags + Pixiv.Tools.GetPixivWorkLink(image.PixivID) + " Page " + image.Page + Environment.NewLine;
            }
            Clipboard.SetText(links);
        }

        private void artistLinkToolStripMenuItem_Click(object sender, EventArgs e) {
            using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\db.sqlite")) {
                sql_conn.Open();
                string sql_query = "SELECT member_id FROM pixiv_master_image WHERE image_id=" + TGallery.CurrentImage.PixivID;
                SQLiteCommand sql_comm = new SQLiteCommand(sql_query, sql_conn);
                var result = sql_comm.ExecuteScalar().ToString();
                sql_conn.Close();
                Clipboard.SetText(Pixiv.Tools.GetPixivArtistLink(result));
            }
        }

        private void tempLibraryAddLastRejectedButton_Click(object sender, EventArgs e) {
            tempLibraryPicBox.ImageLocation = TGallery.AddLastRejected();
        }

        private void moveRejectedImagesToolStripMenuItem_Click(object sender, EventArgs e) {
            TGallery.MoveRejectedImages();
        }

        private void openArtistProfileToolStripMenuItem_Click(object sender, EventArgs e) {
            using (SQLiteConnection sql_conn = new SQLiteConnection("Data Source=" + @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\db.sqlite")) {
                sql_conn.Open();
                string sql_query = "SELECT member_id FROM pixiv_master_image WHERE image_id=" + TGallery.CurrentImage.PixivID;
                SQLiteCommand sql_comm = new SQLiteCommand(sql_query, sql_conn);
                var result = sql_comm.ExecuteScalar().ToString();
                sql_conn.Close();
                System.Diagnostics.Process.Start(Pixiv.Tools.GetPixivArtistLink(result));
                
            }
        }
    }
}
