using System;
using System.Linq;

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

        public static bool HasTilesForWord(string w, char given, GraphicTile[] AI_tiles) // This function assumes there is only one tile we're building off of (one given tile)
        {
            foreach (char c in w)
            {
                if (c == given) // This if statement takes into account the fact that we don't need the tile we're playing off of to play a word.
                {
                    if (w.Count(x => x == c) - 1 > AI_tiles.Count(x => x.letter_value == c)) // Ensures AI has the tiles needed to play this word
                        return false; // Word unplayable, not enough tiles
                }
                else
                {
                    if (w.Count(x => x == c) > AI_tiles.Count(x => x.letter_value == c)) // Ensures AI has the tiles needed to play this word
                        return false; // Word unplayable, not enough tiles
                }
            }
            return true;
        }

        public static bool HasTilesForWord(string w, char[] given, GraphicTile[] AI_tiles) // This function takes in an array of all the characters that are given to us
        {
            foreach (char c in w)
            {
                if (given.Contains(c)) // This if statement takes into the account that we don't need the tiles we're playing off of to play a word.
                {
                    if (w.Count(x => x == c) - given.Count(x => x == c) > AI_tiles.Count(x => x.letter_value == c)) // Ensures the AI has the tiles needed to play this word
                        return false; // Word unplayable because even with the given characters, the AI still doesn't have enough tiles to play the word
                }
                else // New tile required for a word play
                {
                    if (w.Count(x => x == c) > AI_tiles.Count(x => x.letter_value == c)) // Ensures AI has the tiles needed to play this word
                        return false;// Word unplayable, not enough tiles
                }
            }
            return true;
        }
    }
}
