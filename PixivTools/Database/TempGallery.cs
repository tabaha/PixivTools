using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PixivTools.Database {
    class TempGallery {
        private static string _TempLibraryLocation = @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\";
        private static string _LocalLibraryLocation = @"D:\pictures\japan_related\pixiv\";
        private static string _UgoiraLocation = @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\.ugoira\";
        private static string _RejectedLocation = @"C:\Users\tabaha\Desktop\pixivutil_fetchnew\dl\.rejected\";
        public string Location { get; private set; }
        private Stack<ItemData> _NextImages;
        private Stack<ItemData> _PrevImages;
        private Stack<ItemData> _RejectedImages;
        private List<Stack<ItemData>> _DeletedHistory;
        public List<ItemData> AcceptedImages { get; private set; }
        public ItemData CurrentImage;
        public int NumPages;

        public TempGallery(string path) {
            Location = path;
            _NextImages = new Stack<ItemData>();
            _PrevImages = new Stack<ItemData>();
            _RejectedImages = new Stack<ItemData>();
            _DeletedHistory = new List<Stack<ItemData>>();
            AcceptedImages = new List<ItemData>();
        }

        private void MoveToUgoiraDirectory() {
            foreach (var filePath in Directory.GetFiles(Location)) {
                if (filePath.EndsWith(".zip"))
                    File.Move(filePath, _UgoiraLocation + Path.GetFileName(filePath));
            }
        }

        private void DeleteUserAvatar() {
            if (File.Exists(Location + "folder.jpg")) {
                File.Delete(Location + "folder.jpg");
            }
        }

        public void Load(int minimumID, int maximumID) {
            AcceptedImages.Clear();
            _NextImages.Clear();
            _PrevImages.Clear();
            _DeletedHistory.Add(_RejectedImages);
            _RejectedImages = new Stack<ItemData>();

            MoveToUgoiraDirectory();
            DeleteUserAvatar();

            var worksList = new List<ItemData>();
            foreach (var filename in Directory.GetFiles(Location)) {
                var id = Pixiv.Tools.GetPixivIdFromPath(filename);
                if(id <= maximumID && id >= minimumID) {
                    var image = new ItemData(filename);
                    worksList.Add(image);
                }
            }

            //worksList.Reverse();
            _NextImages = new Stack<ItemData>((worksList.OrderByDescending(itemData => itemData.PixivID).ThenByDescending(itemData => itemData.Page)));
            //_NextImages.OrderBy(item => item.PixivID);
            //_NextImages.Reverse();

            NumPages = 0;

            NextImage(false);
        }

        public string AcceptImage(List<int> tags, string favReason, string note) {
            /*done?*/
            foreach (var tag in tags) {
                CurrentImage.AddTag(tag);
            }
            CurrentImage.FavReason = favReason;
            CurrentImage.Note = note;
            AcceptedImages.Add(CurrentImage);

            /*check if can generalize the next function*/
            return NextImage(false);
        }

        public string RejectImage() {
            _RejectedImages.Push(CurrentImage);
            return NextImage(false);
        }

        public int CountWorks() {
            return 0;
        }

        public string NextImage(bool putToPrevious) {
            if (_NextImages.Count == 0) return null;
            if (putToPrevious) _PrevImages.Push(CurrentImage);
            CurrentImage = _NextImages.Pop();
            if(CurrentImage.Page == 0) {
                NumPages = _NextImages.Count(itemData => itemData.PixivID == CurrentImage.PixivID);
            }
            return CurrentImage.Path;
        }

        public string PreviousImage() {
            if (_PrevImages.Count == 0) return null;
            _NextImages.Push(CurrentImage);
            CurrentImage = _PrevImages.Pop();
            return CurrentImage.Path;
        }

        public int GetImageCount() {
            return _NextImages.Count();
        }

        public void DeleteRejected() {
            foreach (var path in Directory.GetFiles(_RejectedLocation)) {
                File.Delete(path);
            }
        }

        /*public void MoveRejected() {
            foreach(var image in _RejectedImages) {
                Pixiv.Tools.MoveToDir(image.Path, _RejectedLocation);
            }
        }*/

        public string AddLastRejected() {
            if (_RejectedImages.Count == 0) return null;
            _NextImages.Push(CurrentImage);
            CurrentImage = _RejectedImages.Pop();
            return CurrentImage.Path;
        }

        public void MoveRejectedImages() {
            foreach(var image in _RejectedImages) {
                Pixiv.Tools.MoveToDir(image.Path, _RejectedLocation);
            }
            _DeletedHistory.Add(_RejectedImages);
            _RejectedImages = new Stack<ItemData>();
        }
    }
}
