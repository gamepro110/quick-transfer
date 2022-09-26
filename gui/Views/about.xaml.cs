using gui.Models;

namespace gui.Views;

public partial class about : ContentPage
{
    public about()
    {
        InitializeComponent();
    }

    private async void LearnMore_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is Models.About about)
        {
            await Launcher.Default.OpenAsync(about.MoreInfoUrl);
        }
    }
}