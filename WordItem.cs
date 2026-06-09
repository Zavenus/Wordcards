using System;
using System.Linq;

namespace WinFormsListViewDemo;

public sealed class WordItem
{
    public string Word { get; }
    public string Pronunciation { get; }
    public string AudioPath { get; }
    public string Explanation { get; }

    // 預期輸入格式：
    // 0=單字, 1=音標, 2=音檔路徑, 3=解釋（解釋可含 Tab 或其他分隔符：此處假設剩餘欄位要合併）
    public WordItem(string line)
    {
        line = line.TrimEnd('\r', '\n');

        var parts = line.Split('\t');
        if (parts.Length < 4)
            throw new FormatException($"TSV line must contain at least 4 columns. Actual columns: {parts.Length}. Line: {line}");

        Word = parts[0];
        Pronunciation = parts[1];
        AudioPath = parts[2];

        // 第 4 欄開始視為解釋
        Explanation = string.Join(Environment.NewLine, parts.Skip(3));
    }

    public override string ToString() => Word;

    // 儲存回 TSV 時，將 Explanation 內的換行還原成 TSV 的定位字元（\t）。
    public string ToLineString()
    {
        var explanationNormalized = (Explanation ?? string.Empty).Replace(Environment.NewLine, "\t");
        return string.Join("\t", new[]
        {
            Word ?? string.Empty,
            Pronunciation ?? string.Empty,
            AudioPath ?? string.Empty,
            explanationNormalized
        });
    }
}

