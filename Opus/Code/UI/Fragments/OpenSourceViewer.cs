﻿using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Widget;
using Opus.Adapter;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Opus.Fragments
{
    [Register("Opus/Fragments/OpenSourceViewer")]
    public class OpenSourceViewer : ListFragment
    {
        public static OpenSourceViewer instance;
        private bool isPaused = false;
        /// <summary>
        /// To update this dictionary, run the "Get-Package | Select-Object Id,LicenseUrl" command on the package manager console.
        /// Then copy the output to a notepad and remove every spaces. Then replace http to: '", "http'
        /// Then replace every \n to '{ "'
        /// Then in extended mode (of notepad ++), replace $ to '" },'.
        /// Finaly, past the result below (replace the initialized dictionary and solve the few errors that may be here.
        /// </summary>
        private readonly ReadOnlyDictionary<string, string> libraries = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { "Google.Apis", "https://aka.ms/deprecateLicenseUrl" },
            { "Google.Apis.Auth", "https://aka.ms/deprecateLicenseUrl" },
            { "Google.Apis.Core", "https://aka.ms/deprecateLicenseUrl" },
            { "Google.Apis.YouTube.v3", "https://aka.ms/deprecateLicenseUrl" },
            { "Karamunting.Android.AnderWeb.DiscreteSeekBar", "https://github.com/AnderWeb/discreteSeekBar/blob/master/LICENSE" },
            { "LtGt", "https://licenses.nuget.org/MIT" },
            { "Microsoft.CSharp", "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT" },
            { "Microsoft.NETCore.Platforms", "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT" },
            { "NETStandard.Library", "https://github.com/dotnet/standard/blob/master/LICENSE.TXT" },
            { "Newtonsoft.Json", "https://licenses.nuget.org/MIT" },
            { "PCLCrypto", "https://raw.githubusercontent.com/AArnott/PCLCrypto/313d8a787a/LICENSE" },
            { "PInvoke.BCrypt", "https://raw.githubusercontent.com/AArnott/pinvoke/cf0176c42b/LICENSE" },
            { "PInvoke.Kernel32", "https://raw.githubusercontent.com/AArnott/pinvoke/cf0176c42b/LICENSE" },
            { "PInvoke.NCrypt", "https://raw.githubusercontent.com/AArnott/pinvoke/cf0176c42b/LICENSE" },
            { "PInvoke.Windows.Core", "https://raw.githubusercontent.com/AArnott/pinvoke/cf0176c42b/LICENSE" },
            { "Sprache", "https://github.com/sprache/Sprache/blob/master/licence.txt" },
            { "sqlite-net-pcl", "https://raw.githubusercontent.com/praeclarum/sqlite-net/master/LICENSE.md" },
            { "SQLitePCLRaw.bundle_green", "https://licenses.nuget.org/Apache-2.0" },
            { "SQLitePCLRaw.core", "https://licenses.nuget.org/Apache-2.0" },
            { "SQLitePCLRaw.lib.e_sqlite3.android", "https://licenses.nuget.org/Apache-2.0" },
            { "SQLitePCLRaw.provider.e_sqlite3.android", "https://licenses.nuget.org/Apache-2.0" },
            { "Square.Ok", "https://raw.githubusercontent.com/mattleibow/square-bindings/master/LICENSE" },
            { "Square.OkIO", "https://raw.githubusercontent.com/mattleibow/square-bindings/master/LICENSE" },
            { "Square.Picasso", "https://raw.githubusercontent.com/mattleibow/square-bindings/master/LICENSE" },
            { "System.Collections", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.ComponentModel", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.ComponentModel.TypeConverter", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Console", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Diagnostics.Process", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Globalization", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.IO", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.IO.Compression", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.IO.Compression.ZipFile", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.IO.FileSystem", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.IO.FileSystem.Primitives", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Json", "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT" },
            { "System.Linq", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Linq.Expressions", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Net.", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Net.Primitives", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Net.Requests", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Net.Sockets", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.ObjectModel", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Reflection", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Reflection.Extensions", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Reflection.Primitives", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Reflection.TypeExtensions", "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT" },
            { "System.Resources.ResourceManager", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime.CompilerServices.Unsafe", "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT" },
            { "System.Runtime.Extensions", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime.Handles", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime.InteropServices", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime.InteropServices.RuntimeInformation", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime.Numerics", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime.Serialization.Formatters", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Runtime.Serialization.Primitives", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Security.Claims", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Security.Cryptography.Encoding", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Security.Cryptography.Primitives", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Text.Encoding", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Text.Encoding.CodePages", "https://github.com/dotnet/corefx/blob/master/LICENSE.TXT" },
            { "System.Text.Encoding.Extensions", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Text.RegularExpressions", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Threading", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Threading.Tasks", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Threading.Timer", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Xml.ReaderWriter", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Xml.XDocument", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "System.Xml.XmlDocument", "http://go.microsoft.com/fwlink/?LinkId=329770" },
            { "TagLib.Portable", "https://github.com/timheuer/taglib-sharp-portable/blob/master/LICENSE" },
            { "Validation", "https://raw.githubusercontent.com/AArnott/Validation/912324149e/LICENSE.txt" },
            { "Xam.Plugins.Android.ExoPlayer", "https://raw.githubusercontent.com/martijn00/ExoPlayerXamarin/develop/LICENSE.md" },
            { "Xam.Plugins.Android.ExoPlayer.Core", "https://raw.githubusercontent.com/martijn00/ExoPlayerXamarin/develop/LICENSE.md" },
            { "Xam.Plugins.Android.ExoPlayer.Dash", "https://raw.githubusercontent.com/martijn00/ExoPlayerXamarin/develop/LICENSE.md" },
            { "Xam.Plugins.Android.ExoPlayer.Hls", "https://raw.githubusercontent.com/martijn00/ExoPlayerXamarin/develop/LICENSE.md" },
            { "Xam.Plugins.Android.ExoPlayer.SmoothStreaming", "https://raw.githubusercontent.com/martijn00/ExoPlayerXamarin/develop/LICENSE.md" },
            { "Xam.Plugins.Android.ExoPlayer.UI", "https://raw.githubusercontent.com/martijn00/ExoPlayerXamarin/develop/LICENSE.md" },
            { "Xamarin.Android.Arch.Core.Common", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Arch.Core.Runtime", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Arch.Lifecycle.Common", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Arch.Lifecycle.LiveData", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Arch.Lifecycle.LiveData.Core", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Arch.Lifecycle.Runtime", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Arch.Lifecycle.ViewModel", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Animated.Vector.Drawable", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Annotations", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.AsyncLayoutInflater", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Collections", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Compat", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.CoordinaterLayout", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Core.UI", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Core.Utils", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.CursorAdapter", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.CustomView", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Design", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.DocumentFile", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.DrawerLayout", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Fragment", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Interpolator", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Loader", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.LocalBroadcastManager", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Media.Compat", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Print", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.SlidingPaneLayout", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.SwipeRefreshLayout", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Transition", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.v4", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.v7.AppCompat", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.v7.CardView", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.v7.MediaRouter", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.v7.Palette", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.v7.Preference", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.v7.RecyclerView", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.Vector.Drawable", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.VersionedParcelable", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Android.Support.ViewPager", "https://go.microsoft.com/fwlink/?linkid=865381" },
            { "Xamarin.Build.Download", "https://go.microsoft.com/fwlink/?linkid=864965" },
            { "Xamarin.GooglePlayServices.Auth", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Auth.Api.Phone", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Auth.Base", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Base", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Basement", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Cast", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Cast.Framework", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Flags", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "Xamarin.GooglePlayServices.Tasks", "https://go.microsoft.com/fwlink/?linkid=865373" },
            { "YoutubeExplode", "https://licenses.nuget.org/LGPL-3.0-only" }
        });


        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            instance = this;
            base.OnActivityCreated(savedInstanceState);
            Preferences.instance.SupportActionBar.Title = GetString(Resource.String.opensource);
            //ListView.Divider = null;
            ListView.TextFilterEnabled = true;
            ListView.DividerHeight = 0;
            ListAdapter = new LibrariesAdapter(Preferences.instance, 0, libraries.Keys.ToList());
            ListView.ItemClick += (_, e) => 
            {
                isPaused = true;
                Intent intent = new Intent(Intent.ActionView);
                intent.SetData(Uri.Parse(libraries.ElementAt(e.Position).Value));
                StartActivity(intent);
            };
        }

        public override void OnResume()
        {
            base.OnResume();
            instance = this;
            isPaused = false;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            instance = null;
        }

        public override void OnStop()
        {
            base.OnStop();
            if(!isPaused)
                Preferences.instance.SupportActionBar.Title = GetString(Resource.String.about);
        }
    }
}