using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Habitica.Models;

namespace Habitica
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWnd1, IntPtr hWnd2, string lpsz1, string lpsz2);
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public MainWindow()
        {
            InitializeComponent();
        }

        public void ShowMessage(string message, bool isSuccess)
        {
            MessageBar.Visibility = Visibility.Visible;
            MessageBarContent.Text = message;
            MessageBar.Background = isSuccess ? Utils.Colors.Green : Utils.Colors.Red;
            
            DoubleAnimation showAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(.4))
            };
            DoubleAnimation hiddenAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(2))
            };
            showAnimation.Completed += new EventHandler((object a, EventArgs b) => { MessageBar.BeginAnimation(OpacityProperty, hiddenAnimation); });
            hiddenAnimation.Completed += new EventHandler((object a, EventArgs b) => { MessageBar.Visibility = Visibility.Collapsed; });
            MessageBar.BeginAnimation(OpacityProperty, hiddenAnimation);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //// 窗口嵌入桌面
            //IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            //IntPtr pWnd = FindWindow("Progman", null);
            //pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefVIew", null);
            //pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
            //SetParent(hWnd, pWnd);

            Setting setting = getSetting();
            userIdInput.Text = setting.UserId;
            apiTokenInput.Text = setting.ApiToken;
        }

        private void AddTodyTargetButton_Click(object sender, RoutedEventArgs e)
        {
            //StackPanel p = FindName("TodayTargetsList") as StackPanel;
            //DoubleAnimation a = new DoubleAnimation
            //{
            //    From = 0,
            //    To = -Width,
            //    Duration = new Duration(TimeSpan.FromSeconds(.5))
            //};
            //TranslateTransform t = new TranslateTransform(0, 0);
            //p.Children[0].RenderTransform = t;
            //p.Children[0].RenderTransform.BeginAnimation(TranslateTransform.XProperty, a);
            //a.Children.RemoveAt(0);
        }

        private void AddNewTodayTarget(object sender, RoutedEventArgs e)
        {
            if (newTodayTargetName.Text == "")
            {
                return;
            }
            SimpleTaskCard card = new SimpleTaskCard
            {
                Title = newTodayTargetName.Text,
                Deadline = DateTime.Now.Date,
                IsShowDeadline = false,
            };
            card.CardRemove += TodayTargetRemoved;
            todayTargetsList.Children.Add(card);
            todayTargetsListScroll.ScrollToBottom();
            newTodayTargetName.Text = "";
        }

        private void TodayTargetRemoved(object sender, SimpleTaskCard e)
        {
            todayTargetsList.Children.Remove(e);
        }

        private void SaveSetting(object sender, RoutedEventArgs e)
        {
            try
            {
                string dataDir = Directory.GetCurrentDirectory() + @"\Data";
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }

                Setting setting = new Setting(userIdInput.Text, apiTokenInput.Text);
                string settingJson = JsonConvert.SerializeObject(setting);
                string settingPath = dataDir + @"\setting.json";
                File.WriteAllText(settingPath, settingJson);
                ShowMessage("保存设置成功", true);
            }
            catch (Exception exception)
            {
                ShowMessage(exception.ToString(), false);
            }
        }

        private Setting getSetting()
        {
            string settingPath = Directory.GetCurrentDirectory() + @"\Data\setting.json";
            if (!File.Exists(settingPath))
            {
                return new Setting("", "");
            }

            Setting setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(settingPath));
            return setting;
        }
    }
}
