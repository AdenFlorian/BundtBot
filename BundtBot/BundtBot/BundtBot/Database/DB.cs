using BundtBot.BundtBot.Models;
using LiteDB;

namespace BundtBot.BundtBot.Database {
    static class DB {
        static readonly LiteDatabase _db = new LiteDatabase("MyData.db");

        public static LiteCollection<AudioClip> AudioClips => _db.GetCollection<AudioClip>("AudioClips");
        public static LiteCollection<AudioClipVote> AudioClipVotes => _db.GetCollection<AudioClipVote>("AudioClipVotes");
        public static LiteCollection<User> Users => _db.GetCollection<User>("Users");
        public static LiteCollection<YoutubeSearchString> YoutubeSearchStrings => _db.GetCollection<YoutubeSearchString>("YoutubeSearchStrings");
    }
}
