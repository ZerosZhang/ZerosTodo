using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Brushes = System.Windows.Media.Brushes;

namespace ZerosToDo;

/// <summary>
/// SuspensionWindow.xaml 的交互逻辑
/// </summary>
public partial class SuspensionWindow : Window
{
    public SuspensionWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();

            App.Setting.LastLeft = (int)Left;
            App.Setting.LastTop = (int)Top;
        }
    }

    public void ShowTodoList(TodoInfo[] _list)
    {
        ListBox_TodoList.DataContext = _list;
    }

    private async void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        TodoInfo[] _into = await App.GetTodoInfo();
        ShowTodoList(_into);
    }
}

public class BackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TodoInfo _todo_info)
        {
            return _todo_info.StateOnComplete switch
            {
                StateOnComplete.Todo when _todo_info.IsImportant => Brushes.Red,
                StateOnComplete.OnGoing when _todo_info.IsImportant => Brushes.Red,
                StateOnComplete.Todo => Brushes.Black,
                StateOnComplete.OnGoing => Brushes.LimeGreen,
                StateOnComplete.Completed => Brushes.LightGray,
                _ => DependencyProperty.UnsetValue,
            };
        }
        return DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// 不支持反向转换
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported.");
    }
}

public class IsCheckedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TodoInfo _todo_info)
        {
            return _todo_info.StateOnComplete is StateOnComplete.Completed;
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ConvertBack is not supported.");
    }
}

