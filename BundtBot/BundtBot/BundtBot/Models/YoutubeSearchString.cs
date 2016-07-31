using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class YoutubeSearchString {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonIndex(true)]
        public string Text { get; set; }
        [BsonIndex]
        public AudioClip AudioClip { get; set; }

        public YoutubeSearchString() {
            BsonMapper.Global.Entity<YoutubeSearchString>()
                .DbRef(x => x.AudioClip, "AudioClips");
        }
    }
}
