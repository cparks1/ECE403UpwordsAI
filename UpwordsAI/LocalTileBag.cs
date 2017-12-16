using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpwordsAI
{
    class LocalTileBag
    {

        Stack<char> tile_bag;

        /// <summary>
        /// Instantiates and initializes a tile bag instance.
        /// </summary>
        public LocalTileBag()
        {
            Initialize(); // Fills up the tile_bag and randomizes the order in which the tiles appear.
        }

        /// <summary>
        /// Fills up the tile_bag stack with all characters included in the Upwords game, with the correct distributions.
        /// </summary>
        private void Initialize()
        {
            tile_bag = new Stack<char>();

            /* ONE OF EACH TILE */
            tile_bag.Push('J');
            tile_bag.Push('Q');
            tile_bag.Push('V');
            tile_bag.Push('X');
            tile_bag.Push('Z');

            /* TWO OF EACH TILE */
            for (int i = 0; i < 2; i++)
            {
                tile_bag.Push('K');
                tile_bag.Push('W');
                tile_bag.Push('Y');
            }

            /* THREE OF EACH TILE */
            for (int i = 0; i < 3; i++)
            {
                tile_bag.Push('B');
                tile_bag.Push('F');
                tile_bag.Push('G');
                tile_bag.Push('H');
                tile_bag.Push('P');
            }

            /* FOUR OF EACH TILE */
            for (int i = 0; i < 4; i++) 
                tile_bag.Push('C');

            /* FIVE OF EACH TILE */
            for (int i = 0; i < 5; i++) 
            {
                tile_bag.Push('D');
                tile_bag.Push('L');
                tile_bag.Push('M');
                tile_bag.Push('N');
                tile_bag.Push('R');
                tile_bag.Push('T');
                tile_bag.Push('U');
            }

            /* SIX OF EACH TILE */
            for (int i = 0; i < 6; i++) 
                tile_bag.Push('S');

            /* SEVEN OF EACH TILE */
            for (int i = 0; i < 7; i++)
            {
                tile_bag.Push('A');
                tile_bag.Push('I');
                tile_bag.Push('O');
            }

            /* EIGHT OF EACH TILE */
            for (int i = 0; i < 8; i++)
                tile_bag.Push('E');

            /* Randomize the tile order */
            Shuffle();
        }

        /// <summary>
        /// Randomizes the order of the tiles in the tile bag.
        /// </summary>
        private void Shuffle()
        {
            tile_bag = new Stack<char>(tile_bag.OrderBy(a => Guid.NewGuid())); // Randomizes the order in which the tiles are stacked
        }

        /// <summary>
        /// Resets the tile bag by emptying it, filling it back up, and randomizing the tile order.
        /// </summary>
        public void Reset()
        {
            Initialize();
        }

        /// <summary>
        /// Takes a tile out of the tile bag, if possible.
        /// </summary>
        /// <param name="tile">The tile retrieved from the tile bag.</param>
        /// <returns>True if there were tiles in the bag, otherwise false.</returns>
        public bool Take(out char tile)
        {
            if(tile_bag.Count > 0)
            {
                tile = tile_bag.Pop();
                return true;
            }
            else
            {
                tile = Tile.BLANK_LETTER;
                return false;
            }
        }
    }
}
