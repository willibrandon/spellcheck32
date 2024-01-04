using Windows.Win32.Globalization;

namespace spellcheck32;

public partial class SpellChecker
{
    private class SpellCheckEvents(SpellChecker spellChecker) : ISpellCheckerChangedEventHandler
    {
        public void Invoke(ISpellChecker sender)
        {
            if (spellChecker.SpellCheckerChanged is null)
            {
                return;
            }

            // Invoke each delegate in the invocation list once in reverse order.
            for (int i = spellChecker.SpellCheckerChanged.GetInvocationList().Length -1; i >= 0; i--)
            {
                spellChecker.SpellCheckerChanged.GetInvocationList()[i].DynamicInvoke();
            }
        }
    }
}
