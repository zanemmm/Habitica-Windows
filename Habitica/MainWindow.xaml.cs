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
using Habitica.Utils;
using Task = System.Threading.Tasks.Task;
using AppTask = Habitica.Models.Task;

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

        private Setting AppSetting;
        private HttpApi HttpApi;

        private List<AppTask> Tasks;
        private List<Tag> Tags;
        private Tag TodayTargetTag;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void ShowMessage(string message, bool isSuccess = true)
        {
            if (message == string.Empty)
            {
                return;
            }
            MessageBar.Visibility = Visibility.Visible;
            MessageBarContent.Text = message;
            MessageBar.Background = isSuccess ? Utils.Colors.Green : Utils.Colors.Red;
            // 显示动画
            DoubleAnimation showAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(.4))
            };
            // 消失动画
            DoubleAnimation hiddenAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(2))
            };
            // 显示动画执行完毕后执行消失动画
            showAnimation.Completed += new EventHandler((object a, EventArgs b) => { MessageBar.BeginAnimation(OpacityProperty, hiddenAnimation); });
            // 消失动画执行完毕后隐藏消息通知
            hiddenAnimation.Completed += new EventHandler((object a, EventArgs b) => { MessageBar.Visibility = Visibility.Collapsed; });
            MessageBar.BeginAnimation(OpacityProperty, hiddenAnimation);
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //// 窗口嵌入桌面
            //IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            //IntPtr pWnd = FindWindow("Progman", null);
            //pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefVIew", null);
            //pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
            //SetParent(hWnd, pWnd);

            AppSetting = GetSetting();
            userIdInput.Text = AppSetting.UserId;
            apiTokenInput.Text = AppSetting.ApiToken;
            try
            {
                HttpApi = new HttpApi(AppSetting);
                // 获取 Habitica 数据
                Tasks = await HttpApi.GetAllTasks();
                Tags = await HttpApi.GetAllTags();
                // 获取今日目标标签
                TodayTargetTag = HttpApi.TodayTargetTagFilter(Tags);
                if (TodayTargetTag == null)
                {
                    TodayTargetTag = HttpApi.CreateTag("TodayTarget").Result;
                }
                // 初始化今日目标列表
                InitTodayTargetList();
                InitPlanTargetList();
            }
            catch (Exception exception)
            {
                ShowMessage(exception.Message, false);
            }
        }

        private void InitTodayTargetList()
        {
            List<AppTask> todayTargetTasks = HttpApi.TodayTargetTaskFilter(Tasks, TodayTargetTag);
            foreach (AppTask task in todayTargetTasks)
            {
                SimpleTaskCard card = task.ToSimpleTaskCard(false);
                card.CardRemove += TargetRemoved;
                card.StatusChange += TargetStatusChange;
                todayTargetsList.Children.Add(card);
            }
        }

        private void InitPlanTargetList()
        {

            List<AppTask> planTargetTasks = HttpApi.PlanTargetTaskFilter(Tasks, TodayTargetTag);
            foreach (AppTask task in planTargetTasks)
            {
                SimpleTaskCard card = task.ToSimpleTaskCard(false);
                card.CardRemove += TargetRemoved;
                card.StatusChange += TargetStatusChange;
                planTargetsList.Children.Add(card);
            }
        }

        private async void AddNewTodayTarget(object sender, RoutedEventArgs e)
        {
            newTodayTargetName.Text = newTodayTargetName.Text.Trim();
            if (newTodayTargetName.Text == "")
            {
                return;
            }
            try
            {
                string[] tags = new string[] { TodayTargetTag.Id };
                AppTask task = await HttpApi.CreateTask(newTodayTargetName.Text, "todo", tags, DateTime.Now.AddDays(1).Date.ToString());
                SimpleTaskCard card = task.ToSimpleTaskCard(false);
                card.CardRemove += TargetRemoved;
                card.StatusChange += TargetStatusChange;
                todayTargetsList.Children.Add(card);
                todayTargetsListScroll.ScrollToBottom();
                newTodayTargetName.Text = "";
            }
            catch (Exception exception)
            {
                ShowMessage(exception.Message, false);
            }
        }
        private async void AddNewPlanTarget(object sender, RoutedEventArgs e)
        {
            newPlanTargetName.Text = newPlanTargetName.Text.Trim();
            if (newPlanTargetName.Text == "")
            {
                return;
            }
            try
            {
                AppTask task = await HttpApi.CreateTask(newPlanTargetName.Text, "todo", date: newPlanTargetDeadline.SelectedDate?.AddDays(1).Date.ToString());
                SimpleTaskCard card = task.ToSimpleTaskCard(false);
                card.CardRemove += TargetRemoved;
                card.StatusChange += TargetStatusChange;
                planTargetsList.Children.Add(card);
                planTargetsListScroll.ScrollToBottom();
                newPlanTargetName.Text = "";
            }
            catch (Exception exception)
            {
                ShowMessage(exception.Message, false);
            }
        }

        private void TargetRemoved(object sender, SimpleTaskCard e)
        {
            if (!e.IsFinsh)
            {
                SimpleTaskCard card = (SimpleTaskCard)sender;
                _ = HttpApi.DeleteTask(card.Id);
            }
            todayTargetsList.Children.Remove(e);
        }

        private void TargetStatusChange(object sender, SimpleTaskCard.Status e)
        {
            SimpleTaskCard card = (SimpleTaskCard)sender;
            if (e == SimpleTaskCard.Status.Finish)
            {
                _ = HttpApi.ScoreTask(card.Id, "up");
            }
            else if (e == SimpleTaskCard.Status.Process)
            {
                _ = HttpApi.ScoreTask(card.Id, "down");
            }
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

                AppSetting.UserId = userIdInput.Text;
                AppSetting.ApiToken = apiTokenInput.Text;
                string settingJson = JsonConvert.SerializeObject(AppSetting);
                string settingPath = dataDir + @"\setting.json";
                File.WriteAllText(settingPath, settingJson);
                ShowMessage("保存设置成功", true);
            }
            catch (Exception exception)
            {
                ShowMessage(exception.ToString(), false);
            }
        }

        private Setting GetSetting()
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
