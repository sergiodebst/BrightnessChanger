using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static BrightnessChanger.KeyboardManager;

namespace BrightnessChanger
{
    public class KeyboardShortcutBox : Control, IHandleKeyboardHookControl
    {
        static KeyboardShortcutBox()
        {
            var res = new ResourceDictionary() { Source = new Uri("pack://application:,,,/BrightnessChanger;component/View/Theme/KeyboardShortcutBoxTemplate.xaml", UriKind.Absolute) };
            TemplateProperty.OverrideMetadata(typeof(KeyboardShortcutBox), new FrameworkPropertyMetadata(res[typeof(KeyboardShortcutBox)]));
        }

        public static readonly DependencyProperty KeyboardShortcutProperty = DependencyProperty.Register(nameof(KeyboardShortcut), typeof(KeyboardShortcut), typeof(KeyboardShortcutBox), new FrameworkPropertyMetadata(null, OnKeyboardShortcutPropertyChanged));

        public KeyboardShortcut KeyboardShortcut
        {
            get => (KeyboardShortcut)GetValue(KeyboardShortcutProperty);
            set => SetValue(KeyboardShortcutProperty, value);
        }
        private static void OnKeyboardShortcutPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KeyboardShortcutBox sb = (KeyboardShortcutBox)sender;
            sb.Text = sb.KeyboardShortcut?.ToString();
        }

        private static readonly DependencyPropertyKey TextPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Text), typeof(string), typeof(KeyboardShortcutBox), new FrameworkPropertyMetadata(null));
        private static readonly DependencyProperty TextProperty = TextPropertyKey.DependencyProperty;
        public string Text
        {
            get => (string)GetValue(TextProperty);
            private set => SetValue(TextPropertyKey, value);
        }

        public bool CanHandleHook { get { return this.TextBox != null && this.TextBox.IsFocused; } }

        private TextBox TextBox;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.TextBox = (TextBox) this.Template.FindName("ShortcutBox", this);
        }

        public void HandleHook(KeyEvent e)
        {
            var keyPressed = e.Key;
            var key = keyPressed.Key;
            // Pressing delete, backspace or escape without modifiers clears the current value
            if (!keyPressed.CtrlModifier && !keyPressed.ShiftModifier && !keyPressed.AltModifier && !keyPressed.WinModifier &&
                (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                this.KeyboardShortcut = null;
                e.Handled = true;
                return;
            }

            // If no actual key was pressed - return
            if (key == Key.LeftCtrl ||
                key == Key.RightCtrl ||
                key == Key.LeftAlt ||
                key == Key.RightAlt ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LWin ||
                key == Key.RWin ||
                key == Key.Clear ||
                key == Key.OemClear ||
                key == Key.Apps)
            {
                return;
            }
            else
            {
                // Update the value
                this.KeyboardShortcut =keyPressed;
                e.Handled = true;
            }
        }
    }
}
