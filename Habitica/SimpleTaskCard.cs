using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        private Image taskCheckbox;
        private Border taskCheckboxBorder;
        private TextBlock taskNameBlock;
        private TextBlock taskDeadliineBlock;

        private static readonly BitmapImage blankImage = new BitmapImage(new Uri("Resources/check-box-outline-blank.png", UriKind.Relative));
        private static readonly BitmapImage fullImage = new BitmapImage(new Uri("Resources/check-box-outline.png", UriKind.Relative));

        static SimpleTaskCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleTaskCard), new FrameworkPropertyMetadata(typeof(SimpleTaskCard)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // 获取子控件实例
            taskCheckbox = GetTemplateChild("TasKCheckbox") as Image;
            taskCheckboxBorder = GetTemplateChild("TaskCheckboxBorder") as Border;
            taskNameBlock = GetTemplateChild("taskNameBlock") as TextBlock;
            taskDeadliineBlock = GetTemplateChild("taskDeadliineBlock") as TextBlock;
            // 附加事件
            taskCheckbox.MouseLeftButtonUp += Checkbox_Click;
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

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SimpleTaskCard), new PropertyMetadata(System.String.Empty));
        [Bindable(true)]
        [Category("Appearance")]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
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

        private void CheckDeadline()
        {
            // 没有设置 Deadline 或 DeadLine 未至
            if (Deadline == null || Deadline == DateTime.MinValue || Deadline.Date >= DateTime.Now.Date)
            {
                IsOverdue = false;
                return;
            }
            taskDeadliineBlock.Text = Deadline.ToShortDateString();
            IsOverdue = true;
        }

        private void CheckStatus()
        {
            if (IsFinsh)
            {
                taskCheckbox.Source = fullImage;
                taskCheckboxBorder.Background = FinishColor;
                taskNameBlock.Foreground = FinishColor;
            }
            else if (IsOverdue)
            {
                taskCheckbox.Source = blankImage;
                taskCheckboxBorder.Background = OverdueColor;
                taskNameBlock.Foreground = OverdueColor;
            }
            else
            {
                taskCheckbox.Source = blankImage;
                taskCheckboxBorder.Background = ProcessColor;
                taskNameBlock.Foreground = ProcessColor;
            }
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            IsFinsh = !IsFinsh;
            CheckStatus();
        }
    }
}
