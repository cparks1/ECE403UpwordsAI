using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpwordsAI
{
    public class GraphicTile:Tile
    {
        public static Size tile_size = new Size(32, 32);
        public string tag = "";
        public PictureBox tile_box;

        public GraphicTile()
        {
            tile_box = new PictureBox();    // Initialize the picture box object
            tile_box.Size = tile_size;      // Set the picture box's size
            tile_box.MouseClick += Tile_Click;
        }

        /// <summary>
        /// Constructor function used to create a graphic tile object that will be displayed on the GUI
        /// </summary>
        /// <param name="location">Coordinate location of the tile on the form GUI</param>
        /// <param name="index_tag">Tag used to find and access specific tiles</param>
        /// <param name="is_gameboard_tile">Whether or not the tile is being used for the game board or the AI's tile hand. Determines which functions will be called upon clicking and mousing over the tile.</param>
        public GraphicTile(Point location, string index_tag, bool is_gameboard_tile):this()
        {
            tile_box.Location = location;
            tile_box.Image = new Bitmap(tile_size.Width, tile_size.Height);
            tag = index_tag;

            if (is_gameboard_tile)
                tile_box.MouseEnter += Tile_Mouse_Enter;
        }

        public void DrawTile()
        {
            Bitmap bmp = new Bitmap(tile_size.Width, tile_size.Height); // Create a blank bitmap object to draw onto

            RectangleF rectf = new RectangleF(1, 1, 31, 31); // Used to define the rectangle in which the letter will be drawn
            RectangleF rectuf = new RectangleF(16, 6, 31, 31); // Rectangle containing the "U" after "Q"
            RectangleF rectnum = new RectangleF(22, 18, 9, 13); // Used to define the rectangle in which the stack number will be drawn

            Graphics g = Graphics.FromImage(bmp);       // Create a graphics object we'll be using to draw.
            g.DrawRectangle(Pens.Black, 0, 0, 31, 31);  // Draw a black border around the tile.

            if (this.letter_value != BLANK_LETTER)
            {
                /* Apply some extra options to make the tile letters look more pleasant */
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawString(letter_value.ToString(), new Font("Arial", 12), Brushes.Black, rectf); // Draw the letter

                /* If the letter is a Q, we draw a smaller U next to it. */
                if (letter_value == 'Q')
                    g.DrawString("U", new Font("Arial", 8), Brushes.Black, rectuf);

                /* Draw the stack level */
                if (stack_value > -1) // Stack value of -1 will tell the function to NOT draw the stack number.
                    g.DrawString(stack_value.ToString(), new Font("Arial", 8), Brushes.Red, rectnum);
            }
            tile_box.Image = bmp;

            g.Flush(); // Release resources used by drawing
        }

        /// <summary>
        /// Changes the letter and stack values before drawing the tile.
        /// </summary>
        /// <param name="letter">Letter to be held on the tile.</param>
        /// <param name="stack">Stack level to be held on the tile.</param>
        public void DrawTile(char letter, sbyte stack)
        {
            letter_value = letter;
            stack_value = stack;
            DrawTile();
        }

        /// <summary>
        /// Sets the tile to a blank tile.
        /// </summary>
        public void SetBlank()
        {
            DrawTile(BLANK_LETTER, 0);
        }

        private void Tile_Click(object sender, EventArgs e) // Allows user to manually edit tiles
        {
            MouseEventArgs m = e as MouseEventArgs;
            sbyte new_stack_level = (sbyte)((tag.ToString().Contains(',')) ? 0 : -1); // The tags for the board tiles contain a comma to give you their (r,c) position. The AI tiles simply give you their 1d index.

            if (m.Button == MouseButtons.Left)
            {
                char let = GetNewChar();
                if (char.IsUpper(let))
                    DrawTile(let, new_stack_level);
                else
                    MessageBox.Show("Invalid character.", "Invalid character", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (m.Button == MouseButtons.Right)
                DrawTile(BLANK_LETTER, new_stack_level);
        }

        private void Tile_Mouse_Enter(object sender, EventArgs e)
        {
            ((sender as PictureBox).Parent as Form1).MouseposLBL_text = $"Position: {this.tag}";
        }

        public char GetNewChar()
        {

            using (Form2 testDialog = new Form2())
            {
                testDialog.ShowDialog();
                if (testDialog.goodclose == true)
                    return testDialog.TextBox1.Text[0];
                else
                    return '!'; // Return a character signifying a character was not selected.
            }
        }
    }
}
