﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpwordsAI
{
    class PlacedWord
    {
        public bool dir; // Indicates the direction the word was placed in. TRUE = VERTICAL, FALSE = HORIZONTAL
        public string word; // Indicates the word in string form.
        public int row; // Indicates the row of the starting tile
        public int column; // Indicates the column of the starting tile

        public int Length // Returns the length of a placed word
        { get { return word.Length; } }

        /// <summary>
        /// Creates a new object of type PlacedWord. Used to store data about words placed on the game board.
        /// </summary>
        /// <param name="direction">The direction the word is built in. TRUE for VERTICAL, FALSE for HORIZONTAL.</param>
        /// <param name="placedword">The string form of the placed word.</param>
        /// <param name="startrow">The row of the first letter tile in the word.</param>
        /// <param name="startcolumn">The column of the first letter tile in the word.</param>
        public PlacedWord(bool direction, string placedword, int startrow, int startcolumn)
        {
            dir = direction;
            word = placedword;
            row = startrow;
            column = startcolumn;
        }

        public override string ToString()
        {
            return word + ": " + ((dir) ? "VER" : "HOR") + ": (" + row.ToString() + ", " + column.ToString() + ")"; // Not used for any real purposes other than making it easier to read what the object is when debugging in watch
        }

        public static bool operator !=(PlacedWord left, PlacedWord right)
        {
            return left.column != right.column || left.row != right.row || left.dir != right.dir;
        }
        public static bool operator ==(PlacedWord left, PlacedWord right)
        {
            return left.column == right.column && left.row == right.row && left.dir == right.dir;
        }

    }
}
