namespace SmartHome.Mobile;

    public partial class AppShell : Shell
    {
        public AppShell()
        {

        InitializeComponent();

        Routing.RegisterRoute(nameof(DeviceDetailsPage), typeof(DeviceDetailsPage));

        }
    }

