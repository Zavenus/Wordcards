using System;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace WinFormsListViewDemo;

internal sealed class WordCardsPlayer
{
    private SoundPlayer? _player;

    public void Stop()
    {
        try { _player?.Stop(); } catch { /* ignore */ }
    }

    public void Play(string word, string? soundPath)
    {
        // 目前只支援 wav：SoundPlayer
        // 若 soundPath 無效，會直接靜默（避免在測試機環境造成錯誤中斷）。
        Stop();

        if (string.IsNullOrWhiteSpace(soundPath)) return;

        // 允許相對路徑（以執行目錄為基準）
        var fullPath = soundPath;
        if (!Path.IsPathRooted(fullPath))
            fullPath = Path.Combine(Application.StartupPath, fullPath);

        if (!File.Exists(fullPath))
            return;

        _player = new SoundPlayer(fullPath);
        try
        {
            // 使用 Play()（非阻塞）
            _player.Play();
        }
        catch
        {
            // ignore
        }
    }
}

