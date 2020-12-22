using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightnessChanger
{
    //Original code from https://stackoverflow.com/a/61417175
    public static class BrightnessManager
    {
        static BrightnessManager()
        {
            UpdateMonitors();
        }

        private static int Get()
        {
            
            ManagementClass mclass = null;
            ManagementObjectCollection instances = null;
            try
            {
                if(Monitors.Count > 0)
                {
                    return (int)Monitors.Average(d => d.CurrentValue);
                }
                else
                {
                    mclass = new ManagementClass("WmiMonitorBrightness") { Scope = new ManagementScope(@"\\.\root\wmi") };
                    instances = mclass.GetInstances();
                    foreach (ManagementObject instance in instances)
                    {
                        return (byte)instance.GetPropertyValue("CurrentBrightness");
                    }
                }
                
            }
            catch
            {

            }
            finally
            {
                if (mclass != null) mclass.Dispose();
                if (instances != null) instances.Dispose();
            }
            return 0;
        }

        private static void Set(uint brightness)
        {
            Set(brightness, true);
        }

        private static void Set(uint brightness, bool updateMonitors)
        {

            ManagementClass mclass = null;
            ManagementObjectCollection instances = null;
            try
            {
                if(Monitors.Count > 0)
                {
                    bool failed = false;
                    foreach (var monitor in Monitors)
                    {
                        uint realNewValue = (monitor.MaxValue - monitor.MinValue) * brightness / 100 + monitor.MinValue;
                        if (SetMonitorBrightness(monitor.Handle, realNewValue))
                        {
                            monitor.CurrentValue = realNewValue;
                        }else
                        {
                            failed = true;
                        }
                    }
                    if (failed && updateMonitors)
                    {
                        UpdateMonitors();
                        Set(brightness, false);
                    }
                } else
                {
                    mclass = new ManagementClass("WmiMonitorBrightness") { Scope = new ManagementScope(@"\\.\root\wmi") };
                    instances = mclass.GetInstances();
                    var args = new object[] { 1, brightness };
                    foreach (ManagementObject instance in instances)
                    {
                        instance.InvokeMethod("WmiSetBrightness", args);
                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (mclass != null) mclass.Dispose();
                if (instances != null) instances.Dispose();
            }
        }

        public static void BrightUp()
        {
            var current = Get();
            Set((uint) Math.Min(100, current + 2));
        }

        public static void BrightDown()
        {
            var current = Get();
            Set((uint) Math.Max(0, current - 2));
        }

        #region DllImport
        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorBrightness(IntPtr handle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maxBrightness);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetMonitorBrightness(IntPtr handle, uint newBrightness);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitor")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);
        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);
        #endregion

        private static IReadOnlyCollection<MonitorInfo> Monitors { get; set; }

        
        private static void UpdateMonitors()
        {
            DisposeMonitors(Monitors);

            var monitors = new List<MonitorInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) =>
            {
                uint physicalMonitorsCount = 0;
                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref physicalMonitorsCount))
                {
                    // Cannot get monitor count
                    return true;
                }

                var physicalMonitors = new PHYSICAL_MONITOR[physicalMonitorsCount];
                if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitorsCount, physicalMonitors))
                {
                    // Cannot get phisical monitor handle
                    return true;
                }

                foreach (PHYSICAL_MONITOR physicalMonitor in physicalMonitors)
                {
                    uint minValue = 0, currentValue = 0, maxValue = 0;
                    if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, ref minValue, ref currentValue, ref maxValue))
                    {
                        DestroyPhysicalMonitor(physicalMonitor.hPhysicalMonitor);
                        continue;
                    }

                    var info = new MonitorInfo
                    {
                        Handle = physicalMonitor.hPhysicalMonitor,
                        MinValue = minValue,
                        CurrentValue = currentValue,
                        MaxValue = maxValue,
                    };
                    monitors.Add(info);
                }

                return true;
            }, IntPtr.Zero);

            Monitors = monitors;
        }


        private static void DisposeMonitors(IEnumerable<MonitorInfo> monitors)
        {
            if (monitors?.Any() == true)
            {
                PHYSICAL_MONITOR[] monitorArray = monitors.Select(m => new PHYSICAL_MONITOR { hPhysicalMonitor = m.Handle }).ToArray();
                DestroyPhysicalMonitors((uint)monitorArray.Length, monitorArray);
            }
        }

        #region Classes
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public class MonitorInfo
        {
            public uint MinValue { get; set; }
            public uint MaxValue { get; set; }
            public IntPtr Handle { get; set; }
            public uint CurrentValue { get; set; }
        }
        #endregion
    }
}
