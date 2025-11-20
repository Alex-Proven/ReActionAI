using System;

namespace ReActionAI.Modules.RevitChatGPT.Text
{
    /// <summary>
    /// ВРЕМЕННАЯ заглушка переноса слов.
    /// Ничего не делает, просто возвращает исходный текст,
    /// чтобы исключить любые падения из-за NHyphenator.
    /// </summary>
    public static class RussianHyphenator
    {
        public static string Hyphenate(string text)
        {
            return text ?? string.Empty;
        }
    }
}
