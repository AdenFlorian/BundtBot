using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class AudioClipVote {
        [BsonId]
        public int Id { get; set; }
        [BsonIndex]
        public AudioClip AudioClip { get; set; }
        [BsonIndex]
        public User User { get; set; }
    }
}
