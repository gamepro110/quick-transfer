namespace quick_transfer_gui
{
    public partial class MainPage : ContentPage
    {
        private int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            CounterBtn.Text = $"working dir: {Directory.GetCurrentDirectory()} ...";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}