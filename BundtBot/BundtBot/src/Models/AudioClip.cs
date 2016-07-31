using System.IO;
using BundtBot.Database;
using LiteDB;

namespace BundtBot.Models {
    public class AudioClip {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonIndex(true)]
        public string Path { get; set; }
        [BsonIndex(true)]
        public string YoutubeId { get; set; }
        public string Title { get; set; }
        public uint Length { get; set; }

        public override string ToString() {
            return $"Id: {Id}, Title: {Title}, TimeLimit: {Length}, Path: {Path}, YoutubeId: {YoutubeId}";
        }

        internal static bool TryGetAudioClipByYoutubeSearchString(string ytSearchString, out AudioClip audioClip) {
            audioClip = DB.YoutubeSearchStrings
                .Include(x => x.AudioClip)
                .FindOne(x => x.Text == ytSearchString)?.AudioClip;
            return audioClip != null;
        }

        internal static bool TryGetAudioClipByYoutubeId(string youtubeVideoID, out AudioClip audioClip) {
            audioClip = DB.AudioClips.FindOne(x => x.YoutubeId == youtubeVideoID);
            return audioClip != null;
        }

        internal static AudioClip NewAudioClip(string youtubeVideoTitle, FileInfo outputWAVFile, string youtubeVideoID, string ytSearchString) {
            var clip = NewAudioClip(youtubeVideoTitle, outputWAVFile, youtubeVideoID);
            clip.AddSearchString(ytSearchString);
            return clip;
        }

        internal static AudioClip NewAudioClip(string youtubeVideoTitle, FileInfo outputWAVFile, string youtubeVideoID) {
            var audioClip = new AudioClip {
                Title = youtubeVideoTitle,
                Path = outputWAVFile.FullName,
                YoutubeId = youtubeVideoID
            };
            return DB.AudioClips.FindById(DB.AudioClips.Insert(audioClip));
        }

        internal void AddSearchString(string ytSearchString) {
            if (DB.YoutubeSearchStrings.Exists(x => x.Text == ytSearchString) == false) {
                DB.YoutubeSearchStrings.Insert(new YoutubeSearchString {
                    Text = ytSearchString,
                    AudioClip = this
                });
            }
        }

        internal void Save() {
            DB.AudioClips.Update(this);
        }
    }
}
