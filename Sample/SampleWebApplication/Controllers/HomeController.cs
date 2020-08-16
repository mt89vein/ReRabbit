using Microsoft.AspNetCore.Mvc;

namespace SampleWebApplication.Controllers
{
    /// <summary>
    /// Входной контроллер по умолчанию.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public sealed class HomeController : ControllerBase
    {
        /// <summary>
        /// Редирект на swagger-документацию.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return new RedirectResult("~/api-docs");
        }
    }
}