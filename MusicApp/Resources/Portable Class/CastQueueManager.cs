﻿using Android.Gms.Cast.Framework.Media;
using MusicApp.Resources.values;

namespace MusicApp.Resources.Portable_Class
{
    public class CastQueueManager : MediaQueue.Callback
    {
        public override void ItemsReloaded()
        {
            base.ItemsReloaded();
            Queue.instance?.adapter.NotifyDataSetChanged();
            Home.instance?.QueueAdapter?.NotifyDataSetChanged();
        }

        public override void ItemsRemovedAtIndexes(int[] indexes)
        {
            base.ItemsRemovedAtIndexes(indexes);
            foreach(int index in indexes)
            {
                Queue.instance?.adapter.NotifyItemRemoved(index);
                Home.instance?.QueueAdapter?.NotifyItemRemoved(index);
            }
        }

        public override void ItemsUpdatedAtIndexes(int[] indexes)
        {
            base.ItemsUpdatedAtIndexes(indexes);
            foreach (int index in indexes)
            {
                Song song = (Song)MusicPlayer.RemotePlayer.MediaQueue.GetItemAtIndex(index);

                if (song == null && (index == MusicPlayer.currentID || index == MusicPlayer.currentID + 1))
                    continue;

                if (MusicPlayer.queue.Count > index)
                    MusicPlayer.queue[index] = song;
                else
                {
                    while (MusicPlayer.queue.Count < index)
                        MusicPlayer.queue.Add(null);

                    MusicPlayer.queue.Add(song);
                }

                if(song != null)
                {
                    Queue.instance?.adapter.NotifyItemChanged(index, song.Title);
                    Home.instance?.QueueAdapter?.NotifyItemChanged(index, song.Title);
                }

                MusicPlayer.WaitForIndex.Remove(index);
            }
        }
    }
}