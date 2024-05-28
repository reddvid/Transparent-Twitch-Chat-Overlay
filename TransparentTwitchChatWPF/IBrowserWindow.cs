using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransparentTwitchChatWPF
{
    interface IBrowserWindow
    {
        void HideBorders();
        void DrawBorders();
        void ToggleBorderVisibility();
        void ResetWindowState();

    }
}
