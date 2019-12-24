using System;
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
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using VMProtect;
using Keyboard;

namespace AutoChatting
{
    public class MyWowItem : INotifyPropertyChanged
    {
        // These fields hold the values for the public properties.  
        private string idValue = String.Empty;
        private string ptrValue = String.Empty;
        private string nameValue = String.Empty;
        private string statusValue = String.Empty;

        public string Id
        {
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
        public string Ptr
        {
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
        public string Name
        {
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
                Keyboard.Messaging.VKeys key = (Keyboard.Messaging.VKeys)Marshal.ReadInt32(lParam);

                ModifierKeys mods = ModifierKeys.Control | ModifierKeys.Alt;
                if (key == Keyboard.Messaging.VKeys.KEY_8 && (System.Windows.Input.Keyboard.Modifiers & mods) == mods)
                {
                    StringBuilder Buff = new StringBuilder(nChars);
                    IntPtr handle = GetForegroundWindow();

                    if (GetWindowText(handle, Buff, nChars) > 0)
                    {
                        if (Buff.ToString() == "魔兽世界")
                        {
                            mWindows.Add(handle);  // 把窗口handle加入List
                        }
                    }
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
            var x = mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString()).Single();
            x.Status = "正在发送中";

            var channel = ChattingChannelComboBox.SelectedIndex + 1;
            var content = ChattingContentTextBox.Text;

            string message = "/" + channel + " " + content;
            Keyboard.Messaging.SendChatTextSend(winHandle, message);

            await Task.Delay(1000); // 推迟1秒执行
            x.Status = "发送完毕";
        }

        [VMProtect.Begin]
        async void AntiAFKTimerFunc(object sender, EventArgs e)
        {
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
        void UpdateUITimerFunc(object sender, EventArgs e)
        {
            for (int i = 0; i < mWindows.Count(); i++)
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

            mUITimer.Interval = TimeSpan.FromSeconds(1); // 1秒执行一次
            mUITimer.Tick += UpdateUITimerFunc;
            mUITimer.Start();

            mAFKTimer.Interval = TimeSpan.FromSeconds(60 * 5); // 5分钟执行一次
            mAFKTimer.Tick += AntiAFKTimerFunc;
            mAFKTimer.Start();

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
            mLogoutInterval = ChattingIntervalComboBox.SelectedIndex + 1;
            mAFKTimer.Interval = TimeSpan.FromSeconds(10 * mLogoutInterval);
        }

        [VMProtect.Begin]
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < mWindows.Count(); i++)
            {
                IntPtr winHandle = mWindows[i];
                if (IsWindow(winHandle)) // 如果是一个有效的窗口Handle
                {
                    await AvoidOffline(winHandle);
                }
            }
        }
    }
}
