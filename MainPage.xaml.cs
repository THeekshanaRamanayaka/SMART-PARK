using Microsoft.Extensions.Logging;

namespace SmartPark;

public partial class MainPage : ContentPage
{
    private readonly ILogger<MainPage> _logger;

    public MainPage(ILogger<MainPage> logger)
    {
        _logger = logger;
        _logger.LogInformation("MainPage constructor called");
        InitializeComponent();
        _logger.LogInformation("MainPage initialized");
    }
}
