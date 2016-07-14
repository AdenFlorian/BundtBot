using System.IO;

namespace BundtBot.BundtBot.Extensions {
    static class FileInfoExtensions {
        public static string GetTitleTag(this FileInfo fileInfo) {
            return TagLib.File.Create(fileInfo.FullName).Tag.Title;
        }
        public static void SetTitleTag(this FileInfo fileInfo, string newTitle) {
            var taglibFile = TagLib.File.Create(fileInfo.FullName);
            taglibFile.Tag.Title = newTitle;
            taglibFile.Save();
        }
    }
}
