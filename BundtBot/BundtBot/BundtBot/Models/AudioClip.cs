using System.Collections.Generic;

namespace BundtBot.BundtBot.Models {
    public class AudioClip {
        public int Id{ get; set; }
        public string Title { get; set; }
        public uint Length { get; set; }
        public uint Likes { get; set; }
        public string Path { get; set; }
        public string YoutubeID { get; set; }

        public override string ToString() {
            return $"Id: {Id}, Title: {Title}, Length: {Length}, Likes: {Likes}, Path: {Path}, YoutubeID: {YoutubeID}";
        }
    }

    
}
