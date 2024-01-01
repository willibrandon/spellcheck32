using System.Collections.Generic;

namespace spellcheck32;

public class SpellCheckOption
{
    /// <summary>
    ///  Gets the description of the spell checker option.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    ///  Gets the heading for the spell checker option.
    /// </summary>
    public string? Heading { get; internal set; }


    /// <summary>
    ///  Gets the identifier for the spell checker option.
    /// </summary>
    public string? Identifier { get; internal set; }

    /// <summary>
    ///  Gets the list of labels for the spell checker option.
    /// </summary>
    public List<string>? Labels { get; internal set; }
}
