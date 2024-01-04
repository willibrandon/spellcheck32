# spellcheck32

A .NET wrapper around the Microsoft Spell Checking API.

```csharp
using SpellChecker spellChecker = new("en-US");
spellChecker.AutoCorrect("Linux", "Windows");

string text = "Cann I I haev some Linux?";
Console.WriteLine(string.Concat("Check \"", text, "\"", Environment.NewLine));

foreach (SpellingError error in spellChecker.Check(text))
{
    string mistake = text.Substring((int)error.StartIndex, (int)error.Length);

    switch (spellingError.CorrectiveAction)
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
            continue;
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
