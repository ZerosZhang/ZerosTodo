using BaseTool;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace ZerosToDo;

public partial class SoftwareSetting : ObservableObject
{
    [ObservableProperty]
    private string m_AppID = string.Empty;

    [ObservableProperty]
    private string m_AppSecret = string.Empty;

    [ObservableProperty]
    private string m_AppToken = string.Empty;

    [ObservableProperty]
    private string m_DataBaseID = string.Empty;

    /// <summary>
    /// 上次打开的位置
    /// </summary>
    [ObservableProperty]
    private int m_LastLeft = 0;

    /// <summary>
    /// 上次打开的位置
    /// </summary>
    [ObservableProperty]
    private int m_LastTop = 0;

    [ObservableProperty]
    private string m_DirectoryOnSaveLogImage = Path.Combine(BaseDefine.DirectoryPathOnSaveFile, "LogImage");

    [ObservableProperty]
    private int m_TimeOnSaveLogImage = 90;

    [ObservableProperty]
    private string m_DirectoryOnSaveTxtFile = Path.Combine(BaseDefine.DirectoryPathOnSaveFile, "TxTFile");

    [ObservableProperty]
    private ObservableCollection<string> m_TxtFileNameList = [];
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
