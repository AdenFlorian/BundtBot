using LiteDB;

namespace BundtBot.Models {
    public class Like {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonIndex]
        public ObjectId TrackId { get; set; }
        [BsonIndex]
        public ObjectId UserId { get; set; }
    }
}
