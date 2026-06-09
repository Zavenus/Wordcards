using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace WinFormsListViewDemo;

public sealed class WordCollectionStorage : Collection<WordItem>
{
    public void LoadFromStringArray(string[] lines)
    {
        Clear();
        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;
            Add(new WordItem(raw));
        }
    }

    public void SaveToTsv(string path)
    {
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        var lines = this.Select(wi => wi.ToLineString()).ToArray();

        File.WriteAllLines(path, lines, utf8NoBom);
    }
}

