using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpwordsAI
{
    class ComputerPlayer
    {
        const int MAX_NUM_TILES = 7;

        /// <summary>
        /// Array of graphic tiles that represent the AI's tile hand.
        /// </summary>
        public GraphicTile[] tileset = new GraphicTile[MAX_NUM_TILES];

        public ComputerPlayer()
        {

        }

        private void PlaceTile(char c, int[] pos, GraphicTile[,] gameboard, bool update = false) // position array assumes form of { row, column }
        { // CAP : Update function to handle stacking
            if (tileset.Select(x => x.letter_value).Contains(c)) // If the AI has a tile containing the character specified by c
            {
                GraphicTile selected_tile = gameboard[pos[0], pos[1]];
                selected_tile.DrawTile(c, (sbyte)(selected_tile.stack_value + 1));

                if (!update)
                {
                    int letter_index = tileset.ToList().FindIndex(x => x.letter_value == c);
                    tileset[letter_index].DrawTile(Utilities.BLANK_LETTER, -1);  // Remove the tile being placed from the AI's letter hand
                }
            }
        }

        public void PlaceStack(PossibleWordStackPlacement p, GraphicTile[,] gameboard)
        {
            string word = p.pwords[0];
            if (word.Length > 1)
            {
                if (p.dir)
                { // When placing a stack vertically, you're working in one column and moving DOWN the rows.
                    for (int i = 0; i < word.Length; i++)
                        if (gameboard[p.r + i, p.c].letter_value != word[i]) // If the tile we're placing onto isn't the same as the new word being created
                            PlaceTile(word[i], new int[] { p.r + i, p.c }, gameboard);
                }
                else
                { // When placing a stack horizontally, you're working in one row and moving DOWN the columns.
                    for (int i = 0; i < word.Length; i++)
                        if (gameboard[p.r, p.c + i].letter_value != word[i]) // If the tile we're placing onto isn't the same as the new word being created
                            PlaceTile(word[i], new int[] { p.r, p.c + i }, gameboard);
                }
            }
        }

        public void PlaceWord(string word, int[] pos, bool dir, GraphicTile[,] gameboard) // Position array assumes form of { row, column } where position is the position of the first letter of the word.
        { // The boolean "dir" determines the direction of the word placement, where TRUE is 
            if (word != Utilities.BLANK_LETTER.ToString() && word.Length > 1) // Make sure we're not trying to play a blank word or an invalid word
            {
                if (dir) // Vertical
                { // When placing a word vertically, you are working in one column and moving DOWN the rows.
                    for (int i = 0; i < word.Length; i++)
                        if (gameboard[pos[0] + i, pos[1]].IsBlank) // This function is only made to handle placement of words onto blank space. It does not handle stacking and will assume that a tile with a letter on it is not to be stacked upon.
                            PlaceTile(word[i], new int[] { pos[0] + i, pos[1] }, gameboard);
                }
                else // Horizontal
                { // When placing a word horizontally, you are working in one row and moving RIGHT the columns.
                    for (int i = 0; i < word.Length; i++)
                        if (gameboard[pos[0], pos[1] + i].IsBlank) // This function is only made to handle placement of words onto blank space. It does not handle stacking and will assume that a tile with a letter on it is not to be stacked upon.
                            PlaceTile(word[i], new int[] { pos[0], pos[1] + i }, gameboard);
                }
            }
            else
            {
                Debug.WriteLine("[!] AI attempted to place a blank word.");
            }
        }

        public void PlaceWord(PossibleWordPlacement p, GraphicTile[,] gameboard)
        { // Normally we would have to check if p.longest_word != "", but the function calling this function should be checking ahead so it can do its own thing
            int start = (p.dir) ? p.lrow - p.longest_word.IndexOf(p.letter) : p.lcol - p.longest_word.IndexOf(p.letter); // Shifts the word to where it needs to begin
            int[] pos = (p.dir) ? new int[] { start, p.lcol } : new int[] { p.lrow, start };// Letter coords depend upon placement direction
            PlaceWord(p.longest_word, pos, p.dir, gameboard);
        }

        public bool HasTilesForWord(string w, char given) // This function assumes there is only one tile we're building off of (one given tile)
        {
            foreach (char c in w)
            {
                if (c == given) // This if statement takes into account the fact that we don't need the tile we're playing off of to play a word.
                {
                    if (w.Count(x => x == c) - 1 > tileset.Count(x => x.letter_value == c)) // Ensures AI has the tiles needed to play this word
                        return false; // Word unplayable, not enough tiles
                }
                else
                {
                    if (w.Count(x => x == c) > tileset.Count(x => x.letter_value == c)) // Ensures AI has the tiles needed to play this word
                        return false; // Word unplayable, not enough tiles
                }
            }
            return true;
        }

        public bool HasTilesForWord(string w, char[] given) // This function takes in an array of all the characters that are given to us
        {
            foreach (char c in w)
            {
                if (given.Contains(c)) // This if statement takes into the account that we don't need the tiles we're playing off of to play a word.
                {
                    if (w.Count(x => x == c) - given.Count(x => x == c) > tileset.Count(x => x.letter_value == c)) // Ensures the AI has the tiles needed to play this word
                        return false; // Word unplayable because even with the given characters, the AI still doesn't have enough tiles to play the word
                }
                else // New tile required for a word play
                {
                    if (w.Count(x => x == c) > tileset.Count(x => x.letter_value == c)) // Ensures AI has the tiles needed to play this word
                        return false;// Word unplayable, not enough tiles
                }
            }
            return true;
        }

        /// <summary>
        /// Gives the AI tiles from a local tile bag. Not to be used in tournament plays.
        /// </summary>
        /// <returns>True if a tile was available, false otherwise.</returns>
        public bool GrabTiles(LocalTileBag tile_bag)
        {
            foreach (GraphicTile ai_tile in tileset.Where(x => x.IsBlank))  // Loop through all blank spaces in the AI's tile hand
            {
                if (tile_bag.Take(out char new_tile))  // If we were able to grab a tile from the bag
                {
                    ai_tile.DrawTile(new_tile, -1);  // Grab a tile from the bag, give it to the AI. Draw the new tile in the AI's tile hand.
                }
                else
                {
                    Debug.WriteLine("The tile bag is empty.", "No more tiles");
                    return false;
                }
            }
            return true;
        }
    }
}
