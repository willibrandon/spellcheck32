# spellcheck32

A .NET wrapper around the Microsoft Spell Checking API

## Example

```csharp
string text = "Cann I I haev some Linux?";

using SpellChecker spellChecker = new("en-US");
spellChecker.AutoCorrect("Linux", "Windows");

Console.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));

foreach (SpellingError error in spellChecker.Check(text))
{
    string mistake = text.Substring(error.StartIndex, error.Length);

    switch (error.CorrectiveAction)
    {
        case CorrectiveAction.Delete:
            Console.WriteLine(string.Concat("Delete \"", mistake, "\"", Environment.NewLine));
            break;

        case CorrectiveAction.GetSuggestions:
            Console.WriteLine(string.Concat("Suggest replacing \"", mistake, "\" with:"));

            foreach (string suggestion in spellChecker.Suggest(mistake))
            {
                Console.WriteLine(string.Concat("\"", suggestion, "\""));
            }

            Console.WriteLine(string.Empty);
            break;

        case CorrectiveAction.Replace:
            Console.WriteLine(
                string.Concat("Replace \"", mistake, "\" with \"",
                    spellChecker.Suggest(mistake).First(), "\"", Environment.NewLine));
            break;

        case CorrectiveAction.None:
        default:
            break;
    }
}
```

Output:
```
Check "Cann I I haev some Linux?"

Replace "Cann" with "Can"

Delete "I"

Replace "haev" with "have"

Replace "Linux" with "Windows"
```

## API

```csharp
namespace spellcheck32;

public class SpellChecker
{
    /// <summary>
    ///  Occurs when there is a change to the state of the spell checker that could cause the text to be treated differently. A
    ///  client should recheck the text when this event is received.
    /// </summary>
    public event EventHandler<EventArgs>? SpellCheckerChanged;

    /// <summary>
    ///  Gets the identifier of the spell checker.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///  Gets the BCP47 language tag this instance of the spell checker supports.
    /// </summary>
    public string LanguageTag { get; }

    /// <summary>
    ///  Gets text, suitable to display to the user, that describes this spell checker.
    /// </summary>
    public string LocalizedName { get; }

    /// <summary>
    ///  Creates a new instance of the <see cref="SpellChecker"/> class.
    /// </summary>
    /// <param name="languageTag">
    ///  A BCP47 language tag that identifies the language for the requested spell checker.
    /// </param>
    public SpellChecker(string languageTag)

    /// <summary>
    ///  Treats the provided word as though it were part of the original dictionary.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  The word will no longer be considered misspelled, and will also be considered as a candidate for suggestions.
    /// </para>
    /// </remarks>
    public void Add(string word)

    /// <summary>
    ///  Causes the occurrences of one word to be replaced by another.
    /// </summary>
    /// <param name="from">
    ///  The incorrectly spelled word to be autocorrected.
    /// </param>
    /// <param name="to">
    ///  The correctly spelled word that should replace <paramref name="from"/>.
    /// </param>
    public void AutoCorrect(string from, string to)

    /// <summary>
    ///  Checks the spelling of the supplied text and returns a collection of spelling errors.
    /// </summary>
    public IEnumerable<SpellingError> Check(string text)

    /// <summary>
    ///  Checks the spelling of the supplied text in a more thorough manner than <see cref="Check(string)"/>, and returns a
    ///  collection of spelling errors."/>
    /// </summary>
    public IEnumerable<SpellingError> ComprehensiveCheck(string text)

    /// <summary>
    ///  Ignores the provided word for the rest of this session.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  The word will no longer be considered misspelled, but it will not be considered as a candidate for suggestions.
    /// </para>
    /// </remarks>
    public void Ignore(string word)

    /// <summary>
    ///  Determines whether the specified language is supported by a registered spell checker.
    /// </summary>
    /// <param name="languageTag">
    ///  A BCP47 language tag that identifies the language for the requested spell checker.
    /// </param>
    /// <returns>
    ///  <see langword="true"/> if the specified language is supported by a registered spell checker, otherwise
    ///  <see langword="false"/>.
    /// </returns>
    public bool IsLanguageSupported(string languageTag)

    /// <summary>
    ///  Registers a file to be used as a user dictionary for the current user, until unregistered.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  Allows clients to persistently register and unregister user dictionary files that exist in locations other than the
    ///  usual directory path (%AppData%\Microsoft\Spelling). The dictionaries must have the same files formats as the ones
    ///  located in the normal path and also should have the appropriate file extensions. However, it is strongly recommended
    ///  for clients to place their dictionaries under %AppData%\Microsoft\Spelling whenever it is possible--the spell checking
    ///  functionality does not pick up changes in dictionaries outside that directory tree.
    /// </para>
    /// <para>
    ///  The filename must have the extension .dic (added words), .exc (excluded words), or .acl (autocorrect word pairs). The
    ///  files are UTF-16 LE plaintext that must start with the aprropriate Byte Order Mark (BOM). Each line conains a word (in
    ///  the Added and Excluded word lists), or an autocorrect pair with the words separated by a vertical var ("|") (in the
    ///  AutoCorrect word list). The wordlist in which the dictionary is included is inferred through the file extension.
    /// </para>
    /// <para>
    ///  A file registered for a language subtag will be picked up for all languages that contain it. For example, a dictionary
    ///  registered for "en" will also be used by an "en-US" spell checker.
    /// </para>
    /// </remarks>
    public void RegisterUserDictionary(string dictionaryPath, string languageTag)

    /// <summary>
    ///  Removes a word that was previously added by <see cref="Add(string)"/>, or set by <see cref="Ignore(string)"/> to be
    ///  ignored.
    /// </summary>
    public void Remove(string word)

    /// <summary>
    ///  Retrieves spelling suggestions for the supplied text.
    /// </summary>
    public IEnumerable<string> Suggest(string word)

    /// <summary>
    ///  Gets the set of languages/dialects supported by any of the registered spell checkers.
    /// </summary>
    public IEnumerable<string> SupportedLanguages()

    /// <summary>
    ///  Unregisters a previously registered user dictionary. The dictionary will no longer be used by the spell checking
    ///  functionality.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  To unregister a given file, this method must be passed the same arguments that were previously used to register it.
    /// </para>
    /// </remarks>
    public void UnregisterUserDictionary(string dictionaryPath, string languageTag)
}
```