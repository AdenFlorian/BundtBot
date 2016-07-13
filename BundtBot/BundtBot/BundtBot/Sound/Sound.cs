using System;
using System.Diagnostics.Contracts;
using System.IO;
using Discord;

namespace BundtBot.BundtBot.Sound {
    /// <summary>
    /// Describes a sound that can be played to a Discord voice channel.
    /// </summary>
    class Sound {
        #region Required
        /// <summary>The absolute path to the sound file on the local file system.</summary>
        public FileInfo SoundFile { get; private set; }
        /// <summary>The text channel to send messages to.</summary>
        public Channel TextChannel { get; private set; }
        /// <summary>The voice channel to play the sound in.</summary>
        public Channel VoiceChannel { get; private set; }
        #endregion

        #region Optional
        /// <summary>Determines if the sound file will be deleted from the
        /// file system after the sound has finished playing.</summary>
        public bool DeleteAfterPlay = false;
        public bool Reverb = false;
        public bool Echo = false;
        public int EchoLength = 0;
        public float EchoFactor = 0;
        /// <summary>(0, 1.1f]</summary>
        public float Volume = 1f;
        /// <summary>Length of sound in milliseconds.</summary>
        public int Length = 0;
        #endregion


        public Sound(FileInfo soundFile, Channel textChannel, Channel voiceChannel) {
            Contract.Requires<ArgumentNullException>(soundFile != null);
            Contract.Requires<FileNotFoundException>(soundFile.Exists);
            Contract.Requires<ArgumentNullException>(textChannel != null);
            Contract.Requires<ArgumentException>(textChannel.Type == ChannelType.Text);
            Contract.Requires<ArgumentNullException>(voiceChannel != null);
            Contract.Requires<ArgumentException>(voiceChannel.Type == ChannelType.Voice);
            SoundFile = soundFile;
            TextChannel = textChannel;
            VoiceChannel = voiceChannel;
        }
    }
}
