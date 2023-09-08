using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using debstDevelopments.HotKeyManager;

namespace BrightnessChanger
{
    public class Config
    {
        private static string FileName = "config";
        private static FileInfo ConfigFile
        {
            get
            {
                return new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName));
            }
        }

        public KeyboardShortcut BrightUpShortcut { get; set; }
        public KeyboardShortcut BrightDownShortcut { get; set; }

        public Config()
        {
            var file = ConfigFile;
            if (file.Exists)
            {
                var saved = File.ReadAllLines(file.FullName);
                Type configType = typeof(Config);
                foreach (string line in saved)
                {
                    var splitted = line.Split('=');
                    var prop = configType.GetProperty(splitted[0]);
                    if (prop != null)
                    {
                        object value = splitted[1];
                        if ((string)value == "null" || string.IsNullOrEmpty((string)value))
                        {
                            value = null;
                        }
                        else if (prop.PropertyType.IsEnum)
                        {
                            value = Enum.Parse(prop.PropertyType, (string) value);
                        }else if (prop.PropertyType == typeof(KeyboardShortcut))
                        {
                            value = new KeyboardShortcut((string)value);
                        }
                        prop.SetValue(this, value);
                    }
                }
            }
            this.Apply();
        }

        public void Apply()
        {
            if (this.BrightUpShortcut != null) HotKeyManager.RegisterHotKey(App.Handler, new KeyboardShortcutAction(App.BRIGHT_UP_HOTKEY_ID, this.BrightUpShortcut, BrightnessManager.BrightUp));
            if (this.BrightDownShortcut != null) HotKeyManager.RegisterHotKey(App.Handler, new KeyboardShortcutAction(App.BRIGHT_DOWN_HOTKEY_ID, this.BrightDownShortcut, BrightnessManager.BrightDown));
        }

        public void Save()
        {
            var builder = new System.Text.StringBuilder();
            foreach (var prop in this.GetType().GetProperties())
            {
                object value = prop.GetValue(this);
                value = value == null ? "null" : value.ToString();
                builder.AppendLine($"{prop.Name}={value}");
            }
            File.WriteAllText(ConfigFile.FullName, builder.ToString());
            this.Apply();
        }
    }
}
