using System.Diagnostics;
using System.Globalization;
using Windows.Win32.Globalization;
using Xunit.Abstractions;

namespace spellcheck32.tests;

public class SpellCheckerTests(ITestOutputHelper testOutputHelper) : TestsBase
{
    private readonly string _languageTag = CultureInfo.CurrentCulture.IetfLanguageTag;

    [Fact]
    public void AddWord()
    {
        SpellChecker.Add("readible");
        List<SpellingError> spellingErrors = SpellChecker.Check("readible").ToList();

        Assert.Empty(spellingErrors);
    }

    [Fact]
    public void AddWord_SpellCheckerChanged()
    {
        int changedCount = 0;

        SpellChecker.SpellCheckerChanged += (sender, args) =>
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

        SpellChecker.Add("cowabunga");

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void AutoCorrectWord()
    {
        SpellChecker.AutoCorrect("Linux", "Windows");
        List<SpellingError> spellingErrors = SpellChecker.Check("Linux").ToList();

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
        SpellChecker.SpellCheckerChanged += (sender, args) =>
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

        SpellChecker.AutoCorrect(from: "Linux", to: "Windows");

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void Check()
    {
        SpellChecker.AutoCorrect("Linux", "Windows");

        string text = "Cann I I haev some Linux?";
        testOutputHelper.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));

        foreach (SpellingError error in SpellChecker.Check(text))
        {
            Assert.True(error.Length > 0);
            Assert.True(Enum.IsDefined(error.CorrectiveAction));

            string mistake = text.Substring(error.StartIndex, error.Length);

            Assert.NotNull(mistake);
            Assert.NotEmpty(mistake);

            switch (error.CorrectiveAction)
            {
                case CorrectiveAction.Delete:
                    testOutputHelper.WriteLine(string.Concat("Delete \"", mistake, "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.GetSuggestions:
                    testOutputHelper.WriteLine(string.Concat("Suggest replacing \"", mistake, "\" with:"));

                    foreach (string suggestion in SpellChecker.Suggest(mistake))
                    {
                        testOutputHelper.WriteLine(string.Concat("\"", suggestion, "\""));
                    }

                    testOutputHelper.WriteLine(string.Empty);
                    break;

                case CorrectiveAction.Replace:
                    testOutputHelper.WriteLine(
                        string.Concat("Replace \"", mistake, "\" with \"",
                            SpellChecker.Suggest(mistake).First(), "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.None:
                default:
                    break;
            }
        }
    }

    /// <summary>
    ///  A simple test to compare the performance of the serial and parallel wavefront edit distance algorithms.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  <a href="https://en.wikipedia.org/wiki/Edit_distance"/>
    /// </para>
    /// <para>
    ///  <a href="https://en.wikipedia.org/wiki/Wavefront"/>
    /// </para>
    /// </remarks>
    [Fact]
    public void ComputeEditDistance_SerialVersusParallelWavefront()
    {
        Random rand = new();
        Stopwatch sw = new();

        string s1 = CommonTestHelper.GenerateRandomString(rand);
        string s2 = CommonTestHelper.GenerateRandomString(rand);

        sw.Restart();
        int result = CommonTestHelper.SerialEditDistance(s1, s2);
        sw.Stop();
        testOutputHelper.WriteLine($"Serial  :\t{result}\t{sw.Elapsed}");

        sw.Restart();
        result = CommonTestHelper.ParallelEditDistance(s1, s2);
        sw.Stop();
        testOutputHelper.WriteLine($"Parallel:\t{result}\t{sw.Elapsed}");

        testOutputHelper.WriteLine("-------------------------------------------------------");
    }

    [Fact]
    public void ComprehensiveCheck()
    {
        SpellChecker.AutoCorrect("Linux", "Windows");

        string text = "Cann I I haev some Linux?";
        testOutputHelper.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));

        foreach (SpellingError error in SpellChecker.ComprehensiveCheck(text))
        {
            Assert.True(error.Length > 0);
            Assert.True(Enum.IsDefined(error.CorrectiveAction));

            string mistake = text.Substring(error.StartIndex, error.Length);

            Assert.NotNull(mistake);
            Assert.NotEmpty(mistake);

            switch (error.CorrectiveAction)
            {
                case CorrectiveAction.Delete:
                    testOutputHelper.WriteLine(string.Concat("Delete \"", mistake, "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.GetSuggestions:
                    testOutputHelper.WriteLine(string.Concat("Suggest replacing \"", mistake, "\" with:"));

                    foreach (string suggestion in SpellChecker.Suggest(mistake))
                    {
                        testOutputHelper.WriteLine(string.Concat("\"", suggestion, "\""));
                    }

                    testOutputHelper.WriteLine(string.Empty);
                    break;

                case CorrectiveAction.Replace:
                    testOutputHelper.WriteLine(
                        string.Concat("Replace \"", mistake, "\" with \"",
                            SpellChecker.Suggest(mistake).First(), "\"", Environment.NewLine));
                    break;

                case CorrectiveAction.None:
                default:
                    break;
            }
        }
    }

    [Fact]
    public void Edit_Delete()
    {
        List<string> suggestions = SpellChecker.Suggest("arrainged").ToList();

        Assert.NotEmpty(suggestions);
        Assert.Contains("arranged", suggestions);
    }

    [Fact]
    public void Edit_Insert()
    {
        List<string> suggestions = SpellChecker.Suggest("speling").ToList();

        Assert.NotEmpty(suggestions);
        Assert.Contains("spelling", suggestions);
    }

    [Fact]
    public void Edit_Insert2()
    {
        List<string> suggestions = SpellChecker.Suggest("inconvient").ToList();

        Assert.NotEmpty(suggestions);
        Assert.Contains("inconvenient", suggestions);
    }

    [Fact]
    public void Edit_Replace()
    {
        List<string> suggestions = SpellChecker.Suggest("bycycle").ToList();

        Assert.NotEmpty(suggestions);
        Assert.Contains("bicycle", suggestions);
    }

    [Fact]
    public void Edit_Replace2()
    {
        List<string> suggestions = SpellChecker.Suggest("korrectud").ToList();

        Assert.NotEmpty(suggestions);
        Assert.Contains("corrected", suggestions);
    }

    [Fact]
    public void Edit_Transpose()
    {
        List<string> suggestions = SpellChecker.Suggest("peotry").ToList();

        Assert.NotEmpty(suggestions);
        Assert.Contains("poetry", suggestions);
    }

    [Fact]
    public void Edit_TransposeDelete()
    {
        List<string> suggestions = SpellChecker.Suggest("peotryy").ToList();

        Assert.NotEmpty(suggestions);
        Assert.Contains("poetry", suggestions);
    }

    [Fact]
    public void Id()
    {
        Assert.Equal("MsSpell", SpellChecker.Id);
    }

    [Fact]
    public void IgnoreWord()
    {
        SpellChecker.Ignore("secratary");
        List<SpellingError> spellingErrors = SpellChecker.Check("secratary").ToList();

        Assert.Empty(spellingErrors);
    }

    [Fact]
    public void IgnoreWord_SpellCheckerChanged()
    {
        int changedCount = 0;
        SpellChecker.SpellCheckerChanged += (sender, args) =>
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

        SpellChecker.Ignore("cowabunga");

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void InvalidLanguageTag_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => new SpellChecker("english"));
    }

    [Fact]
    public void IsLanguageSupported()
    {
        foreach (string language in SpellChecker.SupportedLanguages())
        {
            testOutputHelper.WriteLine(language);

            Assert.True(SpellChecker.IsLanguageSupported(language));
        }
    }

    [Fact]
    public void LanguageTag()
    {
        Assert.Equal(_languageTag, SpellChecker.LanguageTag);
    }

    [Fact]
    public void LocalizedName()
    {
        Assert.Equal("Microsoft Windows Spellchecker", SpellChecker.LocalizedName);
    }

    [Fact]
    public void RegisterUnregister_UserDictionary_SpellCheckerChanged()
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
        SpellChecker.SpellCheckerChanged += (sender, args) =>
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

        SpellChecker.RegisterUserDictionary(userDictionary, _languageTag);
        SpellChecker.UnregisterUserDictionary(userDictionary, _languageTag);
        File.Delete(userDictionary);

        Assert.Equal(2, changedCount);
    }

    [Fact]
    public void RemoveWord()
    {
        SpellChecker.Ignore("repitition");
        List<SpellingError> spellingErrors = SpellChecker.Check("repitition").ToList();

        Assert.Empty(spellingErrors);

        SpellChecker.Remove("repitition");
        spellingErrors = SpellChecker.Check("repitition").ToList();

        Assert.Single(spellingErrors);
    }

    [Fact]
    public void SuggestWords()
    {
        List<string> suggestions = SpellChecker.Suggest("publically").ToList();

        Assert.True(suggestions.Count > 0);
        Assert.Contains("publicly", suggestions);
    }

    [Fact]
    public void SupportedLanguages()
    {
        foreach (string language in SpellChecker.SupportedLanguages())
        {
            Assert.NotNull(language);
            Assert.NotEmpty(language);

            testOutputHelper.WriteLine(language);
        }
    }
}