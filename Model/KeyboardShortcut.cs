using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrightnessChanger
{
    public class KeyboardShortcut
    {
        private const string CTRL_TEXT = "Ctrl";
        private const string SHIFT_TEXT = "Shift";
        private const string ALT_TEXT = "ALT";
        private const string WIN_TEXT = "Win";

        public Key Key;
        public bool CtrlModifier { get; set; }
        public bool ShiftModifier { get; set; }
        public bool AltModifier { get; set; }
        public bool WinModifier { get; set; }

        public KeyboardShortcut(Key key)
        {
            this.Key = key;
        }

        public KeyboardShortcut(string shortcut)
        {
            var parts = shortcut.Split('+').Select((p) => p.Trim()).ToList();
            this.CtrlModifier = parts.Contains(CTRL_TEXT);
            this.ShiftModifier = parts.Contains(SHIFT_TEXT);
            this.AltModifier = parts.Contains(ALT_TEXT);
            this.WinModifier = parts.Contains(WIN_TEXT);

            this.Key = (Key) Enum.Parse(typeof(Key), (string)parts.Last());
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            if (this.CtrlModifier)
                str.Append("Ctrl + ");
            if (this.ShiftModifier)
                str.Append("Shift + ");
            if (this.AltModifier)
                str.Append("Alt + ");
            if (this.WinModifier)
                str.Append("Win + ");

            str.Append(this.Key);

            return str.ToString();
        }
    }
}
