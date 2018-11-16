﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
//using System.Windows.Forms;
using System.Windows.Automation;
using System.Windows.Forms;
using static WindowsSharp.Processes.ProcessExtensions;

namespace WindowsSharp.Processes
{
    public class ProcessWindow : INotifyPropertyChanged
    {
        private static ProcessWindow _activeWindow = new ProcessWindow(NativeMethods.GetForegroundWindow());
        public static ProcessWindow ActiveWindow
        {
            get => _activeWindow;
            internal set
            {
                _activeWindow = value;
                ActiveWindowChanged?.Invoke(null, new WindowEventArgs(_activeWindow));
            }
        }

        public static event EventHandler<WindowEventArgs> WindowOpened;

        internal static void RaiseWindowOpened(IntPtr hwnd)
        {
            RaiseWindowOpened(hwnd, false);
        }

        internal static void RaiseWindowOpened(IntPtr hwnd, bool isVisibilityChange)
        {
            ProcessWindow.WindowOpened?.Invoke(null, new WindowEventArgs(new ProcessWindow(hwnd))
            {
                IsVisibilityChange = isVisibilityChange
            });
        }

        public static event EventHandler<WindowEventArgs> ActiveWindowChanged;

        public static event EventHandler<WindowEventArgs> WindowClosed;

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal static void RaiseWindowClosed(IntPtr hwnd)
        {
            RaiseWindowOpened(hwnd, false);
        }

        internal static void RaiseWindowClosed(IntPtr hwnd, bool isVisibilityChange)
        {
            ProcessWindow.WindowClosed?.Invoke(null, new WindowEventArgs(new ProcessWindow(hwnd))
            {
                IsVisibilityChange = isVisibilityChange
            });
            //Debug.WriteLine("WindowClosed");
        }

        public static IEnumerable<ProcessWindow> RealProcessWindows
        {
            get
            {
                var collection = new List<IntPtr>();

                Boolean Filter(IntPtr hWnd, Int32 lParam)
                {
                    var strbTitle = new StringBuilder(NativeMethods.GetWindowTextLength(hWnd));
                    NativeMethods.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                    var strTitle = strbTitle.ToString();


                    if (NativeMethods.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                        collection.Add(hWnd);

                    return true;
                }

                if (!NativeMethods.EnumDesktopWindows(IntPtr.Zero, Filter, IntPtr.Zero)) yield break;

                foreach (var hwnd in collection)
                    yield return new ProcessWindow(hwnd);
            }
        }

        static Int32 TASKSTYLE = 0x10000000 | 0x00800000;

        public static IEnumerable<ProcessWindow> ProcessWindows
        {
            /*get
            {
                var collection = new List<IntPtr>();

                Boolean Filter(IntPtr hWnd, Int32 lParam)
                {
                    var strbTitle = new StringBuilder(NativeMethods.GetWindowTextLength(hWnd));
                    NativeMethods.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                    var strTitle = strbTitle.ToString();


                    if (NativeMethods.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                        collection.Add(hWnd);

                    return true;
                }

                if (!NativeMethods.EnumDesktopWindows(IntPtr.Zero, Filter, IntPtr.Zero)) yield break;

                foreach (var hwnd in collection)
                {
                    try
                    {
                        if (IsWindowUserAccessible(hwnd))
                        {
                            yield return new ProcessWindow(hwnd);
                        }
                    }
                    finally
                    {

                    }
                }
            }*/
            get
            {
                var collection = new List<IntPtr>();

                Boolean Filter(IntPtr hWnd, Int32 lParam)
                {
                    var strbTitle = new StringBuilder(NativeMethods.GetWindowTextLength(hWnd));
                    NativeMethods.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                    var strTitle = strbTitle.ToString();


                    if (NativeMethods.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                        collection.Add(hWnd);

                    return true;
                }

                if (!NativeMethods.EnumDesktopWindows(IntPtr.Zero, Filter, IntPtr.Zero)) yield break;

                foreach (var hwnd in collection)
                {
                    try
                    {
                        if (Environment.Is64BitProcess)
                        {
                            if ((TASKSTYLE == (TASKSTYLE & NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlStyle).ToInt64())) && ((NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlExStyle).ToInt64() & NativeMethods.WsExToolWindow) != NativeMethods.WsExToolWindow))
                            {
                                yield return new ProcessWindow(hwnd);
                            }
                        }
                        else
                        {
                            if ((TASKSTYLE == (TASKSTYLE & NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlStyle).ToInt32())) && ((NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlExStyle).ToInt32() & NativeMethods.WsExToolWindow) != NativeMethods.WsExToolWindow))
                            {
                                yield return new ProcessWindow(hwnd);
                            }
                        }
                    }
                    finally
                    {

                    }
                }
            }
        }

        public static bool IsWindowUserAccessible(IntPtr hwnd)
        {
            if (Environment.Is64BitProcess)
                return ((TASKSTYLE == (TASKSTYLE & NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlStyle).ToInt64()))
                    && ((NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlExStyle).ToInt64() & ~NativeMethods.WsExToolWindow) != 0));
            else
                return ((TASKSTYLE == (TASKSTYLE & NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlStyle).ToInt32()))
                    && ((NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlExStyle).ToInt32() & ~NativeMethods.WsExToolWindow) != 0));
        }

        static ProcessWindow()
        {
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowOpenedEvent,
                AutomationElement.RootElement,
                TreeScope.Children,
                (sender, e) => WindowOpened?.Invoke(null,
                                                    new WindowEventArgs(
                                                        new ProcessWindow(new IntPtr(((AutomationElement)sender).Current.NativeWindowHandle))
                                                        )));

            /*Automation.AddAutomationEventHandler(
				WindowPattern.WindowClosedEvent,
				AutomationElement.RootElement,
				TreeScope.Subtree,
				(sender, e) => WindowClosed?.Invoke(null,
													new WindowEventArgs(
														new ProcessWindow(new IntPtr(((AutomationElement) sender).Cached.NativeWindowHandle)))));*/

            System.Timers.Timer timer = new System.Timers.Timer(1);
            timer.Elapsed += (sneder, args) =>
            {
                //Debug.WriteLine("timer elapsed");
                if (ActiveWindow != null)
                {
                    //Debug.WriteLine("active window is non-null");
                    IntPtr newActive = NativeMethods.GetForegroundWindow();

                    if ((ActiveWindow.Handle != newActive) && IsWindowUserAccessible(newActive))
                        ActiveWindow = new ProcessWindow(newActive);
                }
            };
            timer.Start();
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        public Process Process
        {
            get
            {
                NativeMethods.GetWindowThreadProcessId(Handle, out var pid);
                return Process.GetProcessById((Int32)pid);
            }
        }

        string _windowTitle = string.Empty;

        public string Title
        {
            get
            {
                if (_windowTitle == string.Empty)
                    _windowTitle = GetWindowTitle();
                /*var strbTitle = new StringBuilder(NativeMethods.GetWindowTextLength(Handle));
                NativeMethods.GetWindowText(Handle, strbTitle, strbTitle.Capacity + 1);
                return strbTitle.ToString();*/
                return _windowTitle;
            }
            internal set
            {
                /*try
                {*/
                    _windowTitle = value;
                /*}
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }*/
                NotifyPropertyChanged("Title");
            }
        }

        string GetWindowTitle()
        {
            var strbTitle = new StringBuilder(NativeMethods.GetWindowTextLength(Handle));
            NativeMethods.GetWindowText(Handle, strbTitle, strbTitle.Capacity + 1);
            return strbTitle.ToString();
        }

        //Rectangle _windowRectangle = new Rectangle();

        public Rectangle WindowRect
        {
            get
            {
                NativeMethods.RECT rect;
                NativeMethods.GetWindowRect(Handle, out rect);
                Rectangle rectangle = new Rectangle();

                try
                {
                    rectangle = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return rectangle;
            }
            set
            {
                Rectangle rect = value;
                NativeMethods.MoveWindow(Handle, value.X, value.Y, value.Width, value.Height, true);
                NotifyPropertyChanged("WindowRect");
            }
        }

        /// <summary>
        /// Gets the icon of the window, preferring the large variant.
        /// </summary>
		public Icon Icon
        {
            get
            {
                var iconHandle = NativeMethods.SendMessage(Handle, NativeMethods.WmGetIcon, NativeMethods.IconBig, 0);

                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.GetClassLongPtr(Handle, NativeMethods.GclHIcon);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.SendMessage(Handle, NativeMethods.WmGetIcon, NativeMethods.IconSmall, 0);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.SendMessage(Handle, NativeMethods.WmGetIcon, NativeMethods.IconSmall2, 0);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.GetClassLongPtr(Handle, NativeMethods.GclHIconSm);

                if (iconHandle == IntPtr.Zero)
                    return null;

                try
                {
                    return Icon.FromHandle(iconHandle);
                }
                finally
                {
                    NativeMethods.DestroyIcon(iconHandle);
                }
            }
        }

        /// <summary>
        /// Gets the icon of the window, preferring the large variant.
        /// </summary>
		public Icon SmallIcon
        {
            get
            {
                var iconHandle = NativeMethods.SendMessage(Handle, NativeMethods.WmGetIcon, NativeMethods.IconSmall2, 0);

                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.SendMessage(Handle, NativeMethods.WmGetIcon, NativeMethods.IconSmall, 0);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.SendMessage(Handle, NativeMethods.WmGetIcon, NativeMethods.IconBig, 0);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.GetClassLongPtr(Handle, NativeMethods.GclHIcon);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = NativeMethods.GetClassLongPtr(Handle, NativeMethods.GclHIconSm);

                if (iconHandle == IntPtr.Zero)
                    return null;

                try
                {
                    return Icon.FromHandle(iconHandle);
                }
                finally
                {
                    NativeMethods.DestroyIcon(iconHandle);
                }
            }
        }

        public bool IsMinimized
        {
            get
            {
                var placement = GetPlacement();
                return (placement.ShowCmd == NativeMethods.SwShowMinimized);
            }
        }

        public bool IsMaximized
        {
            get
            {
                var placement = GetPlacement();
                return (placement.ShowCmd == NativeMethods.SwShowMaximized);
            }
        }

        public ProcessWindow(IntPtr windowHandle)
        {
            Handle = windowHandle;
            System.Timers.Timer timer = new System.Timers.Timer(10);
            timer.Elapsed += (sneder, args) =>
            {
                if (!NativeMethods.IsWindow(Handle))
                {
                    try
                    {
                        ProcessWindow.WindowClosed?.Invoke(null, new WindowEventArgs(this));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    timer.Stop();
                }
                else
                {
                    string currentTitle = GetWindowTitle();
                    if (Title != currentTitle)
                        Title = currentTitle;

                    //bool visibleNow = NativeMethods.IsWindowVisible(Handle);
                    /*(IsVisible && (!visibleNow))
                    || ((!IsVisible) && visibleNow)
                    )*/
                    /*if (visibleNow != IsWindowVisible)
                    {
                        Debug.WriteLine("IsVisible: " + IsWindowVisible.ToString() + "\nvisibleNow: " + visibleNow.ToString());

                        //visibleNow;

                        if (visibleNow)
                        {
                            IsWindowVisible = true;
                            RaiseWindowOpened(Handle, true);
                        }
                        else
                        {
                            IsWindowVisible = false;
                            RaiseWindowClosed(Handle, true);
                        }
                    }*/
                }
            };
            //SetWinEventHook
            timer.Start();
        }

        public bool Close()
        {
            //Debug.WriteLine("Close()");
            return NativeMethods.PostMessage(Handle, NativeMethods.WmClose, IntPtr.Zero, IntPtr.Zero);
        }

        public Int32 Maximize()
        {
            //Debug.WriteLine("Maximize()");
            return NativeMethods.ShowWindow(Handle, NativeMethods.SwMaximize);
        }

        public Int32 Minimize()
        {
            //Debug.WriteLine("Minimize()");
            return NativeMethods.ShowWindow(Handle, NativeMethods.SwMinimize);
        }
         
        public Int32 Restore()
        {
            //Debug.WriteLine("Restore()");
            return NativeMethods.ShowWindow(Handle, NativeMethods.SwRestore);
        }

        NativeMethods.WINDOWPLACEMENT GetPlacement()
        {
            NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
            placement.Length = Marshal.SizeOf(placement);
            NativeMethods.GetWindowPlacement(this.Handle, ref placement);
            return placement;
        }

        public void Show()
        {
            //Debug.WriteLine("Show()");
            var placement = GetPlacement();

            if (IsMinimized)
                NativeMethods.ShowWindow(Handle, NativeMethods.SwShow);

            NativeMethods.SetForegroundWindow(Handle);

            if (IsMaximized)
                Maximize();
            else
                Restore();

            /*else if ((placement.ShowCmd == NativeMethods.SwMinimize) || (placement.ShowCmd == NativeMethods.SwForceMinimize) || (placement.ShowCmd == NativeMethods.SwShowMinimized) || (placement.ShowCmd == NativeMethods.SwShowMinimizedNoActive))
                NativeMethods.ShowWindow(Handle, NativeMethods.SwShow);*/
        }

        public void ShowSystemMenu()
        {
            IntPtr menu = NativeMethods.GetSystemMenu(Handle, false);
            int command = NativeMethods.TrackPopupMenuEx(menu, 0x0000 | 0x0100, 100, 100, Handle, IntPtr.Zero);
            if (command != 0)
                NativeMethods.PostMessage(Handle, 0x112, new IntPtr(command), IntPtr.Zero);
        }
    }

    internal class ProcessWindowMonitorForm : Form
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterShellHookWindow(IntPtr hWnd);

        int HShellWindowCreated = 1;
        int HShellWindowDestroyed = 2;
        int HShellActivateShellWindow = 3;
        int HShellWindowActivated = 4;
        int HShellGetMinRect = 5;
        int HShellRedraw = 6;
        int HShellTaskMan = 7;
        int HShellLanguage = 8;
        int HShellAccessibilityState = 11;
        int HShellAppCommand = 12;

        private readonly uint notificationMsg;
        public delegate void EventHandler(object sender, string data);
        public event EventHandler WindowEvent;
        protected virtual void OnWindowEvent(string data)
        {
            var handler = WindowEvent;
            if (handler != null)
            {
                handler(this, data);
            }
        }

        internal ProcessWindowMonitorForm()
        {
            notificationMsg = RegisterWindowMessage("SHELLHOOK");
            RegisterShellHookWindow(this.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == notificationMsg)
            base.WndProc(ref m);

            if (m.WParam.ToInt32() == HShellWindowDestroyed)
            {
                ProcessWindow.RaiseWindowClosed(m.HWnd);
            }
            else if (m.WParam.ToInt32() == HShellWindowActivated)
            {
                ProcessWindow.ActiveWindow = new ProcessWindow(m.HWnd);
            }
            else if (m.WParam.ToInt32() == HShellWindowCreated)
            {
                //ProcessWindow.RaiseWindowOpened(m.HWnd);
            }
        }
    }

    public class WindowEventArgs : EventArgs
    {
        public WindowEventArgs(ProcessWindow window)
        {
            Window = window;
        }

        public ProcessWindow Window { get; }
        public bool IsVisibilityChange { get; set; } = false;
    }
}
