using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameStore.CrossCutting.Localization
{
    public interface ITranslationService
    {
        string Translate(string key);
        void SetCulture(string culture);
    }
}
