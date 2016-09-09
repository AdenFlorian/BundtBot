using LiteDB;

namespace BundtBot.Models {
    public class YoutubeSearchString {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonIndex(true)]
        public string Text { get; set; }
        [BsonIndex]
        public ObjectId TrackId { get; set; }
    }
}
