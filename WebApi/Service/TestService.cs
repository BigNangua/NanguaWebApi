using WebApi.IService;

namespace WebApi.Service
{
    public class TestService : ITestService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestService> _logger;

        public TestService(IConfiguration configuration, ILogger<TestService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string getTest()
        {
            return "GET OK";
        }

        public string postTest(String boby)
        {
            return $"POST OK  boby:{boby}";
        }
    }
}
