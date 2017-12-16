using System;

namespace UpwordsAI
{
    class Utilities
    {
        public Utilities()
        {
        }

        /// <summary>
        /// Returns the number of letters that are different between two words of the same length.
        /// If the words are not of the same length, the function will return an error.
        /// Function assumes both words are a maximum of 10 characters long.
        /// </summary>
        /// <param name="word1">The first word</param>
        /// <param name="word2">The second word</param>
        /// <returns>The number of characters different between the two words; -1 if the two words are not the same length.</returns>
        public static sbyte NumberDifferentLetters(string word1, string word2)
        {
            if (word1.Length != word2.Length)
                return -1;

            sbyte numdiff = 0;
            for (int i = 0; i < word1.Length && i < word2.Length; i++)
                if (word1[i] != word2[i])
                    numdiff++;

            return numdiff;
        }
    }
}
