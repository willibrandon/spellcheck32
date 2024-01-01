namespace spellcheck32;

public class SpellingError
{
    /// <summary>
    ///  Indicates which corrective action should be taken for the spelling error.
    /// </summary>
    public CorrectiveAction CorrectiveAction { get; internal set; }

    /// <summary>
    ///  Gets the length of the erroneous text.
    /// </summary>
    public long Length { get; internal set; }

    /// <summary>
    ///  Gets the text to use as replacement text when the corrective action is <see cref="CorrectiveAction.Replace"/>.
    /// </summary>
    public string? Replacement { get; internal set; }
    
    /// <summary>
    ///  Gets the position in the checked text where the error begins.
    /// </summary>
    public long StartIndex { get; internal set; }
}
