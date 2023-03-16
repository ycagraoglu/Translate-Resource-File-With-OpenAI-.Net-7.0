using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using System.Collections;
using System.Resources.NetStandard;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace OpenAI
{
    public class ResourceTranslator : IResourceTranslator
    {
        private readonly IOpenAIService _openAIService;
        private readonly IWebHostEnvironment _environment;
        private readonly RequestLocalizationOptions _localizationOptions;
        private readonly RequestCulture _defaultRequestCulture;

        private readonly ResXResourceWriter? _Writer;
        private readonly ResXResourceReader? _Reader;
        private readonly Dictionary<string, string>? _Values;
        private readonly Dictionary<string, string>? _translatetResultSet;
        private readonly string _defaultSourceFileName;

        public ResourceTranslator(
            IOpenAIService openAIService,
            IWebHostEnvironment environment,
            IOptions<RequestLocalizationOptions> localizationOptions)
        {
            _localizationOptions = localizationOptions.Value;
            _defaultRequestCulture = _localizationOptions.DefaultRequestCulture;
            _environment = environment;
            _openAIService = openAIService;
            _defaultSourceFileName = Path.Combine(_environment.ContentRootPath, "Resources", $"Resource.{_defaultRequestCulture.Culture.Name}.resx");
            _Reader = new ResXResourceReader(_defaultSourceFileName);
            _translatetResultSet = new();
            _Values = new Dictionary<string, string>();

            if (File.Exists(_defaultSourceFileName))
                LoadResources();
        }

        private void LoadResources()
        {
            foreach (DictionaryEntry entry in _Reader)
                _Values.Add(entry.Key.ToString(), entry.Value.ToString());
            _Reader.Close();
        }

        public async Task<Dictionary<string, string>> AddResource()
        {
            IEnumerable<CultureInfo>? cultureList = _localizationOptions.SupportedCultures.Where(t => t.Name != _defaultRequestCulture.Culture.Name);

            if (cultureList.Any())
            {
                CultureInfo defaultCulture = _defaultRequestCulture.Culture;
                foreach (var culture in cultureList)
                {
                    _Values.Keys.ToList().ForEach(x => _Values[x] = string.Empty);

                    string resourceFilePath = Path.Combine(_environment.ContentRootPath, "Resources", $"Resource.{culture.Name}.resx");

                    Dictionary<string, string> resourceValues = new();

                    if (File.Exists(resourceFilePath))
                    {
                        ResXResourceReader resourceReader = new(resourceFilePath);

                        foreach (DictionaryEntry entry in resourceReader)
                            resourceValues.Add(entry.Key.ToString(), entry.Value.ToString());

                        resourceReader.Close();
                    }

                    ResXResourceWriter? _Writer = new ResXResourceWriter(resourceFilePath);
                    foreach (string key in _Values.Keys)
                    {
                        if (string.IsNullOrEmpty(_Values[key]) && _Values.ContainsKey(key))
                        {
                            if (!resourceValues.ContainsKey(key))
                            {
                                CompletionCreateResponse completionResult = await _openAIService.Completions.CreateCompletion(new CompletionCreateRequest()
                                {
                                    Prompt = $"Translate the following sentence from {GetCultureEnglishName(defaultCulture.EnglishName)} to {GetCultureEnglishName(culture.EnglishName)}: {key}",
                                    Model = Models.TextDavinciV3,
                                    Temperature = 0.5F,
                                    //MaxTokens = 1024,
                                    N = 1,
                                    Stop = null,
                                    TopP = 1,
                                    FrequencyPenalty = 0,
                                    PresencePenalty = 0

                                });

                                if (!completionResult.Successful)
                                    throw new Exception(completionResult.Error.Message);


                                string translateText = completionResult.Choices.FirstOrDefault().Text.ToString().Replace(".", "").Trim();
                                _Values[key] = translateText;
                                _Writer.AddResource(key, _Values[key]);
                                _translatetResultSet.Add($"{key} Translate to {GetCultureEnglishName(culture.EnglishName)} =>", _Values[key]);
                                System.Diagnostics.Debug.WriteLine($"{key} :{defaultCulture.DisplayName} Translate To {culture.DisplayName} Complete => {translateText}");

                            }
                            else
                            {
                                _Writer.AddResource(key, resourceValues[key]);
                            }
                        }
                    }

                    _Writer.Generate();
                    _Writer.Close();
                }
            }

            return _translatetResultSet;
        }

        private static string GetCultureEnglishName(string englishName)
        {
            var trimMe = englishName.Replace(" ", "").Trim();
            var indexOf = trimMe.IndexOf('(');
            return trimMe.Remove(indexOf);
        }

        public async Task<Dictionary<string, string>> Generate()
        {
            //foreach (string key in _Values.Keys)
            //{
            //    await AddResource(key, _Values[key]);
            //    _Writer.AddResource(key, _Values[key]);
            //}
            return await AddResource();
            //_Writer.Generate();
            //_Writer.Close();
            //_Reader.Close();
        }
    }

    public interface IResourceTranslator
    {
        Task<Dictionary<string, string>> Generate();
    }
}
