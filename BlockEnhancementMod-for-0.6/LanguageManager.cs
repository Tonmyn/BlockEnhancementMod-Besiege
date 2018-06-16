using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockEnhancementMod
{
    class LanguageManager : SingleInstance<LanguageManager>
    {
        public override string Name { get; } = "Language Manager";

        public enum Language
        {
            Chinese = 0,
            English = 1
        }

        public Language currentLanguage = Language.Chinese;

        public Language GetLanguage()
        {
            Language L = new Language();

            return L;
        }
    }
}
