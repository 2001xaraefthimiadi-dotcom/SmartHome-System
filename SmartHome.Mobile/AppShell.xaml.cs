namespace SmartHome.Mobile;

    public partial class AppShell : Shell
    {
        public AppShell()
        {
        InitializeComponent();

        Routing.RegisterRoute(nameof(DeviceDetailsPage), typeof(DeviceDetailsPage));
        Routing.RegisterRoute(nameof(CreateAutomationPage),typeof(CreateAutomationPage));
        Routing.RegisterRoute(nameof(EditAutomationPage),typeof(EditAutomationPage));

         }
    }

