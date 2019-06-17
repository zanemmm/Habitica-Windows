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

        private static BitmapImage blankImage = new BitmapImage(new Uri("Resources/check-box-outline-blank.png", UriKind.Relative));
        private static BitmapImage fullImage = new BitmapImage(new Uri("Resources/check-box-outline.png", UriKind.Relative));

        static SimpleTaskCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleTaskCard), new FrameworkPropertyMetadata(typeof(SimpleTaskCard)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            taskCheckbox = GetTemplateChild("TasKCheckbox") as Image;
            taskCheckboxBorder = GetTemplateChild("TaskCheckboxBorder") as Border;
            taskNameBlock = GetTemplateChild("taskNameBlock") as TextBlock;
            taskCheckbox.MouseLeftButtonUp += Checkbox_Click;
            IsFinsh_Changed();
        }

        public static readonly DependencyProperty IsFinshProperty = DependencyProperty.Register("IsFinsh", typeof(bool), typeof(SimpleTaskCard), new PropertyMetadata(false));
        [Bindable(true)]
        [Category("Appearance")]
        public bool IsFinsh
        {
            get => (bool)GetValue(IsFinshProperty);
            set => SetValue(IsFinshProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SimpleTaskCard), new PropertyMetadata(System.String.Empty));
        [Bindable(true)]
        [Category("Appearance")]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty ProcessColorProperty = DependencyProperty.Register("ProcessColor", typeof(Brush), typeof(SimpleTaskCard), new PropertyMetadata(null));
        [Bindable(true)]
        [Category("Appearance")]
        public Brush ProcessColor
        {
            get => (Brush)GetValue(ProcessColorProperty);
            set => SetValue(ProcessColorProperty, value);
        }

        public static readonly DependencyProperty FinishColorProperty = DependencyProperty.Register("FinishColor", typeof(Brush), typeof(SimpleTaskCard), new PropertyMetadata(null));
        [Bindable(true)]
        [Category("Appearance")]
        public Brush FinishColor
        {
            get => (Brush)GetValue(FinishColorProperty);
            set => SetValue(FinishColorProperty, value);
        }


        private void IsFinsh_Changed()
        {
            if (IsFinsh)
            {
                taskCheckbox.Source = fullImage;
                taskCheckboxBorder.Background = FinishColor;
                taskNameBlock.Foreground = FinishColor;
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
            IsFinsh_Changed();
        }
    }
}
