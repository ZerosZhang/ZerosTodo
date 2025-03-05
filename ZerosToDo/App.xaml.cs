using BaseTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Threading;
using ZerosToDo.Tools;
using MessageBox = System.Windows.MessageBox;

namespace ZerosToDo;

public partial class App : System.Windows.Application
{
    [AllowNull]
    public static SoftwareSetting Setting { get; set; }

    [AllowNull]
    public static SuspensionWindow SuspensionWindow { get; set; }


    public App()
    {
        Startup += App_Startup;
        Exit += App_Exit;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    /// <summary>
    /// 应用程序启动时的执行代码
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void App_Startup(object sender, StartupEventArgs e)
    {
        // 检测程序是否已开启，如果已开启，则打开主界面
        if (BaseFunction.ApplicationRepeatRunOnProcess())
        {
            MessageBox.Show($"软件已开启，如未在任务栏找到已开启的软件，需要在任务管理器中搜索 {BaseDefine.CurrentProcess.ProcessName} 手动结束任务");
            Shutdown();
            return;
        }

        // 创建保存运行数据的文件夹
        Directory.CreateDirectory(BaseDefine.DirectoryPathOnSaveLog);
        Directory.CreateDirectory(BaseDefine.DirectoryPathOnSaveData);
        Directory.CreateDirectory(BaseDefine.DirectoryPathOnSaveFile);
        Directory.CreateDirectory(BaseDefine.DirectoryPathOnSaveConfig);

        // 程序实例
        Setting = BaseFunction.LoadConfig_Json<SoftwareSetting>(BaseDefine.FilePathOnSaveSoftwareSetting) ?? new SoftwareSetting();

        WolaiDataBase.InitializeTool();
        SuspensionWindow.ShowTodoList(await WolaiDataBase.GetTodoInfo());

        AutoScreenShot.InitializeTool();
        AutoScreenShot.DoLoopScreenShotAction();

        TextSpeech.InitializeTool();
        TextSpeech.ChangeFile();

        MainWindow = new MainWindow();
        SuspensionWindow = new SuspensionWindow
        {
            Topmost = true,
            Left = Setting.LastLeft,
            Top = Setting.LastTop,
        };
        SuspensionWindow.Show();
    }

    /// <summary>
    /// 应用程序退出时的执行代码
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_Exit(object sender, ExitEventArgs e)
    {
        if (Setting is null) return;

        BaseFunction.SaveConfig_Json(Setting, BaseDefine.FilePathOnSaveSoftwareSetting);

        TextSpeech.Close();
    }

    /// <summary>
    /// 处理未捕获的异常
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"{e.Exception.Message}");
        e.Handled = true;
        Shutdown();
    }
}
