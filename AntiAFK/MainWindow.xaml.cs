using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using VMProtect;
using System.Net;
using Newtonsoft.Json.Linq;
using Tesseract;
using System.Drawing;
using System.Drawing.Imaging;
using Notifications.Wpf;

namespace AntiAFK
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const bool mDebug = false;

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_CLOSE = 0x0010;

        private const int W_WIDTH = 800;
        private const int W_HEIGHT = 600;

        private static List<IntPtr> mWindows = new List<IntPtr>();

        // 微信窗口
        private static IntPtr mWeChatWindow = IntPtr.Zero;
        // 战网窗口
        private static IntPtr mBattleNetWindow = IntPtr.Zero;
        // 魔兽窗口
        private static IntPtr mWowWindow = IntPtr.Zero;

        private static int mLogoutInterval = 5;
        DispatcherTimer mUITimer = new DispatcherTimer();
        DispatcherTimer mAFKTimer = new DispatcherTimer();
        DispatcherTimer mDetectorTimer = new DispatcherTimer();

        NotificationManager mNotificationManager;

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

        // 控制台日志显示
        private void LoggingIt(string s)
        {
            string c = string.Format("{0} {1}{2}", DateTime.Now.ToString(), s, Environment.NewLine);
            LogConentTextBox.AppendText(c);
        }

        private Bitmap BitmapImage2Bitmap(System.Windows.Media.Imaging.BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                System.Windows.Media.Imaging.BitmapEncoder enc = new System.Windows.Media.Imaging.BmpBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        async Task DetectDropLineImage(IntPtr winHandle)
        {
            try
            {
                // 完整的屏幕截图
                Bitmap s = CaptureWindow(winHandle);

                //string bitmapPath = System.IO.Path.Combine(Environment.CurrentDirectory) + "\\sample.png";
                //var bitmap = new Bitmap(bitmapPath);
                var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://application:,,,/"
                            //+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
                            //+ ";component/Resources/"
                            + "sample.png", UriKind.Absolute));

                //var bitmap = new Bitmap(@"sample.png");

                System.Drawing.Point p = new System.Drawing.Point(s.Width, s.Height);

                await Task.Delay(500); // 推迟500ms执行

                List<System.Drawing.Point> pl = FindPicture(BitmapImage2Bitmap(bitmap), s, 50, new System.Drawing.Rectangle(), 0.7);

                if (pl.Count() > 0)
                {
                    string c = "我尊贵的主人，游戏账号掉线啦！";
                    LoggingIt(c);

                    /*if (AutoWechatCheckbox.IsChecked == true && mWeChatWindow != IntPtr.Zero && IsWindow(mWeChatWindow))
                    {
                        await Task.Delay(1000);
                        SetForegroundWindow(mWeChatWindow);
                        await Task.Delay(1000);
                        
                        SendKeys.SendWait(DateTime.Now.ToString() + " " + "我的魔兽游戏掉线了!");
                        SendKeys.SendWait("{Enter}");
                        SendKeys.Flush();
                    }*/

                    if (AutoReloginCheckbox.IsChecked == true && mBattleNetWindow != IntPtr.Zero && IsWindow(mBattleNetWindow))
                    {
                        LoggingIt("正在自动重新登录游戏中...");

                        SendMessage(winHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                        await Task.Delay(1000);
                        SetForegroundWindow(mBattleNetWindow);
                        await Task.Delay(1000);

                        SendKeys.SendWait("{Enter}");
                        SendKeys.Flush();
                    }
                    
                    if (AutoDesktopNotificationCheckbox.IsChecked == true)
                    {
                        mNotificationManager.Show(new NotificationContent
                        {
                            Title = "魔兽世界怀旧服",
                            Message = c,
                            Type = NotificationType.Warning
                        });
                    }
                }
            }
            catch
            {
                //Console.WriteLine("Some errors ...");
            }
            
        }

        [VMProtect.Begin]
        async Task AvoidOffline(IntPtr winHandle)
        {
            string destPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"sn.txt");
            try
            {
                var sn = File.ReadAllText(destPath);
                VMProtect.SDK.SetSerialNumber(sn);
                VMProtect.SerialNumberData sd;
                var res = VMProtect.SDK.GetSerialNumberData(out sd);

                var status = VMProtect.SDK.GetSerialNumberState();

                //System.Windows.MessageBox.Show("res" + res.ToString());
                //System.Windows.MessageBox.Show("status" + status.ToString());

                if (res)
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
                    
                    LoggingIt("正在小退中");

                    string message = "/logout";
                    Keyboard.Messaging.SendChatTextSend(winHandle, message);

                    await Task.Delay(30000); // 推迟30秒执行
                    LoggingIt("正在登录角色中");

                    // 自动向下，选择新的角色
                    if (RandomCharacterCheckbox.IsChecked == true)
                    {
                        LoggingIt("自动切换下一个角色");
                        Keyboard.Messaging.SendVKeys(winHandle, Keyboard.Messaging.VKeys.KEY_DOWN);  // 下键
                    }

                    await Task.Delay(2000); // 推迟2秒执行
                                            // 执行选择角色，进入游戏
                    Keyboard.Messaging.SendChatTextSend(winHandle, "");  // 回车键
                    
                    LoggingIt("进入游戏中");

                    // 执行登录聊天说话
                    if (RandomTalkCheckbox_Normal.IsChecked == true || RandomTalkCheckbox_Shout.IsChecked == true || RandomTalkCheckbox_Group.IsChecked == true || RandomTalkCheckbox_Team.IsChecked == true)
                    {
                        await Task.Delay(20000); // 推迟20秒执行

                        LoggingIt("发送聊天消息");

                        string s = TalkConentTextBox.Text;
                        if (RandomTalkContentCheckbox_Robot.IsChecked == true)
                        {
                            s = GetTalk();
                        }

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
                        if (RandomTalkCheckbox_Team.IsChecked == true)
                        {
                            Keyboard.Messaging.SendChatTextSend(winHandle, "/p " + s);  // 发送聊天                    
                            await Task.Delay(2000); // 推迟2秒执行
                        }
                    }

                    // 以下方法不靠谱，因为任何一个窗口的激活，都会导致游戏窗口不在Foreground状态中，从而无法发送键盘输入
                    /*SetForegroundWindow(winHandle);
                    SendKeys.SendWait("{Enter}");
                    SendKeys.Flush();
                    Console.WriteLine("SEND KEYS TO ENTER");*/
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        #region 找图
        /// <summary>
        /// 查找图片，不能镂空
        /// </summary>
        /// <param name="subBitmap">要查找坐标的小图</param>
        /// <param name="parBitmap">在哪个大图里查找</param>
        /// <param name="errorRange">容错，单个色值范围内视为正确0~255</param>
        /// <param name="searchRect">如果为empty，则默认查找整个图像</param>
        /// <param name="matchRate">图片匹配度，默认90%</param>
        /// <param name="isFindAll">是否查找所有相似的图片</param>
        /// <returns>返回查找到的图片的中心点坐标</returns>
        public List<System.Drawing.Point> FindPicture(Bitmap subBitmap, Bitmap parBitmap, byte errorRange = 0, Rectangle searchRect = new System.Drawing.Rectangle(), double matchRate = 0.9, bool isFindAll = false)
        {
            List<System.Drawing.Point> ListPoint = new List<System.Drawing.Point>();
            int subWidth = subBitmap.Width;
            int subHeight = subBitmap.Height;
            int parWidth = parBitmap.Width;
            int parHeight = parBitmap.Height;
            if (searchRect.IsEmpty)
            {
                searchRect = new Rectangle(0, 0, parBitmap.Width, parBitmap.Height);
            }
            var searchLeftTop = searchRect.Location;
            var searchSize = searchRect.Size;
            System.Drawing.Color startPixelColor = subBitmap.GetPixel(0, 0);
            var subData = subBitmap.LockBits(new Rectangle(0, 0, subBitmap.Width, subBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var parData = parBitmap.LockBits(new Rectangle(0, 0, parBitmap.Width, parBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var byteArrarySub = new byte[subData.Stride * subData.Height];
            var byteArraryPar = new byte[parData.Stride * parData.Height];
            Marshal.Copy(subData.Scan0, byteArrarySub, 0, subData.Stride * subData.Height);
            Marshal.Copy(parData.Scan0, byteArraryPar, 0, parData.Stride * parData.Height);
            var iMax = searchLeftTop.Y + searchSize.Height - subData.Height;//行
            var jMax = searchLeftTop.X + searchSize.Width - subData.Width;//列
            int smallOffsetX = 0, smallOffsetY = 0;
            int smallStartX = 0, smallStartY = 0;
            int pointX = -1; int pointY = -1;
            for (int i = searchLeftTop.Y; i < iMax; i++)
            {
                for (int j = searchLeftTop.X; j < jMax; j++)
                {
                    //大图x，y坐标处的颜色值
                    int x = j, y = i;
                    int parIndex = i * parWidth * 4 + j * 4;
                    var colorBig = System.Drawing.Color.FromArgb(byteArraryPar[parIndex + 3], byteArraryPar[parIndex + 2], byteArraryPar[parIndex + 1], byteArraryPar[parIndex]);
                    ;
                    if (ColorAEqualColorB(colorBig, startPixelColor, errorRange))
                    {
                        smallStartX = x - smallOffsetX;//待找的图X坐标
                        smallStartY = y - smallOffsetY;//待找的图Y坐标
                        int sum = 0;//所有需要比对的有效点
                        int matchNum = 0;//成功匹配的点
                        for (int m = 0; m < subHeight; m++)
                        {
                            for (int n = 0; n < subWidth; n++)
                            {
                                int x1 = n, y1 = m;
                                int subIndex = m * subWidth * 4 + n * 4;
                                var color = System.Drawing.Color.FromArgb(byteArrarySub[subIndex + 3], byteArrarySub[subIndex + 2], byteArrarySub[subIndex + 1], byteArrarySub[subIndex]);
                                sum++;
                                int x2 = smallStartX + x1, y2 = smallStartY + y1;
                                int parReleativeIndex = y2 * parWidth * 4 + x2 * 4;//比对大图对应的像素点的颜色
                                var colorPixel = System.Drawing.Color.FromArgb(byteArraryPar[parReleativeIndex + 3], byteArraryPar[parReleativeIndex + 2], byteArraryPar[parReleativeIndex + 1], byteArraryPar[parReleativeIndex]);
                                if (ColorAEqualColorB(colorPixel, color, errorRange))
                                {
                                    matchNum++;
                                }
                            }
                        }
                        if ((double)matchNum / sum >= matchRate)
                        {
                            //Console.WriteLine((double)matchNum / sum);
                            pointX = smallStartX + (int)(subWidth / 2.0);
                            pointY = smallStartY + (int)(subHeight / 2.0);
                            var point = new System.Drawing.Point(pointX, pointY);
                            if (!ListContainsPoint(ListPoint, point, 10))
                            {
                                ListPoint.Add(point);
                            }
                            if (!isFindAll)
                            {
                                goto FIND_END;
                            }
                        }
                    }
                    //小图x1,y1坐标处的颜色值
                }
            }
        FIND_END:
            subBitmap.UnlockBits(subData);
            parBitmap.UnlockBits(parData);
            subBitmap.Dispose();
            parBitmap.Dispose();
            GC.Collect();
            return ListPoint;
        }
        #endregion
        public bool ColorAEqualColorB(System.Drawing.Color colorA, System.Drawing.Color colorB, byte errorRange = 10)
        {
            return colorA.A <= colorB.A + errorRange && colorA.A >= colorB.A - errorRange &&
                colorA.R <= colorB.R + errorRange && colorA.R >= colorB.R - errorRange &&
                colorA.G <= colorB.G + errorRange && colorA.G >= colorB.G - errorRange &&
                colorA.B <= colorB.B + errorRange && colorA.B >= colorB.B - errorRange;
        }
        public bool ListContainsPoint(List<System.Drawing.Point> listPoint, System.Drawing.Point point, double errorRange = 10)
        {
            bool isExist = false;
            foreach (var item in listPoint)
            {
                if (item.X <= point.X + errorRange && item.X >= point.X - errorRange && item.Y <= point.Y + errorRange && item.Y >= point.Y - errorRange)
                {
                    isExist = true;
                }
            }
            return isExist;
        }

        [VMProtect.Begin]
        async void AntiAFKTimerFunc(object sender, EventArgs e)
        {
            if (mDebug)
                LoggingIt("===AntiAFKTimerFunc START===");

            if (AutoExitReturnCheckbox.IsChecked == true)
            {
                if (IsWindow(mWowWindow)) // 如果是一个有效的窗口Handle
                {
                    LoggingIt("AntiAFKTimerFunc");
                    await AvoidOffline(mWowWindow);
                    UpdateInterval();
                }
            }

            if (mDebug)
                LoggingIt("===AntiAFKTimerFunc END===");
        }

        [VMProtect.Begin]
        async void UpdateUITimerFunc(object sender, EventArgs e)
        {
            // 如果魔兽窗口没找到，或者窗口已经关闭
            if (!IsWindow(mWowWindow) || !IsWindow(mBattleNetWindow))
            {
                foreach (Process pList in Process.GetProcesses())
                {
                    if (pList.MainWindowTitle.Contains("暴雪战网"))
                    {
                        mBattleNetWindow = pList.MainWindowHandle;
                        LoggingIt("获取到战网客户端");
                    }
                    if (pList.MainWindowTitle.Contains("魔兽世界") && !IsWindow(mWowWindow)) // 防止多个窗口，赋值覆盖
                    {
                        mWowWindow = pList.MainWindowHandle;
                        LoggingIt("获取到游戏客户端");

                        // 设置指定窗口大小
                        SetWindowPos(mWowWindow, 0, 0, 0, W_WIDTH, W_HEIGHT, 0x46);
                    }
                }
            }
        }

        [VMProtect.Begin]
        async void DetectorTimerFunc(object sender, EventArgs e)
        {
            if (IsWindow(mWowWindow)) // 如果是一个有效的窗口Handle
            {
                await DetectDropLineImage(mWowWindow);
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
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, uint lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32")]
        private static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int w, int h, int flag);

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

            string destPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"sn.txt");
            try
            {
                var sn = File.ReadAllText(destPath);
                var status = VMProtect.SDK.SetSerialNumber(sn);
                VMProtect.SerialNumberData sd;
                var res = VMProtect.SDK.GetSerialNumberData(out sd);
                if (res)
                {
                    ExpireDateTime.Content = sd.Expires.ToString();
                }
            }
            catch (FileNotFoundException)
            {
            }

            // 改单机版了，无需再hook了 @ 20200303
            //_hookID = SetHook(_proc);

            mUITimer.Interval = TimeSpan.FromSeconds(5); // 5秒执行一次
            mUITimer.Tick += UpdateUITimerFunc;
            mUITimer.Start();

            //mAFKTimer.Interval = TimeSpan.FromSeconds(60 * mLogoutInterval); // 默认，5分钟执行一次
            UpdateInterval();
            mAFKTimer.Tick += AntiAFKTimerFunc;
            mAFKTimer.Start();

            mDetectorTimer.Interval = TimeSpan.FromSeconds(15); // 15秒一次
            mDetectorTimer.Tick += DetectorTimerFunc;
            mDetectorTimer.Start();

            mNotificationManager = new NotificationManager();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // pass
        }

        [VMProtect.Begin]
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
                string destPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"sn.txt");
                File.WriteAllText(destPath, sn);

                var s = VMProtect.SDK.SetSerialNumber(sn);
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

        public enum DwFilterFlag : uint
        {
            LIST_MODULES_DEFAULT = 0x0,    // This is the default one app would get without any flag.
            LIST_MODULES_32BIT = 0x01,   // list 32bit modules in the target process.
            LIST_MODULES_64BIT = 0x02,   // list all 64bit modules. 32bit exe will be stripped off.
            LIST_MODULES_ALL = (LIST_MODULES_32BIT | LIST_MODULES_64BIT)   // list all the modules
        }

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [Out] IntPtr lphModule,
            UInt32 cb,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpcbNeeded,
            DwFilterFlag dwff);


        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(
            IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        private static String CutPath(String path)
        {
            int last = path.LastIndexOf("\\");
            return path.Substring(last + 1, path.Length - last - 1);
        }
        /// <summary>
        /// Returns the HMODULE, or module base address
        /// </summary>
        /// <param name="pid">IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, pid);</param>
        /// <param name="moduleName"></param>
        public static IntPtr RemoteGetModuleHandleA(IntPtr hProcess, string moduleName)
        {
            IntPtr moduleBase = IntPtr.Zero;
            uint[] modsInt = new uint[1024];

            // Setting up the variable for the second argument for EnumProcessModules
            IntPtr[] hMods = new IntPtr[1024];

            GCHandle gch = GCHandle.Alloc(hMods, GCHandleType.Pinned); // Don't forget to free this later
            IntPtr pModules = gch.AddrOfPinnedObject();

            // Setting up the rest of the parameters for EnumProcessModules
            uint uiSize = (uint)(Marshal.SizeOf(typeof(IntPtr)) * (hMods.Length));
            uint cbNeeded = 0;

            if (EnumProcessModulesEx(hProcess, pModules, uiSize, out cbNeeded, DwFilterFlag.LIST_MODULES_ALL) == true)
            {

                Int32 uiTotalNumberofModules = (Int32)(cbNeeded / (Marshal.SizeOf(typeof(IntPtr))));

                for (int i = 0; i < (int)uiTotalNumberofModules; i++)
                {
                    StringBuilder strbld = new StringBuilder(1024);

                    GetModuleFileNameEx(hProcess, hMods[i], strbld, (int)(strbld.Capacity));

                    String module = strbld.ToString();
                    String processModuleName = CutPath(module);
                    //Console.WriteLine("Comparing {0} {1}", processModuleName, moduleName);
                    if (processModuleName.Equals(moduleName))
                    {
                        moduleBase = hMods[i];
                        break;
                    }
                }
            }

            // Must free the GCHandle object
            gch.Free();

            return moduleBase;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess,
        IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        static void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("byte[] = ");

            foreach (var b in bytes)
                sb.Append(b + ", ");

            //Console.WriteLine(sb.ToString());
        }

        private static SlimDX.Direct3D9.Direct3D _direct3D9 = new SlimDX.Direct3D9.Direct3D();
        private static Dictionary<IntPtr, SlimDX.Direct3D9.Device>_direct3DDeviceCache = new Dictionary<IntPtr, SlimDX.Direct3D9.Device>();
 
        public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            return CaptureRegionDirect3D(hWnd, NativeMethods.GetAbsoluteClientRect(hWnd));
        }

        public static Bitmap CaptureRegionDirect3D(IntPtr handle, Rectangle region)
        {
            IntPtr hWnd = handle;
            Bitmap bitmap = null;

            // We are only supporting the primary display adapter for Direct3D mode
            SlimDX.Direct3D9.AdapterInformation adapterInfo = _direct3D9.Adapters.DefaultAdapter;
            SlimDX.Direct3D9.Device device;

            #region Get Direct3D Device
            // Retrieve the existing Direct3D device if we already created one for the given handle
            if (_direct3DDeviceCache.ContainsKey(hWnd))
            {
                device = _direct3DDeviceCache[hWnd];
            }
            // We need to create a new device
            else
            {
                // Setup the device creation parameters
                SlimDX.Direct3D9.PresentParameters parameters = new SlimDX.Direct3D9.PresentParameters();
                parameters.BackBufferFormat = adapterInfo.CurrentDisplayMode.Format;
                Rectangle clientRect = NativeMethods.GetAbsoluteClientRect(hWnd);
                parameters.BackBufferHeight = clientRect.Height;
                parameters.BackBufferWidth = clientRect.Width;
                parameters.Multisample = SlimDX.Direct3D9.MultisampleType.None;
                parameters.SwapEffect = SlimDX.Direct3D9.SwapEffect.Discard;
                parameters.DeviceWindowHandle = hWnd;
                parameters.PresentationInterval = SlimDX.Direct3D9.PresentInterval.Default;
                parameters.FullScreenRefreshRateInHertz = 0;

                // Create the Direct3D device
                device = new SlimDX.Direct3D9.Device(_direct3D9, adapterInfo.Adapter, SlimDX.Direct3D9.DeviceType.Hardware, hWnd, SlimDX.Direct3D9.CreateFlags.SoftwareVertexProcessing, parameters);
                _direct3DDeviceCache.Add(hWnd, device);
            }
            #endregion

            // Capture the screen and copy the region into a Bitmap
            using (SlimDX.Direct3D9.Surface surface = SlimDX.Direct3D9.Surface.CreateOffscreenPlain(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, SlimDX.Direct3D9.Format.A8R8G8B8, SlimDX.Direct3D9.Pool.SystemMemory))
            {
                device.GetFrontBufferData(0, surface);

                // Update: thanks digitalutopia1 for pointing out that SlimDX have fixed a bug
                // where they previously expected a RECT type structure for their Rectangle
                bitmap = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(surface, SlimDX.Direct3D9.ImageFileFormat.Bmp, new Rectangle(region.Left, region.Top, region.Width, region.Height)));
                // Previous SlimDX bug workaround: new Rectangle(region.Left, region.Top, region.Right, region.Bottom)));

            }

            return bitmap;
        }
    }

    #region Native Win32 Interop
    /// &lt;summary&gt;
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// &lt;/summary&gt;
    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public Rectangle AsRectangle
        {
            get
            {
                return new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
            }
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public static RECT FromRectangle(Rectangle rect)
        {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }

    [System.Security.SuppressUnmanagedCodeSecurity()]
    internal sealed class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// &lt;summary&gt;
        /// Get a windows client rectangle in a .NET structure
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;hwnd&quot;&gt;The window handle to look up&lt;/param&gt;
        /// &lt;returns&gt;The rectangle&lt;/returns&gt;
        internal static Rectangle GetClientRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetClientRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        /// &lt;summary&gt;
        /// Get a windows rectangle in a .NET structure
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;hwnd&quot;&gt;The window handle to look up&lt;/param&gt;
        /// &lt;returns&gt;The rectangle&lt;/returns&gt;
        internal static Rectangle GetWindowRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        internal static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = NativeMethods.GetWindowRect(hWnd);
            Rectangle clientRect = NativeMethods.GetClientRect(hWnd);

            // This gives us the width of the left, right and bottom chrome - we can then determine the top height
            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);

            //Rectangle r = new Rectangle(new System.Drawing.Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
            Rectangle r = new Rectangle(new System.Drawing.Point(windowRect.X + chromeWidth + 300, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth) + 300), new System.Drawing.Size(200, 200));
            //Console.WriteLine(r.X.ToString() + "==" + r.Y.ToString() + "==" + r.Width.ToString() + "==" + r.Height.ToString());
            return r;
        }
    }
    #endregion
}
