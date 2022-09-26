using gui.Views;

namespace gui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(NotePage), typeof(Views.NotePage));
    }
}