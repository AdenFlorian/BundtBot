using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BundtBot.BundtBot.Extensions;
using RedditSharp.Things;

namespace BundtBot.BundtBot.Reddit {
    class RedditManager {

        static List<Post> _cachedTop100YtHaikus = new List<Post>();

        /// <summary>Get a youtube url from /r/youtubehaiku.
        /// The top 100 posts are cached until all 100 have been returned</summary>
        public static async Task<Uri> GetYoutubeHaikuUrlAsync() {
            if (_cachedTop100YtHaikus.Count > 0) {
                var post = _cachedTop100YtHaikus.GetRandom();
                _cachedTop100YtHaikus.Remove(post);
                return post.Url;
            }
            var reddit = new RedditSharp.Reddit();
            var subreddit = await reddit.GetSubredditAsync("/r/youtubehaiku");
            var posts = await Task.Run(() => subreddit.GetTop(FromTime.All));
            _cachedTop100YtHaikus = posts.Take(100).ToList();
            return _cachedTop100YtHaikus.GetRandom().Url;
        }
    }
}
