using Discord;
using System;
using System.Diagnostics.Contracts;

namespace BundtBot.BundtBot {
    /// <summary>
    /// Describes a sound that can be played to a Discord voice channel.
    /// </summary>
    class Sound {
        #region Required
        /// <summary>The absolute path to the sound file on the local file system.</summary>
        public string soundPath { get; private set; }
        /// <summary>The text channel to send messages to.</summary>
        public Channel textChannel { get; private set; }
        /// <summary>The voice channel to play the sound in.</summary>
        public Channel voiceChannel { get; private set; }
        #endregion

        #region Optional
        /// <summary>Determines if the sound file will be deleted from the
        /// file system after the sound has finished playing.</summary>
        public bool deleteAfterPlay = false;
        public bool reverb = false;
        public bool echo = false;
        public int echoLength = 0;
        public float echoFactor = 0;
        public float volume = 0f;
        public float length_seconds = 0f;
        #endregion

        public int length_ms {
            get { return (int)(length_seconds * 1000); }
        }

        public Sound(string soundPath, Channel textChannel, Channel voiceChannel) {
            Contract.Requires<ArgumentNullException>(soundPath != null);
            Contract.Requires<ArgumentNullException>(textChannel != null);
            Contract.Requires<ArgumentNullException>(voiceChannel != null);
            Contract.Requires<ArgumentException>(textChannel.Type.Equals(ChannelType.Text));
            Contract.Requires<ArgumentException>(voiceChannel.Type.Equals(ChannelType.Voice));
            this.soundPath = soundPath;
            this.textChannel = textChannel;
            this.voiceChannel = voiceChannel;
        }
    }
}
