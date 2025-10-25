namespace GameStore.CrossCutting.Localization
{
    public interface ITranslationService
    {
        string Translate(string key);
        void SetCulture(string culture);
    }
}
