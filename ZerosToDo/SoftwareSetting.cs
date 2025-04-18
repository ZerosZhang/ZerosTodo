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
    public partial string AppID { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string AppSecret { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string AppToken { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string DataBaseID { get; set; } = string.Empty;

    /// <summary>
    /// 上次Todo窗口打开的位置
    /// </summary>
    [ObservableProperty]
    public partial int LastLeft { get; set; } = 0;

    /// <summary>
    /// 上次Todo窗口打开的位置
    /// </summary>
    [ObservableProperty]
    public partial int LastTop { get; set; } = 0;

    /*******************************************************************************************************/
    /// <summary>
    /// 保存截图文件的位置
    /// </summary>
    [ObservableProperty]
    public partial string DirectoryOnSaveLogImage { get; set; } = Path.Combine(BaseDefine.DirectoryPathOnSaveFile, "LogImage");

    /// <summary>
    /// 保留图片的时间
    /// </summary>
    [ObservableProperty]
    public partial int TimeOnSaveLogImage { get; set; } = 90;

    /*******************************************************************************************************/

    /// <summary>
    /// 朗读的语速
    /// </summary>
    [ObservableProperty]
    public partial int SpeechRate { get; set; } = 0;

    /// <summary>
    /// 保存文件的位置
    /// </summary>
    [ObservableProperty]
    public partial string DirectoryOnSaveTxtFile { get; set; } = Path.Combine(BaseDefine.DirectoryPathOnSaveFile, "TxTFile");

    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileMetaData> TxtFileNameList { get; set; } = [];

    /// <summary>
    /// 上一次朗读文件的ID
    /// </summary>
    [ObservableProperty]
    public partial Guid? LastFileID { get; set; } = null;
}


