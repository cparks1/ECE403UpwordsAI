namespace UpwordsAI
{
    public class Tile
    {
        public static char BLANK_LETTER = '~';

        /// <summary>
        /// Letter currently held on the tile.
        /// </summary>
        public char letter_value;
        /// <summary>
        /// Current stack level of the tile. Goes from 0 (no tile) to 5 (maximum stack).
        /// </summary>
        public sbyte stack_value;

        /// <summary>
        /// True if the tile's letter value is equal to BLANK_LETTER, false otherwise.
        /// </summary>
        public bool IsBlank => letter_value == BLANK_LETTER;

        /// <summary>
        /// Initialize a default tile object.
        /// </summary>
        public Tile()
        {
            letter_value = '~'; // '~' represents a blank tile.
            stack_value = 0;    // Init stack level to 0 unless otherwise specified by other constructors
        }

        /// <summary>
        /// Initialize a tile object with specified parameters.
        /// </summary>
        /// <param name="letter">Letter currently held on the tile.</param>
        /// <param name="stack">Current stack level of the tile. Goes from 0 (no tile) to 5 (maximum stack).</param>
        public Tile(char letter, sbyte stack)
        {
            letter_value = letter;
            stack_value = stack;
        }
    }
}
