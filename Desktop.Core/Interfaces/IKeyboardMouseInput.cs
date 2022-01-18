using nexRemoteFree.Desktop.Core.Enums;
using nexRemoteFree.Desktop.Core.Services;

namespace nexRemoteFree.Desktop.Core.Interfaces
{
    public interface IKeyboardMouseInput
    {
        void Init();
        void SendKeyDown(string key);
        void SendKeyUp(string key);
        void SendMouseMove(double percentX, double percentY, Services.Viewer viewer);
        void SendMouseWheel(int deltaY);
        void SendText(string transferText);
        void ToggleBlockInput(bool toggleOn);
        void SetKeyStatesUp();
        void SendMouseButtonAction(int button, ButtonAction buttonAction, double percentX, double percentY, Viewer viewer);
    }
}
