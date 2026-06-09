using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WinFormsListViewDemo;


public sealed class MainForm : Form
{
    private const string CardsFileName = "WordCards.txt";

    private readonly ListBox lstWords = new();
    private readonly Label lblWord = new();
    private readonly Label lblPhonogram = new();
    private readonly Label lblExplainCaption = new();
    private readonly TextBox txtExplain = new();

    private readonly TextBox txtWord = new();
    private readonly TextBox txtPhonogram = new();
    private readonly TextBox txtSoundPath = new();

    private readonly Button btnPlay = new();
    private readonly CheckBox chkAuto = new();
    private readonly Button btnSave = new();

    private readonly StatusStrip statusStrip = new();
    private readonly ToolStripStatusLabel tsslMessage = new();

    private readonly WordCollectionStorage wordList = new();

    private readonly WordCardsPlayer player = new();



private readonly System.Windows.Forms.Timer autoTimer = new();


    private const int AutoIntervalMs = 1200;
    private bool isAutoRunning;

    public MainForm()
    {
        Text = "WordCards";
        Width = 980;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        InitLayout();
        InitEvents();

        autoTimer.Interval = AutoIntervalMs;
        autoTimer.Tick += (_, _) => NextCard();

        Load += (_, _) => LoadCardsOnStartup();
        FormClosing += (_, _) => player.Stop();

    }

    private void InitLayout()
    {
        var left = new Panel { Left = 12, Top = 44, Width = 360, Height = 520, BorderStyle = BorderStyle.FixedSingle };
        lstWords.Left = 6;
        lstWords.Top = 6;
        lstWords.Width = 345;
        lstWords.Height = 470;
        lstWords.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        left.Controls.Add(lstWords);

        btnPlay.Text = "播放";
        btnPlay.Left = 6;
        btnPlay.Top = 482;
        btnPlay.Width = 90;
        left.Controls.Add(btnPlay);

        chkAuto.Text = "Auto";
        chkAuto.Left = 102;
        chkAuto.Top = 482;
        chkAuto.Width = 70;
        left.Controls.Add(chkAuto);

        btnSave.Text = "存檔";
        btnSave.Left = 6;
        btnSave.Top = 512;
        btnSave.Width = 90;
        left.Controls.Add(btnSave);

        Controls.Add(left);

        var rightLeft = 390;

        lblWord.AutoSize = true;
        lblWord.Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold);
        lblWord.Left = rightLeft;
        lblWord.Top = 62;
        lblWord.Width = 560;
        Controls.Add(lblWord);

        lblPhonogram.AutoSize = true;
        lblPhonogram.Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
        lblPhonogram.Left = rightLeft;
        lblPhonogram.Top = 102;
        lblPhonogram.Width = 560;
        Controls.Add(lblPhonogram);

        lblExplainCaption.Text = "解釋";
        lblExplainCaption.AutoSize = true;
        lblExplainCaption.Left = rightLeft;
        lblExplainCaption.Top = 132;
        Controls.Add(lblExplainCaption);

        txtExplain.Multiline = true;
        txtExplain.Left = rightLeft;
        txtExplain.Top = 154;
        txtExplain.Width = 560;
        txtExplain.Height = 220;
        txtExplain.ScrollBars = ScrollBars.Vertical;
        Controls.Add(txtExplain);

        var editTop = 392;
        var editLabelW = 70;
        var labelX = rightLeft;
        var inputX = rightLeft + editLabelW + 6;

        var lbl1 = new Label { Text = "單字", Left = labelX, Top = editTop + 0, Width = editLabelW };
        txtWord.Left = inputX;
        txtWord.Top = editTop + 0;
        txtWord.Width = 240;

        var lbl2 = new Label { Text = "音標", Left = labelX, Top = editTop + 38, Width = editLabelW };
        txtPhonogram.Left = inputX;
        txtPhonogram.Top = editTop + 38;
        txtPhonogram.Width = 240;

        var lbl3 = new Label { Text = "音檔", Left = labelX, Top = editTop + 76, Width = editLabelW };
        txtSoundPath.Left = inputX;
        txtSoundPath.Top = editTop + 76;
        txtSoundPath.Width = 400;

        Controls.Add(lbl1);
        Controls.Add(txtWord);
        Controls.Add(lbl2);
        Controls.Add(txtPhonogram);
        Controls.Add(lbl3);
        Controls.Add(txtSoundPath);

        statusStrip.Dock = DockStyle.Bottom;
        tsslMessage.Text = "Ready";
        statusStrip.Items.Add(tsslMessage);
        Controls.Add(statusStrip);
    }

    private void InitEvents()
    {
        lstWords.SelectedIndexChanged += (_, _) =>
        {
            if (lstWords.SelectedIndex >= 0)
            {
                isAutoRunning = false;
                chkAuto.Checked = false;
                autoTimer.Stop();
                ShowCardByIndex(lstWords.SelectedIndex);
            }
        };

        btnPlay.Click += (_, _) => PlayCurrent();

        chkAuto.CheckedChanged += (_, _) =>
        {
            isAutoRunning = chkAuto.Checked;
            if (isAutoRunning)
            {
                autoTimer.Start();
                PlayCurrent();
            }
            else
            {
                autoTimer.Stop();
            }
        };

        btnSave.Click += (_, _) => SaveCurrent();

        KeyDown += MainForm_KeyDown;
        lstWords.KeyDown += MainForm_KeyDown;
        txtExplain.KeyDown += MainForm_KeyDown;
        txtWord.KeyDown += MainForm_KeyDown;
        txtPhonogram.KeyDown += MainForm_KeyDown;
        txtSoundPath.KeyDown += MainForm_KeyDown;
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        bool inTextBox = ActiveControl is TextBox;

        if (e.KeyCode == Keys.Space)
        {
            if (!inTextBox)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                PlayCurrent();
            }
            return;
        }

        if (e.KeyCode == Keys.Enter)
        {
            if (!inTextBox)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                NextCard();
            }
            return;
        }
    }

    private string CardsPath => Path.Combine(Application.StartupPath, CardsFileName);

    private void LoadCardsOnStartup()
    {
        try
        {
            if (!File.Exists(CardsPath))
            {
                // 建立範例（A 格式：單字\t音標\t音檔路徑\t解釋）
                var sample = new[]
                {
                    "apple\taple\t.\tA fruit",
                    "book\tbʊk\t.\tSomething you read"
                };
                File.WriteAllLines(CardsPath, sample, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            var lines = File.ReadAllLines(CardsPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            wordList.LoadFromStringArray(lines);

            lstWords.Items.Clear();
            foreach (var w in wordList) lstWords.Items.Add(w.Word);

            if (wordList.Count > 0)
            {
                lstWords.SelectedIndex = 0;
                ShowCardByIndex(0);
            }

            tsslMessage.Text = $"Loaded {wordList.Count} words";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"讀取 WordCards.txt 失敗：{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private int CurrentIndex => lstWords.SelectedIndex;

    private void ShowCardByIndex(int index)
    {
        if (index < 0 || index >= wordList.Count) return;

        var w = wordList[index];
        lblWord.Text = w.Word;
        lblPhonogram.Text = w.Phonogram;
        txtExplain.Text = w.Explain;

        txtWord.Text = w.Word;
        txtPhonogram.Text = w.Phonogram;
        txtSoundPath.Text = w.SoundPath;
    }

    private void NextCard()
    {
        if (wordList.Count == 0) return;

        int next = CurrentIndex + 1;
        if (next >= wordList.Count) next = 0;

        lstWords.SelectedIndex = next;
        ShowCardByIndex(next);
        PlayCurrent();
    }

    private void PlayCurrent()
    {
        if (wordList.Count == 0) return;
        if (CurrentIndex < 0 || CurrentIndex >= wordList.Count) return;

        var w = wordList[CurrentIndex];
        lblWord.Text = w.Word;

        try
        {
            // 目前實作：播放 wav（SoundPlayer）
            player.Play(w.Word, w.SoundPath);

        }
        catch (Exception ex)
        {
            tsslMessage.Text = "Play failed: " + ex.Message;
        }
    }

    private void SaveCurrent()
    {
        if (wordList.Count == 0) return;
        int idx = CurrentIndex;
        if (idx < 0 || idx >= wordList.Count) return;

        var newWord = txtWord.Text ?? string.Empty;
        var newPhon = txtPhonogram.Text ?? string.Empty;
        var newSound = txtSoundPath.Text ?? string.Empty;
        var newExplain = txtExplain.Text ?? string.Empty;

        // 把多行解釋轉回 TSV 欄位分隔符（與 WordItem.ToLineString 同邏輯）
        var explainNormalized = newExplain.Replace(Environment.NewLine, "\t");
        var line = string.Join("\t", new[] { newWord, newPhon, newSound, explainNormalized });

        var updated = new WordItem(line);
        wordList[idx] = updated;

        lstWords.Items[idx] = updated.Word;
        wordList.SaveToTsv(CardsPath);

        tsslMessage.Text = $"Saved: {updated.Word}";
    }
}

