﻿using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Opus.DataStructure;
using Opus.Resources.values;
using Square.Picasso;
using System;
using System.Collections.Generic;

namespace Opus.Resources.Portable_Class
{
    public class PlaylistAdapter : RecyclerView.Adapter
    {
        private List<PlaylistItem> LocalPlaylists = new List<PlaylistItem>();
        private List<PlaylistItem> YoutubePlaylists = new List<PlaylistItem>();
        public bool forkSaved = false;
        public int listPadding;
        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemLongCLick;

        public PlaylistAdapter(List<PlaylistItem> LocalPlaylists, List<PlaylistItem> YoutubePlaylists)
        {
            this.LocalPlaylists = LocalPlaylists;
            this.YoutubePlaylists = YoutubePlaylists;
        }

        public void UpdateElement(int position, PlaylistItem newPlaylist)
        {
            if(position < LocalPlaylists.Count)
                LocalPlaylists[position] = newPlaylist;
            else
                YoutubePlaylists[position] = newPlaylist;

            NotifyItemChanged(position);
        }

        public override int ItemCount => LocalPlaylists.Count + YoutubePlaylists.Count + (forkSaved ? 1 : 0);

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            if (position == 0)
            {
                HeaderHolder holder = (HeaderHolder)viewHolder;
                holder.headerText.Text = MainActivity.instance.Resources.GetString(Resource.String.local_playlists);
            }
            else if (position - LocalPlaylists.Count == 0)
            {
                HeaderHolder holder = (HeaderHolder)viewHolder;
                holder.headerText.Text = MainActivity.instance.Resources.GetString(Resource.String.youtube_playlists);
            }
            else if (position == 1 && LocalPlaylists[1].Name == "EMPTY")
            {
                EmptyHolder holder = (EmptyHolder)viewHolder;
                holder.text.Text = LocalPlaylists[1].Owner;
            }
            else if (position - LocalPlaylists.Count == 1 && YoutubePlaylists[1].Name == "EMPTY")
            {
                EmptyHolder holder = (EmptyHolder)viewHolder;
                holder.text.Text = YoutubePlaylists[1].Owner;
            }
            else if (position == LocalPlaylists.Count + YoutubePlaylists.Count)
            {
                UslessHolder holder = (UslessHolder)viewHolder;
                if (MainActivity.Theme == 1)
                    ((CardView)viewHolder.ItemView).SetCardBackgroundColor(Color.ParseColor("#212121"));
                else
                    ((CardView)viewHolder.ItemView).SetCardBackgroundColor(Color.White);

                holder.ItemView.FindViewById<ImageView>(Resource.Id.icon).SetImageResource(Resource.Drawable.PlaylistAdd);
                holder.ItemView.FindViewById<TextView>(Resource.Id.text).Text = MainActivity.instance.GetString(Resource.String.add_playlist);

                float scale = MainActivity.instance.Resources.DisplayMetrics.Density;
                if (position + 1 == YoutubePlaylists.Count + LocalPlaylists.Count)
                    holder.ItemView.SetPadding((int)(8 * scale + 0.5f), (int)(8 * scale + 0.5f), (int)(8 * scale + 0.5f), listPadding);
                else
                    holder.ItemView.SetPadding((int)(8 * scale + 0.5f), (int)(8 * scale + 0.5f), (int)(8 * scale + 0.5f), (int)(8 * scale + 0.5f));
            }
            else if(position >= LocalPlaylists.Count && YoutubePlaylists[position - LocalPlaylists.Count].Name == "Loading" && YoutubePlaylists[position - LocalPlaylists.Count].YoutubeID == null) { }
            else if(position >= LocalPlaylists.Count && YoutubePlaylists[position - LocalPlaylists.Count].Name == "Error" && YoutubePlaylists[position - LocalPlaylists.Count].YoutubeID == null)
            {
                EmptyHolder holder = (EmptyHolder)viewHolder;
                holder.text.Text = MainActivity.instance.Resources.GetString(Resource.String.youtube_loading_error);
                holder.text.SetTextColor(Color.Red);
            }
            else if (LocalPlaylists.Count >= position)
            {
                TwoLineHolder holder = (TwoLineHolder) viewHolder;
                holder.Line1.Text = LocalPlaylists[position].Name;
                holder.Line2.Text = LocalPlaylists[position].Count.ToString() + " " + (LocalPlaylists[position].Count < 2 ? MainActivity.instance.GetString(Resource.String.element) : MainActivity.instance.GetString(Resource.String.elements));

                if (!holder.more.HasOnClickListeners)
                {
                    holder.more.Click += (sender, e) =>
                    {
                        Playlist.instance.More(holder.AdapterPosition);
                    };
                }

                if (LocalPlaylists[position].SyncState == SyncState.Error)
                {
                    holder.sync.Visibility = ViewStates.Visible;
                    if (MainActivity.Theme == 1)
                        holder.sync.SetColorFilter(Color.White);
                }
                else
                {
                    holder.sync.Visibility = ViewStates.Gone;
                }

                if (MainActivity.Theme == 1)
                {
                    holder.more.SetColorFilter(Color.White);
                    holder.Line1.SetTextColor(Color.White);
                    holder.Line2.SetTextColor(Color.White);
                    holder.Line2.Alpha = 0.7f;
                }
            }
            else if (position > LocalPlaylists.Count && YoutubePlaylists.Count >= position - LocalPlaylists.Count)
            {
                PlaylistHolder holder = (PlaylistHolder)viewHolder;
                PlaylistItem playlist = YoutubePlaylists[position - LocalPlaylists.Count];

                holder.Title.Text = playlist.Name;
                holder.Owner.Text = playlist.Owner;
                Picasso.With(Application.Context).Load(playlist.ImageURL).Placeholder(Resource.Color.background_material_dark).Transform(new RemoveBlackBorder(true)).Into(holder.AlbumArt);

                if (playlist.HasWritePermission)
                {
                    holder.edit.Visibility = ViewStates.Visible;
                    if (MainActivity.Theme == 1)
                        holder.edit.SetColorFilter(Color.White);
                }
                else
                    holder.edit.Visibility = ViewStates.Gone;

                if (playlist.SyncState == SyncState.Loading || Downloader.queue.Find(x => x.playlist == playlist.Name && (x.State == DownloadState.Downloading || x.State == DownloadState.Initialization || x.State == DownloadState.MetaData || x.State == DownloadState.None)) != null)
                {
                    holder.sync.Visibility = ViewStates.Gone;
                    holder.SyncLoading.Visibility = ViewStates.Visible;
                    if (MainActivity.Theme == 1)
                        holder.SyncLoading.IndeterminateTintList = ColorStateList.ValueOf(Color.White);
                }
                else if (playlist.SyncState == SyncState.True)
                {
                    holder.sync.SetImageResource(Resource.Drawable.Sync);
                    holder.sync.Visibility = ViewStates.Visible;
                    holder.SyncLoading.Visibility = ViewStates.Gone;
                    if (MainActivity.Theme == 1)
                        holder.sync.SetColorFilter(Color.White);
                }
                else if (playlist.SyncState == SyncState.Error)
                {
                    holder.sync.SetImageResource(Resource.Drawable.SyncError);
                    holder.sync.Visibility = ViewStates.Visible;
                    holder.SyncLoading.Visibility = ViewStates.Gone;
                    if (MainActivity.Theme == 1)
                        holder.sync.SetColorFilter(Color.White);
                }
                else if (playlist.SyncState == SyncState.False)
                {
                    holder.sync.Visibility = ViewStates.Gone;
                    holder.SyncLoading.Visibility = ViewStates.Gone;
                }

                if (!holder.more.HasOnClickListeners)
                {
                    holder.more.Click += (sender, e) =>
                    {
                        Playlist.instance.More(holder.AdapterPosition);
                    };
                }

                if (MainActivity.Theme == 1)
                {
                    holder.more.SetColorFilter(Color.White);
                    holder.Title.SetTextColor(Color.White);
                    holder.Owner.SetTextColor(Color.White);
                    holder.Owner.Alpha = 0.7f;
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == 0)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PreferenceCategory, parent, false);
                return new HeaderHolder(itemView, null, null);
            }
            else if(viewType == 1)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.TwoLineLayout, parent, false);
                return new TwoLineHolder(itemView, OnClick, OnLongClick);
            }
            else if(viewType == 2)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.smallLoading, parent, false);
                return new UslessHolder(itemView);
            }
            else if(viewType == 3)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PlaylistItem, parent, false);
                return new PlaylistHolder(itemView, OnClick, OnLongClick);
            }
            else if (viewType == 4)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ShuffleButton, parent, false);
                return new UslessHolder(itemView, OnClick);
            }
            else
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EmptyListCategory, parent, false);
                return new EmptyHolder(itemView);
            }
        }

        public override int GetItemViewType(int position)
        {
            if (position == 0 || position - LocalPlaylists.Count == 0)
                return 0;
            else if (LocalPlaylists.Count >= position && (LocalPlaylists.Count > 2 || LocalPlaylists[1].Name != "EMPTY"))
                return 1;
            else if (position == LocalPlaylists.Count + YoutubePlaylists.Count)
                return 4;
            else if (position >= LocalPlaylists.Count && YoutubePlaylists[position - LocalPlaylists.Count].Name == "Loading" && YoutubePlaylists[position - LocalPlaylists.Count].YoutubeID == null)
                return 2;
            else if (position >= LocalPlaylists.Count && (YoutubePlaylists[position - LocalPlaylists.Count].Name == "Error" || YoutubePlaylists[position - LocalPlaylists.Count].Name == "EMPTY") && YoutubePlaylists[position - LocalPlaylists.Count].YoutubeID == null)
                return 5;
            else if (position > LocalPlaylists.Count && position < LocalPlaylists.Count + YoutubePlaylists.Count)
                return 3;
            else
                return 5;
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        void OnLongClick(int position)
        {
            ItemLongCLick?.Invoke(this, position);
        }
    }
}