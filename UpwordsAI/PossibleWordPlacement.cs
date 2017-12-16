using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpwordsAI
{
    class PossibleWordPlacement
    {
        public bool     dir; // Direction the word will be placed in. TRUE means the word will be placed VERTICALLY, FALSE means the word will be placed HORIZONTALLY.
        public char  letter; // Letter the word will be building off of.
        public int     lrow; // Row position of the letter we will be building off of.
        public int     lcol; // Column position of the letter we will be building off of.
        public int    start; // Column/row that is the furthest (up/left) possible away from the tile that we can build on.
        public int      end; // Column/row that is the furthest (down/right) possible away from the tile that we can build on.
        public int stacklev; // Stack level of the connecting tile
        public List<string> playablewords = new List<string>();

        /// <summary>
        /// Creates a possible word placement object that stores information needed to make a word play without scoring in mind possible.
        /// </summary>
        /// <param name="direction">Whether the word will be placed vertically or horizontally. TRUE = VERTICAL, FALSE = HORIZONTAL</param>
        /// <param name="let">Letter of the tile we're building off of</param>
        /// <param name="letrow">Row position of the tile we're building off of</param>
        /// <param name="letcol">Column position of the tile we're building off of</param>
        /// <param name="strt">Starting row/column of the word</param>
        /// <param name="en">Ending row/column of the word</param>
        /// 
        public PossibleWordPlacement(bool direction, char let, int letrow, int letcol, int strt, int en)
        {
            dir = direction;
            letter = let;
            lrow = letrow;
            lcol = letcol;
            start = strt;
            end = en;
        }

        /// <summary>
        /// Creates a possible word placement object that stores information needed to make a word play with scoring in mind possible.
        /// </summary>
        /// <param name="direction">Whether the word will be placed vertically or horizontally. TRUE = VERTICAL, FALSE = HORIZONTAL</param>
        /// <param name="let">Letter of the tile we're building off of</param>
        /// <param name="stack">Stack level of the connected tile</param>
        /// <param name="letrow">Row position of the tile we're building off of</param>
        /// <param name="letcol">Column position of the tile we're building off of</param>
        /// <param name="strt">Starting row/column of the word</param>
        /// <param name="en">Ending row/column of the word</param>
        public PossibleWordPlacement(bool direction, char let, int stack, int letrow, int letcol, int strt, int en)
        {
            dir = direction;
            letter = let;
            lrow = letrow;
            lcol = letcol;
            start = strt;
            end = en;
            stacklev = stack;
        }

        public override string ToString()
        {
            return ((dir)?"VER":"HOR")+" : "+letter.ToString() + " (" + lrow.ToString() + "," + lcol.ToString() + ") S:" + start.ToString() + " E:" + end.ToString();
        }

        public int Score()
        {
            string w = this.playablewords[0];   // Grab the best word from what should be a sorted array
            int regscore = 0;                   // Initialize return score to 0
            if (this.stacklev == 1)
            {
                if (w.Length > 7)
                    regscore += (w.Contains('Q')) ? w.Length * 2 + 22 : w.Length * 2 + 20;
                else
                    regscore += (w.Contains('Q')) ? w.Length * 2 + 2 : w.Length * 2;
            }
            else
            {
                regscore += (w.Length > 7) ? w.Length - 1 + this.stacklev + 20 : w.Length - 1 + this.stacklev;
            }
            return regscore;
        }
    }

    class PossibleWordStackPlacement
    {
        public bool dir;
        public int r;
        public int c;
        public List<string> pwords;
        public string oldword;

        /// <summary>
        /// Constructor function that will create a new object of type PossibleStackPlacement that is used to assist the AI with making stack plays.
        /// </summary>
        /// <param name="row">Row of the tile to be stacked.</param>
        /// <param name="column">Column of the tile to be stacked.</param>
        /// <param name="newword">String containing the new word to be placed upon the old word.</param>
        /// <param name="stack">New stack level of the tile to be stacked.</param>
        public PossibleWordStackPlacement(bool direction, int row, int column, string oword, List<string> possiblewords)
        {
            dir = direction;
            r = row; c = column;
            pwords = possiblewords;
            oldword = oword;
        }

        /// <summary>
        /// Calculates and returns the score of this possible stack move.
        /// </summary>
        /// <param name="gameboard">The gameboard's current state.</param>
        /// <returns>The number of points to be gained from playing this move.</returns>
        public int Score(GraphicTile[,] gameboard) //Similar to ScoreCalcReg this calculates the score for a stack play based on the current stack level and the number of changes (increase score by 1 for each change)
        {
            int stackscore = 0;
            if (this.dir)
            {
                for (int rcount = this.r; rcount <= this.oldword.Length + this.r - 1; rcount++)
                {
                    stackscore += gameboard[rcount, this.c].stack_value;
                }
            }
            else
            {
                for (int ccount = this.c; ccount <= this.oldword.Length + this.c - 1; ccount++)
                {
                    stackscore += gameboard[this.r, ccount].stack_value;
                }
            }
            int changes = Utilities.NumberDifferentLetters(this.oldword, this.pwords[0]);
            stackscore += (changes == 7) ? changes + 20 : changes; //Checks if the number of changes equals the number of tiles in hand (7) to determine full usage bonus
            return stackscore;
        }
    }

    class NewPossibleWordPlacement
    {
        public bool dir; // Direction the word will be placed in. TRUE means the word will be placed VERTICALLY, FALSE means the word will be placed HORIZONTALLY.
        public int lrow; // Row position of the letter we will be building off of.
        public int lcol; // Column position of the letter we will be building off of.
        public int start; // Column/row that is the furthest (up/left) possible away from the tile that we can build on.
        public int end; // Column/row that is the furthest (down/right) possible away from the tile that we can build on.
        public List<string> playablewords = new List<string>();


        public string regexmatch = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction">Direction the word will be placed in. TRUE: Vertical, FALSE: Horizontal</param>
        /// <param name="letters">List of letters the word will be built through.</param>
        /// <param name="letrow">Row of the starting letter</param>
        /// <param name="letcol">Column of the starting letter</param>
        /// <param name="starts">List of the starting positions of the letters</param>
        /// <param name="ends">List of the ending positions of the letters</param>
        public NewPossibleWordPlacement(bool direction, List<char> letters, int letrow, int letcol, List<int> starts, List<int> ends)
        {
            List<int> positions = new List<int>();
            positions.AddRange(starts); positions.AddRange(ends); // Merge the positions into one list

            if (positions.Exists(x => x > -1) && letters.Count>0)
                for (int i = 0; i < 10; i++) // Build regex string
                    regexmatch += (positions.Contains(i)) ? letters[positions.IndexOf(i)] : '.';

            regexmatch = regexmatch.Replace('~', '.'); // Replace blank letters with wildcards

            dir = direction; // If TRUE, then all letters will have the same column. If FALSE, then all letters will have the same row.

            lrow = letrow;
            lcol = letcol;
            start = starts[starts.Count - 1];
            end = ends[ends.Count - 1];
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
