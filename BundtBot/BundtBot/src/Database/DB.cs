using BundtBot.Models;
using LiteDB;

namespace BundtBot.Database {
    static class DB {
        static readonly LiteDatabase _db = new LiteDatabase("BundtBot.db");

        public static LiteCollection<Track> Tracks => _db.GetCollection<Track>("Tracks");
        public static LiteCollection<Like> Likes => _db.GetCollection<Like>("Likes");
        public static LiteCollection<User> Users => _db.GetCollection<User>("Users");
        public static LiteCollection<YoutubeSearchString> YoutubeSearchStrings => _db.GetCollection<YoutubeSearchString>("YoutubeSearchStrings");
    }
}
