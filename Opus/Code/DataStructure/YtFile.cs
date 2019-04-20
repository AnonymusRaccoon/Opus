﻿namespace Opus.DataStructure
{
    [System.Serializable]
    public class YtFile
    {
        public Song song;
        public PlaylistItem playlist;
        public YtKind Kind;

        public YtFile(Song song, YtKind kind)
        {
            this.song = song;
            Kind = kind;
        }

        public YtFile(PlaylistItem playlist, YtKind kind)
        {
            this.playlist = playlist;
            Kind = kind;
        }
    }

    public enum YtKind { Null, Video, Playlist, Channel, ChannelPreview, Loading }
}