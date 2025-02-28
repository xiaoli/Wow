﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Management;  // <=== Add Reference required!!
using Keyboard;
using VMProtect;

namespace AntiAFK
{
    public class MyWowItem : INotifyPropertyChanged
    {
        // These fields hold the values for the public properties.  
        private string idValue = String.Empty;
        private string ptrValue = String.Empty;
        private string nameValue = String.Empty;
        private string statusValue = String.Empty;

        public string Id {
            get
            {
                return this.idValue;
            }
            set
            {
                if (value != this.idValue)
                {
                    this.idValue = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Ptr {
            get
            {
                return this.ptrValue;
            }
            set
            {
                if (value != this.ptrValue)
                {
                    this.ptrValue = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Name {
            get
            {
                return this.nameValue;
            }
            set
            {
                if (value != this.nameValue)
                {
                    this.nameValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Status
        {
            get
            {
                return this.statusValue;
            }
            set
            {
                if (value != this.statusValue)
                {
                    this.statusValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;

        private static List<IntPtr> mWindows;

        private static int mLogoutInterval = 5;
        DispatcherTimer mUITimer = new DispatcherTimer();
        DispatcherTimer mAFKTimer = new DispatcherTimer();

        private static bool ControlAltPressed
        {
            get
            {
                Keys mods = Keys.Control | Keys.Alt; //需要Ctrl和Alt同时按键
                return (System.Windows.Forms.Control.ModifierKeys & mods) == mods;
            }
        }
        private const int nChars = 256;

        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;

        [VMProtect.Begin]
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        [VMProtect.Begin]
        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                Keys key = (Keys)Marshal.ReadInt32(lParam);
                if (key == Keys.D9 && ControlAltPressed)
                {
                    StringBuilder Buff = new StringBuilder(nChars);
                    IntPtr handle = GetForegroundWindow();

                    if (GetWindowText(handle, Buff, nChars) > 0)
                    {
                        Console.WriteLine(Buff.ToString());
                        if (Buff.ToString() == "魔兽世界")
                        {
                            mWindows.Add(handle);  // 把窗口handle加入List
                        }
                    }

                    //Console.WriteLine("获取到游戏窗口");
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        //private TrulyObservableCollection<MyWowItem> _mWowWindowList = new TrulyObservableCollection<MyWowItem>();
        //public TrulyObservableCollection<MyWowItem> mWowWindowList { get { return _mWowWindowList; } }

        private ObservableCollection<MyWowItem> _mWowWindowList = new ObservableCollection<MyWowItem>();
        public ObservableCollection<MyWowItem> mWowWindowList { get { return _mWowWindowList; } }

        [VMProtect.Begin]
        async Task AvoidOffline(IntPtr winHandle)
        {
            while(true)
            {
                // 以下是自动打怪的指令，还不成熟 me@20200109
                /*var x = mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString()).Single();
                x.Status = "正在打怪中";

                var key_5 = new Keyboard.Key(Messaging.VKeys.KEY_5);
                var key_3 = new Keyboard.Key(Messaging.VKeys.KEY_3);
                var key_j = new Keyboard.Key(Messaging.VKeys.KEY_J);
                var key_6 = new Keyboard.Key(Messaging.VKeys.KEY_6);
                var key_w = new Keyboard.Key(Messaging.VKeys.KEY_W);
                var key_q = new Keyboard.Key(Messaging.VKeys.KEY_Q);
                var key_d = new Keyboard.Key(Messaging.VKeys.KEY_D);

                key_5.PressBackground(winHandle);
                await Task.Delay(500);
                key_j.PressBackground(winHandle);
                await Task.Delay(500);
                key_3.PressBackground(winHandle);
                await Task.Delay(1500);
                key_3.PressBackground(winHandle);
                await Task.Delay(1500);
                key_3.PressBackground(winHandle);
                await Task.Delay(1500);
                key_3.PressBackground(winHandle);
                await Task.Delay(1500);
                key_3.PressBackground(winHandle);
                await Task.Delay(1500);
                key_6.PressBackground(winHandle);
                await Task.Delay(1000);
                key_j.PressBackground(winHandle);
                await Task.Delay(1500);

                key_w.PressBackground(winHandle);
                key_q.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_q.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_q.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_q.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_q.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_q.PressBackground(winHandle);

                key_d.PressBackground(winHandle);
                key_d.PressBackground(winHandle);
                key_d.PressBackground(winHandle);
                key_d.PressBackground(winHandle);
                key_d.PressBackground(winHandle);*/



                var x = mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString()).Single();
                x.Status = "正在战场中";

                var key_1 = new Keyboard.Key(Messaging.VKeys.KEY_1);
                var key_5 = new Keyboard.Key(Messaging.VKeys.KEY_5);
                var key_3 = new Keyboard.Key(Messaging.VKeys.KEY_3);
                var key_4 = new Keyboard.Key(Messaging.VKeys.KEY_4);
                var key_j = new Keyboard.Key(Messaging.VKeys.KEY_J);
                var key_6 = new Keyboard.Key(Messaging.VKeys.KEY_6);
                var key_w = new Keyboard.Key(Messaging.VKeys.KEY_W);
                var key_space = new Keyboard.Key(Messaging.VKeys.KEY_SPACE);
                var key_q = new Keyboard.Key(Messaging.VKeys.KEY_Q);
                var key_d = new Keyboard.Key(Messaging.VKeys.KEY_D);

                key_1.PressBackground(winHandle);
                await Task.Delay(1000);
                key_j.PressBackground(winHandle);
                await Task.Delay(3000);
                key_1.PressBackground(winHandle);
                await Task.Delay(1000);
                key_w.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_space.PressBackground(winHandle);
                key_w.PressBackground(winHandle);
                key_space.PressBackground(winHandle);
                await Task.Delay(1000);
                key_q.PressBackground(winHandle);
                key_q.PressBackground(winHandle);
                key_3.PressBackground(winHandle);
                await Task.Delay(1000);
            }
        }

        [VMProtect.Begin]
        async void AntiAFKTimerFunc(object sender, EventArgs e)
        {
            Console.WriteLine("======AntiAFKTimerFunc=======");
            for (int i = 0; i < mWindows.Count(); i++)
            {
                IntPtr winHandle = mWindows[i];
                if (IsWindow(winHandle)) // 如果是一个有效的窗口Handle
                {
                    await AvoidOffline(winHandle);
                }
            }
        }

        [VMProtect.Begin]
        async void UpdateUITimerFunc(object sender, EventArgs e)
        {
            for (int i=0; i<mWindows.Count(); i++)
            {
                IntPtr winHandle = mWindows[i];
                if (IsWindow(winHandle)) // 如果是一个有效的窗口Handle
                {
                    // 查找，如果没有显示出来，则添加至界面显示
                    var found = mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString());
                    if (found.Count() == 0)
                    {
                        //GameListView.Items.Add(new MyWowItem { Id = (mWindows.Count()+1).ToString(), Ptr = winHandle.ToString(), Name = "魔兽世界" });
                        mWowWindowList.Add(new MyWowItem { Id = (mWindows.Count()).ToString(), Ptr = winHandle.ToString(), Name = "魔兽世界怀旧服", Status = "准备好了" });

                        //MyWowItem newItem = new MyWowItem { Id = (mWindows.Count() + 1).ToString(), Ptr = winHandle.ToString(), Name = "魔兽世界" };
                        //System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => this.mWowWindowList.Add(newItem)));

                        // 立即执行一次操作
                        await AvoidOffline(winHandle);
                    }
                }
                else // 否则，删除掉已经关闭/无效的窗口
                {
                    //GameListView.Items.Remove(found);
                    //mWowWindowList.Remove(mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString()).Single());
                    var found = mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString());
                    if (found.Count() > 0)
                    {
                        //mWowWindowList.Remove(mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString()).Single());
                        var x = mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString()).Single();
                        x.Status = "游戏窗口已经关闭";
                    }
                    //mWindows.Remove(winHandle);
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, uint lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public static void KeyPress(IntPtr windowHwnd, Keys key, int sleep = 100)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;

            PostMessage(windowHwnd, WM_KEYDOWN, (int)key, 0x001F0001);
            System.Threading.Thread.Sleep(sleep);
            PostMessage(windowHwnd, WM_KEYUP, (int)key, 0xC01F0001);
        }

        public MainWindow()
        {
            InitializeComponent();

            mWindows = new List<IntPtr>();

            _hookID = SetHook(_proc);

            /*System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = 5000; // In milliseconds
            t.AutoReset = true; // Stops it from repeating
            t.Elapsed += new System.Timers.ElapsedEventHandler(AntiTimer);
            t.Start();*/
            
            mUITimer.Interval = TimeSpan.FromSeconds(0.5); // 1秒执行一次
            mUITimer.Tick += UpdateUITimerFunc;
            mUITimer.Start();
            
            mAFKTimer.Interval = TimeSpan.FromSeconds(mLogoutInterval); // 默认，5秒钟执行一次
            mAFKTimer.Tick += AntiAFKTimerFunc;
 //           mAFKTimer.Start();

            DataContext = mWowWindowList;
        }

        private void CheckSerialNumber()
        {
            var fileName = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "\\sn.txt");

            string sn_content = File.ReadAllText(fileName);

            //VMProtect.SDK.ActivateLicense(sn_content);

            /*var status = VMProtect.SDK.SetSerialNumber(path);
            txtResult.Clear();
            AppendResultLine("VMProtectSetSerialNumber() returned: {0}", status);
            AppendResultLine("");

            var status2 = VMProtect.SDK.GetSerialNumberState();
            AppendResultLine("VMProtectGetSerialNumberState() returned: {0}", status2);
            AppendResultLine("");

            VMProtect.SerialNumberData sd;
            var res = VMProtect.SDK.GetSerialNumberData(out sd);
            AppendResultLine("VMProtectGetSerialNumberData() returned: {0}", res);
            if (res)
            {
                AppendResultLine("State = {0}", sd.State);
                AppendResultLine("User Name = {0}", sd.UserName);
                AppendResultLine("E-Mail = {0}", sd.EMail);
                AppendResultLine("Date of expiration = {0}", sd.Expires);
                AppendResultLine("Max date of build = {0}", sd.MaxBuild);
                AppendResultLine("Running time limit = {0} minutes", sd.RunningTime);
                AppendResultLine("Length of user data = {0} bytes", sd.UserData.Length);
            }*/
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // pass
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mLogoutInterval = LogoutIntervalComboBox.SelectedIndex + 1;
            mAFKTimer.Interval = TimeSpan.FromSeconds(mLogoutInterval);
        }
    }
}
