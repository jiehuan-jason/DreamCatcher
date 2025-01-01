using DreamCatcher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamCatcher
{
    public partial class DreamListPage : ContentPage
    {

        public DreamListPage()
        {
            InitializeComponent();

            // ����ListView��ItemsSource
            getAndSetDreamListSource();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing(); // ȷ�����û���� OnAppearing ����

            // ִ������ˢ�²���
            getAndSetDreamListSource();
        }
        private async void getAndSetDreamListSource()
        {
            var database = new DreamDataRepo();
            var dreams = await database.GetAllDreams();
            dreams = dreams.OrderByDescending(dream => dream.Id).ToList();
            dreamListView.ItemsSource = dreams;
        }

        private async void dreamListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem is Dream dream)
            {
               await Navigation.PushAsync(new DreamInfoPage(dream));
            }
        }


        private async void SearchButton_Clicked(object sender, EventArgs e){
            searchButton.IsEnabled = false;

            var dream_list = await AIModelAPI.SearchDreamsAIAsync(inputView.Text);
            if(dream_list.Count == 0){
                await DisplayAlert("δ�ҵ�����ξ�", $"û���ҵ���{inputView.Text}�йص��ξ�", "ȷ��");
                searchButton.IsEnabled = true;
                return;
            }else{
                dream_list = dream_list.OrderByDescending(dream => dream.Id).ToList();
                dreamListView.ItemsSource = dream_list;
                searchButton.IsEnabled = true;
            }
        }

    }
    public class DreamTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int dreamType)
            {
                switch (dreamType)
                {
                    case 1:
                        return "����";
                    case 2:
                        return "ج��";
                    case 3:
                        return "���½��";
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TextTruncatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                // ȥ�����л��з�
                text = text.Replace("\n", " ").Replace("\r", " ");
                
                // ����Ϊǰ20���ַ�
                if (text.Length > 20)
                {
                    text = text.Substring(0, 20) + "...";
                }

                return text;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ����ֻ��һ������ת����������Ҫʵ��
            throw new NotImplementedException();
        }
    }
}
