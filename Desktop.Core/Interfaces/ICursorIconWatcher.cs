using nexRemoteFree.Shared.Models;
using System;

namespace nexRemoteFree.Desktop.Core.Interfaces
{
    public interface ICursorIconWatcher
    {
        event EventHandler<CursorInfo> OnChange;

        CursorInfo GetCurrentCursor();
    }

}
