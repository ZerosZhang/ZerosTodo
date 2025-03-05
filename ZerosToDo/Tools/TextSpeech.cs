using System.IO;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using BaseTool;
using System.Linq;
using System.Threading.Tasks;

namespace ZerosToDo.Tools;

public static class TextSpeech
{
    /// <summary>
    /// 语音转换
    /// </summary>
    static SpeechSynthesizer Speech { get; set; } = new();

    static TextSpeech()
    {
        Speech.Pause();
        Speech.SpeakCompleted += (sender, e) =>
        {
            FileMetaData? _file = App.Setting.TxtFileNameList.FirstOrDefault(_file_status => _file_status.ID == App.Setting.LastFileID);
            if (_file is not null) { _file.LineCount++; }
        };
    }

    public static void InitializeTool()
    {
        Speech.Volume = 100;
        Speech.Rate = App.Setting.SpeechRate;
    }

    public static uint ChangeFile()
    {
        FileMetaData? _file = App.Setting.TxtFileNameList.FirstOrDefault(_file_status => _file_status.ID == App.Setting.LastFileID);
        if (_file is null) { return BaseAction.ResultError; }
        if (_file.FileExists is false) { return BaseAction.ResultError; }

        // 跳过已经朗读的行
        if (_file.LineCount > 5) { _file.LineCount -= 3; }
        IEnumerable<string> _todo_list = File.ReadLines(_file.FilePath).Skip(_file.LineCount);

        // 这里会一次性将所有的线程创建好
        foreach (string _line in _todo_list)
        {
            if (!string.IsNullOrEmpty(_line))
            {
                // 使用正则表达式替换空白行
                string _pattern_1 = @"^\s*$\n?";
                string _temp = Regex.Replace(_line, _pattern_1, "", RegexOptions.Multiline);

                Speech.SpeakAsync(_temp);
            }
        }

        return BaseAction.ResultOK;
    }

    public static void Start() => Speech.Resume();

    public static void Stop() => Speech.Pause();

    public static void Cancel() => Speech.SpeakAsyncCancelAll();

    public static void Close() 
    {
        Speech.SpeakAsyncCancelAll();
        Speech.Dispose();
    }
}

public class FileMetaData
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public required string FileName { get; set; } = string.Empty;
    public required string FilePath { get; set; } = string.Empty;

    public bool FileExists => File.Exists(FilePath);
    public int LineCount { get; set; } = 0;
}
