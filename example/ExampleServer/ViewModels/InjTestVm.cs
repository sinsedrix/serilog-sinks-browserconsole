using Microsoft.Extensions.Logging;

namespace ExampleServer.ViewModels
{
    public class InjTestVm
    {
        readonly ILogger _logger;
        public InjTestVm(ILogger<InjTestVm> logger)
        {
            _logger = logger;
        }

        public void SendSomeLogs()
        {
            _logger.LogInformation("Hello");
            _logger.LogWarning("Be carefull");
            _logger.LogError("Failed");
        }
    }
}
