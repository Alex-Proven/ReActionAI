using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ReActionAI.Modules.RevitChatGPT.Text
{
    /// <summary>
    /// Простой русский перенос слов для WPF без внешних библиотек.
    /// Добавляет мягкий перенос (SOFT HYPHEN) внутри длинных слов.
    /// Алгоритм эвристический, но безопасный: при любой ошибке
    /// возвращает исходный текст, чтобы не уронить Revit.
    /// </summary>
    public static class RussianHyphenator
    {
        private const char SoftHyphen = '\u00AD';
        private static readonly string Vowels = "аеёиоуыэюяАЕЁИОУЫЭЮЯ";

        /// <summary>
        /// Добавляет мягкие переносы в русский текст.
        /// </summary>
        public static string Hyphenate(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            try
            {
                // Разбиваем строку с сохранением пробелов.
                var parts = Regex.Split(text, "(\\s+)");

                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];

                    if (!string.IsNullOrEmpty(part) && char.IsLetter(part[0]))
                    {
                        parts[i] = HyphenateWord(part);
                    }
                }

                return string.Concat(parts);
            }
            catch
            {
                // Любая ошибка – отдаём исходный текст.
                return text ?? string.Empty;
            }
        }

        /// <summary>
        /// Добавляет мягкие переносы в одном слове.
        /// Эвристика: ищем гласные и ставим перенос после них,
        /// не ближе чем через 3 буквы от предыдущего разрыва
        /// и оставляя минимум 2 буквы до конца слова.
        /// </summary>
        private static string HyphenateWord(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 6)
                return word;

            var sb = new StringBuilder(word.Length + 4);
            int lastBreak = 0;

            for (int i = 1; i < word.Length - 2; i++)
            {
                char c = word[i];

                if (IsRussianVowel(c) && i - lastBreak >= 3)
                {
                    // Добавляем участок слова до и включая текущую букву,
                    // затем ставим мягкий перенос.
                    sb.Append(word, lastBreak, i - lastBreak + 1);
                    sb.Append(SoftHyphen);
                    lastBreak = i + 1;
                }
            }

            // Добавляем хвост.
            if (lastBreak < word.Length)
                sb.Append(word, lastBreak, word.Length - lastBreak);

            return sb.ToString();
        }

        private static bool IsRussianVowel(char c)
        {
            return Vowels.IndexOf(c) >= 0;
        }
    }
}
