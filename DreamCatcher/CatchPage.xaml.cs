using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text.Json;
using DreamCatcher.Models;

namespace DreamCatcher
{
    public partial class CatchPage : ContentPage
    {
        // ģ�������б�

        private readonly string[] _editor_questions = {
            "��������һ��������ΰ�"
        };

        public CatchPage()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // ��������
            CatchPageTitles(title1, title2, questionLayout);
        }

        private async void CatchPageTitles(Label title1, Label title2, VerticalStackLayout questionLayout)
        {
            title1.IsVisible = true; // ʹ�ؼ��ɼ�
            await title1.FadeTo(1, 500, Easing.SinInOut);

            await Task.Delay(2000); // ��ʾ��һ������ 2 ��

            // ���ص�һ�����⣨ʹ�� FadeOut ������
            await title1.FadeTo(0, 500, Easing.SinInOut); // ͸���Ƚ��䵽 0
            title1.IsVisible = false; // ʹ�ؼ����ɼ�

            // ��ʾ�ڶ������⣨ʹ�� FadeIn ������
            title2.IsVisible = true; // ʹ�ؼ��ɼ�
            await title2.FadeTo(1, 500, Easing.SinInOut); // ͸���Ƚ��䵽 1

            // �Եڶ�������ִ����С�����ƶ���
            await title2.ScaleTo(0.7, 500, Easing.SinInOut);
            await title2.TranslateTo(0, -20, 500, Easing.SinInOut);

            // ��ʾ�����б�ʹ�� FadeIn ������
            questionLayout.IsVisible = true; // ʹ�����б�ɼ�
            await questionLayout.FadeTo(1, 500, Easing.SinInOut); // ͸���Ƚ��䵽 1

            
        }

        async void SubmitButton_Clicked(System.Object sender, System.EventArgs e){
            _loadingIndicator.IsRunning = true;
            _loadingIndicator.IsVisible = true;

            submitButton.IsEnabled = false;

            // ��ʾ����ָʾ��
            var database = new DreamDataRepo();

            String Text = "";
            if (string.IsNullOrWhiteSpace(editor.Text.Trim()))
            {
                await DisplayAlert("����", "�ξ����ݲ���Ϊ��", "OK");
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

                    // ���ؼ���ָʾ��
                    _loadingIndicator.IsRunning = false;
                    _loadingIndicator.IsVisible = false;

                    await Navigation.PushAsync(new DreamInfoPage(dream));
                });
            });  
        }
    }
}
