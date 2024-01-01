using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.System.Com;

namespace spellcheck32;

/// <summary>
///  A .NET wrapper around the Microsoft Spell Checking API.
/// </summary>
/// <remarks>
/// <para>
///  Represents a spell checker for a particular language, with the added ability to remove words from the added words
///  dictionary, or from the ignore list.
/// </para>
/// <para>
///  The <see cref="SpellChecker"/> can also be used to check text, get suggestions, update user directories, and maintain
///  options.
/// </para>
/// </remarks>
public partial class SpellChecker : IDisposable
{
    private bool _disposedValue;
    private readonly uint _eventCookie;
    private readonly ISpellCheckerChangedEventHandler? _handler;
    private readonly string _languageTag;
    private IUserDictionariesRegistrar? _registrar;
    private ISpellChecker2? _spellChecker;
    private ISpellCheckerFactory? _spellCheckFactory;

    /// <summary>
    ///  Occurs when there is a change to the state of the spell checker that could cause the text to be treated differently. A
    ///  client should recheck the text when this event is received.
    /// </summary>
    public event EventHandler<EventArgs>? SpellCheckerChanged;

    /// <summary>
    ///  Gets the identifier of the spell checker.
    /// </summary>
    public string? Id => _spellChecker?.Id.ToString();

    /// <summary>
    ///  Gets the BCP47 language tag this instance of the spell checker supports.
    /// </summary>
    public string? LanguageTag => _spellChecker?.LanguageTag.ToString();

    /// <summary>
    ///  Gets text, suitable to display to the user, that describes this spell checker.
    /// </summary>
    public string? LocalizedName => _spellChecker?.LocalizedName.ToString();

    /// <summary>
    ///  Creates a new instance of the <see cref="SpellChecker"/> class.
    /// </summary>
    /// <param name="languageTag">
    ///  A BCP47 language tag that identifies the language for the requested spell checker.
    /// </param>
    public SpellChecker(string languageTag)
    {
        if (!OsVersion.IsWindows8OrGreater())
        {
            throw new PlatformNotSupportedException("The Microsoft Spell Checking API is only supported on Windows 8 and later.");
        }

        _spellCheckFactory = (ISpellCheckerFactory)new SpellCheckerFactory();

        if (!_spellCheckFactory.IsSupported(languageTag))
        {
            throw new NotSupportedException($"The language tag '{_languageTag}' is not supported.");
        }

        _languageTag = languageTag;
        _spellChecker = (ISpellChecker2)_spellCheckFactory.CreateSpellChecker(_languageTag);
        _handler = new SpellCheckEvents(this);
        _eventCookie = _spellChecker.add_SpellCheckerChanged(_handler);
        _registrar = (IUserDictionariesRegistrar)new SpellCheckerFactory();
    }

    ~SpellChecker() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (_registrar is not null)
            {
                Marshal.ReleaseComObject(_registrar);
                _registrar = null;
            }

            if (_spellChecker is not null)
            {
                if (_eventCookie != 0)
                {
                    _spellChecker.remove_SpellCheckerChanged(_eventCookie);
                }

                Marshal.ReleaseComObject(_spellChecker);
                _spellChecker = null;
            }

            if (_spellCheckFactory is not null)
            {
                Marshal.ReleaseComObject(_spellCheckFactory);
                _spellCheckFactory = null;
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    ///  Treats the provided word as though it were part of the original dictionary.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  The word will no longer be considered misspelled, and will also be considered as a candidate for suggestions.
    /// </para>
    /// </remarks>
    public void Add(string word) => _spellChecker.Add(word);

    /// <summary>
    ///  Causes the occurrences of one word to be replaced by another.
    /// </summary>
    /// <param name="from">
    ///  The incorrectly spelled word to be autocorrected.
    /// </param>
    /// <param name="to">
    ///  The correctly spelled word that should replace <paramref name="from"/>.
    /// </param>
    public void AutoCorrect(string from, string to) => _spellChecker.AutoCorrect(from, to);

    /// <summary>
    ///  Checks the spelling of the supplied text and returns a collection of spelling errors.
    /// </summary>
    public IEnumerable<SpellingError> Check(string text)
    {
        IEnumSpellingError spellingErrors = _spellChecker.Check(text);

        try
        {
            while (EnumerateSpellingErrors(spellingErrors, out ISpellingError spellingError))
            {
                yield return new SpellingError()
                {
                    CorrectiveAction = (CorrectiveAction)spellingError.CorrectiveAction,
                    Length = spellingError.Length,
                    Replacement = spellingError.Replacement.ToString(),
                    StartIndex = spellingError.StartIndex
                };
            }
        }
        finally
        {
            if (spellingErrors is not null)
            {
                Marshal.ReleaseComObject(spellingErrors);
            }
        }
    }

    /// <summary>
    ///  Checks the spelling of the supplied text in a more thorough manner than <see cref="Check(string)"/>, and returns a
    ///  collection of spelling errors."/>
    /// </summary>
    public IEnumerable<SpellingError> ComprehensiveCheck(string text)
    {
        IEnumSpellingError spellingErrors = _spellChecker.ComprehensiveCheck(text);

        try
        {
            while (EnumerateSpellingErrors(spellingErrors, out ISpellingError spellingError))
            {
                yield return new SpellingError()
                {
                    CorrectiveAction = (CorrectiveAction)spellingError.CorrectiveAction,
                    Length = spellingError.Length,
                    Replacement = spellingError.Replacement.ToString(),
                    StartIndex = spellingError.StartIndex
                };
            }
        }
        finally
        {
            if (spellingErrors is not null)
            {
                Marshal.ReleaseComObject(spellingErrors);
            }
        }
    }

    /// <summary>
    ///  Ignores the provided word for the rest of this session.
    /// </summary>
    /// <remarks>
    /// <para>
    ///  The word will no longer be considered misspelled, but it will not be considered as a candidate for suggestions.
    /// </para>
    /// </remarks>
    public void Ignore(string word) => _spellChecker.Ignore(word);

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
    public bool IsLanguageSupported(string languageTag) => _spellCheckFactory.IsSupported(languageTag);

    public IEnumerable<SpellCheckOption> Options()
    {
        IEnumString? optionIds = _spellChecker?.OptionIds;

        try
        {
            while (EnumerateStrings(optionIds, out string optionId))
            {
                IOptionDescription optionDescription = _spellChecker.GetOptionDescription(optionId);
                IEnumString labelStrings = optionDescription.Labels;
                List<string> labels = [];

                while (EnumerateStrings(labelStrings, out string labelString))
                {
                    labels.Add(labelString);
                }

                yield return new SpellCheckOption()
                {
                    Description = optionDescription.Description.ToString(),
                    Heading = optionDescription.Heading.ToString(),
                    Identifier = optionDescription.Id.ToString(),
                    Labels = labels
                };
            }
        }
        finally
        {
            if (optionIds is not null)
            {
                Marshal.ReleaseComObject(optionIds);
            }
        }
    }

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
        => _registrar.RegisterUserDictionary(dictionaryPath, languageTag);

    /// <summary>
    ///  Removes a word that was previously added by <see cref="Add(string)"/>, or set by <see cref="Ignore(string)"/> to be
    ///  ignored.
    /// </summary>
    public void Remove(string word) => _spellChecker.Remove(word);

    /// <summary>
    ///  Retrieves spelling suggestions for the supplied text.
    /// </summary>
    public IEnumerable<string> Suggest(string word)
    {
        IEnumString suggestions = _spellChecker.Suggest(word);

        try
        {
            while (EnumerateStrings(suggestions, out string suggestion))
            {
                yield return suggestion;
            }
        }
        finally
        {
            if (suggestions is not null)
            {
                Marshal.ReleaseComObject(suggestions);
            }
        }
    }

    /// <summary>
    ///  Gets the set of languages/dialects supported by any of the registered spell checkers.
    /// </summary>
    public IEnumerable<string> SupportedLanguages()
    {
        IEnumString? supportedLanguages = _spellCheckFactory?.SupportedLanguages;

        try
        {
            while (EnumerateStrings(supportedLanguages, out string language))
            {
                yield return language;
            }
        }
        finally
        {
            if (supportedLanguages is not null)
            {
                Marshal.ReleaseComObject(supportedLanguages);
            }
        }
    }

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
        => _registrar.UnregisterUserDictionary(dictionaryPath, languageTag);

    /// <summary>
    ///  Retrieves the next spelling error in the enumeration sequence.
    /// </summary>
    private bool EnumerateSpellingErrors(IEnumSpellingError spellingErrors, out ISpellingError spellingError)
        => spellingErrors.Next(out spellingError).Succeeded && spellingError is not null;

    /// <summary>
    ///  Retrieves the next string in the enumeration sequence.
    /// </summary>
    private bool EnumerateStrings(IEnumString? strings, out string next)
    {
        if (strings is null)
        {
            next = string.Empty;
            return false;
        }

        Span<PWSTR> nextSpan = null;
        PWSTR nextString = null;
        HRESULT result;

        unsafe
        {
            fixed (char* pNext = next)
            {
                nextString = new(value: pNext);
                nextSpan = new Span<PWSTR>(pointer: &nextString, length: 1);
                result = strings.Next(nextSpan, null);
            }
        }

        next = nextSpan.Length > 0 ? nextSpan[0].ToString() : string.Empty;
        return result.Succeeded && !string.IsNullOrWhiteSpace(next);
    }
}
