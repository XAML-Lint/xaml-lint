using System.Text;

namespace XamlLint.Core.Helpers;

/// <summary>
/// Recognises strings that are not localisable prose: icon-font glyphs in the Unicode
/// Private Use Area (Segoe MDL2 Assets, Segoe Fluent Icons, Material Icons, FontAwesome,
/// and similar — PUA code points are not classified as letters or digits by Unicode
/// category) and UI-chrome punctuation like "+", "-", "...", "→" used as button captions
/// or separators.
/// </summary>
/// <remarks>
/// Returns true when the value is non-empty and contains no letters and no digits among
/// its non-whitespace runes. Single letters ("X") and digits ("1") still return false —
/// "X" could be localisable copy and "1" becomes a different glyph in Arabic/Thai/etc.
/// Mixed values like "+ Add" return false because of the prose segment.
/// </remarks>
public static class SymbolGlyphHelper
{
    public static bool IsSymbolOrGlyph(string value)
    {
        var sawNonWhitespace = false;
        foreach (var rune in value.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune)) continue;
            sawNonWhitespace = true;
            if (Rune.IsLetter(rune) || Rune.IsDigit(rune)) return false;
        }
        return sawNonWhitespace;
    }
}
