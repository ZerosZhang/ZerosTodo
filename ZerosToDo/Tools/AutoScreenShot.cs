using BaseTool;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ZerosToDo.Tools;

public static class AutoScreenShot
{
    /// <summary>
    /// 循环截图的任务，防止 Task 被 GC 回收
    /// </summary>
    [AllowNull]
    private static Task ScreenShotTask { get; set; } = null;

    public static void InitializeTool()
    {
        BaseFunction.DeleteFileByTime(App.Setting.DirectoryOnSaveLogImage, App.Setting.TimeOnSaveLogImage);
    }

    /// <summary>
    /// 运行循环截图任务
    /// </summary>
    public static void DoLoopScreenShotAction()
    {
        if (ScreenShotTask is not null) { return; }

        ScreenShotTask = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    // 保存图片的目录，按天分文件夹
                    string _directory_path = Path.Combine(App.Setting.DirectoryOnSaveLogImage, $"{DateTime.Now:yyyy-MM-dd}");
                    Directory.CreateDirectory(_directory_path);

                    // 截图并保存，可能存在多个屏幕，因此进行循环
                    Screen[] screens = Screen.AllScreens;
                    for (int i = 0; i < screens.Length; i++)
                    {
                        using Bitmap _image = CaptureScreen(screens[i].Bounds);
                        string _file_path = Path.Combine(_directory_path, $"{DateTime.Now:HH-mm-ss}【{i}】.jpg");
                        BaseFunction.SaveBitmapAsJpeg(_image, _file_path, 50);
                    }
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode is 6)
                {
                    // 句柄无效异常：不具备截图条件，在以下情况下会发生：
                    // - 1. Ctrl+Alt+Del 界面
                    // - 2. 以管理员运行某个程序后弹出确认窗口时
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"截图异常：{ex.Message}{Environment.NewLine}{ex}");
                }

                await Task.Delay(5000);
            }
        });
    }

    /// <summary>
    /// 截取屏幕的区域
    /// </summary>
    /// <param name="_region"></param>
    /// <returns></returns>
    private static Bitmap CaptureScreen(Rectangle _region)
    {
        Bitmap bitmap = new Bitmap(_region.Width, _region.Height);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(_region.Location, Point.Empty, _region.Size);
        }
        return bitmap;
    }
}
