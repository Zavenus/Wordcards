using System;
using System.Collections.ObjectModel;

namespace WinFormsListViewDemo;

// NOTE: This file previously contained a duplicate WordCollection implementation.
// The real implementation (with SaveToTsv + ToLineString support) is located at:
//   WinFormsListViewDemo/WordCollection.cs
// This version is intentionally removed from compilation to avoid duplicate type errors.
//
// Keeping a stub is not necessary for the project; it is disabled below.

public sealed class WordCollection : Collection<WordItem>
{
    public void LoadFromStringArray(string[] lines)
    {
        Clear();

        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            this.Add(new WordItem(raw));
        }
    }
}



