using BaseTool;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Net.Http.Headers;

namespace ZerosToDo.Tools;

public static class WolaiDataBase
{
    public static HttpClient WolaiClient { get; set; } = new HttpClient();

    public static void InitializeTool()
    {
        WolaiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(App.Setting.AppToken);
    }

    public static async Task<uint> GetToken()
    {
        try
        {
            // ! 提供两种方式 Token 不存在时创建，Token 存在时刷新
            if (string.IsNullOrEmpty(App.Setting.AppToken))
            {
                string _json_data = JsonConvert.SerializeObject(new { appId = App.Setting.AppID, appSecret = App.Setting.AppSecret });
                StringContent _content = new StringContent(_json_data);
                HttpResponseMessage _response = await WolaiClient.PostAsync("https://openapi.wolai.com/v1/token", _content, BaseAction.Token);
                _response.EnsureSuccessStatusCode();
                string _json_string = await _response.Content.ReadAsStringAsync(BaseAction.Token);
                JObject json_object = JObject.Parse(_json_string);
                App.Setting.AppToken = json_object["data"]?["app_token"]?.ToString() ?? throw new Exception("回复异常");
                WolaiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(App.Setting.AppToken);
            }
            else
            {
                string _json_data = JsonConvert.SerializeObject(new { appId = App.Setting.AppID, appToken = App.Setting.AppToken });
                StringContent _content = new StringContent(_json_data);
                HttpResponseMessage _response = await WolaiClient.PutAsync("https://openapi.wolai.com/v1/token", _content, BaseAction.Token);
                _response.EnsureSuccessStatusCode();
                string _json_string = await _response.Content.ReadAsStringAsync(BaseAction.Token);
                JObject json_object = JObject.Parse(_json_string);
                App.Setting.AppToken = json_object["data"]?["app_token"]?.ToString() ?? throw new Exception("回复异常");
                WolaiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(App.Setting.AppToken);
            }

            return BaseAction.ResultOK;
        }
        catch (Exception)
        {
            return BaseAction.ResultError;
        }
    }

    public static async Task<TodoInfo[]> GetTodoInfo()
    {
        try
        {
            HttpResponseMessage _response = await WolaiClient.GetAsync($"https://openapi.wolai.com/v1/databases/{App.Setting.DataBaseID}", BaseAction.Token);
            _response.EnsureSuccessStatusCode();
            string _json_string = await _response.Content.ReadAsStringAsync(BaseAction.Token);
            JToken _json_object = JToken.Parse(_json_string);

            JToken _data = _json_object["data"] ?? throw new Exception("获取 data 失败");
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

            return [.. _list];
        }
        catch (Exception)
        {
            return [];
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
        HttpResponseMessage _response = await WolaiClient.PostAsync($"https://openapi.wolai.com/v1/databases/{App.Setting.DataBaseID}/rows", _content, BaseAction.Token);
        _response.EnsureSuccessStatusCode();
    }
}

public partial class TodoInfo : ObservableObject
{
    private string m_Title = string.Empty;

    /// <summary>
    /// 待办事项
    /// </summary>
    public required string Title
    {
        get => m_Title;
        set => SetProperty(ref m_Title, value);
    }

    /// <summary>
    /// 状态
    /// </summary>
    private StateOnComplete m_StateOnComplete = StateOnComplete.Todo;

    public required StateOnComplete StateOnComplete
    {
        get => m_StateOnComplete;
        set => SetProperty(ref m_StateOnComplete, value);
    }

    /// <summary>
    /// 截止日期
    /// </summary>
    private DateTime m_DeadLine;

    public DateTime DeadLine
    {
        get => m_DeadLine;
        set => SetProperty(ref m_DeadLine, value);
    }

    /// <summary>
    /// 是否紧急
    /// </summary>
    private bool m_IsImportant;

    public bool IsImportant
    {
        get => m_IsImportant;
        set => SetProperty(ref m_IsImportant, value);
    }
}

public enum StateOnComplete
{
    Todo,
    OnGoing,
    Completed
}
