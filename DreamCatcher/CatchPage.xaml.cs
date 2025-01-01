using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text.Json;
using DreamCatcher.Models;

namespace DreamCatcher
{
    public partial class CatchPage : ContentPage
    {
        // 模拟问题列表

        private readonly string[] _editor_questions = {
            "具体形容一下昨晚的梦吧"
        };

        public CatchPage()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // 动画处理
            CatchPageTitles(title1, title2, questionLayout);
        }

        private async void CatchPageTitles(Label title1, Label title2, VerticalStackLayout questionLayout)
        {
            title1.IsVisible = true; // 使控件可见
            await title1.FadeTo(1, 500, Easing.SinInOut);

            await Task.Delay(2000); // 显示第一个标题 2 秒

            // 隐藏第一个标题（使用 FadeOut 动画）
            await title1.FadeTo(0, 500, Easing.SinInOut); // 透明度渐变到 0
            title1.IsVisible = false; // 使控件不可见

            // 显示第二个标题（使用 FadeIn 动画）
            title2.IsVisible = true; // 使控件可见
            await title2.FadeTo(1, 500, Easing.SinInOut); // 透明度渐变到 1

            // 对第二个标题执行缩小和上移动画
            await title2.ScaleTo(0.7, 500, Easing.SinInOut);
            await title2.TranslateTo(0, -20, 500, Easing.SinInOut);

            // 显示问题列表（使用 FadeIn 动画）
            questionLayout.IsVisible = true; // 使问题列表可见
            await questionLayout.FadeTo(1, 500, Easing.SinInOut); // 透明度渐变到 1

            
        }

        async void SubmitButton_Clicked(System.Object sender, System.EventArgs e){
            _loadingIndicator.IsRunning = true;
            _loadingIndicator.IsVisible = true;

            submitButton.IsEnabled = false;

            // 显示加载指示器
            var database = new DreamDataRepo();

            String Text = "";
            if (string.IsNullOrWhiteSpace(editor.Text.Trim()))
            {
                await DisplayAlert("错误", "梦境内容不能为空", "OK");
                return;
            }
            Text = editor.Text;
            
            await Task.Run(async () =>{
                var dream = new Dream
                {
                    Tag = await AIModelAPI.GetAIOneTagAsync(Text),
                    DreamType = await AIModelAPI.GetAIDreamTypeAsync(Text),
                    AIDreamTags = (await AIModelAPI.GetAITagsJSONAsync(Text)),
                    DreamText = Text,
                    Time = DateTime.Now
                };
                await database.AddNewDream(dream);
                this.Dispatcher.Dispatch(async() => {
                    editor.Text = "";
                    submitButton.IsEnabled = true;

                    // 隐藏加载指示器
                    _loadingIndicator.IsRunning = false;
                    _loadingIndicator.IsVisible = false;

                    await Navigation.PushAsync(new DreamInfoPage(dream));
                });
            });  
        }
    }
}
