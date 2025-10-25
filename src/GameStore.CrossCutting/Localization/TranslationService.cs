using GameStore.CrossCutting;
using GameStore.CrossCutting.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

public class TranslationService : ITranslationService
{
    private readonly IStringLocalizer _localizer;
    private readonly LocalizationSettings _settings;

    public TranslationService(
        IStringLocalizer<SharedResource> localizer,
        IOptions<LocalizationSettings> options)
    {
        _localizer = localizer;
        _settings = options.Value;

        SetCulture(_settings.DefaultLanguage);
    }

    public string Translate(string key)
    {
        return _localizer[key];
    }

    public void SetCulture(string culture)
    {
        var ci = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
    }
}
