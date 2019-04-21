﻿using Android.App;
using Android.Database;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Widget;
using Opus.Api.Services;
using Opus.DataStructure;
using Opus.Fragments;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib;
using ContentResolver = Android.Content.ContentResolver;
using ContentValues = Android.Content.ContentValues;
using Intent = Android.Content.Intent;
using Random = System.Random;

namespace Opus.Api
{
    public class LocalManager
    {
        #region Playback
        /// <summary>
        /// Handle playback of a local file using it's path.
        /// </summary>
        /// <param name="path"></param>
        public static void Play(string path)
        {
            Intent intent = new Intent(Application.Context, typeof(MusicPlayer));
            intent.PutExtra("file", path);
            Application.Context.StartService(intent);

            MainActivity.instance.ShowPlayer();
            MusicPlayer.UpdateQueueDataBase();
        }

        /// <summary>
        /// Add a local item to the queue.
        /// </summary>
        /// <param name="path"></param>
        public static void PlayNext(string path)
        {
            Intent intent = new Intent(Application.Context, typeof(MusicPlayer));
            intent.PutExtra("file", path);
            intent.SetAction("PlayNext");
            Application.Context.StartService(intent);
        }

        /// <summary>
        /// Add a local item at the end of the queue.
        /// </summary>
        /// <param name="path"></param>
        public static void PlayLast(string path)
        {
            Intent intent = new Intent(Application.Context, typeof(MusicPlayer));
            intent.PutExtra("file", path);
            intent.SetAction("PlayLast");
            Application.Context.StartService(intent);
        }

        /// <summary>
        /// RandomPlay all file in the folder you selected or all file on the device if the folderPath = null.
        /// </summary>
        /// <param name="folderPath">The path of a folder if you want to shuffle files from this folder only.</param>
        public async static void ShuffleAll(string folderPath = null)
        {
            List<Song> songs = new List<Song>();

            await Task.Run(() => 
            {
                if (Looper.MyLooper() == null)
                    Looper.Prepare();

                CursorLoader cursorLoader = new CursorLoader(Application.Context, MediaStore.Audio.Media.ExternalContentUri, null, MediaStore.Audio.Media.InterfaceConsts.Data + " LIKE '%" + folderPath + "%'", null, null);
                ICursor musicCursor = (ICursor)cursorLoader.LoadInBackground();
                if (musicCursor != null && musicCursor.MoveToFirst())
                {
                    int titleID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
                    int artistID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Artist);
                    int albumID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Album);
                    int thisID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Id);
                    int pathID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Data);
                    do
                    {
                        string Artist = musicCursor.GetString(artistID);
                        string Title = musicCursor.GetString(titleID);
                        string Album = musicCursor.GetString(albumID);
                        long AlbumArt = musicCursor.GetLong(musicCursor.GetColumnIndex(MediaStore.Audio.Albums.InterfaceConsts.AlbumId));
                        long id = musicCursor.GetLong(thisID);
                        string path = musicCursor.GetString(pathID);

                        if (Title == null)
                            Title = "Unknown Title";
                        if (Artist == null)
                            Artist = "Unknow Artist";
                        if (Album == null)
                            Album = "Unknow Album";

                        songs.Add(new Song(Title, Artist, Album, null, AlbumArt, id, path));
                    }
                    while (musicCursor.MoveToNext());
                    musicCursor.Close();
                }
            });

            if (songs.Count == 0)
            {
                Snackbar snackBar = Snackbar.Make(MainActivity.instance.FindViewById<CoordinatorLayout>(Resource.Id.snackBar), Resource.String.no_song_mix, Snackbar.LengthLong);
                snackBar.View.FindViewById<TextView>(Resource.Id.snackbar_text).SetTextColor(Color.White);
                snackBar.Show();
                return;
            }

            Random r = new Random();
            songs = songs.OrderBy(x => r.Next()).ToList();
            SongManager.Play(songs[0]);
            songs.RemoveAt(0);

            await Task.Delay(1000);
            while (MusicPlayer.instance == null)
                await Task.Delay(10);

            MusicPlayer.instance.AddToQueue(songs.ToArray());
        }

        /// <summary>
        /// Play all file from a folder in order.
        /// </summary>
        /// <param name="folderPath"></param>
        public async static void PlayInOrder(string folderPath)
        {
            List<Song> songs = new List<Song>();

            await Task.Run(() =>
            {
                if (Looper.MyLooper() == null)
                    Looper.Prepare();

                CursorLoader cursorLoader = new CursorLoader(Application.Context, MediaStore.Audio.Media.ExternalContentUri, null, MediaStore.Audio.Media.InterfaceConsts.Data + " LIKE '%" + folderPath + "%'", null, null);
                ICursor musicCursor = (ICursor)cursorLoader.LoadInBackground();
                if (musicCursor != null && musicCursor.MoveToFirst())
                {
                    int titleID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
                    int artistID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Artist);
                    int albumID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Album);
                    int thisID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Id);
                    int pathID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Data);
                    do
                    {
                        string Artist = musicCursor.GetString(artistID);
                        string Title = musicCursor.GetString(titleID);
                        string Album = musicCursor.GetString(albumID);
                        long AlbumArt = musicCursor.GetLong(musicCursor.GetColumnIndex(MediaStore.Audio.Albums.InterfaceConsts.AlbumId));
                        long id = musicCursor.GetLong(thisID);
                        string path = musicCursor.GetString(pathID);

                        if (Title == null)
                            Title = "Unknown Title";
                        if (Artist == null)
                            Artist = "Unknow Artist";
                        if (Album == null)
                            Album = "Unknow Album";

                        songs.Add(new Song(Title, Artist, Album, null, AlbumArt, id, path));
                    }
                    while (musicCursor.MoveToNext());
                    musicCursor.Close();
                }
            });

            if (songs.Count == 0)
            {
                Snackbar snackBar = Snackbar.Make(MainActivity.instance.FindViewById<CoordinatorLayout>(Resource.Id.snackBar), Resource.String.no_song_mix, Snackbar.LengthLong);
                snackBar.View.FindViewById<TextView>(Resource.Id.snackbar_text).SetTextColor(Color.White);
                snackBar.Show();
                return;
            }

            songs.Reverse();
            SongManager.Play(songs[0]);
            songs.RemoveAt(0);

            await Task.Delay(1000);
            while (MusicPlayer.instance == null)
                await Task.Delay(10);

            MusicPlayer.instance.AddToQueue(songs.ToArray());
        }
        #endregion

        #region Metadata
        /// <summary>
        /// Return a song using only it's path (can be normal path or content:// path). This method read metadata in the device database.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async static Task<Song> GetSong(string filePath)
        {
            string Title = "Unknow";
            string Artist = "Unknow";
            long AlbumArt = 0;
            long id = 0;
            string path;
            Uri musicUri = MediaStore.Audio.Media.ExternalContentUri;

            if (filePath.StartsWith("content://"))
                musicUri = Uri.Parse(filePath);

            await Task.Run(() => 
            {
                if (Looper.MyLooper() == null)
                    Looper.Prepare();

                CursorLoader cursorLoader = new CursorLoader(Application.Context, musicUri, null, null, null, null);
                ICursor musicCursor = (ICursor)cursorLoader.LoadInBackground();
                if (musicCursor != null && musicCursor.MoveToFirst())
                {
                    int titleID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
                    int artistID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Artist);
                    int thisID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Id);
                    int pathID = musicCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Data);
                    do
                    {
                        path = musicCursor.GetString(pathID);

                        if (path == filePath || filePath.StartsWith("content://"))
                        {
                            Artist = musicCursor.GetString(artistID);
                            Title = musicCursor.GetString(titleID);
                            AlbumArt = musicCursor.GetLong(musicCursor.GetColumnIndex(MediaStore.Audio.Albums.InterfaceConsts.AlbumId));
                            id = musicCursor.GetLong(thisID);

                            if (Title == null)
                                Title = "Unknown Title";
                            if (Artist == null)
                                Artist = "Unknow Artist";

                            if (filePath.StartsWith("content://"))
                                filePath = path;
                            break;
                        }
                    }
                    while (musicCursor.MoveToNext());
                    musicCursor.Close();
                }
            });
            return new Song(Title, Artist, null, null, AlbumArt, id, filePath);
        }

        /// <summary>
        /// Read the youtubeID of a local song in the file's metadata and add it to the song object.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Song CompleteItem(Song item)
        {
            item.YoutubeID = GetYtID(item.Path);
            return item;
        }

        /// <summary>
        /// Read the youtubeID of a local song in the file's metadata using the path of the file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetYtID(string path)
        {
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var meta = TagLib.File.Create(new StreamFileAbstraction(path, stream, stream));
            string ytID = meta.Tag.Comment;
            stream.Dispose();
            return ytID;
        }

        /// <summary>
        /// Open the EditMetadata page for a song.
        /// </summary>
        /// <param name="item"></param>
        public static void EditMetadata(Song item)
        {
            item = CompleteItem(item);
            Intent intent = new Intent(Application.Context, typeof(EditMetaData));
            intent.PutExtra("Song", item.ToString());
            MainActivity.instance.StartActivity(intent);
        }
        #endregion

        #region PlaylistsImplementation
        /// <summary>
        /// Return true if the local song of id audioID is contained in the local playlist with the id of playlistID.
        /// </summary>
        /// <param name="audioID"></param>
        /// <param name="playlistID"></param>
        /// <returns></returns>
        public async static Task<bool> SongIsContained(long audioID, long playlistID)
        {
            Uri uri = MediaStore.Audio.Playlists.Members.GetContentUri("external", playlistID);
            return await Task.Run(() => 
            {
                if (Looper.MyLooper() == null)
                    Looper.Prepare();

                CursorLoader loader = new CursorLoader(Application.Context, uri, null, null, null, null);
                ICursor cursor = (ICursor)loader.LoadInBackground();

                if (cursor != null && cursor.MoveToFirst())
                {
                    int idColumn = cursor.GetColumnIndex(MediaStore.Audio.Playlists.Members.AudioId);
                    do
                    {
                        long id = cursor.GetLong(idColumn);
                        if (id == audioID)
                            return true;
                    }
                    while (cursor.MoveToNext());
                    cursor.Close();
                }

                return false;
            });
        }

        /// <summary>
        /// Add an array of local song in a playlist.
        /// </summary>
        /// <param name="item">The array of songs you want to add to the playlist. Will only add local file, if you input youtube file in this array, they will be ignored<</param>
        /// <param name="playList">The name of the playlist</param>
        /// <param name="LocalID">The id of the local playlist or -1 if you want to add this song to a playlist that will be created after.</param>
        /// <param name="saveAsSynced">Used only if you want to create a playlist with this method. True if the newly created playlist should be synced on youtube.</param>
        public async static void AddToPlaylist(Song[] items, string playList, long LocalID, bool saveAsSynced = false)
        {
            if (LocalID == -1)
            {
                LocalID = await PlaylistManager.GetPlaylistID(playList);
                if (LocalID == -1)
                    PlaylistManager.CreateLocalPlaylist(playList, items, saveAsSynced);
                else
                    AddToPlaylist(items, playList, LocalID);
            }
            else
            {
                if(await MainActivity.instance.GetWritePermission())
                {
                    ContentResolver resolver = MainActivity.instance.ContentResolver;
                    List<ContentValues> values = new List<ContentValues>();

                    foreach (Song item in items)
                    {
                        if (item != null && item.Id != 0 && item.Id != -1)
                        {
                            ContentValues value = new ContentValues();
                            value.Put(MediaStore.Audio.Playlists.Members.AudioId, item.Id);
                            value.Put(MediaStore.Audio.Playlists.Members.PlayOrder, 0);
                            values.Add(value);
                        }
                    }

                    resolver.BulkInsert(MediaStore.Audio.Playlists.Members.GetContentUri("external", LocalID), values.ToArray());
                }
            }
        }
        #endregion
    }
}