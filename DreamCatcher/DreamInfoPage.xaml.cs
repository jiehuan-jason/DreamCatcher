using DreamCatcher.Models;
using System.Text.Json;

namespace DreamCatcher;

public partial class DreamInfoPage : ContentPage
{
	private Dream dream;

	public DreamInfoPage(Dream dream)
	{
		InitializeComponent();
		this.dream = dream;
		SetupUI();
	}

	private void SetupUI(){
		switch(dream.DreamType){
			case 1:
				DreamTitleLabel.Text = "美梦 "+"#" +dream.Tag;
				break;
			case 2:
				DreamTitleLabel.Text = "噩梦 " + "#" + dream.Tag;
				break;
			case 3:
				DreamTitleLabel.Text = "光怪陆离 " + "#" + dream.Tag;
				break;
		}
		var dreamTags = JsonSerializer.Deserialize<DreamTags>(dream.AIDreamTags);
		if (dreamTags != null)
		{	
			if((dreamTags.scenes) != null)
				foreach (var tag in dreamTags.scenes)
					DreamTagsList.Text += "#" + tag + " ";
            if ((dreamTags.characters) != null)
                foreach (var tag in dreamTags.characters)
					DreamTagsList.Text += "#" + tag + " ";
		}
		DreamDescriptionEditor.Text = dream.DreamText;
		DreamTimeLabel.Text = dream.Time.ToString("yyyy-MM-dd HH:mm");
		if (dream.IsImageGenerated)
		{
			DreamAIPictureButton.IsVisible = false;
            DreamAIPictureImage.Source = ImageSource.FromStream(() => new MemoryStream(dream.ImageData));
        }
        if (dream.IsAnalyseGenerated)
        {
			DreamAITextEditor.IsVisible = true;
            DreamAITextButton.IsVisible = false;
			DreamAITextEditor.Text = dream.Analyse;
        }
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
		await new DreamDataRepo().ModifyDream(dream);
		await Navigation.PopAsync();
	}

    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
		await new DreamDataRepo().DeleteDream(dream);
		await Navigation.PopAsync();
    }

	//TODO share
	private async void DreamAIPictureButton_Clicked(object sender, EventArgs e){
		disenableButtons();

        _loadingIndicator.IsRunning = true;
		_loadingIndicator.IsVisible = true;

		await Task.Run(async () =>{
			var pic = await AIModelAPI.GetAIPicFromDreamTextAsync(dream.DreamText);
			dream.ImageData = pic;
			dream.IsImageGenerated = true;
			await new DreamDataRepo().ModifyDream(dream);
			this.Dispatcher.Dispatch(() =>{
				DreamAIPictureImage.Source = ImageSource.FromStream(() => new MemoryStream(pic));
				_loadingIndicator.IsRunning = false;
				_loadingIndicator.IsVisible = false;
				enableButtons();
				DreamAIPictureButton.IsVisible = false;
			});
			});

		
    }

    private async void DreamAITextButton_Clicked(object sender, EventArgs e)
    {
		disenableButtons();

        _loadingIndicatorText.IsRunning = true;
        _loadingIndicatorText.IsVisible = true;

		await Task.Run(async () =>{
			var content = StringUtils.RemoveMarkdown(await AIModelAPI.GetAIAnalyseAsync(dream.DreamText));
			await new DreamDataRepo().ModifyDream(dream);

			this.Dispatcher.Dispatch(() =>{
				DreamAITextEditor.IsVisible = true;
				DreamAITextEditor.Text = content;
				dream.Analyse = content;
				dream.IsAnalyseGenerated = true;
				_loadingIndicatorText.IsRunning = false;
				_loadingIndicatorText.IsVisible = false;
				enableButtons();
				DreamAITextButton.IsVisible = false;
			});
		});
    }
	private void disenableButtons()
	{
        DreamAIPictureButton.IsEnabled = false;
        SaveButton.IsEnabled = false;
        DeleteButton.IsEnabled = false;
        DreamAITextButton.IsEnabled = false;
    }

	private void enableButtons()
	{
        DreamAIPictureButton.IsEnabled = true;
        SaveButton.IsEnabled = true;
        DeleteButton.IsEnabled = true;
        DreamAITextButton.IsEnabled = true;
    }
}