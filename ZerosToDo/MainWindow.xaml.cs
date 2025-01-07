using BaseTool;
using BaseWPFControl.ToolDialog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Windows;
using System.Net.Http.Headers;
using System.IO;
using Point = System.Drawing.Point;
using System.Drawing.Imaging;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Diagnostics;


namespace ZerosToDo;

public partial class MainWindow : Window
{
    readonly NotifyIcon NotifyIcon = new NotifyIcon();

    public MainWindow()
    {
        InitializeComponent();
        NotifyIcon.BalloonTipText = "设置界面已隐藏";
        NotifyIcon.Icon = new Icon("ZerosToDo.ico");
        NotifyIcon.Visible = true;
        NotifyIcon.DoubleClick += NotifyIcon_DoubleClick;

        ContextMenuStrip _menu = new ContextMenuStrip();
        NotifyIcon.ContextMenuStrip = _menu;

        ToolStripMenuItem _item_01 = new ToolStripMenuItem() { Text = "刷新数据" };
        _item_01.Click += async (_, _) =>
        {
            TodoInfo[] _info = await App.GetTodoInfo();
            App.SuspensionWindow.ShowTodoList(_info);
        };
        _menu.Items.Add(_item_01);

        ToolStripMenuItem _item_02 = new ToolStripMenuItem() { Text = "打开日志图片目录" };
        _item_02.Click += (_, _) => { Process.Start("explorer.exe", App.Setting.DirectoryOnSaveLogImage); ; };
        _menu.Items.Add(_item_02);

        ToolStripMenuItem _item_99 = new ToolStripMenuItem() { Text = "退出" };
        _item_99.Click += (_, _) => { System.Windows.Application.Current.Shutdown(); };
        _menu.Items.Add(_item_99);

        BaseFunction.DeleteFileByTime(App.Setting.DirectoryOnSaveLogImage, App.Setting.TimeOnSaveLogImage);

        Task.Run(async () =>
        {
            int _error_count = 0;
            while (true)
            {
                try
                {
                    Screen[] screens = Screen.AllScreens;
                    for (int i = 0; i < screens.Length; i++)
                    {
                        using Bitmap _image = CaptureFullScreen(screens[i].Bounds);
                        string _directory_path = Path.Combine(App.Setting.DirectoryOnSaveLogImage, $"{DateTime.Now:yyyy-MM-dd}");
                        string _file_path = Path.Combine(_directory_path, $"{DateTime.Now:HH-mm-ss}【{i}】.jpg");

                        Directory.CreateDirectory(_directory_path);
                        JPEGSave.SaveBitmapAsJpeg(_image, _file_path, 50);
                    }

                    _error_count = 0;
                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    _error_count++;
                    if (_error_count > 3)
                    {
                        MessageBox.Show($"截图异常：{ex.Message}");
                    }
                }
            }
        });
    }
    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        Show();
    }

    private async void Button_RefreshToken_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadingCancelDialog.Show("请等待...", BaseAction.TokenSource);
        // ! 提供两种方式 Token 不存在时创建，Token 存在时刷新
        if (string.IsNullOrEmpty(App.Setting.AppToken))
        {
            string _json_data = JsonConvert.SerializeObject(new { appId = App.Setting.AppID, appSecret = App.Setting.AppSecret });
            StringContent _content = new StringContent(_json_data);
            HttpResponseMessage _response = await App.WolaiClient.PostAsync("https://openapi.wolai.com/v1/token", _content, BaseAction.Token);
            _response.EnsureSuccessStatusCode();
            string _json_string = await _response.Content.ReadAsStringAsync(BaseAction.Token);
            JObject json_object = JObject.Parse(_json_string);
            App.Setting.AppToken = json_object["data"]?["app_token"]?.ToString() ?? throw new Exception("回复异常");
            App.WolaiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(App.Setting.AppToken);
        }
        else
        {
            string _json_data = JsonConvert.SerializeObject(new { appId = App.Setting.AppID, appToken = App.Setting.AppToken });
            StringContent _content = new StringContent(_json_data);
            HttpResponseMessage _response = await App.WolaiClient.PutAsync("https://openapi.wolai.com/v1/token", _content, BaseAction.Token);
            _response.EnsureSuccessStatusCode();
            string _json_string = await _response.Content.ReadAsStringAsync(BaseAction.Token);
            JObject json_object = JObject.Parse(_json_string);
            App.Setting.AppToken = json_object["data"]?["app_token"]?.ToString() ?? throw new Exception("回复异常");
            App.WolaiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(App.Setting.AppToken);
        }
        LoadingCancelDialog.Close();
    }

    private async void Button_RefreshContent_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadingCancelDialog.Show("请等待...", BaseAction.TokenSource);
        TodoInfo[] _info = await App.GetTodoInfo();
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


    public static Bitmap CaptureFullScreen(Rectangle _region)
    {
        Bitmap bitmap = new Bitmap(_region.Width, _region.Height);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(_region.Location, Point.Empty, _region.Size);
        }
        return bitmap;
    }

    private void Button_OpenDirectoryOnSaveLogImage_Click(object sender, RoutedEventArgs e)
    {
        // 打开保存日志图片的目录
        Process.Start("explorer.exe", App.Setting.DirectoryOnSaveLogImage);
    }

    private void Button_SelectDirectoryOnSaveLogImage_Click(object sender, RoutedEventArgs e)
    {
        // 选择保存日志图片的目录
        using FolderBrowserDialog _dialog = new FolderBrowserDialog();
        if (_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            App.Setting.DirectoryOnSaveLogImage = _dialog.SelectedPath;
        }
    }
}

public class JPEGSave
{
    private static readonly ImageCodecInfo ImageCodecInfo;

    static JPEGSave()
    {
        ImageCodecInfo = GetJpegCodec() ?? throw new Exception();
    }

    public static void SaveBitmapAsJpeg(Bitmap bitmap, string filePath, long quality)
    {
        // 确保品质在 0-100 之间
        quality = Math.Max(0, Math.Min(quality, 100));
        // 创建编码器参数
        EncoderParameters encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        // 保存为 JPEG 文件
        bitmap.Save(filePath, ImageCodecInfo, encoderParameters);
    }

    static ImageCodecInfo? GetJpegCodec()
    {
        // 查找 JPEG 编码器
        ImageCodecInfo[] _codecs_list = ImageCodecInfo.GetImageEncoders();
        foreach (ImageCodecInfo _codec in _codecs_list)
        {
            if (_codec.MimeType == "image/jpeg")
            {
                return _codec;
            }
        }
        return null;
    }
}