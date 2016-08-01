using System;

namespace BundtBot.Youtube {
    public class YoutubeHelper {
        const string HostName = "www.youtube.com";
        const string Path = "/watch";
        const string QueryStart = "?v=";

        public static bool IsYoutubeUrl(string ytSearchString) {
            Uri uri;
            try {
                uri = new Uri(ytSearchString);
            } catch (Exception) {
                return false;
            }
            if (uri.Host != HostName) {
                return false;
            }
            if (uri.AbsolutePath != Path) {
                return false;
            }
            if (uri.Query.StartsWith(QueryStart) == false) {
                return false;
            }
            if (uri.Query.Length < QueryStart.Length + 11) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the Youtube video ID fromn the given youtube video URL.
        /// </summary>
        /// <param name="youtubeUrl">A valid Youtube video URL.</param>
        /// <returns>Youtube video ID</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="youtubeUrl"/> is not a valid Youtube video URL.</exception>
        public static string GetVideoIdFromUrl(string youtubeUrl) {
            if (IsYoutubeUrl(youtubeUrl) == false) {
                throw new ArgumentException(nameof(youtubeUrl) + " must be a valid youtube video URL");
            }
            return new Uri(youtubeUrl).Query.Substring(3);
        }
    }
}
