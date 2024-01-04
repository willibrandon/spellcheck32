using System.Globalization;
using Windows.Win32.Globalization;
using Xunit.Abstractions;

namespace spellcheck32.tests;

public class SpellCheckerTests(ITestOutputHelper testOutputHelper)
{
    private readonly string _languageTag = CultureInfo.CurrentCulture.IetfLanguageTag;

    [Fact]
    public void AddWord()
    {
        using SpellChecker spellChecker = new(_languageTag);

        spellChecker.Add("readible");
        List<SpellingError> spellingErrors = spellChecker.Check("readible").ToList();

        Assert.True(spellingErrors.Count == 0);
    }

    [Fact]
    public void AddWord_SpellCheckerChanged()
    {
        int changedCount = 0;
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.SpellCheckerChanged += (sender, args) =>
        {
            changedCount++;
            ISpellChecker? checker = sender as ISpellChecker;
            Assert.NotNull(checker);

            testOutputHelper.WriteLine("SpellCheckerChanged " + changedCount);
            testOutputHelper.WriteLine(checker.Id.ToString());
            testOutputHelper.WriteLine(checker.LanguageTag.ToString());
            testOutputHelper.WriteLine(checker.LocalizedName.ToString());

            Assert.Equal("MsSpell", checker.Id.ToString());
            Assert.Equal(_languageTag, checker.LanguageTag.ToString());
            Assert.Equal("Microsoft Windows Spellchecker", checker.LocalizedName.ToString());
        };

        spellChecker.Add("cowabunga");

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void AutoCorrectWord()
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
    public void AutoCorrectWord_SpellCheckerChanged()
    {
        int changedCount = 0;
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.SpellCheckerChanged += (sender, args) =>
        {
            changedCount++;
            ISpellChecker? checker = sender as ISpellChecker;
            Assert.NotNull(checker);

            testOutputHelper.WriteLine("SpellCheckerChanged " + changedCount);
            testOutputHelper.WriteLine(checker.Id.ToString());
            testOutputHelper.WriteLine(checker.LanguageTag.ToString());
            testOutputHelper.WriteLine(checker.LocalizedName.ToString());

            Assert.Equal("MsSpell", checker.Id.ToString());
            Assert.Equal(_languageTag, checker.LanguageTag.ToString());
            Assert.Equal("Microsoft Windows Spellchecker", checker.LocalizedName.ToString());
        };

        spellChecker.AutoCorrect(from: "Linux", to: "Windows");

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void Check()
    {
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.AutoCorrect("Linux", "Windows");

        string text = "Cann I I haev some Linux?";
        testOutputHelper.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));

        foreach (SpellingError error in spellChecker.Check(text))
        {
            Assert.True(error.Length > 0);
            Assert.True(Enum.IsDefined(error.CorrectiveAction));

            string mistake = text.Substring((int)error.StartIndex, (int)error.Length);

            Assert.NotNull(mistake);
            Assert.NotEmpty(mistake);

            switch (error.CorrectiveAction)
            {
                case CorrectiveAction.Delete:
                    testOutputHelper.WriteLine(string.Concat("Delete \"", mistake, "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.GetSuggestions:
                    testOutputHelper.WriteLine(string.Concat("Suggest replacing \"", mistake, "\" with:"));

                    foreach (string suggestion in spellChecker.Suggest(mistake))
                    {
                        testOutputHelper.WriteLine(string.Concat("\"", suggestion, "\""));
                    }

                    testOutputHelper.WriteLine(string.Empty);
                    break;

                case CorrectiveAction.Replace:
                    testOutputHelper.WriteLine(
                        string.Concat("Replace \"", mistake, "\" with \"",
                            spellChecker.Suggest(mistake).First(), "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.None:
                default:
                    continue;
            }
        }
    }

    [Fact]
    public void ComprehensiveCheck()
    {
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.AutoCorrect("Linux", "Windows");

        string text = "Cann I I haev some Linux?";
        testOutputHelper.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));

        foreach (SpellingError error in spellChecker.ComprehensiveCheck(text))
        {
            Assert.True(error.Length > 0);
            Assert.True(Enum.IsDefined(error.CorrectiveAction));

            string mistake = text.Substring((int)error.StartIndex, (int)error.Length);

            Assert.NotNull(mistake);
            Assert.NotEmpty(mistake);

            switch (error.CorrectiveAction)
            {
                case CorrectiveAction.Delete:
                    testOutputHelper.WriteLine(string.Concat("Delete \"", mistake, "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.GetSuggestions:
                    testOutputHelper.WriteLine(string.Concat("Suggest replacing \"", mistake, "\" with:"));

                    foreach (string suggestion in spellChecker.Suggest(mistake))
                    {
                        testOutputHelper.WriteLine(string.Concat("\"", suggestion, "\""));
                    }

                    testOutputHelper.WriteLine(string.Empty);
                    break;

                case CorrectiveAction.Replace:
                    testOutputHelper.WriteLine(
                        string.Concat("Replace \"", mistake, "\" with \"",
                            spellChecker.Suggest(mistake).First(), "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.None:
                default:
                    continue;
            }
        }
    }

    [Fact]
    public void Id()
    {
        using SpellChecker spellChecker = new(_languageTag);

        Assert.Equal("MsSpell", spellChecker.Id);
    }

    [Fact]
    public void IgnoreWord()
    {
        using SpellChecker spellChecker = new(_languageTag);

        spellChecker.Ignore("secratary");
        List<SpellingError> spellingErrors = spellChecker.Check("secratary").ToList();

        Assert.True(spellingErrors.Count == 0);
    }

    [Fact]
    public void IgnoreWord_SpellCheckerChanged()
    {
        int changedCount = 0;
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.SpellCheckerChanged += (sender, args) =>
        {
            changedCount++;
            ISpellChecker? checker = sender as ISpellChecker;
            Assert.NotNull(checker);

            testOutputHelper.WriteLine("SpellCheckerChanged " + changedCount);
            testOutputHelper.WriteLine(checker.Id.ToString());
            testOutputHelper.WriteLine(checker.LanguageTag.ToString());
            testOutputHelper.WriteLine(checker.LocalizedName.ToString());

            Assert.Equal("MsSpell", checker.Id.ToString());
            Assert.Equal(_languageTag, checker.LanguageTag.ToString());
            Assert.Equal("Microsoft Windows Spellchecker", checker.LocalizedName.ToString());
        };

        spellChecker.Ignore("cowabunga");

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void IsLanguageSupported()
    {
        using SpellChecker spellChecker = new(_languageTag);

        foreach (string language in spellChecker.SupportedLanguages())
        {
            testOutputHelper.WriteLine(language);

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
    public void RegisterUserDictionary_SpellCheckerChanged()
    {
        string userDictionary = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "Spelling",
            _languageTag,
            "test.exc");
        File.WriteAllText(userDictionary, string.Empty);
        Assert.True(File.Exists(userDictionary));

        int changedCount = 0;
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.SpellCheckerChanged += (sender, args) =>
        {
            changedCount++;
            ISpellChecker? checker = sender as ISpellChecker;
            Assert.NotNull(checker);

            testOutputHelper.WriteLine("SpellCheckerChanged " + changedCount);
            testOutputHelper.WriteLine(checker.Id.ToString());
            testOutputHelper.WriteLine(checker.LanguageTag.ToString());
            testOutputHelper.WriteLine(checker.LocalizedName.ToString());

            Assert.Equal("MsSpell", checker.Id.ToString());
            Assert.Equal(_languageTag, checker.LanguageTag.ToString());
            Assert.Equal("Microsoft Windows Spellchecker", checker.LocalizedName.ToString());
        };

        spellChecker.RegisterUserDictionary(userDictionary, _languageTag);

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void RemoveWord()
    {
        using SpellChecker spellChecker = new(_languageTag);

        spellChecker.Ignore("repitition");
        List<SpellingError> spellingErrors = spellChecker.Check("repitition").ToList();

        Assert.True(spellingErrors.Count == 0);

        spellChecker.Remove("repitition");
        spellingErrors = spellChecker.Check("repitition").ToList();

        Assert.Single(spellingErrors);
    }

    [Fact]
    public void SuggestWords()
    {
        using SpellChecker spellChecker = new(_languageTag);

        List<string> suggestions = spellChecker.Suggest("publically").ToList();

        Assert.True(suggestions.Count > 0);
        Assert.Contains("publicly", suggestions);
    }

    [Fact]
    public void SupportedLanguages()
    {
        using SpellChecker spellChecker = new(_languageTag);

        foreach (string language in spellChecker.SupportedLanguages())
        {
            Assert.NotNull(language);
            Assert.NotEmpty(language);

            testOutputHelper.WriteLine(language);
        }
    }

    [Fact]
    public void UnregisterUserDictionary_SpellCheckerChanged()
    {
        string userDictionary = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "Spelling",
            _languageTag,
            "test.exc");
        File.WriteAllText(userDictionary, string.Empty);
        Assert.True(File.Exists(userDictionary));

        int changedCount = 0;
        using SpellChecker spellChecker = new(_languageTag);
        spellChecker.SpellCheckerChanged += (sender, args) =>
        {
            changedCount++;
            ISpellChecker? checker = sender as ISpellChecker;
            Assert.NotNull(checker);

            testOutputHelper.WriteLine("SpellCheckerChanged " + changedCount);
            testOutputHelper.WriteLine(checker.Id.ToString());
            testOutputHelper.WriteLine(checker.LanguageTag.ToString());
            testOutputHelper.WriteLine(checker.LocalizedName.ToString());

            Assert.Equal("MsSpell", checker.Id.ToString());
            Assert.Equal(_languageTag, checker.LanguageTag.ToString());
            Assert.Equal("Microsoft Windows Spellchecker", checker.LocalizedName.ToString());
        };

        spellChecker.UnregisterUserDictionary(userDictionary, _languageTag);

        Assert.Equal(1, changedCount);
    }
}