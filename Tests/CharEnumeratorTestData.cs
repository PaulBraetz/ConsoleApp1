namespace Tests;
static class CharEnumeratorTestData
{
    public static String[] Strings =>
        [
            "Hello, World!", // Basic ASCII
            "1234567890", // Numbers
            "こんにちは", // Japanese characters
            "Привет мир", // Cyrillic
            "안녕하세요", // Korean
            "你好", // Simplified Chinese
            "مرحبا", // Arabic
            "¡Hola! ¿Cómo estás?", // Spanish with punctuation
            " ", // Single space
            "\t\n\r", // Whitespace characters
            "a\0b\0c\0", // Null bytes interleaved with chars
            "😀😃😄😁😆", // Emojis
            "\uD83D\uDE00", // Single surrogate pair (grinning face emoji)
            "éèêëēėę", // Accented characters
            "𝄞𝄢𝄩", // Musical symbols
            "💻👨‍💻👩‍💻", // Emoji sequences
            "🐍🦀🐘🐿️", // Animal emojis
            "Combining\u0301Mark", // Combining diacritical mark
            "\uFFFF", // Invalid Unicode character
            "ZeroWidth\u200BSpace", // Zero-width space
            "🏳️‍🌈 Flag Sequence", // Flag emoji sequence
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", // Large string of repeated characters
            "𐍈𐍉𐍊𐍋𐍌", // Gothic script
            "a", // Single character
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ", // Uppercase ASCII letters
            "abcdefghijklmnopqrstuvwxyz", // Lowercase ASCII letters
            "0123456789", // Digits
            "!@#$%^&*()-_=+[]{}|;:',.<>?/`~", // Common ASCII symbols
            "\"'\\\\", // Escape sequences
            "MixedCase123!", // Mixed case with numbers and symbols
            "áéíóú", // Accented vowels
            "ñÑçÇ", // Spanish-specific letters
            "üöäëï", // Umlauted characters
            "✓✔✗✘", // Checkmarks and crosses
            "“”‘’", // Quotation marks and apostrophes
            "😀😃😄😁😆😂🤣😅😇", // Common emojis
            "👨‍💻👩‍💻💻", // Emoji sequences
            "🏳️‍🌈", // Rainbow flag sequence
            "🌍🌎🌏", // Earth globes
            "🕵️‍♂️🕵️‍♀️", // Gendered emojis
            "\u200B", // Zero-width space
            "abcdef", // simple case
            "ab", // Two characters
        ];
    public static Object[][] ToCharEnumerableData => Strings.Select(s => new Object[] { s }).ToArray();
}
