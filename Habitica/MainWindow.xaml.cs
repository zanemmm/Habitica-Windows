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
using AppTask = Habitica.Models.Task;
using System.Timers;
using System.Windows.Threading;

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

        private readonly DispatcherTimer GCTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            GCTimer.Interval = TimeSpan.FromSeconds(30);
            GCTimer.Tick += new EventHandler((object a, EventArgs b) =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            });
            GCTimer.Start();
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

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 窗口嵌入桌面
            IntPtr hWnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            IntPtr pWnd = FindWindow("Progman", null);
            pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefVIew", null);
            pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
            SetParent(hWnd, pWnd);
            // 获取设置并更新数据
            AppSetting = GetSetting();
            userIdInput.Text = AppSetting.UserId;
            apiTokenInput.Text = AppSetting.ApiToken;
            if (HasNotEmptySetting())
            {
                UpdateDataFromHabitica();
            }
            else
            {
                settingTab.IsSelected = true;
            }
            // 设置窗口位置
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = AppSetting.Position.X;
            Top = AppSetting.Position.Y;
            IsPinned = AppSetting.IsPinned;
            PinnedChange();
            // 每半小时自动更新数据
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(30)
            };
            timer.Tick += new EventHandler((object a, EventArgs b) =>
            {
                if (HasNotEmptySetting())
                {
                    UpdateDataFromHabitica();
                }
            });
            timer.Start();
        }

        private async void UpdateDataFromHabitica()
        {
            DoubleAnimation rotateAnimation = new DoubleAnimation()
            {
                From = 360,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(.8)),
                RepeatBehavior = RepeatBehavior.Forever,
            };
            RotateTransform rotateTransform = new RotateTransform(0);
            RefreshButton.RenderTransform = rotateTransform;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
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
                // 清理目标列表
                ClearShouldGetFromHabiticaTask();
                // 初始化目标列表
                InitTodayTargetList();
                InitDailyTargetList();
                InitPlanTargetList();
                ShowMessage("更新数据成功");
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            }
            catch (Exception exception)
            {
                ShowMessage(exception.Message, false);
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            }
        }

        private void ClearShouldGetFromHabiticaTask()
        {
            List<StackPanel> lists = new List<StackPanel>() { todayTargetsList, dailyTargetsList, planTargetsList };

            foreach (StackPanel panel in lists)
            {
                // C# 不支持 foreach 遍历过程中删除遍历的 List 中的元素
                // 所以用两个 foreach，第一个 foreach 用来寻找符合条件的元素
                // 第二个 foreach 执行删除操作
                List<SimpleTaskCard> readyToRemove = new List<SimpleTaskCard>();
                foreach (SimpleTaskCard card in panel.Children)
                {
                    // 除了已完成且未过期的任务需要保留，其他都清除，重新从 Habitica 上拉取
                    if (!card.IsFinsh || card.Deadline == null || DateTime.Now.Date >= card.Deadline)
                    {
                        readyToRemove.Add(card);
                    }
                }
                foreach(SimpleTaskCard card in readyToRemove)
                {
                    panel.Children.Remove(card);
                }
                readyToRemove = null;
            }
        }

        private void Refresh(object sender, MouseButtonEventArgs e)
        {
            if (!HasNotEmptySetting())
            {
                ShowMessage("请填写用户信息", false);
                return;
            }
            UpdateDataFromHabitica();
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

        private void InitDailyTargetList()
        {
            List<AppTask> dailyTargetTasks = HttpApi.DailyTargetTaskFilter(Tasks);
            foreach (AppTask task in dailyTargetTasks)
            {
                SimpleTaskCard card = task.ToSimpleTaskCard(false);
                card.CardRemove += TargetRemoved;
                card.StatusChange += TargetStatusChange;
                dailyTargetsList.Children.Add(card);
            }
        }

        private void InitPlanTargetList()
        {

            List<AppTask> planTargetTasks = HttpApi.PlanTargetTaskFilter(Tasks, TodayTargetTag);
            foreach (AppTask task in planTargetTasks)
            {
                SimpleTaskCard card = task.ToSimpleTaskCard(true);
                card.CardRemove += TargetRemoved;
                card.StatusChange += TargetStatusChange;
                planTargetsList.Children.Add(card);
            }
        }

        private async void AddNewTodayTarget(object sender, RoutedEventArgs e)
        {
            if (!HasNotEmptySetting())
            {
                ShowMessage("请填写用户信息", false);
                return;
            }
            newTodayTargetName.Text = newTodayTargetName.Text.Trim();
            if (newTodayTargetName.Text == "")
            {
                return;
            }
            try
            {
                string[] tags = new string[] { TodayTargetTag.Id };
                AppTask task = await HttpApi.CreateTask(newTodayTargetName.Text, AppTask.TypeToString(TaskType.Todo), tags, DateTime.Now.AddDays(1).Date.ToString());
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

        private async void AddNewDailyTarget(object sender, RoutedEventArgs e)
        {
            if (!HasNotEmptySetting())
            {
                ShowMessage("请填写用户信息", false);
                return;
            }
            newDailyTargetName.Text = newDailyTargetName.Text.Trim();
            if (newDailyTargetName.Text == "")
            {
                return;
            }
            try
            {
                AppTask task = await HttpApi.CreateTask(newDailyTargetName.Text, AppTask.TypeToString(TaskType.Daily));
                SimpleTaskCard card = task.ToSimpleTaskCard(false);
                card.CardRemove += TargetRemoved;
                card.StatusChange += TargetStatusChange;
                dailyTargetsList.Children.Add(card);
                dailyTargetsListScroll.ScrollToBottom();
                newDailyTargetName.Text = "";
            }
            catch (Exception exception)
            {
                ShowMessage(exception.Message, false);
            }
        }

        private async void AddNewPlanTarget(object sender, RoutedEventArgs e)
        {
            if (!HasNotEmptySetting())
            {
                ShowMessage("请填写用户信息", false);
                return;
            }
            newPlanTargetName.Text = newPlanTargetName.Text.Trim();
            if (newPlanTargetName.Text == "")
            {
                return;
            }
            try
            {
                AppTask task = await HttpApi.CreateTask(newPlanTargetName.Text, AppTask.TypeToString(TaskType.Todo), date: newPlanTargetDeadline.SelectedDate?.AddDays(1).Date.ToString());
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

        private void ResetFormPosition_Click(object sender, RoutedEventArgs e)
        {
            IsPinned = true;
            PinnedChange();
            Left = SystemParameters.PrimaryScreenWidth - 288;
            Top = 0;
        }

        private void SaveSettingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (userIdInput.Text.Trim() == string.Empty || apiTokenInput.Text.Trim() == string.Empty)
                {
                    ShowMessage("信息不能为空", false);
                    return;
                }
                AppSetting.UserId = userIdInput.Text;
                AppSetting.ApiToken = apiTokenInput.Text;
                SaveSetting(AppSetting);
                ShowMessage("保存设置成功", true);
                // 用户信息修改后更新数据
                UpdateDataFromHabitica();
            }
            catch (Exception exception)
            {
                ShowMessage(exception.ToString(), false);
            }
        }

        private void SaveSetting(Setting setting)
        {
            string dataDir = Directory.GetCurrentDirectory() + @"\Data";
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            string settingJson = JsonConvert.SerializeObject(setting);
            string settingPath = dataDir + @"\setting.json";
            File.WriteAllText(settingPath, settingJson);
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

        private bool HasNotEmptySetting()
        {
            return AppSetting.UserId.Trim() != string.Empty && AppSetting.ApiToken.Trim() != string.Empty;
        }

        bool IsPinned = true;
        private static readonly BitmapImage pinImage = new BitmapImage(new Uri("Resources/pin.png", UriKind.Relative));
        private static readonly BitmapImage pinOffImage = new BitmapImage(new Uri("Resources/pin-off.png", UriKind.Relative));
        private void PinButton_Click(object sender, MouseButtonEventArgs e)
        {
            IsPinned = !IsPinned;
            PinnedChange();
        }

        private void PinnedChange()
        {
            if (IsPinned)
            {
                TitleBar.Opacity = .85;
                PinButton.Source = pinImage;
            }
            else
            {
                TitleBar.Opacity = 1;
                PinButton.Source = pinOffImage;
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsPinned)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            AppSetting.Position.X = Left;
            AppSetting.Position.Y = Top;
            AppSetting.IsPinned = IsPinned;
            SaveSetting(AppSetting);
            Close();
        }
    }
}
