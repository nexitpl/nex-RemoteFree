using nexRemote.Desktop.Core.Interfaces;
using nexRemote.Shared.Models;
using System;
using System.Drawing;

namespace nexRemote.Desktop.XPlat.Services
{
    public class CursorIconWatcherLinux : ICursorIconWatcher
    {
#pragma warning disable
        public event EventHandler<CursorInfo> OnChange;
#pragma warning restore

        public CursorInfo GetCurrentCursor() => new(null, Point.Empty, "default");
    }
}
