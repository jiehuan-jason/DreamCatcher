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

            // 设置ListView的ItemsSource
            getAndSetDreamListSource();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing(); // 确保调用基类的 OnAppearing 方法

            // 执行数据刷新操作
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
                await DisplayAlert("未找到相关梦境", $"没有找到与{inputView.Text}有关的梦境", "确定");
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
                        return "美梦";
                    case 2:
                        return "噩梦";
                    case 3:
                        return "光怪陆离";
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
                // 去除所有换行符
                text = text.Replace("\n", " ").Replace("\r", " ");
                
                // 限制为前20个字符
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
            // 这里只是一个单向转换器，不需要实现
            throw new NotImplementedException();
        }
    }
}
