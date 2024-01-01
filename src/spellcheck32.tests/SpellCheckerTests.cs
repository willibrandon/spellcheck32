using System.Globalization;
using Windows.Win32.Globalization;
using Xunit.Abstractions;

namespace spellcheck32.tests;

public class SpellCheckerTests(ITestOutputHelper testOutputHelper)
{
    private readonly string _languageTag = CultureInfo.CurrentCulture.IetfLanguageTag;
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void Add()
    {
        using SpellChecker spellChecker = new(_languageTag);

        spellChecker.Add("readible");
        List<SpellingError> spellingErrors = spellChecker.Check("readible").ToList();

        Assert.True(spellingErrors.Count == 0);
    }

    [Fact]
    public void AutoCorrect()
    {
        using SpellChecker spellChecker = new(_languageTag);

        spellChecker.AutoCorrect("Linux", "Windows");
        List<SpellingError> spellingErrors = spellChecker.Check("Linux").ToList();

        Assert.Single(spellingErrors);
        Assert.Equal(CorrectiveAction.Replace, spellingErrors[0].CorrectiveAction);
        Assert.Equal("Linux".Length, spellingErrors[0].Length);
        Assert.Equal("Windows", spellingErrors[0].Replacement);
        Assert.Equal(0, spellingErrors[0].StartIndex);
    }

    [Fact]
    public void Check()
    {
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.AutoCorrect("Linux", "Windows");
        
        string text = "Cann I I haev some Linux?";
        _testOutputHelper.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));
        List<SpellingError> spellingErrors = spellChecker.Check(text).ToList();

        foreach (SpellingError spellingError in spellingErrors)
        {
            string misspelledWord = text.Substring((int)spellingError.StartIndex, (int)spellingError.Length);

            switch (spellingError.CorrectiveAction)
            {
                case CorrectiveAction.Delete:
                    _testOutputHelper.WriteLine(string.Concat("Delete \"", misspelledWord, "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.GetSuggestions:
                    _testOutputHelper.WriteLine(string.Concat("Suggest replacing \"", misspelledWord, "\" with:"));

                    foreach (string suggestion in spellChecker.Suggest(misspelledWord))
                    {
                        _testOutputHelper.WriteLine(string.Concat("\"", suggestion, "\""));
                    }

                    _testOutputHelper.WriteLine(string.Empty);
                    break;

                case CorrectiveAction.Replace:
                    _testOutputHelper.WriteLine(
                        string.Concat("Replace \"", misspelledWord, "\" with \"",
                            spellChecker.Suggest(misspelledWord).First(), "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.None:
                default:
                    continue;
            }
        }

        Assert.True(spellingErrors.Count > 0);
    }

    [Fact]
    public void ComprehensiveCheck()
    {
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.AutoCorrect("Linux", "Windows");

        string text = "Cann I I haev some Linux?";
        _testOutputHelper.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));
        List<SpellingError> spellingErrors = spellChecker.ComprehensiveCheck(text).ToList();

        foreach (SpellingError spellingError in spellingErrors)
        {
            string misspelledWord = text.Substring((int)spellingError.StartIndex, (int)spellingError.Length);

            switch (spellingError.CorrectiveAction)
            {
                case CorrectiveAction.Delete:
                    _testOutputHelper.WriteLine(string.Concat("Delete \"", misspelledWord, "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.GetSuggestions:
                    _testOutputHelper.WriteLine(string.Concat("Suggest replacing \"", misspelledWord, "\" with:"));

                    foreach (string suggestion in spellChecker.Suggest(misspelledWord))
                    {
                        _testOutputHelper.WriteLine(string.Concat("\"", suggestion, "\""));
                    }

                    _testOutputHelper.WriteLine(string.Empty);
                    break;

                case CorrectiveAction.Replace:
                    _testOutputHelper.WriteLine(
                        string.Concat("Replace \"", misspelledWord, "\" with \"",
                            spellChecker.Suggest(misspelledWord).First(), "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.None:
                default:
                    continue;
            }
        }

        Assert.True(spellingErrors.Count > 0);
    }

    [Fact]
    public void Id()
    {
        using SpellChecker spellChecker = new(_languageTag);

        Assert.Equal("MsSpell", spellChecker.Id);
    }

    [Fact]
    public void Ignore()
    {
        using SpellChecker spellChecker = new(_languageTag);

        spellChecker.Ignore("secratary");
        List<SpellingError> spellingErrors = spellChecker.Check("secratary").ToList();

        Assert.True(spellingErrors.Count == 0);
    }

    [Fact]
    public void IsLanguageSupported()
    {
        using SpellChecker spellChecker = new(_languageTag);

        foreach (string language in spellChecker.SupportedLanguages())
        {
            _testOutputHelper.WriteLine(language);

            Assert.True(spellChecker.IsLanguageSupported(language));
        }
    }

    [Fact]
    public void LanguageTag()
    {
        using SpellChecker spellChecker = new(_languageTag);

        Assert.Equal(_languageTag, spellChecker.LanguageTag);
    }

    [Fact]
    public void LocalizedName()
    {
        using SpellChecker spellChecker = new(_languageTag);

        Assert.Equal("Microsoft Windows Spellchecker", spellChecker.LocalizedName);
    }

    [Fact]
    public void Options()
    {
        using SpellChecker spellChecker = new(_languageTag);

        foreach (SpellCheckOption option in spellChecker.Options())
        {
            _testOutputHelper.WriteLine(option.Identifier);
            _testOutputHelper.WriteLine(option.Heading);
            _testOutputHelper.WriteLine(option.Description);

            foreach (string label in option.Labels!)
            {
                _testOutputHelper.WriteLine(label);
            }
        }
    }

    [Fact]
    public void UserDictionary_SpellCheckerChanged()
    {
        string userDictionary = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "Spelling",
            _languageTag,
            "test.exc");

        using (File.Create(userDictionary))
        Assert.True(File.Exists(userDictionary));

        int changedCount = 0;
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.SpellCheckerChanged += (sender, args) =>
        {
            changedCount++;
            ISpellChecker? checker = sender as ISpellChecker;

            _testOutputHelper.WriteLine("SpellCheckerChanged " + changedCount);

            Assert.NotNull(checker);
            Assert.Equal("MsSpell", checker.Id.ToString());
            Assert.Equal(_languageTag, checker.LanguageTag.ToString());
            Assert.Equal("Microsoft Windows Spellchecker", checker.LocalizedName.ToString());
        };

        spellChecker.RegisterUserDictionary(userDictionary, _languageTag);
        spellChecker.UnregisterUserDictionary(userDictionary, _languageTag);
        File.Delete(userDictionary);

        Assert.True(changedCount > 0);
        Assert.False(File.Exists(userDictionary));
    }

    [Fact]
    public void Remove()
    {
        using SpellChecker spellChecker = new(_languageTag);

        spellChecker.Ignore("repitition");
        List <SpellingError> spellingErrors = spellChecker.Check("repitition").ToList();

        Assert.True(spellingErrors.Count == 0);

        spellChecker.Remove("repitition");
        spellingErrors = spellChecker.Check("repitition").ToList();

        Assert.Single(spellingErrors);
    }

    [Fact]
    public void Suggest()
    {
        using SpellChecker spellChecker = new(_languageTag);

        List<string> suggestions = spellChecker.Suggest("publically").ToList();

        Assert.Single(suggestions);
        Assert.Contains("publicly", suggestions);
    }

    [Fact]
    public void SupportedLanguages()
    {
        using SpellChecker spellChecker = new(_languageTag);

        foreach (string language in spellChecker.SupportedLanguages())
        {
            _testOutputHelper.WriteLine(language);
        }
    }
}