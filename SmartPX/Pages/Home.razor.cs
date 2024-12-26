using Microsoft.AspNetCore.Components;

namespace SmartPX.Pages;

public partial class Home : ComponentBase {
    [Inject] private SmartPXApi api { get; set; } = null!;
    
    public void runTestApi() {
        api.GetDeviceLogs("bf7c446508d30254c9xv8k");
    }
}