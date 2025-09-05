using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Math; // optional; you can just use System
using Tigrinya.App.Mobile.Controls;
using Tigrinya.App.Mobile.Services;

namespace Tigrinya.App.Mobile.Pages;

[QueryProperty(nameof(LessonId), "lessonId")]
public partial class LessonPlayerPage : ContentPage
{
    private readonly ContentService _content;
    private readonly AudioService _audio;
    private readonly TemplateService _templates;

    private Lesson? _lesson;
    private int _index = 0;

    // If your XAML defines these with x:Name, keep them. Otherwise change to your own.
    // E.g., in XAML: <Label x:Name="TitleLabel" .../> etc.
    // TitleLabel, ProgressLabel, and TaskHost are assumed to be named elements in XAML.

    public string? LessonId { get; set; }

    public LessonPlayerPage(ContentService content, AudioService audio, TemplateService templates)
    {
        InitializeComponent();
        _content = content;
        _audio = audio;
        _templates = templates;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_lesson == null)
        {
            var id = LessonId ?? "alphabet-1";
            _lesson = await _content.LoadLessonAsync(id);
            if (_lesson == null)
            {
                await DisplayAlert("Error", "Could not load lesson.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (TitleLabel is not null)
                TitleLabel.Text = _lesson.Title;

            _index = 0;
            RenderTask();
        }
    }

    private void RenderTask()
    {
        if (_lesson == null) return;

        if (ProgressLabel is not null)
            ProgressLabel.Text = $"{_index + 1} / {_lesson.Tasks.Count}";

        var task = _lesson.Tasks[_index];

        View view = task.Type switch
        {
            "trace" => BuildTraceTask(task),
            "listen_select" => BuildListenSelectTask(task),
            "read_tile" => BuildReadTileTask(task),

            // If you later implement these, add their builders and re-enable the cases:
            // "picture_word" => BuildPictureWordTask(task),
            // "type_mini_keyboard" => BuildTypeMiniKeyboardTask(task),
            // "dialog_goal" => BuildDialogGoalTask(task),
            // "grammar_tip" => BuildGrammarTipTask(task),
            // "type_number" => BuildTypeNumberTask(task),
            // "listen_number" => BuildListenNumberTask(task),
            // "price_match" => BuildPriceMatchTask(task),
            // "translate_select" => BuildTranslateSelectTask(task),
            // "order_words" => BuildOrderWordsTask(task),
            // "label_match" => BuildLabelMatchTask(task),
            // "flashcard" => BuildFlashcardTask(task),

            _ => new Label { Text = "Unsupported task", FontSize = 18, HorizontalTextAlignment = TextAlignment.Center }
        };

        if (TaskHost is not null)
            TaskHost.Content = view;
    }

    private View BuildTraceTask(TaskItem task)
    {
        var glyph = task.Glyph ?? "ሀ";

        var bgGlyph = new Label
        {
            Text = glyph,
            Opacity = 0.15,
            FontSize = 220,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        var canvas = new Controls.TraceCanvasView { HeightRequest = 320 };
        var tpl = _templates.LoadTemplateBitmap(glyph, 256, 26);
        canvas.SetTemplate(tpl, 256);

        var prompt = new Label { Text = task.PromptEn ?? "Trace the character", FontSize = 18 };

        var clearBtn = new Button { Text = "Clear" };
        clearBtn.Clicked += (s, e) => canvas.Clear();

        var checkBtn = new Button { Text = "Check" };
        checkBtn.Clicked += async (s, e) =>
        {
            var score = canvas.ComputeScore();
            var pct = Math.Round(score * 100);
            var ok = score >= 0.6f; // threshold
            await DisplayAlert("Trace", ok ? $"Great! Match: {pct}%" : $"Not quite ({pct}%). Add more strokes.", "OK");
            if (ok) Advance();
        };

        var grid = new Grid();
        grid.Children.Add(bgGlyph);
        grid.Children.Add(canvas);

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                prompt,
                grid,
                new HorizontalStackLayout { Spacing = 8, Children = { clearBtn, checkBtn } }
            }
        };
    }

    private View BuildListenSelectTask(TaskItem task)
    {
        var prompt = new Label { Text = task.PromptEn ?? "Tap what you hear", FontSize = 18 };
        var answer = task.Answer ?? "ሀ";
        var choices = task.Choices ?? new List<string> { "ሀ", "ሁ", "ሂ", "ሃ" };
        var status = new Label { Text = "", FontSize = 16 };

        var flex = new FlexLayout { Wrap = FlexWrap.Wrap, JustifyContent = FlexJustify.SpaceAround };

        foreach (var c in choices)
        {
            var btn = new Button { Text = c, FontSize = 28, WidthRequest = 72, HeightRequest = 72 };
            btn.Clicked += (s, e) =>
            {
                if (c == answer)
                {
                    status.Text = "✅ Correct";
                    Advance();
                }
                else
                {
                    status.Text = "❌ Try again";
                }
            };
            flex.Children.Add(btn);
        }

        var play = new Button { Text = "▶ Play audio" };
        play.Clicked += async (s, e) =>
        {
            // Hook up your real audio via _audio later
            await DisplayAlert("Audio", $"(Placeholder) Playing {task.Audio ?? "sample"}.", "OK");
        };

        return new VerticalStackLayout { Spacing = 12, Children = { prompt, play, flex, status } };
    }

    private View BuildReadTileTask(TaskItem task)
    {
        var tiles = task.Tiles ?? new List<string> { "ሀ", "ሁ", "ሂ" };
        var prompt = new Label { Text = task.PromptEn ?? "Tap tiles in order", FontSize = 18 };
        var status = new Label { Text = "" };

        int i = 0;

        var flex = new FlexLayout { Wrap = FlexWrap.Wrap, JustifyContent = FlexJustify.SpaceAround };
        foreach (var t in tiles)
        {
            var btn = new Button { Text = t, FontSize = 28, WidthRequest = 72, HeightRequest = 72 };
            btn.Clicked += (s, e) =>
            {
                if (btn.Text == tiles[i])
                {
                    btn.IsEnabled = false;
                    btn.Opacity = 0.6;
                    i++;
                    if (i >= tiles.Count)
                    {
                        status.Text = "✅ Sequence complete";
                        Advance();
                    }
                }
                else
                {
                    status.Text = "❌ Wrong tile — try that one later";
                }
            };
            flex.Children.Add(btn);
        }

        return new VerticalStackLayout { Spacing = 12, Children = { prompt, flex, status } };
    }

    private async void Advance()
    {
        if (_lesson == null) return;

        _index++;

        if (_index >= _lesson.Tasks.Count)
        {
            await DisplayAlert("Lesson", "All tasks completed! 🎉", "Done");
            await Shell.Current.GoToAsync("..");
            return;
        }

        RenderTask();
    }

    // If you have buttons wired in XAML:
    private void OnNext(object sender, EventArgs e) => Advance();
    private void OnSkip(object sender, EventArgs e) => Advance();
}
