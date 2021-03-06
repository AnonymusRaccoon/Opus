﻿using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Opus.Adapter;
using Opus.Api;
using Opus.Api.Services;
using Opus.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CursorLoader = Android.Support.V4.Content.CursorLoader;
using PlaylistItem = Opus.DataStructure.PlaylistItem;

namespace Opus.Fragments
{
    public class Home : Fragment
    {
        public static Home instance;
        public RecyclerView ListView;
        public SectionAdapter adapter;
        public LineAdapter QueueAdapter;
        public ItemTouchHelper itemTouchHelper;
        public static List<Section> sections = new List<Section>();
        public List<string> selectedTopics = new List<string>();
        public List<string> selectedTopicsID = new List<string>();
        public View view;
        private bool populating = false;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            MainActivity.instance.contentRefresh.Refresh += OnRefresh;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            instance = null;
        }

#pragma warning disable CS4014
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.CompleteRecycler, container, false);
            view.FindViewById(Resource.Id.loading).Visibility = ViewStates.Visible;
            ListView = view.FindViewById<RecyclerView>(Resource.Id.recycler);
            ListView.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));

            if (adapter != null)
            {
                ListView.SetAdapter(adapter);
                view.FindViewById(Resource.Id.loading).Visibility = ViewStates.Gone;
            }
            else
                PopulateView();
            return view;
        }
#pragma warning restore CS4014 

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            MainActivity.instance.contentRefresh.Refresh -= OnRefresh;
        }

        private async Task PopulateView()
        {
            if (!populating)
            {
                populating = true;
                sections = new List<Section>();

                if (MusicPlayer.UseCastPlayer || (MusicPlayer.queue != null && MusicPlayer.queue?.Count > 0))
                {
                    Section queue = new Section("Queue", SectionType.SinglePlaylist, MusicPlayer.queue);
                    sections.Add(queue);
                }

                Section shuffle = new Section(Resources.GetString(Resource.String.shuffle), SectionType.Shuffle);
                sections.Add(shuffle);

                await Task.Run(() => 
                {
                    if (MainActivity.instance.HasReadPermission())
                    {
                        if (Looper.MyLooper() == null)
                            Looper.Prepare();

                        Android.Net.Uri musicUri = MediaStore.Audio.Media.ExternalContentUri;

                        List<Song> allSongs = new List<Song>();
                        CursorLoader cursorLoader = new CursorLoader(MainActivity.instance, musicUri, null, null, null, null);
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

                                allSongs.Add(new Song(Title, Artist, Album, null, AlbumArt, id, path));
                            }
                            while (musicCursor.MoveToNext());
                            musicCursor.Close();
                        }
                        Random r = new Random();
                        List<Song> songList = allSongs.OrderBy(x => r.Next()).ToList();

                        if (songList.Count > 0)
                        {
                            Section featured = new Section(Resources.GetString(Resource.String.featured), SectionType.SinglePlaylist, songList.GetRange(0, songList.Count > 50 ? 50 : songList.Count));
                            sections.Add(featured);
                        }
                    }
                });

                List<Song> favorites = await SongManager.GetFavorites();
                if(favorites.Count > 0)
                    sections.Add(new Section("Fav", SectionType.SinglePlaylist, favorites));

                view.FindViewById(Resource.Id.loading).Visibility = ViewStates.Gone;
                adapter = new SectionAdapter(sections);
                ListView.SetAdapter(adapter);
                adapter.ItemClick += ListView_ItemClick;
                ListView.SetItemAnimator(new DefaultItemAnimator());

                (List<PlaylistItem> playlists, string error) = await PlaylistManager.GetLocalPlaylists(false);
                if(playlists != null)
                {
                    (List<PlaylistItem> pl, List<PlaylistItem> sp) = await PlaylistManager.ProcessSyncedPlaylists(playlists);
                    List<PlaylistItem> saved = await PlaylistManager.GetSavedYoutubePlaylists(sp, null);
                    sp.AddRange(saved);
                    sp.AddRange(pl);

                    sections.Add(new Section(GetString(Resource.String.playlists), SectionType.PlaylistList, sp));
                    adapter.NotifyItemInserted(sections.Count - 1);
                }
                else
                {
                    List<PlaylistItem> saved = await PlaylistManager.GetSavedYoutubePlaylists(null, null);

                    if(saved != null && saved.Count > 0)
                    {
                        sections.Add(new Section(GetString(Resource.String.playlists), SectionType.PlaylistList, saved));
                        adapter.NotifyItemInserted(sections.Count - 1);
                    }
                }

                populating = false;
            }
        }

        public void AddQueue()
        {
            if (sections[0].SectionTitle != "Queue")
            {
                Section queue = new Section("Queue", SectionType.SinglePlaylist, MusicPlayer.queue);
                sections.Insert(0, queue);
                adapter?.NotifyItemInserted(0);
            }
        }

        public static Fragment NewInstance()
        {
            if(instance == null)
                instance = new Home { Arguments = new Bundle() };
            return instance;
        }

        public async void OnRefresh(object sender, EventArgs e)
        {
            await Refresh();
            MainActivity.instance.contentRefresh.Refreshing = false;
        }

        public async Task Refresh()
        {
            await PopulateView();
        }

        public void RefreshQueue(bool scroll = true)
        {
            if (sections.Count > 0)
            {
                QueueAdapter?.NotifyDataSetChanged();
                if (scroll && MusicPlayer.CurrentID() != -1 && MusicPlayer.CurrentID() <= MusicPlayer.queue.Count)
                    sections[0].recycler?.ScrollToPosition(MusicPlayer.CurrentID());
            }
        }

        public async void RefreshFavs()
        {
            Section section = sections.Find(x => x.SectionTitle == "Fav");

            if(section != null)
                ((LineAdapter)section.recycler.GetAdapter())?.Refresh();
            else
            {
                List<Song> favorites = await SongManager.GetFavorites();
                if (favorites.Count > 0)
                {
                    int x = MainActivity.instance.HasReadPermission() ? 1 : 0;
                    sections.Insert(sections.Count - x, new Section("Fav", SectionType.SinglePlaylist, favorites));
                    adapter.NotifyItemInserted(sections.Count - x);
                }
            }
        }

        public void NotifyQueueInserted(int position)
        {
            if (sections.Count > 0)
            {
                if (MusicPlayer.queue.Count == 1)
                    QueueAdapter?.NotifyItemChanged(0);
                else
                    QueueAdapter?.NotifyItemInserted(position);
            }
        }

        public void NotifyQueueRangeInserted(int position, int count)
        {
            if (sections.Count > 0)
                QueueAdapter?.NotifyItemRangeInserted(position, count);
        }

        public void NotifyQueueChanged(int position, Java.Lang.Object payload)
        {
            if (sections.Count > 0)
                QueueAdapter?.NotifyItemChanged(position, payload);
        }

        public void NotifyQueueRemoved(int position)
        {
            if (sections.Count > 0)
                QueueAdapter?.NotifyItemRemoved(position);
        }

        private void ListView_ItemClick(object sender, int position)
        {
            if(sections[position].contentType == SectionType.Shuffle)
            {
                LocalManager.ShuffleAll();
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            instance = this;

            if(sections.Count > 0)
            {
                sections[0].recycler?.GetAdapter()?.NotifyDataSetChanged();
                if (MusicPlayer.CurrentID() != -1 && MusicPlayer.CurrentID() <= MusicPlayer.queue.Count)
                    sections[0].recycler?.ScrollToPosition(MusicPlayer.CurrentID());
            }
        }
    }
}