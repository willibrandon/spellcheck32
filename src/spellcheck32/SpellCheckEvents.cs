using Windows.Win32.Globalization;

namespace spellcheck32;

public partial class SpellChecker
{
    private class SpellCheckEvents(SpellChecker spellChecker) : ISpellCheckerChangedEventHandler
    {
        public void Invoke(ISpellChecker sender)
        {
            if (sender == spellChecker?._spellChecker)
            {
                spellChecker.OnSpellCheckerChanged();
            }
        }
    }
}
