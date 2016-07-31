using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class AudioClipVote {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonIndex]
        public AudioClip AudioClip { get; set; }
        [BsonIndex]
        public User User { get; set; }

        public AudioClipVote() {
            BsonMapper.Global.Entity<AudioClipVote>()
                .DbRef(x => x.AudioClip, "AudioClips")
                .DbRef(x => x.User, "Users");
        }
    }
}
