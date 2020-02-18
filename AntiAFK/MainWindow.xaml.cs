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
using System.Net;
using Newtonsoft.Json.Linq;

namespace AntiAFK
{
    public class MyWowItem : INotifyPropertyChanged
    {
        // These fields hold the values for the public properties.  
        private string idValue = String.Empty;
        private string ptrValue = String.Empty;
        private string nameValue = String.Empty;
        private string statusValue = String.Empty;
        private string pidValue = String.Empty;

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

        public string Pid
        {
            get
            {
                return this.pidValue;
            }
            set
            {
                if (value != this.pidValue)
                {
                    this.pidValue = value;
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
                        //Console.WriteLine(Buff.ToString());
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

        private string GetTalk()
        {
            #region 获得json数据
            try
            {
                string url = "https://v1.hitokoto.cn/";
                int timeout = 5000;
                HttpWebResponse response = HttpWebResponseUtility.CreateGetHttpResponse(url, timeout, null, null);
                if (response != null)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        //jsonText为获得的json字符串
                        string jsonText = reader.ReadToEnd();
                        //将json字符串解析成物料类
                        dynamic stuff = JObject.Parse(jsonText);
                        string sentence = stuff.hitokoto;
                        return sentence;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            #endregion
            return "";
        }

        [VMProtect.Begin]
        async Task AvoidOffline(IntPtr winHandle)
        {
            //KeyPress(handle, Keys.OemQuestion, 500);
            //KeyPress(handle, Keys.D, 500);
            //KeyPress(handle, Keys.A, 500);
            //KeyPress(handle, Keys.N, 500);
            //KeyPress(handle, Keys.C, 500);
            //KeyPress(handle, Keys.E, 500);

            //SetForegroundWindow(winHandle);

            /*System.Windows.Forms.Clipboard.SetText("/logout", System.Windows.Forms.TextDataFormat.Text);
            SendKeys.SendWait("~");
            SendKeys.SendWait("^v");
            SendKeys.SendWait("~");*/
            //System.Threading.Thread.Sleep(500); // 暂停半秒钟
            //SendKeys.SendWait("{Enter}");
            //SendKeys.Flush();

            var x = mWowWindowList.Where(ou => ou.Ptr == winHandle.ToString()).Single();
            x.Status = "正在小退中";
            string message = "/logout";
            Keyboard.Messaging.SendChatTextSend(winHandle, message);

            await Task.Delay(30000); // 推迟30秒执行
            //x.Status = VMProtect.SDK.DecryptString("正在登录中");
            x.Status = "正在登录中";

            // 自动向下，选择新的角色
            if (RandomCharacterCheckbox.IsChecked == true)
            {
                x.Status = "自动切换角色开启";
                Keyboard.Messaging.SendVKeys(winHandle, Keyboard.Messaging.VKeys.KEY_DOWN);  // 下键
            }

            await Task.Delay(2000); // 推迟2秒执行
            // 执行选择角色，进入游戏
            Keyboard.Messaging.SendChatTextSend(winHandle, "");  // 回车键

            x.Status = "进入游戏中";

            // 执行登录聊天说话
            if (RandomTalkCheckbox_Normal.IsChecked == true || RandomTalkCheckbox_Shout.IsChecked == true || RandomTalkCheckbox_Group.IsChecked == true)
            {
                await Task.Delay(20000); // 推迟20秒执行
                string s = GetTalk();

                if (RandomTalkCheckbox_Normal.IsChecked == true)
                {
                    Keyboard.Messaging.SendChatTextSend(winHandle, "");  // 发送聊天
                    Keyboard.Messaging.SendChatTextSend(winHandle, s);  // 发送聊天
                    await Task.Delay(2000); // 推迟2秒执行
                }
                if (RandomTalkCheckbox_Shout.IsChecked == true)
                {
                    Keyboard.Messaging.SendChatTextSend(winHandle, "/y " + s);  // 发送聊天
                    await Task.Delay(2000); // 推迟2秒执行
                }
                if (RandomTalkCheckbox_Group.IsChecked == true)
                {
                    Keyboard.Messaging.SendChatTextSend(winHandle, "/g " + s);  // 发送聊天                    
                    await Task.Delay(2000); // 推迟2秒执行
                }
            }

            // 以下方法不靠谱，因为任何一个窗口的激活，都会导致游戏窗口不在Foreground状态中，从而无法发送键盘输入
            /*SetForegroundWindow(winHandle);
            SendKeys.SendWait("{Enter}");
            SendKeys.Flush();
            Console.WriteLine("SEND KEYS TO ENTER");*/
        }

        [VMProtect.Begin]
        async void AntiAFKTimerFunc(object sender, EventArgs e)
        {
            //Console.WriteLine("======AntiAFKTimerFunc=======");
            for (int i = 0; i < mWindows.Count(); i++)
            {
                IntPtr winHandle = mWindows[i];
                if (IsWindow(winHandle)) // 如果是一个有效的窗口Handle
                {
                    await AvoidOffline(winHandle);
                    UpdateInterval();
                }
            }
        }

        [VMProtect.Begin]
        async void UpdateUITimerFunc(object sender, EventArgs e)
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
                        uint pid;
                        GetWindowThreadProcessId(winHandle, out pid);

                        //GameListView.Items.Add(new MyWowItem { Id = (mWindows.Count()+1).ToString(), Ptr = winHandle.ToString(), Name = "魔兽世界" });
                        mWowWindowList.Add(new MyWowItem { Id = (mWindows.Count()).ToString(), Ptr = winHandle.ToString(), Name = "怀旧服 PID:" + pid.ToString(), Status = "准备好了", Pid = pid.ToString() });

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

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

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

            //mAFKTimer.Interval = TimeSpan.FromSeconds(60 * mLogoutInterval); // 默认，5分钟执行一次
            UpdateInterval();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate the dialog box
            ActivateDialog dlg = new ActivateDialog();

            // Configure the dialog box
            dlg.Owner = this;
            dlg.CodeFound += new CodeFoundEventHandler(dlg_CodeFound);

            // Open the dialog box modally 
            dlg.ShowDialog();
        }

        [VMProtect.Begin]
        void dlg_CodeFound(object sender, EventArgs e)
        {

            //Console.WriteLine("==============================");
            // Get the find dialog box that raised the event
            ActivateDialog dlg = (ActivateDialog)sender;

            // Get find results and select found text
            string code = dlg.ActivateCode;
            string sn;
            ActivationStatus status = VMProtect.SDK.ActivateLicense(code, out sn);
            //Console.WriteLine("==============================" + status);
            if (status == ActivationStatus.Ok)
            {
                var fileName = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "\\sn.txt");
                File.WriteAllText(fileName, sn);

                VMProtect.SerialNumberData sd;
                var res = VMProtect.SDK.GetSerialNumberData(out sd);
                if (res)
                {
                    //AppendResultLine("State = {0}", sd.State);
                    //AppendResultLine("User Name = {0}", sd.UserName);
                    //AppendResultLine("E-Mail = {0}", sd.EMail);
                    //AppendResultLine("Date of expiration = {0}", sd.Expires);
                    //AppendResultLine("Max date of build = {0}", sd.MaxBuild);
                    //AppendResultLine("Running time limit = {0} minutes", sd.RunningTime);
                    //AppendResultLine("Length of user data = {0} bytes", sd.UserData.Length);
                    ExpireDateTime.Content = sd.Expires.ToString();
                }
            }
            else if (status == ActivationStatus.NoConnection)
            {
                ExpireDateTime.Content = "网络连接失败，请检查网络";
            }
            else if (status == ActivationStatus.BadReply)
            {
                ExpireDateTime.Content = "激活服务器出现错误，请通知作者";
            }
            else if (status == ActivationStatus.Banned)
            {
                ExpireDateTime.Content = "此激活码已被禁止使用";
            }
            else if (status == ActivationStatus.Corrupted)
            {
                ExpireDateTime.Content = "激活服务出现异常，请通知作者";
            }
            else if (status == ActivationStatus.BadCode)
            {
                ExpireDateTime.Content = "激活失败，请检查激活码是否正确输入";
            }
            else if (status == ActivationStatus.AlreadyUsed)
            {
                ExpireDateTime.Content = "激活码已被使用";
            }
            else if (status == ActivationStatus.SerialUnknown)
            {
                ExpireDateTime.Content = "激活码不存在，请联系作者";
            }
            else if (status == ActivationStatus.Expired)
            {
                ExpireDateTime.Content = "激活码已过期";
            }
            else if (status == ActivationStatus.NotAvailable)
            {
                ExpireDateTime.Content = "激活服务失效了，请通知作者";
            }
            else
            {
                ExpireDateTime.Content = "激活失败，原因未知，请联系作者";
            }
        }

        private void UpdateInterval()
        {
            int i = LogoutIntervalComboBox.SelectedIndex;
            if (i <= 8)
            {
                mLogoutInterval = LogoutIntervalComboBox.SelectedIndex + 2;
            }
            else
            {
                Random random = new Random();
                if (i == 9)
                {
                    int randomInt = random.Next(2, 5);
                    mLogoutInterval = randomInt;
                }
                else if (i == 10)
                {
                    int randomInt = random.Next(5, 10);
                    mLogoutInterval = randomInt;
                }
                else if (i == 11)
                {
                    int randomInt = random.Next(2, 10);
                    mLogoutInterval = randomInt;
                }
            }

            //System.Diagnostics.Debug.WriteLine("间隔时间=" + mLogoutInterval.ToString());

            mAFKTimer.Interval = TimeSpan.FromSeconds(60 * mLogoutInterval);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateInterval();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MyWowItem selected_lvi = this.GameListView.SelectedItem as MyWowItem;
            mWowWindowList.Remove(mWowWindowList.Where(ou => ou.Ptr == selected_lvi.Ptr.ToString()).Single());
            for (int i = 0; i < mWindows.Count(); i++)
            {
                IntPtr winHandle = mWindows[i];
                if (winHandle.ToString() == selected_lvi.Ptr.ToString())
                {
                    mWindows.Remove(winHandle);
                }
            }
        }
    }
}
