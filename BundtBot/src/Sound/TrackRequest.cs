using System;
using System.IO;
using BundtBot.Models;
using Discord;

namespace BundtBot.Sound {
    /// <summary>
    /// Describes a track that can be played to a Discord voice channel.
    /// </summary>
    class TrackRequest {
        #region Required
        /// <summary>The absolute path to the sound file on the local file system.</summary>
        public Track Track { get; private set; }
        /// <summary>The text channel to send messages to.</summary>
        public Channel TextChannel { get; }
        /// <summary>The voice channel to play the track in.</summary>
        public Channel VoiceChannel { get; private set; }
		public Discord.User Requestor { get; }
        #endregion

        #region Optional
        /// <summary>Determines if the sound file will be deleted from the
        /// file system after the track has finished playing.</summary>
        public bool DeleteAfterPlay = false;
        public bool Reverb = false;
        public bool Echo = false;
        public int EchoLength = 0;
        public float EchoFactor = 0;
        /// <summary>(0, 1.1f]</summary>
        public float Volume = 1f;
        /// <summary>TimeLimit of track in milliseconds.</summary>
        public int TimeLimit = 0;
        /// <summary>Determines whether updates about this TrackRequest should be sent to TextChannel</summary>
        public bool TextUpdates = true;
        #endregion

        /// <param name="track">Must not be null, and track path must exist</param>
        /// <param name="textChannel">Must not be null, and must be a text channel</param>
        /// <param name="voiceChannel">Must not be null, and must be a voice channel</param>
        public TrackRequest(Track track, Channel textChannel, Channel voiceChannel, Discord.User requestor) {
			if (track == null) throw new ArgumentNullException("track");
			if (File.Exists(track.Path)) throw new FileNotFoundException("Track.Path must point to a file that exists", track.Path);
			if (textChannel == null) throw new ArgumentNullException("textChannel");
			if (textChannel.Type != ChannelType.Text) throw new ArgumentException("Must be a text channel", "textChannel");
			if (voiceChannel == null) throw new ArgumentNullException("voiceChannel");
			if (voiceChannel.Type != ChannelType.Voice) throw new ArgumentException("Must be a voice channel", "voiceChannel");
			if (requestor == null) throw new ArgumentNullException("requestor");

			Track = track;
            TextChannel = textChannel;
            VoiceChannel = voiceChannel;
			Requestor = requestor;
        }
    }
}
