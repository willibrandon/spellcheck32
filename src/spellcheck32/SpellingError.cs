namespace spellcheck32;

public readonly struct SpellingError(CorrectiveAction correctiveAction, int length, string? replacement, int startIndex)
{
    /// <summary>
    ///  Indicates which corrective action should be taken for the spelling error.
    /// </summary>
    public readonly CorrectiveAction CorrectiveAction { get; } = correctiveAction;

    /// <summary>
    ///  Gets the length of the erroneous text.
    /// </summary>
    public readonly int Length { get; } = length;

    /// <summary>
    ///  Gets the text to use as replacement text when the corrective action is <see cref="CorrectiveAction.Replace"/>.
    /// </summary>
    public readonly string? Replacement { get; } = replacement;

    /// <summary>
    ///  Gets the position in the checked text where the error begins.
    /// </summary>
    public readonly int StartIndex { get; } = startIndex;
}
