using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace ZerosToDo;

public static class TextSpeech
{
    static readonly SpeechSynthesizer Speech = new() { Volume = 100, Rate = 0 };

    [AllowNull]
    static Prompt CurrentPrompt;

    [AllowNull]
    static StreamReader Reader;

    /// <summary>
    /// 确保当前文件存在
    /// </summary>
    public static bool FileExist { get; private set; }

    /// <summary>
    /// 当前文件路径
    /// </summary>
    private static string _current_file_path = string.Empty;
    public static string CurrentFilePath
    {
        get => _current_file_path;
        set
        {
            _current_file_path = value;
            FileExist = File.Exists(_current_file_path);
        }
    }

    public static void ScanFile(string _directory_path)
    {
        string[] _path_list = Directory.GetFiles(_directory_path, "*.txt");
        App.Setting.TxtFileNameList.Clear();
        foreach (var item in _path_list)
        {
            App.Setting.TxtFileNameList.Add(Path.GetFileNameWithoutExtension(item));
        }
        CurrentFilePath = _path_list[0];
    }

    public static void SkipLine(int _count)
    {
        if (!FileExist) { throw new Exception("文件不存在"); }
        Reader?.Close();
        Reader = new StreamReader(CurrentFilePath);

        if (_count > 5) { _count -= 3; }
        for (int i = 0; i < _count; i++) { Reader.ReadLine(); }
    }

    public static void Start()
    {
        string? _text = Reader.ReadLine();
        while (_text is not null)
        {
            // 使用正则表达式替换空白行，并进行分割
            if (!string.IsNullOrEmpty(_text))
            {
                string _pattern_1 = @"^\s*$\n?";
                string _temp = Regex.Replace(_text, _pattern_1, "", RegexOptions.Multiline);

                string _pattern_2 = @"([.。?？!！])";
                string[] _parts = Regex.Split(_temp, _pattern_2);

                foreach (string _part in _parts)
                {
                    CurrentPrompt = Speech.SpeakAsync(_part);
                }
            }

            _text = Reader.ReadLine();
        }
    }

    public static void Pause()
    {
        if (CurrentPrompt is not null)
        {
            Speech.Pause();
        }
    }

    public static void Close()
    {
        Reader?.Close();
        Reader?.Dispose();
        Reader = null;

        Speech.Dispose();
    }
}
