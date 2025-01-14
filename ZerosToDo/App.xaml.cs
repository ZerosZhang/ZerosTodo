using BaseTool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace ZerosToDo;

public partial class App : System.Windows.Application
{
    public static HttpClient WolaiClient { get; set; } = new HttpClient();

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

        MainWindow = new MainWindow() { DataContext = Setting };
        SuspensionWindow = new SuspensionWindow
        {
            Topmost = true,
            Left = Setting.LastLeft,
            Top = Setting.LastTop,
        };
        SuspensionWindow.Show();

        if (!string.IsNullOrEmpty(Setting.AppToken))
        {
            WolaiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Setting.AppToken);
            TodoInfo[] _info = await GetTodoInfo();
            SuspensionWindow.ShowTodoList(_info);
        }
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

    public static async Task<TodoInfo[]> GetTodoInfo()
    {
        HttpResponseMessage _response = await WolaiClient.GetAsync($"https://openapi.wolai.com/v1/databases/{Setting.DataBaseID}", BaseAction.Token);
        _response.EnsureSuccessStatusCode();
        string _json_string = await _response.Content.ReadAsStringAsync(BaseAction.Token);
        JToken _json_object = JToken.Parse(_json_string);

        TryConvertToInfo(_json_object, out TodoInfo[] _info);
        return _info;
    }

    private static bool TryConvertToInfo(JToken _object, out TodoInfo[] _info_list)
    {
        try
        {
            JToken _data = _object["data"] ?? throw new Exception("获取 data 失败");
            JToken _rows = _data["rows"] ?? throw new Exception("获取 rows 失败");

            List<TodoInfo> _list = [];
            foreach (JToken _item in _rows)
            {
                JToken _rows_data = _item["data"] ?? throw new Exception("获取 rows_data 失败");
                string _title = _rows_data["待办事项"]?["value"]?.ToString() ?? throw new Exception();
                string _on_going = _rows_data["完成进度"]?["value"]?.ToString() ?? throw new Exception();
                string _deadline = _rows_data["截止时间"]?["value"]?.ToString() ?? throw new Exception();
                string _is_important = _rows_data["紧急"]?["value"]?.ToString() ?? throw new Exception();

                if (_on_going is "待完成" or "进行中")
                {
                    _list.Add(new TodoInfo()
                    {
                        Title = _title,
                        StateOnComplete = _on_going switch
                        {
                            "待完成" => StateOnComplete.Todo,
                            "进行中" => StateOnComplete.OnGoing,
                            "已完成" => StateOnComplete.Completed,
                            _ => StateOnComplete.Todo
                        },
                        DeadLine = string.IsNullOrEmpty(_deadline) ? default : DateTime.Parse(_deadline),
                        IsImportant = bool.Parse(_is_important)
                    });
                }
            }

            _info_list = [.. _list];
            return true;
        }
        catch (Exception)
        {
            _info_list = [];
            return false;
        }
    }

    [Obsolete("wolai的API暂时不支持外部修改", true)]
    public static async Task SetTodoInfo(TodoInfo _info)
    {

        var _data = new[]
        {
            new
            {
                待办事项 = $"{_info.Title}",
                完成进度 = _info.StateOnComplete switch
                {
                    StateOnComplete.Todo => "待完成",
                    StateOnComplete.OnGoing => "进行中",
                    StateOnComplete.Completed => "已完成",
                    _ => throw new NotImplementedException(),
                }
            }
        };
        string _json_data = JsonConvert.SerializeObject(new { rows = _data });
        StringContent _content = new StringContent(_json_data);
        HttpResponseMessage _response = await WolaiClient.PostAsync($"https://openapi.wolai.com/v1/databases/{Setting.DataBaseID}/rows", _content, BaseAction.Token);
        _response.EnsureSuccessStatusCode();
    }
}
