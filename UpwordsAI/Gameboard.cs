using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpwordsAI
{
    public class Gameboard
    {
        /// <summary>
        /// 10x10 array of tiles visible to humans that stores data on character and stack level of each tile.
        /// </summary>
        public GraphicTile[,] board = new GraphicTile[10, 10];

        public Gameboard()
        {

        }

        /// <summary>
        /// Initializes the gameboard and displays it on the given form.
        /// </summary>
        /// <param name="form">Form the gameboard should be displayed on.</param>
        /// <param name="first_x_coordinate">X coordinate of tile (0,0)</param>
        /// <param name="first_y_coordinate">Y coordinate of tile (0,0)</param>
        /// <param name="tile_spacing">Number of pixels between each tile</param>
        public void Initialize(Form1 form, int first_x_coordinate, int first_y_coordinate, int tile_spacing)
        {
            int ypos = first_y_coordinate;
            for (int r = 0; r < 10; r++) // This for loop iterates through each tile on the gameboard and creates the picturebox that will show the tile information
            { // It also draws a blank tile onto each tile, and sets the machine readable tile as a blank tile
                int xpos = first_x_coordinate;
                for (int c = 0; c < 10; c++)
                {
                    System.Drawing.Point location = new System.Drawing.Point(xpos, ypos);
                    board[r, c] = new GraphicTile(location, $"({r.ToString()},{c.ToString()})", true); // (r,c)
                    form.Controls.Add(board[r, c].tile_box); // Add graphic tile's picturebox to form1's visible controls
                    board[r, c].SetBlank();
                    xpos += tile_spacing;
                }
                ypos += tile_spacing;
            }
        }

        /// <summary>
        /// Clears the gameboard.
        /// </summary>
        public void Reset()
        {
            foreach (GraphicTile tile in board)
                tile.SetBlank();
        }

        /// <summary>
        /// Sets board state according to the Tournament Adjudicator's JSON response.
        /// </summary>
        public void SetTournamentState(string[,,] board)
        {
            if (board != null) // If we've received a valid board
            {
                for (int r = 0; r < 10; r++) // Loop through all rows
                {
                    for (int c = 0; c < 10; c++) // Loop through each column for each row
                    {
                        char letter = (board[0, r, c] != null) ? board[0, r, c][0] : Utilities.BLANK_LETTER;  // If the letter is NULL, set it to BLANK. Otherwise set it to the letter it represents.
                        sbyte stack_level = sbyte.Parse(board[1, r, c] ?? "0"); // If the stack level is NULL, set it to 0. Else, parse whatever it is and set it.
                        this.board[r, c].DrawTile(letter, stack_level);  // Set tile letter and stack level, draw the tile.
                    }
                }
            }
        }
    }
}
