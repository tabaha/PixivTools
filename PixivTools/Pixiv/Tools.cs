using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace PixivTools.Pixiv {
    public class Tools {

        public static string ShortNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}2{ENTER}";
        public static string SafeNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}4{ENTER}";
        public static string LongNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}10{ENTER}";
        public static string AllNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}{ENTER}";

        private static string pixivWorkUrl = @"http://www.pixiv.net/member_illust.php?mode=medium&illust_id=";
        private static string pixivArtistUrl = @"http://www.pixiv.net/member.php?id=";

        private static Regex _pixivIdRegex = new Regex("[1-9][0-9]*");
        private static Regex pageRegex = new Regex("_p[0-9][0-9]*");


        public static int GetPixivIdFromPath(string path) {
            var filename = Path.GetFileName(path);
            return GetPixivIdFromFilename(filename);
        }

        public static int GetPixivIdFromFilename(string filename) {
            var regexResult = _pixivIdRegex.Match(filename).Value;
            var result = int.Parse(regexResult);
            return result;
        }

        public static string GetPixivIdStringFromPath(string path) {
            var filename = Path.GetFileName(path);
            return GetPixivIdStringFromFilename(filename);
        }

        public static string GetPixivIdStringFromFilename(string filename) {
            var regexResult = _pixivIdRegex.Match(filename).Value;
            return regexResult;
        }

        public static int GetPageFromPath(string path) {
            return GetPageFromFilename(Path.GetFileName(path));
        }

        public static int GetPageFromFilename(string filename) {
            int result;
            Match pageMatch = pageRegex.Match(filename);
            if (pageMatch.Success) {
                string sTemp = pageMatch.Value.Substring(2);
                result = int.Parse(sTemp);
            }
            else {
                result = 0;
            }
            return result;
        }

        public static string GetPixivWorkLink(string path) {
            string id = GetPixivIdStringFromPath(path);
            return pixivWorkUrl + id;
        }

        public static string GetPixivWorkLink(int pixivID) {
            return pixivWorkUrl + pixivID;
        }

        public static string GetPixivArtistLink(int artistID) {
            return pixivArtistUrl + artistID;
        }

        public static string GetPixivArtistLink(string artistID) {
            return pixivArtistUrl + artistID;
        }

        public static void MoveToDir(string file, string destination) {
            string fname = Path.GetFileName(file);
            File.Move(file, destination + fname);
        }

        public static void CopyToDir(string file, string destination) {
            string fname = Path.GetFileName(file);
            File.Copy(file, destination + fname);
        }

        /*private static Regex pixivIdRegex = new Regex("[1-9][0-9]*");
        private static Regex sepRegex = new Regex("[0-9]+");
        private static Regex pageRegex = new Regex("_p[0-9][0-9]*");
        private static char[] pageSeparator = { '.', ' ', '_' };
        private static string preembed = "<script src=\"http://source.pixiv.net/source/embed.js\" data-id=\"";
        private static string posembed = "\" data-size=\"large\" data-border=\"off\" charset=\"utf-8\"></script>";
        private static string pixivWorkUrl = @"http://www.pixiv.net/member_illust.php?mode=medium&illust_id=";

        public static string ShortNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}2{ENTER}";
        public static string SafeNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}4{ENTER}";
        public static string LongNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}10{ENTER}";
        public static string AllNewIllustFromBookmarkUpdate = "8{ENTER}{ENTER}{ENTER}";

        public static string GetSeparatorFromText(string text) {
            return sepRegex.Match(text).Value;
        }

        public static int GetIntSeparatorFromText(string text) {
            return int.Parse(sepRegex.Match(text).Value);
        }

        public static string GetOriginalPixivLink(string path) {
            string id = pixivIdRegex.Match(path).Value;
            return pixivWorkUrl + id;
        }

        public static void UpdateListToNewFormat(string oldList, string newList) {


            using (StreamReader reader = new StreamReader(oldList)) {
                using (StreamWriter writer = new StreamWriter(newList)) {
                    string line;
                    string id;
                    while ((line = reader.ReadLine()) != null) {
                        if (line.StartsWith("-")) {
                            id = sepRegex.Match(line).Value;
                            writer.WriteLine("#" + int.Parse(id).ToString("000000") + "|" + "|" + "|");
                            writer.WriteLine("UNTAGGED");
                        }
                        else {
                            writer.WriteLine(line);
                        }
                    }
                }
            }

        }


        public static HashSet<string> GetIDsFromDir(string path) {
            HashSet<string> uniqueIds = new HashSet<string>();

            string[] pickedFiles = Directory.GetFiles(path);

            foreach (string s in pickedFiles) {

                uniqueIds.Add(pixivIdRegex.Match(s).Value);
            }

            return uniqueIds;

        }

        public static HashSet<int> GetIDsIntFromDir(string path) {
            HashSet<int> uniqueIds = new HashSet<int>();

            string[] pickedFiles = Directory.GetFiles(path);

            foreach (string s in pickedFiles) {

                uniqueIds.Add(int.Parse(pixivIdRegex.Match(s).Value));
            }

            return uniqueIds;

        }

        public static string GetIdFromText(string text) {
            return pixivIdRegex.Match(text).Value;
        }

        public static int GetPageFromFilename(string filename) {
            //assumes it's already a pixiv image
            int result;
            Match pageMatch = pageRegex.Match(filename);
            if(pageMatch.Success) {
                string sTemp = pageMatch.Value.Substring(2);
                result = int.Parse(sTemp);
            }
            else {
                result = 0;
            }
            return result;
        }

        public static List<string> GetFilesFromDir(string path) {
            return new List<string>(Directory.GetFiles(path));
        }

        public static void UpdateMainList(string path, HashSet<string> ids) {
            int sepn;
            string sep = File.ReadLines(path).Last();
            string idsep = sepRegex.Match(sep).Value;

            using (StreamWriter sw = new StreamWriter(path, true)) {
                foreach (string s in ids) {
                    sw.WriteLine(s);
                }
                if (Int32.TryParse(idsep, out sepn)) {
                    sw.WriteLine("--------#" + (sepn + 1).ToString("0000"));
                }
                else sw.WriteLine("youfuckedup");
            }
        }

        public static void WriteALLHTMLsFromMainList(string outAllHtmlPath, string mainIdsPath) {
            Hashtable ht = new Hashtable();
            List<string> currentList = new List<string>();

            using (StreamReader sr = new StreamReader(mainIdsPath)) {
                string line;
                string title;
                while ((line = sr.ReadLine()) != null) {
                    if (line.StartsWith("-")) {
                        string sid = sepRegex.Match(line).Value;
                        currentList = new List<string>();
                        ht.Add(sid, currentList);
                    }
                    else {
                        currentList.Add(line);
                    }
                }
            }

            foreach (string k in ht.Keys) {
                Console.WriteLine(k);

                int i = 0;

                using (StreamWriter sw = new StreamWriter(outAllHtmlPath + k + ".html")) {
                    sw.WriteLine("<table>");
                    foreach (string s in (List<string>)ht[k]) {



                        if (i % 3 == 0) {
                            sw.WriteLine("<tr>");
                        }
                        sw.WriteLine("<td>" + preembed + s + posembed + "</td>");
                        if (i % 3 == 2) {
                            sw.WriteLine("</tr>");
                        }
                        i++;
                    }
                    sw.WriteLine("</table>");
                }

            }
        }

        public static List<string> findFilesById(string id, string pixivDir) {
            List<string> result = new List<string>();
            foreach(string file in Directory.GetFiles(pixivDir)) {
                string fid = pixivIdRegex.Match(file).Value;
                if(id.Equals(fid)) {
                    result.Add(file);
                }
            }
            return result;
        }

        public static void MoveToDir(string file, string destination) {
            string fname = Path.GetFileName(file);
            File.Move(file, destination + fname);
        }

        public static void CopyToDir(string file, string destination) {
            string fname = Path.GetFileName(file);
            File.Copy(file, destination + fname);
        }*/
    }
}
