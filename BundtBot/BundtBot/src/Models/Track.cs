using System.IO;
using BundtBot.Database;
using LiteDB;

namespace BundtBot.Models {
    public class Track {
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

        internal static bool TryGetTrackByYoutubeSearchString(string ytSearchString, out Track track) {
            var trackId = DB.YoutubeSearchStrings
                .FindOne(x => x.Text == ytSearchString)?.TrackId;
            if (trackId == null) {
                track = null;
                return false;
            }
            track = DB.Tracks.FindById(trackId);
            return track != null;
        }

        internal static bool TryGetTrackByYoutubeId(string youtubeVideoID, out Track track) {
            track = DB.Tracks.FindOne(x => x.YoutubeId == youtubeVideoID);
            return track != null;
        }

        internal static Track NewTrack(string youtubeVideoTitle, FileInfo outputWAVFile, string youtubeVideoID, string ytSearchString) {
            var track = NewTrack(youtubeVideoTitle, outputWAVFile, youtubeVideoID);
            track.AddSearchString(ytSearchString);
            return track;
        }

        internal static Track NewTrack(string youtubeVideoTitle, FileInfo outputWAVFile, string youtubeVideoID) {
            var track = new Track {
                Title = youtubeVideoTitle,
                Path = outputWAVFile.FullName,
                YoutubeId = youtubeVideoID
            };
            return DB.Tracks.FindById(DB.Tracks.Insert(track));
        }

        internal void AddSearchString(string ytSearchString) {
            if (DB.YoutubeSearchStrings.Exists(x => x.Text == ytSearchString) == false) {
                DB.YoutubeSearchStrings.Insert(new YoutubeSearchString {
                    Text = ytSearchString,
                    TrackId =Id
                });
            }
        }

        internal void Save() {
            DB.Tracks.Update(this);
        }
    }
}
