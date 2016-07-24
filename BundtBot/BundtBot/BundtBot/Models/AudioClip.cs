using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class AudioClip {
        [BsonId]
        public int Id{ get; set; }
        public string Title { get; set; }
        public uint Length { get; set; }
        public uint Likes { get; set; }
        public string Path { get; set; }
        public string YoutubeID { get; set; }

        public override string ToString() {
            return $"Id: {Id}, Title: {Title}, TimeLimit: {Length}, Likes: {Likes}, Path: {Path}, YoutubeID: {YoutubeID}";
        }

        internal static bool TryGetAudioClipByYoutubeSearchString(string ytSearchString, out AudioClip audioClip) {
            using (var db = new LiteDatabase(@"MyData.db")) {
                var searchStrings = db.GetCollection<YoutubeSearchString>("YoutubeSearchStrings");
                var clips = db.GetCollection<AudioClip>("AudioClips");

                clips.EnsureIndex(x => x.Id);
                searchStrings.EnsureIndex(x => x.Text);

                var clipId = searchStrings.FindOne(x => x.Text == ytSearchString)?.AudioClipId;

                audioClip = clips.FindOne(x => x.Id == clipId);
            }

            if (audioClip == null) return false;

            return true;
        }

        internal static bool TryGetAudioClipByYoutubeId(string youtubeVideoID, out AudioClip audioClip) {
            using (var db = new LiteDatabase(@"MyData.db")) {
                var clips = db.GetCollection<AudioClip>("AudioClips");
                clips.EnsureIndex(x => x.YoutubeID);
                audioClip = clips.FindOne(x => x.YoutubeID == youtubeVideoID);
            }

            if (audioClip == null) return false;

            return true;
        }

        internal static AudioClip NewAudioClip(string youtubeVideoTitle, FileInfo outputWAVFile, string youtubeVideoID, string ytSearchString) {
            var clip = NewAudioClip(youtubeVideoTitle, outputWAVFile, youtubeVideoID);
            clip.AddSearchString(ytSearchString);
            return clip;
        }

        internal static AudioClip NewAudioClip(string youtubeVideoTitle, FileInfo outputWAVFile, string youtubeVideoID) {
            using (var db = new LiteDatabase(@"MyData.db")) {
                var clips = db.GetCollection<AudioClip>("AudioClips");

                var audioClip = new AudioClip {
                    Title = youtubeVideoTitle,
                    Path = outputWAVFile.FullName,
                    YoutubeID = youtubeVideoID
                };

                audioClip = clips.FindById(clips.Insert(audioClip));

                return audioClip;
            }
        }

        internal void AddSearchString(string ytSearchString) {
            using (var db = new LiteDatabase(@"MyData.db")) {
                var searchStrings = db.GetCollection<YoutubeSearchString>("YoutubeSearchStrings");
                if (searchStrings.Exists(x => x.Text == ytSearchString) == false) {
                    searchStrings.Insert(new YoutubeSearchString {
                        Text = ytSearchString,
                        AudioClipId = Id
                    });
                }
            }
        }

        internal void Save() {
            using (var db = new LiteDatabase(@"MyData.db")) {
                var clips = db.GetCollection<AudioClip>("AudioClips");
                clips.Update(this);
            }
        }
    }
}
