using BaseTool;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using ZerosToDo.Tools;

namespace ZerosToDo;

public partial class SoftwareSetting : ObservableObject
{
    /*******************************************************************************************************/

    [ObservableProperty]
    private string m_AppID = string.Empty;

    [ObservableProperty]
    private string m_AppSecret = string.Empty;

    [ObservableProperty]
    private string m_AppToken = string.Empty;

    [ObservableProperty]
    private string m_DataBaseID = string.Empty;

    /// <summary>
    /// 上次Todo窗口打开的位置
    /// </summary>
    [ObservableProperty]
    private int m_LastLeft = 0;

    /// <summary>
    /// 上次Todo窗口打开的位置
    /// </summary>
    [ObservableProperty]
    private int m_LastTop = 0;

    /*******************************************************************************************************/
    /// <summary>
    /// 保存截图文件的位置
    /// </summary>
    [ObservableProperty]
    private string m_DirectoryOnSaveLogImage = Path.Combine(BaseDefine.DirectoryPathOnSaveFile, "LogImage");

    /// <summary>
    /// 保留图片的时间
    /// </summary>
    [ObservableProperty]
    private int m_TimeOnSaveLogImage = 90;

    /*******************************************************************************************************/

    /// <summary>
    /// 朗读的语速
    /// </summary>
    [ObservableProperty]
    private int m_SpeechRate = 0;

    /// <summary>
    /// 保存文件的位置
    /// </summary>
    [ObservableProperty]
    private string m_DirectoryOnSaveTxtFile = Path.Combine(BaseDefine.DirectoryPathOnSaveFile, "TxTFile");

    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FileMetaData> m_TxtFileNameList = [];

    /// <summary>
    /// 上一次朗读文件的ID
    /// </summary>
    [ObservableProperty]
    private Guid? m_LastFileID = null;
}


