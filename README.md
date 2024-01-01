# spellcheck32

A .NET wrapper around the Microsoft Spell Checking API.

```cs
using SpellChecker spellChecker = new("en-US");
spellChecker.AutoCorrect("Linux", "Windows");

string text = "Cann I I haev some Linux?";
Console.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));

foreach (SpellingError spellingError in spellChecker.Check(text))
{
    string misspelledWord = text.Substring((int)spellingError.StartIndex, (int)spellingError.Length);

    switch (spellingError.CorrectiveAction)
    {
        case CorrectiveAction.Delete:
            Console.WriteLine(string.Concat("Delete \"", misspelledWord, "\"", Environment.NewLine));
            break;

        case CorrectiveAction.GetSuggestions:
            Console.WriteLine(string.Concat("Suggest replacing \"", misspelledWord, "\" with:"));

            foreach (string suggestion in spellChecker.Suggest(misspelledWord))
            {
                Console.WriteLine(string.Concat("\"", suggestion, "\""));
            }

            Console.WriteLine(string.Empty);
            break;

        case CorrectiveAction.Replace:
            Console.WriteLine(
                string.Concat("Replace \"", misspelledWord, "\" with \"",
                    spellChecker.Suggest(misspelledWord).First(), "\"", Environment.NewLine));
            break;

        case CorrectiveAction.None:
        default:
            continue;
    }
}
```

Output:
```
Check "Cann I I haev some Linux?"

Suggest replacing "Cann" with:
"Canny"
"Canon"
"Canin"
"Conn"
"CNN"
"Cant"
"Cahn"
"Cano"
"Cain"
"Cen"

Delete "I"

Suggest replacing "haev" with:
"heave"
"hive"
"haven"
"hove"
"haves"

Replace "Linux" with "Windows"
```
