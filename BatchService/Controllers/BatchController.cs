using System.Threading;
using System.Threading.Tasks;
using BatchService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BatchService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BatchController : Controller
    {
        private readonly ILogger<BatchController> _logger;
        private readonly IInitializer _initializer;

        public BatchController(
            ILogger<BatchController> logger,
            IInitializer initializer)
        {
            _logger = logger;
            _initializer = initializer;
        }


        [HttpPost]
        public Task Initialize(CancellationToken cancellationToken) => _initializer.InitializeAsync(cancellationToken);
    }
}
