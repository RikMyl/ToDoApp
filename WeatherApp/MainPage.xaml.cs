using System;

namespace WeatherApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnWeatherPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new WeatherPage());
    }

    private async void OnToDoPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ToDoPage());
    }
}
