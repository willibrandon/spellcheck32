using System.Globalization;

namespace spellcheck32.tests;

public abstract class TestsBase : IDisposable
{
    protected SpellChecker SpellChecker { get; } = new(CultureInfo.CurrentCulture.IetfLanguageTag);

    protected TestsBase()
    {
        // Do global initialization here; Called before every test method.
        SpellChecker = new(CultureInfo.CurrentCulture.IetfLanguageTag);
        DictionaryHelper dictionaryHelper = new(SpellChecker);
        dictionaryHelper.InstallUSEnglishDictionary();
    }

    public void Dispose()
    {
        // Do global cleanup here; Called after every test method.
        SpellChecker?.Dispose();
        GC.SuppressFinalize(this);
    }
}
