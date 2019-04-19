﻿using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V7.Preferences;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Opus.DataStructure;
using Opus.Resources.values;
using Square.Picasso;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opus.Resources.Portable_Class
{
    public class HomeListAdapter : RecyclerView.Adapter
    {
        public RecyclerView recycler;
        public int listPadding = 0;
        public List<Song> channels;
        public List<PlaylistItem> playlists;
        public List<Song> allItems;
        private readonly bool UseChannel;
        public bool expanded = false;

        public override int ItemCount => UseChannel ? (expanded ? (channels.Count > 3 ? 3 : channels.Count) : channels.Count) : 
            expanded ? (playlists.Count > 3 ? 3 : playlists.Count) : playlists.Count;

        public HomeListAdapter(List<Song> channels, RecyclerView recycler)
        {
            this.recycler = recycler;
            this.channels = channels;
            UseChannel = true;
        }

        public HomeListAdapter(List<PlaylistItem> playlists, RecyclerView recycler)
        {
            this.recycler = recycler;
            this.playlists = playlists;
            UseChannel = false;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            SongHolder holder = (SongHolder)viewHolder;

            if(UseChannel)
            {
                holder.Title.Text = channels[position].Title;
                Picasso.With(Application.Context).Load(channels[position].Album).Placeholder(Resource.Color.background_material_dark).Transform(new CircleTransformation()).Into(holder.AlbumArt);
                holder.ItemView.SetPadding(4, 1, 4, 1);
            }
            else
            {
                holder.Title.Text = playlists[position].Name;
                Picasso.With(Application.Context).Load(playlists[position].ImageURL).Placeholder(Resource.Color.background_material_dark).Resize(400, 400).CenterCrop().Into(holder.AlbumArt);
                if(!holder.RightButtons.FindViewById<ImageButton>(Resource.Id.play).HasOnClickListeners)
                {
                    //Only support local playlists for now.
                    holder.RightButtons.FindViewById<ImageButton>(Resource.Id.play).Click += (sender, e) => { Playlist.PlayInOrder(playlists[position].LocalID); };
                    holder.RightButtons.FindViewById<ImageButton>(Resource.Id.shuffle).Click += (sender, e) => { Playlist.RandomPlay(playlists[position].LocalID, MainActivity.instance); };
                }

                if(MainActivity.Theme == 1)
                {
                    holder.RightButtons.FindViewById<ImageButton>(Resource.Id.play).ImageTintList = ColorStateList.ValueOf(Color.White);
                    holder.RightButtons.FindViewById<ImageButton>(Resource.Id.shuffle).ImageTintList = ColorStateList.ValueOf(Color.White);
                }
                else
                {
                    holder.RightButtons.FindViewById<ImageButton>(Resource.Id.play).ImageTintList = ColorStateList.ValueOf(Color.Black);
                    holder.RightButtons.FindViewById<ImageButton>(Resource.Id.shuffle).ImageTintList = ColorStateList.ValueOf(Color.Black);
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if(UseChannel)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.HomeChannel, parent, false);
                return new SongHolder(itemView, OnClick, OnLongClick);
            }
            else
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.HomePlaylist, parent, false);
                return new SongHolder(itemView, OnClick, OnLongClick);
            }
        }

        void OnClick(int position)
        {
            if(UseChannel)
            {
                //Open a channel view
            }
            else
            {
                MainActivity.instance.contentRefresh.Refresh -= Home.instance.OnRefresh;
                Home.instance = null;
                PlaylistItem playlist = playlists[position];

                if (playlist.SyncState == SyncState.True || playlist.SyncState == SyncState.Loading)
                    MainActivity.instance.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.contentView, PlaylistTracks.NewInstance(playlist.YoutubeID, playlist.LocalID, playlist.Name, playlist.HasWritePermission, true, playlist.Owner, playlist.Count, playlist.ImageURL)).AddToBackStack("Playlist Track").Commit();
                else if (playlist.LocalID != 0 && playlist.LocalID != -1)
                    MainActivity.instance.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.contentView, PlaylistTracks.NewInstance(playlist.LocalID, playlist.Name)).AddToBackStack("Playlist Track").Commit();
                else
                    MainActivity.instance.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.contentView, PlaylistTracks.NewInstance(playlist.YoutubeID, playlist.Name, playlist.HasWritePermission, true, playlist.Owner, playlist.Count, playlist.ImageURL)).AddToBackStack("Playlist Track").Commit();
            }
        }

        void OnLongClick(int position) { }
    }
}