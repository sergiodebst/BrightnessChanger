using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrightnessChanger.KeyboardManager;

namespace BrightnessChanger
{
    public interface IHandleKeyboardHookControl
    {
        bool CanHandleHook { get; }
        void HandleHook(KeyEvent e);
    }
}
