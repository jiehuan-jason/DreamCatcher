using System.Windows.Input;

namespace DreamCatcher;

public class HyperlinkUI : Span
{
    public static readonly BindableProperty LinkUrlProperty =
     BindableProperty.Create(nameof(LinkUrl), typeof(string), typeof(HyperlinkUI), null);

    public string LinkUrl
    {
        get
        {
            return (string)GetValue(LinkUrlProperty);
        }
        set
        {
            SetValue(LinkUrlProperty, value);
        }
    }

    public HyperlinkUI()
    {
        ApplyHyperlinkAppearance();
        AddGestureRecognizer();
    }

    void ApplyHyperlinkAppearance()
    {
        this.TextColor = Color.FromArgb("#0000EE");
        this.TextDecorations = TextDecorations.Underline;
    }

    void AddGestureRecognizer()
    {
        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += async (s, e) => await OpenLinkAsync();
        this.GestureRecognizers.Add(tapGestureRecognizer);
    }

    async Task OpenLinkAsync()
    {
        if (!string.IsNullOrWhiteSpace(LinkUrl) && Uri.TryCreate(LinkUrl, UriKind.Absolute, out var uri))
        {
            await Launcher.OpenAsync(uri);
        }
        else
        {
            // 处理无效 URL 的情况，例如显示警告或记录日志
            Console.WriteLine("Invalid URL");
        }
    }
}