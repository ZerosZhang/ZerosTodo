using CommunityToolkit.Mvvm.ComponentModel;

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
}

public partial class TodoInfo : ObservableObject
{
    private string m_Title = string.Empty;

    public required string Title
    {
        get => m_Title;
        set => SetProperty(ref m_Title, value);
    }

    private StateOnComplete m_StateOnComplete = StateOnComplete.Todo;

    public required StateOnComplete StateOnComplete
    {
        get => m_StateOnComplete;
        set => SetProperty(ref m_StateOnComplete, value);
    }

    private DateTime m_DeadLine;

    public DateTime DeadLine
    {
        get => m_DeadLine;
        set => SetProperty(ref m_DeadLine, value);
    }
}

public enum StateOnComplete
{
    Todo,
    OnGoing,
    Completed
}
