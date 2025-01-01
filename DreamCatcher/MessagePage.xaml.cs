using System.Diagnostics;
using System.Text.Json;
using DreamCatcher.Models;
using OpenAI.Chat;

namespace DreamCatcher;

public partial class MessagePage : ContentPage
{
	private DateTime From;
	private DateTime To;
	private List<Dream> dreams = new List<Dream>();
	public MessagePage()
	{
		InitializeComponent();
		From = DateTime.Today;
		To = DateTime.Today;
		FromDate.MaximumDate = DateTime.Today;
		ToDate.MaximumDate = DateTime.Today;
	}

    private void Go_Clicked(object sender, EventArgs e)
    {
		From = FromDate.Date;
		To = ToDate.Date;
		RefreshUI();
    }
    private async void RefreshUI()
	{
        await DateChoosePanel.FadeTo(0, 1000, Easing.SinInOut);
        DateChoosePanel.IsVisible = false;
        
		DataShowPanel.IsVisible = true;

		dreams = await GetDreams();
        var button = new Button { Text = "返回",Margin=5 };
        button.Clicked += GoBack;
        var label = new Label { Text = "这期间你没有做过梦",Margin = 5 };
		if (dreams.Count == 0)
		{
			
			DataShowPanel.Add(label);
            DataShowPanel.Add(button);
            await DataShowPanel.FadeTo(1, 800, Easing.SinInOut);
            return;
		}
		label = new Label
		{
			Text = "这期间你一共做了" + dreams.Count + "个梦",
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = 5,
			FontSize = 16
		};
		DataShowPanel.Add(label);
		await label.FadeTo(1, 800, Easing.SinInOut);
		var good_dreams = dreams.Where(d => d.DreamType == 1).ToList();
		var bad_dreams = dreams.Where(d => d.DreamType == 2).ToList();
		var other_dreams = dreams.Where(d => d.DreamType == 3).ToList();
		label = new Label { Text = $"其中{good_dreams.Count}个美梦，{bad_dreams.Count}个噩梦，{other_dreams.Count}个光怪陆离的梦\n", HorizontalTextAlignment=TextAlignment.Center,Margin=5, FontSize = 16 };
		DataShowPanel.Add(label);
		await label.FadeTo(1, 800, Easing.SinInOut);
		var scenesCount = new Dictionary<string, int>();
		var charactersCount = new Dictionary<string, int>();
		foreach (var dream in dreams)
		{
			var tags = JsonSerializer.Deserialize<DreamTags>(dream.AIDreamTags);
			if(tags != null)
			{
                foreach (var scene in tags.scenes)
                {
                    if (scenesCount.ContainsKey(scene))
                    {
                        scenesCount[scene]++;
                    }
                    else
                    {
                        scenesCount[scene] = 1;
                    }
                }
                foreach (var character in tags.characters)
                {
                    if (charactersCount.ContainsKey(character))
                    {
                        charactersCount[character]++;
                    }
                    else
                    {
                        charactersCount[character] = 1;
                    }
                }
            }
			
			
		}
		var scenesMaxValue = scenesCount.Values.Max();
		var scenesKeysWithMaxValue = scenesCount.Where(pair => pair.Value == scenesMaxValue).Select(pair => pair.Key);
		var charactersMaxValue = charactersCount.Values.Max();
		var charactersKeysWithMaxValue = charactersCount.Where(pair => pair.Value == charactersMaxValue).Select(pair => pair.Key);
		var scenesString = scenesKeysWithMaxValue.First();
		scenesKeysWithMaxValue = scenesKeysWithMaxValue.Skip(1);
		var charactersString = charactersKeysWithMaxValue.First();
		charactersKeysWithMaxValue = charactersKeysWithMaxValue.Skip(1);
		foreach (var scene in scenesKeysWithMaxValue){
			scenesString += ", " + scene;
		}
		foreach (var character in charactersKeysWithMaxValue){
			charactersString += ", " + character;
		}
		var scenesLabel = new Label { Text = $"你最常梦到的场景是{scenesString}", HorizontalTextAlignment=TextAlignment.Center ,Margin=5, FontSize = 16 };
		var charactersLabel = new Label { Text = $"你梦中最常出现的人是{charactersString}\n", HorizontalTextAlignment=TextAlignment.Center, Margin=5, FontSize = 16 };
		DataShowPanel.Add(scenesLabel);
		await scenesLabel.FadeTo(1, 800, Easing.SinInOut);
		DataShowPanel.Add(charactersLabel);
		await charactersLabel.FadeTo(1, 800, Easing.SinInOut);
		label = new Label { Text = "接下来听听AI怎么分析你的梦吧", HorizontalTextAlignment=TextAlignment.Center, FontSize = 16 };
		DataShowPanel.Add(label);
		await label.FadeTo(1, 800, Easing.SinInOut);
		ActivityIndicator _loadingIndicator = new ActivityIndicator
		{
			IsRunning = false,
			IsVisible = false,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Margin = new Thickness(10, 10, 10, 10)
		};
		DataShowPanel.Add(_loadingIndicator);
		_loadingIndicator.IsRunning = true;
		_loadingIndicator.IsVisible = true;
		await Task.Run(async() =>
		{
			var streams = await AIModelAPI.GetAIAnalyseStreamAsync(good_dreams.Count, bad_dreams.Count, other_dreams.Count, scenesCount, charactersCount);
			string content = "";
			foreach (StreamingChatCompletionUpdate completionUpdate in streams)
			{
				if (completionUpdate.ContentUpdate.Count > 0)
				{
					content += completionUpdate.ContentUpdate[0].Text;
				}
			}
			content = StringUtils.RemoveMarkdown(content);

			// 在这里处理content，例如更新UI
			this.Dispatcher.Dispatch(() =>
			{
				_loadingIndicator.IsRunning = false;
				_loadingIndicator.IsVisible = false;
				var editor = new Editor { IsReadOnly = true, AutoSize = EditorAutoSizeOption.TextChanges};
				DataShowPanel.Add(editor);
				editor.Text = content;
			});
		});
		DataShowPanel.Add(button);
	}

    private async void GoBack(object? sender, EventArgs e)
    {
        await DataShowPanel.FadeTo(0, 1000, Easing.SinInOut);
        DataShowPanel.IsVisible = false;
		DataShowPanel.Clear();
		DateChoosePanel.IsVisible = true;
		await DateChoosePanel.FadeTo(1, 1000, Easing.SinInOut);
    }
	private async Task<List<Dream>> GetDreams(){
		return await new DreamDataRepo().GetDreamsBetweenDate(From,To);
	}
	private void FromDateSelect(object sender, DateChangedEventArgs e)
	{
		ToDate.MinimumDate = e.NewDate;
	}

	private void ToDateSelect(object sender, DateChangedEventArgs e)
	{
		FromDate.MaximumDate = e.NewDate;
	}
}