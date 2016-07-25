using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class YoutubeSearchString {
        [BsonId]
        public int Id { get; set; }
        [BsonIndex(true)]
        public string Text { get; set; }
        [BsonIndex]
        public int AudioClipId { get; set; }
    }
}
