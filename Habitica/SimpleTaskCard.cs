using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Habitica
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Habitica"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Habitica;assembly=Habitica"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:SimpleTaskCard/>
    ///
    /// </summary>
    public class SimpleTaskCard : Control

    {
        // 子控件
        private Border taskCard;
        private Image taskCheckbox;
        private Border taskCheckboxBorder;
        private TextBlock taskNameBlock;
        private TextBlock taskDeadliineBlock;
        private Ellipse clickEllipseLayer;

        // 勾选框图片资源
        private static readonly BitmapImage blankImage = new BitmapImage(new Uri("Resources/check-box-outline-blank.png", UriKind.Relative));
        private static readonly BitmapImage fullImage = new BitmapImage(new Uri("Resources/check-box-outline.png", UriKind.Relative));
        
        // 卡片位移
        private TranslateTransform moveTranslateTransform = new TranslateTransform(0, 0);

        // 任务状态类型
        public enum Status
        {
            Process,
            Finish,
            Overdue
        }

        static SimpleTaskCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleTaskCard), new FrameworkPropertyMetadata(typeof(SimpleTaskCard)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // 获取子控件实例
            taskCard = GetTemplateChild("taskCard") as Border;
            taskCheckbox = GetTemplateChild("TasKCheckbox") as Image;
            taskCheckboxBorder = GetTemplateChild("TaskCheckboxBorder") as Border;
            taskNameBlock = GetTemplateChild("taskNameBlock") as TextBlock;
            taskDeadliineBlock = GetTemplateChild("taskDeadliineBlock") as TextBlock;
            clickEllipseLayer = GetTemplateChild("clickEllipseLayer") as Ellipse;
            // 卡片点击动画特效
            taskCard.MouseLeftButtonUp += TaskCard_MouseUp; 
            // 勾选框附加点击事件
            taskCheckbox.MouseLeftButtonUp += Checkbox_Click;
            // 卡片附加左移事件
            // 鼠标左移
            taskCard.MouseLeftButtonDown += TaskCard_MouseLeftButtonDown;
            taskCard.MouseLeftButtonUp += TaskCard_MouseLeftButtonUp;
            taskCard.MouseLeave += TaskCard_MouseLeave;
            taskCard.MouseMove += TaskCard_MouseMove;
            // 触摸左移
            taskCard.IsManipulationEnabled = true; // IsManipulationEnabled 为 true 时 TouchMove 事件才会生效
            taskCard.TouchDown += TaskCard_TouchDown;
            taskCard.TouchUp += TaskCard_TouchUp;
            taskCard.TouchLeave += TaskCard_TouchLeave;
            taskCard.TouchMove += TaskCard_TouchMove;
            // 位移对象
            RenderTransform = moveTranslateTransform;
            // 状态初始化
            CheckDeadline();
            CheckStatus();
        }

        public static readonly DependencyProperty IsFinshProperty = DependencyProperty.Register("IsFinsh", typeof(bool), typeof(SimpleTaskCard), new PropertyMetadata(false));
        [Bindable(true)]
        [Category("Appearance")]
        public bool IsFinsh
        {
            get => (bool)GetValue(IsFinshProperty);
            set => SetValue(IsFinshProperty, value);
        }

        public static readonly DependencyProperty IsOverdueProperty = DependencyProperty.Register("IsOverdue", typeof(bool), typeof(SimpleTaskCard), new PropertyMetadata(false));
        [Bindable(true)]
        [Category("Appearance")]
        public bool IsOverdue
        {
            get => (bool)GetValue(IsOverdueProperty);
            set => SetValue(IsOverdueProperty, value);
        }

        public static readonly DependencyProperty IsShowDeadlineProperty = DependencyProperty.Register("IsShowDeadline", typeof(bool), typeof(SimpleTaskCard), new PropertyMetadata(true));
        [Bindable(true)]
        [Category("Appearance")]
        public bool IsShowDeadline
        {
            get => (bool)GetValue(IsShowDeadlineProperty);
            set => SetValue(IsShowDeadlineProperty, value);
        }

        public static readonly DependencyProperty IsMoveableProperty = DependencyProperty.Register("IsMoveable", typeof(bool), typeof(SimpleTaskCard), new PropertyMetadata(true));
        [Bindable(true)]
        [Category("Appearance")]
        public bool IsMoveable
        {
            get => (bool)GetValue(IsMoveableProperty);
            set => SetValue(IsMoveableProperty, value);
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(string), typeof(SimpleTaskCard), new PropertyMetadata(string.Empty));
        [Bindable(true)]
        [Category("Appearance")]
        public string Id
        {
            get => (string)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SimpleTaskCard), new PropertyMetadata(string.Empty));
        [Bindable(true)]
        [Category("Appearance")]
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty ProcessColorProperty = DependencyProperty.Register("ProcessColor", typeof(Brush), typeof(SimpleTaskCard), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC6459DF"))));
        [Bindable(true)]
        [Category("Appearance")]
        public Brush ProcessColor
        {
            get => (Brush)GetValue(ProcessColorProperty);
            set => SetValue(ProcessColorProperty, value);
        }

        public static readonly DependencyProperty FinishColorProperty = DependencyProperty.Register("FinishColor", typeof(Brush), typeof(SimpleTaskCard), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB"))));
        [Bindable(true)]
        [Category("Appearance")]
        public Brush FinishColor
        {
            get => (Brush)GetValue(FinishColorProperty);
            set => SetValue(FinishColorProperty, value);
        }

        public static readonly DependencyProperty OverdueColorProperty = DependencyProperty.Register("OverdueColor", typeof(Brush), typeof(SimpleTaskCard), new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"))));
        [Bindable(true)]
        [Category("Appearance")]
        public Brush OverdueColor
        {
            get => (Brush)GetValue(OverdueColorProperty);
            set => SetValue(OverdueColorProperty, value);
        }

        public static readonly DependencyProperty DeadlineProperty = DependencyProperty.Register("Deadline", typeof(DateTime), typeof(SimpleTaskCard), new PropertyMetadata(null));
        [Bindable(true)]
        [Category("Appearance")]
        public DateTime Deadline
        {
            get => (DateTime)GetValue(DeadlineProperty);
            set => SetValue(DeadlineProperty, value);
        }

        //public void HiddeCardWithAnimation(Action afterAnimation)
        //{
        //    moveTranslateTransform.X = 0;
        //    DoubleAnimation animation = new DoubleAnimation
        //    {
        //        From = 0,
        //        To = -ActualWidth,
        //        Duration = new Duration(TimeSpan.FromSeconds(.35))
        //    };
        //    animation.Completed += new EventHandler((object a, EventArgs b) =>
        //    {
        //        Visibility = Visibility.Collapsed;
        //        afterAnimation();
        //        RenderTransform = moveTranslateTransform;
        //    });
        //    moveTranslateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        //}

        //public void ShowCardWithAnimation(Action afterAnimation)
        //{
        //    Visibility = Visibility.Visible;
        //    moveTranslateTransform.X = -ActualWidth;
        //    DoubleAnimation animation = new DoubleAnimation
        //    {
        //        From = -ActualWidth,
        //        To = 0,
        //        Duration = new Duration(TimeSpan.FromSeconds(.35))
        //    };
        //    animation.Completed += new EventHandler((object a, EventArgs b) => { afterAnimation(); RenderTransform = moveTranslateTransform; });
        //    moveTranslateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        //}

        private void CheckDeadline()
        {
            taskDeadliineBlock.Text = Deadline.ToShortDateString();
            // 没有设置 Deadline 或 DeadLine 未至
            if (Deadline == null || Deadline == DateTime.MinValue || Deadline.Date >= DateTime.Now.Date)
            {
                IsOverdue = false;
                return;
            }
            IsOverdue = true;
        }

        private Status CheckStatus()
        {
            if (IsFinsh)
            {
                taskCheckbox.Source = fullImage;
                taskCheckboxBorder.Background = FinishColor;
                taskNameBlock.Foreground = FinishColor;
                return Status.Finish;
            }
            else if (IsOverdue)
            {
                taskCheckbox.Source = blankImage;
                taskCheckboxBorder.Background = OverdueColor;
                taskNameBlock.Foreground = OverdueColor;
                return Status.Overdue;
            }
            else
            {
                taskCheckbox.Source = blankImage;
                taskCheckboxBorder.Background = ProcessColor;
                taskNameBlock.Foreground = ProcessColor;
                return Status.Process;
            }
        }


        public event EventHandler<Status> StatusChange;

        public void TriggerStatusChange()
        {
            Status status = CheckStatus();
            // 触发 StatusChangeEvent 事件
            StatusChange?.Invoke(this, status);
        }

        private bool JustClickCard = true;
        private void TaskCard_MouseUp(object sender, RoutedEventArgs e)
        {
            // 只是单纯点击卡片才会触发特效
            if (!JustClickCard)
            {
                JustClickCard = true;
                return;
            }
            // 显示遮罩层
            clickEllipseLayer.Visibility = Visibility.Visible;
            // 获取鼠标位置设为圆形遮罩位置
            Point point = Mouse.GetPosition((Border)sender);
            Canvas.SetLeft(clickEllipseLayer, point.X);
            Canvas.SetTop(clickEllipseLayer, point.Y);
            // 遮罩层动画，为了保持遮罩层以圆心为中心点扩大，不仅需要改变大小还需要改变边距，保持圆心位置不变
            DoubleAnimation sizeAnimation = new DoubleAnimation
            {
                From = 0,
                To = ActualWidth*2,
                Duration = new Duration(TimeSpan.FromSeconds(.3))
            };
            ThicknessAnimation marginAnimation = new ThicknessAnimation
            {
                From = new Thickness(left: 0, right:0, top:0, bottom:0),
                To = new Thickness(left: -ActualWidth, right: 0, top: -ActualWidth, bottom: 0),
                Duration = new Duration(TimeSpan.FromSeconds(.3))
            };
            sizeAnimation.AutoReverse = true;
            marginAnimation.AutoReverse = true;

            clickEllipseLayer.BeginAnimation(MarginProperty, marginAnimation);
            clickEllipseLayer.BeginAnimation(WidthProperty, sizeAnimation);
            clickEllipseLayer.BeginAnimation(HeightProperty, sizeAnimation);
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            IsFinsh = !IsFinsh;
            TriggerStatusChange();
        }

        // 卡片左滑移除功能
        public event EventHandler<SimpleTaskCard> CardRemove;
        private bool isMouseDown = false;
        private Point mouseDownPoint;
        private void TaskCard_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            isMouseDown = true;
            mouseDownPoint = Mouse.GetPosition(Application.Current.MainWindow);
            moveTranslateTransform.X = 0;
        }

        private void TaskCard_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            isMouseDown = false;
            if (moveTranslateTransform.X + ActualWidth * 0.35  < 0)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = moveTranslateTransform.X,
                    To = -ActualWidth,
                    Duration = new Duration(TimeSpan.FromSeconds(.2))
                };
                animation.Completed += new EventHandler((object a, EventArgs b) => { Visibility = Visibility.Collapsed; CardRemove?.Invoke(this, this); });
                moveTranslateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            else
            {
                moveTranslateTransform.X = 0;
            }
        }

        private void TaskCard_MouseLeave(object sender, RoutedEventArgs e)
        {
            isMouseDown = false;
            moveTranslateTransform.X = 0;
        }

        private void TaskCard_MouseMove(object sender, RoutedEventArgs e)
        {
            if (isMouseDown == false || IsMoveable == false)
            {
                return;
            }
            Point movePoint = Mouse.GetPosition(Application.Current.MainWindow);

            if (movePoint.X > mouseDownPoint.X)
            {
                return;
            }

            JustClickCard = false;
            moveTranslateTransform.X = movePoint.X - mouseDownPoint.X;
        }


        private TouchPoint touchDownPoint;
        private bool isTouchDown = false;
        private void TaskCard_TouchDown(object sender, TouchEventArgs e)
        {
            isTouchDown = true;
            touchDownPoint = e.GetTouchPoint(Application.Current.MainWindow);
        }

        private void TaskCard_TouchUp(object sender, TouchEventArgs e)
        {
            isTouchDown = false;
            if (moveTranslateTransform.X + ActualWidth * 0.35 < 0)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = moveTranslateTransform.X,
                    To = -ActualWidth,
                    Duration = new Duration(TimeSpan.FromSeconds(.2))
                };
                animation.Completed += new EventHandler((object a, EventArgs b) => { Visibility = Visibility.Collapsed; CardRemove?.Invoke(this, this); });
                moveTranslateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            else
            {
                moveTranslateTransform.X = 0;
            }
        }

        private void TaskCard_TouchLeave(object sender, RoutedEventArgs e)
        {
            if (Visibility != Visibility.Collapsed)
            {
                moveTranslateTransform.X = 0;
            }
        }

        private void TaskCard_TouchMove(object sender, TouchEventArgs e)
        {
            if (isTouchDown == false || IsMoveable == false)
            {
                return;
            }
            TouchPoint movePoint = e.GetTouchPoint(Application.Current.MainWindow);
            if (movePoint.Position.X > touchDownPoint.Position.X)
            {
                return;
            }
            moveTranslateTransform.X = movePoint.Position.X - touchDownPoint.Position.X;
        }
    }
}
