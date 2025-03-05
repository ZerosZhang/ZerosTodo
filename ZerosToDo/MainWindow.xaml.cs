using BaseTool;
using BaseWPFControl.ToolDialog;
using System.Windows;
using System.IO;
using Application = System.Windows.Application;
using System.Diagnostics;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using ZerosToDo.Tools;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;
using ListBox = System.Windows.Controls.ListBox;


namespace ZerosToDo;

public partial class MainWindow : Window
{
    readonly NotifyIcon NotifyIcon = new NotifyIcon();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = App.Setting;

        NotifyIcon.BalloonTipText = "设置界面已隐藏";
        NotifyIcon.Icon = new Icon("ZerosToDo.ico");
        NotifyIcon.Visible = true;
        NotifyIcon.DoubleClick += NotifyIcon_DoubleClick;

        ContextMenuStrip _menu = new ContextMenuStrip();
        NotifyIcon.ContextMenuStrip = _menu;

        ToolStripMenuItem _item_01 = new ToolStripMenuItem() { Text = "打开日志图片目录" };
        _item_01.Click += (_, _) => { Process.Start("explorer.exe", App.Setting.DirectoryOnSaveLogImage); };
        _menu.Items.Add(_item_01);

        ToolStripMenuItem _item_02 = new ToolStripMenuItem() { Text = "开始朗读" };
        _item_02.Click += (_, _) => { TextSpeech.Start(); };
        _menu.Items.Add(_item_02);

        ToolStripMenuItem _item_03 = new ToolStripMenuItem() { Text = "停止朗读" };
        _item_03.Click += (_, _) => { TextSpeech.Stop(); };
        _menu.Items.Add(_item_03);

        ToolStripMenuItem _item_99 = new ToolStripMenuItem() { Text = "退出" };
        _item_99.Click += (_, _) => { Application.Current.Shutdown(); };
        _menu.Items.Add(_item_99);
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        Show();
    }

    private async void Button_RefreshToken_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadingCancelDialog.Show("请等待...", BaseAction.TokenSource);
        uint _ret = await WolaiDataBase.GetToken();
        LoadingCancelDialog.Close();

        if (_ret is not BaseAction.ResultOK)
        {
            await NotificationDialog.Show("获取 Token 失败");
        }
    }

    private async void Button_RefreshContent_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadingCancelDialog.Show("请等待...", BaseAction.TokenSource);
        TodoInfo[] _info = await WolaiDataBase.GetTodoInfo();
        App.SuspensionWindow.ShowTodoList(_info);
        LoadingCancelDialog.Close();
    }

    private void Button_SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        BaseFunction.SaveConfig_Json(App.Setting, BaseDefine.FilePathOnSaveSoftwareSetting);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }



    private void Button_UpLoadFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog _dialog = new OpenFileDialog()
        {
            Multiselect = false,
            Filter = "文本文件|*.txt",
        };

        if (_dialog.ShowDialog() is true)
        {
            string _new_path = Path.Combine(App.Setting.DirectoryOnSaveTxtFile, Path.GetFileName(_dialog.FileName));
            Directory.CreateDirectory(App.Setting.DirectoryOnSaveTxtFile);
            File.Copy(_dialog.FileName, _new_path, true);
            App.Setting.TxtFileNameList.Add(new FileMetaData()
            {
                FileName = Path.GetFileNameWithoutExtension(_new_path),
                FilePath = _new_path,
                LineCount = 0
            });
        }
    }

    private void NumericBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
    {
        TextSpeech.InitializeTool();
    }

    private void Button_ResumeFile_Click(object sender, RoutedEventArgs e)
    {
        TextSpeech.Start();
    }

    private void Button_PauseFile_Click(object sender, RoutedEventArgs e)
    {
        TextSpeech.Stop();
    }

    private void Button_CancelFile_Click(object sender, RoutedEventArgs e)
    {
        TextSpeech.Cancel();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsActive)
        {
            TextSpeech.Cancel();
            TextSpeech.ChangeFile();
            TextSpeech.Start();
        }
    }
}

public class GuidConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Guid _id)
        {
            return App.Setting.TxtFileNameList.FirstOrDefault(_file_meta => _file_meta.ID == _id) ?? DependencyProperty.UnsetValue;
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FileMetaData _file_meta)
        {
            return _file_meta.ID;
        }
        return Binding.DoNothing;
    }
}