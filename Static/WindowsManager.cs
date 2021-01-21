using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightnessChanger
{
    public class WindowsManager
    {
        static WindowsManager()
        {
            var file = MustHookFile;
            MustHookFileWatcher = new FileSystemWatcher(file.DirectoryName, file.Name);
            MustHookFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            MustHookFileWatcher.Changed += OnMustHookFileChanged;
            MustHookFileWatcher.Created += OnMustHookFileChanged;
            MustHookFileWatcher.Deleted += OnMustHookFileChanged;
            MustHookFileWatcher.EnableRaisingEvents = true;
        }

        //https://stackoverflow.com/a/10280800
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private static readonly FileSystemWatcher MustHookFileWatcher;
        private static readonly string MustHookFileName = "hook";
        private static FileInfo MustHookFile
        {
            get
            {
                return new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MustHookFileName));
            }
        }

        delegate void WinEventHookProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        private static WinEventHookProc _handler;
        private static IntPtr CurrentHook = IntPtr.Zero;

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventHookProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hhk);

        private static bool IsCurrentAppActive()
        {
            return App.Handler == GetForegroundWindow();
        }

        private static bool ShouldMonitorActiveAppChange()
        {
            var file = MustHookFile;
            return !IsCurrentAppActive() && file.Exists && !string.IsNullOrEmpty(File.ReadAllText(file.FullName));
        }

        public static void StartMonitoringActiveAppChangesIfNeeded()
        {
            if (ShouldMonitorActiveAppChange() && CurrentHook == IntPtr.Zero)
            {
                _handler = new WinEventHookProc(OnActiveWindowChanged);
                CurrentHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _handler, 0, 0, WINEVENT_OUTOFCONTEXT);
            }
        }

        public static void StopMonitoringActiveAppChanges()
        {
            if (CurrentHook != IntPtr.Zero)
            {
                UnhookWinEvent(CurrentHook);
                CurrentHook = IntPtr.Zero;
                KeyboardManager.EnableSystemKeys();
            }
        }

        private static void OnActiveWindowChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                bool disabled = false;
                var file = MustHookFile;
                if (file.Exists)
                {
                    var processesToHook = File.ReadAllLines(file.FullName);
                    if (processesToHook.Length > 0)
                    {
                        uint pid;
                        GetWindowThreadProcessId(hwnd, out pid);
                        Process p = Process.GetProcessById((int)pid);
                        var activeProcessFile = new FileInfo(p.MainModule.FileName);
                        if (processesToHook.Contains(activeProcessFile.Name))
                        {
                            KeyboardManager.DisableSystemKeys(null);
                            disabled = true;
                        }
                    }
                }
                if (!disabled && !IsCurrentAppActive())
                {
                    KeyboardManager.EnableSystemKeys();
                }
            }
            catch { }
        }

        private static void OnMustHookFileChanged(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (ShouldMonitorActiveAppChange())
                {
                    StartMonitoringActiveAppChangesIfNeeded();
                }
                else
                {
                    StopMonitoringActiveAppChanges();
                }
            }));
        }
    }
}
