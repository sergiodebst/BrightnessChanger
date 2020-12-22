using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrightnessChanger
{
    public class KeyboardShortcutAction
    {
        public int Id { get; private set; }
        public KeyboardShortcut Shortcut { get; private set; }
        public Action Action { get; private set; }
        public KeyboardShortcutAction(int id, KeyboardShortcut shortcut, Action action)
        {
            this.Id = id;
            this.Shortcut = shortcut;
            this.Action = action;
        }

        public int Modifiers()
        {
            // Modifier keys codes: Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            // Compute the addition of each combination of the keys you want to be pressed
            // ALT+CTRL = 1 + 2 = 3 , CTRL+SHIFT = 2 + 4 = 6...
            int mod = 0;
            if (this.Shortcut.AltModifier) mod += 1;
            if (this.Shortcut.CtrlModifier) mod += 2;
            if (this.Shortcut.ShiftModifier) mod += 4;
            if (this.Shortcut.WinModifier) mod += 8;
            return mod;
        }
    }
}
