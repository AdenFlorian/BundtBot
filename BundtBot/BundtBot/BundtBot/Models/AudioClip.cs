using System;
using System.Collections.Generic;
using System.IO;
using BundtBot.BundtBot.Database;
using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class AudioClip {
        [BsonId]
        public int Id{ get; set; }
        public string Title { get; set; }
        public uint Length { get; set; }
        [BsonIndex(true)]
        public string Path { get; set; }
        [BsonIndex(true)]
        public string YoutubeID { get; set; }

        public override string ToString() {
            return $"Id: {Id}, Title: {Title}, TimeLimit: {Length}, Path: {Path}, YoutubeID: {YoutubeID}";
        }

        internal static bool TryGetAudioClipByYoutubeSearchString(string ytSearchString, out AudioClip audioClip) {
            var clipId = DB.YoutubeSearchStrings.FindOne(x => x.Text == ytSearchString)?.AudioClipId;
            audioClip = DB.AudioClips.FindOne(x => x.Id == clipId);
            return audioClip != null;
        }

        internal static bool TryGetAudioClipByYoutubeId(string youtubeVideoID, out AudioClip audioClip) {
            audioClip = DB.AudioClips.FindOne(x => x.YoutubeID == youtubeVideoID);
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
                YoutubeID = youtubeVideoID
            };
            return DB.AudioClips.FindById(DB.AudioClips.Insert(audioClip));
        }

        internal void AddSearchString(string ytSearchString) {
            if (DB.YoutubeSearchStrings.Exists(x => x.Text == ytSearchString) == false) {
                DB.YoutubeSearchStrings.Insert(new YoutubeSearchString {
                    Text = ytSearchString,
                    AudioClipId = Id
                });
            }
        }

        internal void Save() {
            DB.AudioClips.Update(this);
        }
    }
}
