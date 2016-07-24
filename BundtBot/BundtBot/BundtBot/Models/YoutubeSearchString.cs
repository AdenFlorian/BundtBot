using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class YoutubeSearchString {
        [BsonId]
        public int Id { get; set; }
        [BsonIndex]
        public string Text { get; set; }
        [BsonIndex]
        public int AudioClipId { get; set; }
    }
}
