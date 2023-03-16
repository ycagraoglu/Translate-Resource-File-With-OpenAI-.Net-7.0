using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace OpenAI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranslateController : ControllerBase
    {
        private readonly IStringLocalizer<Resource> _localizer;
        private readonly IResourceTranslator _resourceTranlator;

        public TranslateController(IStringLocalizer<Resource> localizer, IResourceTranslator resourceTranlator)
        {
            _localizer = localizer;
            _resourceTranlator = resourceTranlator;
        }

        [Route("~/TranslateResource")]
        [HttpPut]
        public async Task<IActionResult> TranslateResource()
        {
            var translate = _localizer.GetString(name: "Merhaba Dünya");

            return Ok(await _resourceTranlator.Generate());
        }
    }
}