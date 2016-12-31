#define simulation // Undefine this if the AI is competing
#define HOME_COMPUTER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D; // For drawing text onto the tiles
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
 
using System.Threading;
using System.Threading.Tasks;

// Upwords Gameboard programmed by Christopher Parks (cparks13@live.com)
// 9/13/16 6:45 PM : Current AI strategy : AI will play the first move if able to, and will attempt to randomize the first move without sacrificing points.
// When given the ability to make consecutive moves, the AI will build the longest words it can off of one letter tile on the board (currently unable to make a word with multiple free tiles)

// AI workings: 9/15/16 3:45 PM : Changed AI to modify the dictionary to remove unplayable words (like "NIQAB") and replace the sequence "QU" with "Q" in words that have it so the AI can understand it.

// TO-DO : 
//         Update move making algorithms to become able to make moves off of more than just 1 tile at a time.
//         Add scoring calculator. 'Qu' moves are 2 bonus points if not stacking, but no bonus if stacking
//                                  Additionally, if you can play all 7 tiles at once you get 20 points (7 letter words can ONLY be played after the first turn, as a connecting move)
//                                  Stacking : 

namespace UpwordsAI
{
    public partial class Form1 : Form
    {
        List<long> longestplays = new List<long>();
        List<long> shortestplays = new List<long>();
        int score = 0; // AI will keep track of its own score for sims

        volatile bool playing = false; // Variable used to determine if the AI is currently playing a game til the end
        volatile bool continuousplay = false; // Variable used to determine if the AI should keep playing games over and over

        int myID = -1; // Variable used to remember the AI's player ID
        volatile int turn = -1; // Variable used to remember the current player turn

        const char BLANK_LETTER = '~';

#if !SCHOOL_COMPUTER
        const string DICTIONARY_PATH = "C:\\Users\\Christopher\\Documents\\dictionary.txt"; // "Z:\\dictionary.txt";
#else
        const string DICTIONARY_PATH = "Z:\\dictionary.txt";
#endif

        Stack<char> Tile_Bag = new Stack<char>(); // Stack of characters that will contain all the tiles that can be handed by the gamemaster (for simulations only)

        string[] dictionary = new string[0]; // String array that will hold the entire dictionary.

        PictureBox[,] tiles = new PictureBox[10, 10]; // Array of 10 rows, 10 columns of tiles readable by human
        char[,] tilesc = new char[10, 10]; // Array of 10 rows, 10 columns of tiles readable by machine
        int[,] stacklev = new int[10, 10]; // Array of 10 rows, 10 columns of the stack level of the tiles on the gameboard

        PictureBox[] AI_tiles = new PictureBox[7]; // Array of 7 tiles to be held by the AI
        char[] AI_Letters = new char[7]; // List that will contain the letters used by the AI. Upwords, when played, gives each player 7 tiles each turn.


        bool firstturn = true; // Boolean that determines if the AI will follow special rules for placing the first word.


        Random rand = new Random(); // Random number generator that will be used to generate random characters into the AI's characters and make some decisions that don't require logic. This won't be used in real games

        GameNetworkCommuncation g; // Object used for communications with the tournament adjudicator program

        public Form1()
        {
            InitializeComponent();
            LoadDictionary(); // Loads up the dictionary given in DICTIONARY_PATH into a string array named dictionary
            InitializeBoard(); // Draws the board and sets all tiles to blanks
            InitializeAITiles(); // Draws the AI's held tiles and sets them all to blanks
            ResetTileBag(); // Initializes the bag of tiles
        }

        public void InitializeBoard()
        {
            int ypos = 13;
            for (int r = 0; r < 10; r++) // This for loop iterates through each tile on the gameboard and creates the picturebox that will show the tile information
            { // It also draws a blank tile onto each tile, and sets the machine readable tile as a blank tile
                int xpos = 13;
                for (int c = 0; c < 10; c++)
                {
                    tiles[r, c] = new PictureBox();
                    tiles[r, c].Location = new Point(xpos, ypos);
                    tiles[r, c].Size = new Size(32, 32);
                    tiles[r, c].Image = new Bitmap(32, 32);
                    tiles[r, c].Tag = "(" + r.ToString() + "," + c.ToString() + ")";
                    tiles[r, c].MouseEnter += tilePB_MouseEnter;
                    tiles[r, c].MouseClick += Tile_Click;
                    Controls.Add(tiles[r, c]);
                    DrawText(ref tiles[r, c], BLANK_LETTER, 0);
                    tilesc[r, c] = BLANK_LETTER;
                    xpos += 33;

                    stacklev[r, c] = 0;
                }
                ypos += 33;
            }
        }

        public void InitializeAITiles()
        {
            int xpos = tiles[9, 1].Left + 16, ypos = tiles[9, 1].Bottom + 32;
            for (int i = 0; i < AI_tiles.Length; i++)
            {
                AI_tiles[i] = new PictureBox();
                AI_tiles[i].Location = new Point(xpos, ypos);
                AI_tiles[i].Size = new Size(32, 32);
                AI_tiles[i].Image = new Bitmap(32, 32);
                AI_tiles[i].Tag = i.ToString();
                AI_tiles[i].MouseClick += Tile_Click;
                Controls.Add(AI_tiles[i]);
                DrawText(ref AI_tiles[i], BLANK_LETTER, 0);
                AI_Letters[i] = BLANK_LETTER;
                xpos += 33;
            }

        }

        public void ResetTileBag() // Resets the tile bag, puts all the tiles into it, and then shuffles.
        {
            Tile_Bag = new Stack<char>();
            Tile_Bag.Push('J'); // 1 of each
            Tile_Bag.Push('Q');
            Tile_Bag.Push('V');
            Tile_Bag.Push('X');
            Tile_Bag.Push('Z');
            for (int i = 0; i < 2; i++) // 2 of each
            {
                Tile_Bag.Push('K');
                Tile_Bag.Push('W');
                Tile_Bag.Push('Y');
            }
            for (int i = 0; i < 3; i++) // 3 of each
            {
                Tile_Bag.Push('B');
                Tile_Bag.Push('F');
                Tile_Bag.Push('G');
                Tile_Bag.Push('H');
                Tile_Bag.Push('P');
            }
            for (int i = 0; i < 4; i++) // 4 of each
                Tile_Bag.Push('C');
            for (int i = 0; i < 5; i++) // 5 of each
            {
                Tile_Bag.Push('D');
                Tile_Bag.Push('L');
                Tile_Bag.Push('M');
                Tile_Bag.Push('N');
                Tile_Bag.Push('R');
                Tile_Bag.Push('T');
                Tile_Bag.Push('U');
            }
            for (int i = 0; i < 6; i++) // 6 of each
                Tile_Bag.Push('S');
            for (int i = 0; i < 7; i++) // 7 of each
            {
                Tile_Bag.Push('A');
                Tile_Bag.Push('I');
                Tile_Bag.Push('O');
            }
            for (int i = 0; i < 8; i++)
                Tile_Bag.Push('E');

            Tile_Bag = new Stack<char>(Tile_Bag.OrderBy(a => Guid.NewGuid())); // Randomizes the order in which the tiles are stacked
        }

        public void LoadDictionary()
        {
            try
            { dictionary = System.IO.File.ReadAllLines(DICTIONARY_PATH); FixDictionary(); }
            catch (Exception exc)
            { MessageBox.Show(exc.ToString(), "Unable to load dictionary!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void FixDictionary() // The AI program takes in a normal dictionary containing words that it cannot yet read. It also may contain unplayable words, such as "NIQAB"
        { // This function removes all words that cannot be played (words that contain a 'Q' that doesn't come before a 'U') and changes 'Qu' sequences to just 'Q' so the AI can use the word
            List<string> tempd = new List<string>(); // Temporary dictionary
            foreach (string word in dictionary)
            {
                int index = word.IndexOf('Q');
                if (index > -1) // There is actually a Q in the word
                {
                    if (index < word.Length - 1) // If 'Q' is not the last letter in the word
                    {
                        if (word[index + 1] == 'U') // If the next letter is 'U'
                            tempd.Add(word.Replace("QU", "Q")); // Add the word to the dictionary as a playable word, but change the "QU" to just "Q", so the AI can play the word without extra routines
                    }
                }
                else
                    tempd.Add(word);
            }
            dictionary = tempd.ToArray(); // Replace the loaded dictionary with the new fixed dictionary.
        }

        public void DrawText(ref PictureBox pb, char letter, int stack)
        {
            Bitmap bmp = new Bitmap(32, 32); // Tile size is 32x32 px

            RectangleF rectf = new RectangleF(1, 1, 31, 31); // Used to define the rectangle in which the letter will be drawn
            RectangleF rectuf = new RectangleF(16, 6, 31, 31); // Rectangle containing the "U" after "Q"
            RectangleF rectnum = new RectangleF(22, 18, 9, 13); // Used to define the rectangle in which the stack number will be drawn

            Graphics g = Graphics.FromImage(bmp);

            g.DrawRectangle(Pens.Black, 0, 0, 31, 31);
            if (letter != BLANK_LETTER)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawString(letter.ToString(), new Font("Arial", 12), Brushes.Black, rectf);

                if (letter == 'Q')
                    g.DrawString("U", new Font("Arial", 8), Brushes.Black, rectuf);

                if (stack > -1) // Stack value of -1 will tell the function to NOT draw the stack number.
                    g.DrawString(stack.ToString(), new Font("Arial", 8), Brushes.Red, rectnum);

                g.Flush(); // Release resources used by drawing

            }
            pb.Image = bmp;
        }

        public void SetTile(int[] pos, char letter, int stack) // Draws the tile, and sets it. Pos follows the expected form of { row, column }.
        {
            tilesc[pos[0], pos[1]] = letter;
            stacklev[pos[0], pos[1]] = stack;
            DrawText(ref tiles[pos[0], pos[1]], letter, stack);
        }

        private void Form1_Load(object sender, EventArgs e)
        { }

        private void FindNextMove3() // This function finds possible moves the AI can make after the first turn and plays them, where a move is considered to be building an entirely new word off of one or multiple words.
        {
            List<string> regexes=new List<string>();
            string regex = "";
            for(int r=0; r<10; r++) // Cycle through all rows
            {
                regex = "";

                for (int c = 0; c < 10; c++)
                    regex += (tilesc[r, c] == BLANK_LETTER) ? '.' : tilesc[r, c]; // Generate a regex capable of search

                bool hitlet = false; bool fronttrail = false;

                for (int i = 0; i < 10 && !hitlet; i++)
                    if(!hitlet && Char.IsLetter(regex[i]))//&&!fixedbegin)
                    {
                        hitlet = true;
                        if (i != 0)
                            regex = regex.Remove(0, i).Insert(0, string.Format("^.{{0,{0}}}", i));
                        fronttrail = true;
                    }

                hitlet = false;

                for (int i = regex.Length-1; i > 0 && !hitlet; i--)
                    if (!hitlet && Char.IsLetter(regex[i]))
                    {
                        hitlet = true;
                        if(i!=regex.Length-1)
                        regex = regex.Remove(i+1) + ".{0," + i.ToString() + "}$";
                    }

                if (regex.Count(x => Char.IsLetter(x)) > 0)
                    regexes.Add(regex);
            }

            for(int c=0; c<10; c++) // Cycle through all columns
            {
                regex = "";
                for (int r = 0; r < 10; r++)
                    regex += (tilesc[r, c] == BLANK_LETTER) ? '.' : tilesc[r, c]; // Generate a regex capable of search
                bool hitlet = false; bool fronttrail = false;
                for (int i = 0; i < 10 && !hitlet; i++)
                    if (!hitlet && Char.IsLetter(regex[i])) // If we haven't found a letter yet (indicating we're still searching a trail) and we just found the end of the trail
                    { 
                        hitlet = true;
                        if (i != 0)// If we're not at the beginning of the word (first letter being a letter means no front trail)
                        {
                            regex = regex.Remove(0, i).Insert(0, string.Format("^.{{0,{0}}}", i));
                            fronttrail = true;
                        }
                        else
                            regex = regex.Insert(0, "^");
                    }
                hitlet = false;
                for (int i = regex.Length-1; i > ((fronttrail)?6:0) && !hitlet; i--)
                    if (!hitlet && Char.IsLetter(regex[i]))// != BLANK_LETTER)
                    {
                        hitlet = true;
                        if (i != regex.Length - 1)
                            regex = regex.Remove(i + 1) + ".{0," + (i - ((fronttrail) ? 6 : 0)).ToString() + "}$";
                    }
                if (regex.Count(x => Char.IsLetter(x)) > 0)
                    regexes.Add(regex);
            }
        }

        private bool SearchUpward(int[] pos, out List<int> rstarts, out List<char> letters, out int rstart) // Searches above a given tile (to get the starting position and letters in the way)
        {
            rstart = -1; int r = pos[0], c = pos[1];
            rstarts = new List<int>(); // List that holds all the rstarting positions
            letters = new List<char>(); // List that holds all the letters we've run into

            if (tilesc[r, c] != BLANK_LETTER) // We're only interested in tiles with a letter on them, because we can build words off of tiles with letters
            {
                if (r - 1 >= 0) // If we can check ABOVE the tile we're currently searching (We do this check to avoid an out of bounds exception)
                {
                    if (tilesc[r - 1, c] == BLANK_LETTER) // This is part of the process of checking if we're able to build an entirely new vertical word.
                    {
                        rstart = r; // Set the initial starting row
                        for (int i = r - 1; i >= 0; i--) // Search all the rows behind
                        {
                            letters.Add(tilesc[i, c]); // Add the letter we may be building off of
                            if (tilesc[i, c] == BLANK_LETTER) // Need a blank tile to place a word.
                            {
                                if (c + 1 <= 9) // If we can search to the right
                                    if (tilesc[i, c + 1] != BLANK_LETTER) // Verify we're not building next to other tiles CAP: Modify this so the function will check if it's a valid word
                                        break; // If we are break before rstart is set
                                if (c - 1 >= 0) // If we can search to the left
                                    if (tilesc[i, c - 1] != BLANK_LETTER) // Verify we're not building next to other tiles
                                        break; // If we are break before rstart is set

                                rstart = i; // If the for loop is not yet broken then we set the row starting position higher
                            }
                            else // We've hit a tile that is not blank.
                            { // Add an rstart that is rstart = (rstart == -1) ? -1 : rstart + 1; and continue searching
                                //rstart = (rstart == -1) ? -1 : rstart + 1;
                                rstarts.Add((rstart == -1) ? -1 : rstart + 1); // Add an rstart that is 1 tile lower and continue searching (or set it as an invalid rstart if -1)
                            }
                        }
                    }
                }
                else rstart = r; // If we can't search above the tile then that means a word can start here
            }

            rstarts.Add(rstart);
            rstart = (rstarts.Count > 0) ? rstarts[0] : rstart;

            return rstarts.Exists(x => x > -1) || rstart != -1; // If all the possible row starting positions are -1 then we cannot build a word at all CAP : This isn't necessarily true. Modify this to look into possibility of adding onto words
        }

        private bool SearchDownward(int[] pos, out List<int> rends, out List<char> letters, out int rend) // Searches below a given tile (to get the ending position and letters in the way)
        {
            int r = pos[0], c = pos[1];
            rends = new List<int>(); letters = new List<char>(); rend = -1;

            if (r + 1 <= 9) // If we can check BELOW the tile we're currently searching
            {
                if (tilesc[r + 1, c] == BLANK_LETTER) // This is part of the process of checking if we're able to build an entirely new vertical word.
                {
                    rend = r; // Set the initial ending row
                    for (int i = r + 1; i <= 9; i++) // Search all the rows ahead
                    {
                        letters.Add(tilesc[i, c]); // Add the letter we may be building off of
                        if (tilesc[i, c] == BLANK_LETTER) // Need a blank tile to place a word.
                        {
                            if (c + 1 <= 9) // If we can search to the right
                                if (tilesc[i, c + 1] != BLANK_LETTER) // Verify we're not building next to other tiles CAP: Modify this function to allow us to build next to other tiles if possible
                                    break; // If we are break before rstart is set
                            if (c - 1 >= 0) // If we can search to the left
                                if (tilesc[i, c - 1] != BLANK_LETTER) // Verify we're not building next to other tiles
                                    break; // If we are break before rstart is set

                            rend = i;
                        }
                        else
                        {
                            rends.Add((rend == -1) ? -1 : rend - 1); // Add an rend that is 1 tile higher and continue searching (or set it as an invalid rend if -1)
                        }
                    }
                }
            }
            else rend = r; // If we can't search below the tile then that means a word can end here

            rends.Add(rend);
            rend = (rends.Count > 0) ? rends[0] : rend;

            return rends.Exists(x => x > -1) || rend != -1; // If all the possible row starting positions are -1 then we cannot build a word at all CAP : This isn't necessarily true. Modify this to look into possibility of adding onto words
        }

        private bool SearchLeftward(int[] pos, out List<int> cstarts, out List<char> letters, out int cstart)
        {
            int r = pos[0], c = pos[1]; cstart = -1;
            cstarts = new List<int>(); letters = new List<char>();

            if (c - 1 >= 0) // If we can check to the LEFT of the tile we're currently searching
            {
                if (tilesc[r, c - 1] == BLANK_LETTER) // If the tile to the left of it is a blank letter
                {
                    cstart = c; // Set the initial starting column
                    for (int i = c - 1; i >= 0; i--) // Search all the columns behind
                    {
                        letters.Add(tilesc[r, i]); // Add the letter we may be building off of
                        if (tilesc[r, i] == BLANK_LETTER) // Need a blank tile to place a word.
                        {
                            if (r + 1 <= 9) // If we can search to the right
                                if (tilesc[r + 1, i] != BLANK_LETTER) // Verify we're not building next to other tiles
                                    break; // If we are break before rstart is set
                            if (r - 1 >= 0) // If we can search to the left
                                if (tilesc[r - 1, i] != BLANK_LETTER) // Verify we're not building next to other tiles
                                    break; // If we are break before rstart is set

                            cstart = i; // And set the starting column
                            letters.Add(BLANK_LETTER); // Make the AI remember there was a blank tile here for later
                        }
                        else
                        {
                            cstarts.Add((cstart == -1) ? -1 : cstart + 1); // Add a cstart that is 1 tile to the right and continue searching (or set it as an invalid rstart if -1)
                        }
                    }
                }
            }
            else cstart = c; // If we can't search to the left of the tile then that means we can start here

            cstarts.Add(cstart);
            cstart = (cstarts.Count > 0) ? cstarts[0] : cstart;

            return cstarts.Exists(x => x > -1) || cstart != -1; // If all the possible row starting positions are -1 then we cannot build a word at all CAP : This isn't necessarily true. Modify this to look into possibility of adding onto words
        }

        private bool SearchRightward(int[] pos, out List<int> cends, out List<char> letters, out int cend)
        {
            int r = pos[0], c = pos[1]; cend = -1;
            cends = new List<int>(); letters = new List<char>();

            if (c + 1 <= 9) // If we can check to the RIGHT of the tile we're currently searching
            {
                if (tilesc[r, c + 1] == BLANK_LETTER) // If the tile to the right of it is a blank letter
                {
                    cend = c;
                    for (int i = c + 1; i <= 9; i++) // Search all the columns ahead
                    {
                        letters.Add(tilesc[r, i]);
                        if (tilesc[r, i] == BLANK_LETTER) // Need a blank tile to place a word.
                        {
                            if (r + 1 <= 9) // If we can search to the right
                                if (tilesc[r + 1, i] != BLANK_LETTER) // Verify we're not building next to other tiles
                                    break; // If we are break before rstart is set
                            if (r - 1 >= 0) // If we can search to the left
                                if (tilesc[r - 1, i] != BLANK_LETTER) // Verify we're not building next to other tiles
                                    break; // If we are break before rstart is set

                            cend = i; // And then set the ending column
                        }
                        else
                        {
                            cends.Add((cend == -1) ? -1 : cend - 1); // Add a cend that is 1 tile to the left and continue searching (or set it as an invalid cend if -1)
                        }// If we've hit a blank tile then we want to break this for loop and fix the cend value
                    }
                }
            }
            else cend = c; // If we can't search to the right of a tile then that means we can end here

            cends.Add(cend);
            cend = (cends.Count > 0) ? cends[0] : cend;

            return cends.Exists(x => x > -1) || cend != -1; // If all the possible row starting positions are -1 then we cannot build a word at all CAP : This isn't necessarily true. Modify this to look into possibility of adding onto words
        }

        private string FindNextMove2() // This function finds the next possible move (After the first) move that the AI can make, where a move is considered to be building an entirely new word off of an old one
        {
            List<PossibleWordPlacement> PossibleWordPlacements = new List<PossibleWordPlacement>(); // List of "PossibleWordPlacement" objects that holds information on the words we can place and where we can place them.
            List<NewPossibleWordPlacement> NewPossibleWordPlacements = new List<NewPossibleWordPlacement>(); // List of "NewPossibleWordPlacement" objects that holds information on the words we can place and where we can place them. Enables 2+ tile placements
            for (int r = 0; r < 10; r++) // Search through all of the rows
                for (int c = 0; c < 10; c++) // Search through all of the columns
                {
                    int rstart = -1, rend = -1, cstart = -1, cend = -1; // Used to keep track of the starting/ending position of a word
                    int[] pos = { r, c };

                    if (tilesc[r, c] != BLANK_LETTER) // We're only interested in tiles with a letter on them, because we can build words off of tiles with letters
                    {
                        List<int> rows; List<char> rschars;
                        List<int> rowe; List<char> rechars;
                        List<int> cols; List<char> cschars;
                        List<int> cole; List<char> cechars;

                        SearchUpward(pos, out rows, out rschars, out rstart); // Search in all four directions of the tile and record what we see
                        SearchDownward(pos, out rowe, out rechars, out rend); // as well as record valid data needed for word placement
                        SearchLeftward(pos, out cols, out cschars, out cstart);
                        SearchRightward(pos, out cole, out cechars, out cend);

                        if (rstart != -1 && rend != -1 && rstart != rend) // If we can make a vertical word
                        {
                            PossibleWordPlacements.Add(new PossibleWordPlacement(true, tilesc[r, c], r, c, rstart, rend)); // Add the word to the list of possible word plays

                            List<char> lets = new List<char>(); lets.AddRange(rschars); lets.AddRange(rechars);
                            NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(true, lets, r, c, rows, rowe));
                        }
                        if (cstart != -1 && cend != -1 && cstart != cend) // If we can make a horizontal word
                        {
                            PossibleWordPlacements.Add(new PossibleWordPlacement(false, tilesc[r, c], r, c, cstart, cend)); // Add the word to the list of possible word plays

                            List<char> lets = new List<char>(); lets.AddRange(cschars); lets.AddRange(cechars);
                            NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(false, lets, r, c, cols, cole));
                        }
                    }
                }

            foreach (PossibleWordPlacement p in PossibleWordPlacements)
            {
                int lpos = (p.dir) ? p.lrow : p.lcol;
                int maxlen = p.end - p.start + 1;
                List<string> words = new List<string>();
                //words.AddRange(dictionary.Where(x => x.Length <= maxlen && x.Contains(p.letter.ToString())).Where(x => x.IndexOf(p.letter) <= lpos && x.IndexOf(p.letter)+1 >= (lpos+1) - (maxlen - x.Length))); // CAP : Attempted to fix this issue by adding 1 to lpos, to make it a 1 based (starts at 1 instead of 0) number like the rest of the evals against it
                //words.AddRange(dictionary.Where(x => x.Length <= maxlen && x.Contains(p.letter.ToString())).Where(x => x.IndexOf(p.letter) <= lpos && lpos-x.IndexOf(p.letter) + x.Length-1 <= p.end)); // nah this one doesnt work
                words.AddRange(dictionary.Where(x => x.Length <= maxlen && x.Contains(p.letter.ToString())).Where(x => x.IndexOf(p.letter) <= lpos && x.Length - 1 - x.IndexOf(p.letter) <= p.end - lpos && x.IndexOf(p.letter) <= lpos - p.start)); // CAP : Evaluated lambdas, figured this one would work correctly
                words = words.OrderByDescending(x => x.Length).ToList(); // Simple explanation: These two lines save all possible playable words to a list of strings and sorts them so the longest ones come first. It is later evaluated to see which words CAN be played, based on tiles the AI has.
                /* BREAKDOWN OF THE WIZARDRY:
                * x.Length <= maxlen GETS ALL WORDS THAT'RE AS LONG AS THE MAX LENGTH OR LESS
                * x.IndexOf(p.letter) ENSURES THESE WORDS HAVE THE TILE WE'RE BUILDING OFF OF
                * x.IndexOf(p.letter) <= lpos ENSURES WE'RE NOT TRYING TO SHIFT THE TILE WE'RE BUILDING OFF OF AROUND. YOU MAY NEED TO DO THAT ONE OUT ON PAPER TO BELIEVE ME, I THOUGHT IT WAS BULLSHIT TOO. I HAD TO CHECK THIS PART LIKE 3 TIMES, LIKE WTF MAN. IT CHECKS OUT THO. ACTUALLY THIS SHIT IS WORTHLESS I THINK
                * x.Length - 1 - x.IndexOf(p.letter) <= p.end - lpos : Gets the number of tiles after the letter (Lefthand side) and then (p.end - lpos) gets the number of tiles that CAN come after the letter. The <= ensures that the number of tiles after the letter are as many as can be, and not more.
                * x.IndexOf(p.letter) <= lpos - p.start : Does the same thing as above, but with the tiles before the letter rather than after.
                * It then sorts these words with the largest ones coming first, as we always want to make the largest play we can for maximum points. This may change if we implement war gaming. */

                foreach (string w in words)
                    if (HasTilesForWord(w, p.letter)) // If the AI has tiles to play the word
                        p.playablewords.Add(w); // Save the word as playable word
            }

            PossibleWordPlacements = PossibleWordPlacements.Where(x => x.playablewords.Count > 0).ToList(); // Trim any possible placements with 0 playable words
            PossibleWordPlacements = PossibleWordPlacements.OrderByDescending(x => x.playablewords[0].Length).ToList(); // Order the list so that the possible placements with the longest words are first

            if (PossibleWordPlacements.Count > 0) // We cannot place any words if there are no possible placements
            {
                PossibleWordPlacement p = PossibleWordPlacements[0];
                if (p.playablewords.Count > 0) // We cannot place any words if there are no playable words (PossibleWordPlacements trims out the possible placements that had 0 playable words first)
                {
                    AI_PlaceWord(p);
                    return p.playablewords[0];
                }
                else
                    return BLANK_LETTER.ToString();
            }
            else
                return BLANK_LETTER.ToString();
        }

        public bool HasTilesForWord(string w, char given) // This function assumes there is only one tile we're building off of (one given tile)
        {
            foreach (char c in w)
            {
                if (c == given) // This if statement takes into account the fact that we don't need the tile we're playing off of to play a word.
                {
                    if (w.Count(x => x == c) - 1 > AI_Letters.Count(x => x == c)) // Ensures AI has the tiles needed to play this word
                        return false;// Word unplayable, not enough tiles
                }
                else
                {
                    if (w.Count(x => x == c) > AI_Letters.Count(x => x == c)) // Ensures AI has the tiles needed to play this word
                        return false;// Word unplayable, not enough tiles
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
                    if (w.Count(x => x == c) - given.Count(x => x == c) > AI_Letters.Count(x => x == c)) // Ensures the AI has the tiles needed to play this word
                        return false; // Word unplayable because even with the given characters, the AI still doesn't have enough tiles to play the word
                }
                else // New tile required for a word play
                {
                    if (w.Count(x => x == c) > AI_Letters.Count(x => x == c)) // Ensures AI has the tiles needed to play this word
                        return false;// Word unplayable, not enough tiles
                }
            }
            return true;
        }

        private bool SearchForSpace(string word, out int[] pos, bool dir) // Searches for space for words to be played without making ANY connections.
        { // CAP : This function needs to be updated to include searching for surrounding tiles and determining whether they are blank.
            // When dir is TRUE, we search for vertical. When dir is FALSE, we search for horizontal.
            int r, c; // Used as loop iteration variables
            pos = new int[2];
            int blanktiles = 0;
            if (dir) // Search for vertical first
            {
                for (c = 0; c < 10; c++) // Iterate through the columns
                {
                    blanktiles = 0; // Reset the number of blank tiles when searching in a new column
                    for (r = 0; r < 10; r++) // Iterate through the rows
                    {
                        if (tilesc[r, c] == BLANK_LETTER) // If there is a blank space for the AI to place a word
                            blanktiles++; //
                        else
                            blanktiles = 0;
                        if (blanktiles >= word.Length) // There is space vertically for the word to be placed
                        {
                            pos = new int[] { r - word.Length + 1, c }; // Gives the position the word should begin at CAP : Not sure if that row position math is correct
                            return true; // Indicate that free space for the word to be placed was successfully found
                        }
                    }
                }
                return false;
            }
            else // Search for horizontal second
            {
                for (r = 0; r < 10; r++)
                {
                    blanktiles = 0;
                    for (c = 0; c < 10; c++)
                        if (tilesc[r, c] == BLANK_LETTER)
                            blanktiles++;
                    if (blanktiles >= word.Length)
                    {
                        pos = new int[] { r, c - word.Length };
                        return true;
                    }
                }
                return false;
            }
        }

        private string FindLongestPlayableWord2(string[] dict) // Finds longest playable word in the dictionary by looking at the AI's tileset.
        { // CAP : This function is faster than a human, but terrible in terms of speed when matched against computers.
            List<string> playable_words = new List<string>();
            foreach (string word in dict) // Cycle through each word in the dictionary
            {
                if (HasTilesForWord(word, BLANK_LETTER))
                    playable_words.Add(word);
            }
            if (playable_words.Count > 0)
            {
                playable_words = playable_words.OrderByDescending(x => x.Length).ToList();
                return playable_words[0];
            }
            else
                return BLANK_LETTER.ToString(); // This tells the programmer/AI there was no playable word, given the tileset.

        }

        private void AI_PlaceTile(char c, int[] pos, bool update=false) // position array assumes form of { row, column }
        { // CAP : Update function to handle stacking
            if (AI_Letters.Contains(c))
            {
                SetTile(pos, c, stacklev[pos[0], pos[1]] + 1); // CAP : Eventually update this to retrieve the current stack # and increment it
                if (!update)
                {
                    int li = Array.IndexOf(AI_Letters, c); // Letter index
                    DrawText(ref AI_tiles[li], BLANK_LETTER, -1);
                    AI_Letters[li] = BLANK_LETTER;
                }
            }
        }

        private void AI_PlaceStack(PossibleWordStackPlacement p)
        {
            string word = p.pwords[0];
            if (word.Length > 1)
            {
                if (p.dir)
                { // When placing a stack vertically, you're working in one column and moving DOWN the rows.
                    for (int i = 0; i < word.Length; i++)
                        if (tilesc[p.r + i, p.c] != word[i]) // If the tile we're placing onto isn't the same as the new word being created
                            AI_PlaceTile(word[i], new int[] { p.r + i, p.c });
                }
                else
                { // When placing a stack horizontally, you're working in one row and moving DOWN the columns.
                    for (int i = 0; i < word.Length; i++)
                        if (tilesc[p.r, p.c + i] != word[i]) // If the tile we're placing onto isn't the same as the new word being created
                            AI_PlaceTile(word[i], new int[] { p.r, p.c + i });
                }
            }
        }

        private void AI_PlaceWord(string word, int[] pos, bool dir) // Position array assumes form of { row, column } where position is the position of the first letter of the word.
        { // The boolean "dir" determines the direction of the word placement, where TRUE is 
            if (word != BLANK_LETTER.ToString() && word.Length > 1) // Make sure we're not trying to play a blank word or an invalid word
            {
                score += (word.Contains('Q')) ? word.Length * 2 + 2 : word.Length * 2;
                if (scoreLBL.InvokeRequired)
                    scoreLBL.Invoke(new MethodInvoker(delegate { scoreLBL.Text = "Score: " + score.ToString(); }));
                else
                    scoreLBL.Text = "Score: " + score.ToString();

                if (dir) // Vertical
                { // When placing a word vertically, you are working in one column and moving DOWN the rows.
                    for (int i = 0; i < word.Length; i++)
                        if (tilesc[pos[0] + i, pos[1]] == BLANK_LETTER) // This function is only made to handle placement of words onto blank space. It does not handle stacking and will assume that a tile with a letter on it is not to be stacked upon.
                            AI_PlaceTile(word[i], new int[] { pos[0] + i, pos[1] });
                }
                else // Horizontal
                { // When placing a word horizontally, you are working in one row and moving RIGHT the columns.
                    for (int i = 0; i < word.Length; i++)
                        if (tilesc[pos[0], pos[1] + i] == BLANK_LETTER) // This function is only made to handle placement of words onto blank space. It does not handle stacking and will assume that a tile with a letter on it is not to be stacked upon.
                            AI_PlaceTile(word[i], new int[] { pos[0], pos[1] + i });
                }
            }
            else
            {
                if (logboxTB.InvokeRequired)
                    logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += "AI attempted to place a blank word.\r\n"; }));
                else
                    logboxTB.Text += "AI attempted to place a blank word.\r\n";
            }
        }

        private void AI_PlaceWord(PossibleWordPlacement p)
        { // Normally we would have to check if p.playablewords.count > 0, but the function calling this function should be checking ahead so it can do its own thing
            int strt = (p.dir) ? p.lrow - p.playablewords[0].IndexOf(p.letter) : p.lcol - p.playablewords[0].IndexOf(p.letter); // Shifts the word to where it needs to begin
            int[] pos = (p.dir) ? new int[] { strt, p.lcol } : new int[] { p.lrow, strt };// Letter coords depend upon placement direction
            AI_PlaceWord(p.playablewords[0], pos, p.dir);
        }

        private void AI_PlaceFirstWord(string word)
        // This function is called in the event the AI is able to place the first word, once the largest playable first word has been selected.
        // The boolean "dir" determines the direction of the word placement, where TRUE is Vertical, and FALSE is Horizontal. The position of the word designates the position of the first tile of the word.
        { // The position of the word will be determined randomly (when possible), as well as the word placement direction, to prevent other AIs from being able to guess how we will place it.
            if (word.Length > 5) // If the best possible move is 6 letters long (longest possible word you can play on the first move)
            { // Then we need the position and direction selection to be a bit specific, based on certain parameters.
                int r = (rand.Next(int.MaxValue) % 2 == 0) ? 4 : 5; // We generate the position randomly first, with possible positions being (4,4), (4,5), and (5,4). (5,5) is not included because it cannot place a 6 letter word.
                int c = (r == 4) ? ((rand.Next(int.MaxValue) % 2 == 0) ? 4 : 5) : 4; // If we're in the 4th row, we can choose the 4th or 5th column and still be able to play a 6 letter word while if we're in the 5th row we MUST be in the 4th column

                bool dir = (r == 4) ? ((c == 4) ? (rand.Next(int.MaxValue) % 2 == 0) : true) : false; // If pos = (4,4), dir can be random else if (4,5) must be vertical else if (5,4) must be horizontal. (5,5) means error

                if (dir)
                    for (int i = 0; i < word.Length; i++)
                        AI_PlaceTile(word[i], new int[] { r + i, c });
                else
                    for (int i = 0; i < word.Length; i++)
                        AI_PlaceTile(word[i], new int[] { r, c + i });

                score += (word.Contains('Q')) ? word.Length * 2 + 2 : word.Length * 2;
                if (scoreLBL.InvokeRequired)
                    scoreLBL.Invoke(new MethodInvoker(delegate { scoreLBL.Text = "Score: " + score.ToString(); }));
                else
                    scoreLBL.Text = "Score: " + score.ToString();
            }
            else if (word.Length > 2)
            { // If the word is 5 letters or shorter, and at least 3 letters, then we don't need to worry about constraints.
                bool dir = (rand.Next(int.MaxValue) % 2 == 0) ? true : false; // Choose word placement direction randomly
                int[] pos = new int[] { ((rand.Next(int.MaxValue) % 2 == 0) ? 4 : 5), ((rand.Next(int.MaxValue) % 2 == 0) ? 4 : 5) }; // Choose word tile start randomly

                if (dir) // Vertical
                    for (int i = 0; i < word.Length; i++)
                        AI_PlaceTile(word[i], new int[] { pos[0] + i, pos[1] });
                else // Horizontal
                    for (int i = 0; i < word.Length; i++)
                        AI_PlaceTile(word[i], new int[] { pos[0], pos[1] + i });

                score += (word.Contains('Q')) ? word.Length * 2 + 2 : word.Length * 2;
                if (scoreLBL.InvokeRequired)
                    scoreLBL.Invoke(new MethodInvoker(delegate { scoreLBL.Text = "Score: " + score.ToString(); }));
                else
                    scoreLBL.Text = "Score: " + score.ToString();
            }
            else
                MessageBox.Show("The AI could not make a first turn!");
        }

        private void giveaitilesBUT_Click(object sender, EventArgs e)
        {
            GiveAITiles();
        }
        private void GiveAITiles()
        {
            for (int i = 0; i < AI_Letters.Length; i++) // iterate through the AI's given letters
                if (AI_Letters[i] == BLANK_LETTER) // Only interested in blank tiles
                {
                    //char newc = (char)rand.Next('A', 'Z');
                    if (Tile_Bag.Count > 0)
                    {
                        char newc = Tile_Bag.Pop();
                        AI_Letters[i] = newc;// Give the AI a random character tile between A and Z. (Tile Bag is shuffled when it is reset, so tiles are random)
                        DrawText(ref AI_tiles[i], newc, -1);
                    }
                    else
                        MessageBox.Show("The tile bag is empty.", "No more tiles");
                }
        }

        private void aiplacewordBUT_Click(object sender, EventArgs e)
        { // When the AI places tiles, the program must then set them to "blank character".
            int[] pos = new int[2];
            string word = "";

            if (firstturn) // If the AI is taking the first turn
            {
                word = FindLongestPlayableWord2(dictionary.Where(x => x.Length >= 3 && x.Length <= 6).ToArray());
                AI_PlaceFirstWord(word);
                firstturn = false;
            }
            else
                word = FindNextMove2();
            if (word != BLANK_LETTER.ToString())
                MessageBox.Show("Attempted word play was: " + word.Replace("Q", "QU") + ".");
            else
                MessageBox.Show("The AI was unable to play a word.");
        }

        private void clearaitilesBUT_Click(object sender, EventArgs e)
        { ClearAITiles(); }
        private void ClearAITiles()
        {
            for (int i = 0; i < AI_Letters.Length; i++)
            {
                AI_Letters[i] = BLANK_LETTER;
                DrawText(ref AI_tiles[i], BLANK_LETTER, -1);
            }
        }

        private void clearboardBUT_Click(object sender, EventArgs e)
        { ClearGameboard(); }
        private void ClearGameboard()
        {
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    SetTile(new int[] { r, c }, BLANK_LETTER, 0);
            firstturn = true;
        }

        private void tilePB_MouseEnter(object sender, EventArgs e)
        {
            mouseposLBL.Text = "Position: " + ((sender as PictureBox).Tag as string);
        }


        private void Tile_Click(object sender, EventArgs e) // Allows user to manually edit tiles
        {
            PictureBox tile = sender as PictureBox;
            MouseEventArgs m = e as MouseEventArgs;
            bool IsBoardTile = (tile.Tag.ToString().Contains(',')) ? true : false; // The tags for the board tiles contain a comma to give you their (r,c) position. The AI tiles simply give you their 1d index.
            if (IsBoardTile) // Separate code for AI letter hand and board tiles due to difference in array positioning (1D vs 2D)
            {
                string[] spos = tile.Tag.ToString().Replace("(", "").Replace(")", "").Split(','); // Format the string into a 2d array with the separate coordinates
                int[] pos = new int[] { int.Parse(spos[0]), int.Parse(spos[1]) }; // Parse the coordinates

                if (m.Button == MouseButtons.Left) // Set the tile to something else
                {
                    char let = GetNewChar();
                    if (let >= 'A' && let <= 'Z')
                        SetTile(pos, let, 0);
                }
                else if (m.Button == MouseButtons.Right) // Clear the tile
                    SetTile(pos, BLANK_LETTER, 0);
            }
            else
            {
                int pos = int.Parse(tile.Tag.ToString());
                if (m.Button == MouseButtons.Left) // Set the tile to something else
                {
                    char let = GetNewChar();
                    if (let >= 'A' && let <= 'Z')
                    {
                        AI_Letters[pos] = let;
                        DrawText(ref AI_tiles[pos], let, -1);
                    }
                }
                else if (m.Button == MouseButtons.Right) // Clear the tile
                {
                    AI_Letters[pos] = BLANK_LETTER; // Set the character value to blank
                    DrawText(ref AI_tiles[pos], BLANK_LETTER, -1); // Draw it blank
                }
            }
        }

        public char GetNewChar()
        {

            using (Form2 testDialog = new Form2())
            {
                testDialog.ShowDialog();
                if (testDialog.goodclose == true)
                    return testDialog.TextBox1.Text[0];
                else
                    return '!'; // It returns this to signify an error occurred. If it sent '~' the tile would be set to blank, which may not be desired.
            }
        }

        List<PlacedWord> FindWordsOnBoard() // This function finds all the words placed on the board and compiles them into a list. Will later be used to figure out where we can stack tiles
        { // CAP : This function was written mostly while I was drinking so there may be some unforseen issues
            // CAP : Seriously though I have no idea how this function works, I barely remember.
            List<PlacedWord> WordsOnBoard = new List<PlacedWord>();

            bool[, ,] searched = new bool[2, 10, 10]; // Initialize an 3D array that is 2 units deep with 10 rows and 10 columns that will tell the routine if a tile has been searched. 0,0,0, is if vertically tile 0,0 has been searched, 1,0,0 is if horizontally tile 0,0 has been searched
            for (int i = 0; i < 2; i++) // Set all values to 'false', meaning the tiles haven't yet been searched.
                for (int j = 0; j < 10; j++)
                    for (int k = 0; k < 10; k++)
                        searched[i, j, k] = false;

            for (int z = 0; z < 2; z++)
                for (int r = 0; r < 10; r++)
                    for (int c = 0; c < 10; c++)
                    {
                        bool ca = (r - 1 >= 0), cb = (r + 1 <= 9), cl = (c - 1 >= 0), cr = (c + 1 <= 9); // Used to determine if we can check above, below, left, or right of the tile
                        int rstart = -1, rend = -1, cstart = -1, cend = -1; // Used to keep track of the starting/ending position of a word
                        string rstring = "", cstring = ""; // Strings that will be built as we search vertically/horizontally
                        if (tilesc[r, c] != BLANK_LETTER && (!searched[0, r, c] || !searched[1, r, c])) // If the tile is not a blank letter and hasn't yet been searched then we will search it
                        { // The tile is not blank and hasn't been searched yet (either vertically or horizontally)
                            if (ca && !searched[0, r, c]) // If we're on a tile where we can search above it (row != 0)
                            { // If we can search the tiles above this and this tile hasn't yet been searched vertically
                                if (tilesc[r - 1, c] != BLANK_LETTER && !searched[0, r - 1, c]) // If the tile above it is not a blank letter
                                {
                                    for (int i = r - 1; i >= 0; i--) // Go up in rows until you hit the end of the word
                                    {
                                        if (tilesc[i, c] != BLANK_LETTER) // A letter tile has been placed here
                                        {
                                            searched[0, i, c] = true; // We will want to mark it as searched
                                            rstart = i; // And then set the starting row
                                            rstring = rstring.Insert(0, tilesc[i, c].ToString()); // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }
                                }
                            }

                            if (cb && !searched[0, r, c]) // If we're on a tile where we can search below it (row != 9)
                            {
                                if (tilesc[r + 1, c] != BLANK_LETTER && !searched[0, r + 1, c]) // If the tile below it is not a blank letter and hasn't been searched
                                {
                                    rstart = r;
                                    for (int i = r + 1; i <= 9; i++)
                                    {
                                        if (tilesc[i, c] != BLANK_LETTER) // A letter tile has been placed here
                                        {
                                            searched[0, i, c] = true; // We will want to mark it as searched
                                            rend = i; // And then set the ending row
                                            rstring += tilesc[i, c]; // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }
                                }
                            }

                            searched[0, r, c] = true; rstring = rstring.Insert(0, tilesc[r, c].ToString());

                            if (cl && !searched[1, r, c]) // If we're on a tile where we can search to the left of it (column != 0)
                            {
                                if (tilesc[r, c - 1] != BLANK_LETTER && !searched[1, r, c - 1]) // If the tile to the left of it is not a blank letter
                                {
                                    cstart = c;
                                    for (int i = c - 1; i >= 0; i++)
                                    {
                                        if (tilesc[r, i] != BLANK_LETTER) // A letter tile has been placed here
                                        {
                                            searched[1, r, i] = true; // We will want to mark it as searched
                                            cstart = i; // And then set the starting column
                                            cstring = cstring.Insert(0, tilesc[r, i].ToString()); // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }

                                }
                            }

                            if (cr && !searched[1, r, c]) // If we're on a tile where we can search to the right of it (column != 9)
                            {
                                if (tilesc[r, c + 1] != BLANK_LETTER && !searched[1, r, c + 1]) // If the tile to the right of it is not a blank letter
                                {
                                    cstart = c;
                                    for (int i = c + 1; i <= 9; i++)
                                    {
                                        if (tilesc[r, i] != BLANK_LETTER) // A letter tile has been placed here
                                        {
                                            searched[1, r, i] = true; // We will want to mark it as searched
                                            cend = i; // And then set the ending column
                                            cstring += tilesc[r, i]; // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }
                                }
                            }

                            searched[1, r, c] = true; cstring = cstring.Insert(0, tilesc[r, c].ToString());

                            if (rstart != -1 || rend != -1) // If we found a vertical word
                            {
                                WordsOnBoard.Add(new PlacedWord(true, rstring, rstart, c)); // Add the word to the list of words placed on the game board
                            }
                            if (cstart != -1 || cend != -1) // If we found a horizontal word
                            {
                                WordsOnBoard.Add(new PlacedWord(false, cstring, r, cstart)); // Add the word to the list of words placed on the game board.
                            }
                        }
                        else
                        {
                            searched[0, r, c] = true;
                            searched[1, r, c] = true;
                        }
                    }
            return WordsOnBoard;
        }

        List<PlacedWord> WordsAttachedToTile(int[] pos) // Array pos assumes form of { row, column }.
        {
            int r = pos[0], c = pos[1];
            return FindWordsOnBoard().Where(x => (x.dir) ? ((x.column == c) && (x.row + x.Length - 1 >= r) && (x.row <= r)) : ((x.row == r) && (x.column + x.Length - 1 >= c) && (x.column <= c))).ToList(); // Gets a list of the words attached to the current searched tile
        }
        List<PlacedWord> WordsAttachedToTile(List<PlacedWord> words, int[] pos)
        {
            int r = pos[0], c = pos[1];
            return words.Where(x => (x.dir) ? ((x.column == c) && (x.row + x.Length - 1 >= r) && (x.row <= r)) : ((x.row == r) && (x.column + x.Length - 1 >= c) && (x.column <= c))).ToList(); // Gets a list of the words attached to the current searched tile
        }
        List<PlacedWord> WordsAttachedToWord(PlacedWord word, List<PlacedWord> WordsOnBoard)
        {
            List<PlacedWord> WordsAttached = new List<PlacedWord>();
            if (word.dir)
            {
                bool sl = word.column - 1 >= 0, sr = word.column + 1 < 10; // Whether we can search left/right of the tile
                int c = word.column;
                for (int r = word.row; r < word.row + word.Length; r++) // Search through all tiles attached to word
                {
                    bool found = false; // If we found a word attached to the tile we don't want to check twice by checking left AND right

                    if (sl) // If we can search left (if we can't, there is no word attached to the left)
                    {
                        if (tilesc[r, c - 1] != BLANK_LETTER) // Doing this one comparison saves us time by checking if there is a word attached before trying to get it
                        {
                            WordsAttached.AddRange(WordsAttachedToTile(WordsOnBoard, new int[] { r, c }).Where(x => x != word));
                            found = true;
                        }
                    }
                    if (sr && !found) // If we can search right (if we can't, there is no word attached to the right) and haven't yet found a word
                    {
                        if (tilesc[r, c + 1] != BLANK_LETTER) // Doing this one comparison saves us time
                        {
                            WordsAttached.AddRange(WordsAttachedToTile(WordsOnBoard, new int[] { r, c }).Where(x => x != word));
                        }
                    }
                }
            }
            else
            {
                bool sa = word.row - 1 >= 0, sb = word.row + 1 < 10; // Whether we can search above/below of the tile
                int r = word.row;
                for (int c = word.column; c < word.column + word.Length; c++) // Search through all tiles attached to word
                {
                    bool found = false; // If we found a word attached to the tile we don't want to check twice by checking above AND below

                    if (sa)
                    {
                        if (tilesc[r - 1, c] != BLANK_LETTER)
                        {
                            WordsAttached.AddRange(WordsAttachedToTile(WordsOnBoard, new int[] { r, c }).Where(x => x != word));
                            found = true;
                        }
                    }
                    if (sb && !found)
                    {
                        if (tilesc[r + 1, c] != BLANK_LETTER)
                        {
                            WordsAttached.AddRange(WordsAttachedToTile(WordsOnBoard, new int[] { r, c }).Where(x => x != word));
                        }
                    }
                }
            }

            return WordsAttached;
        }

        private void AIPlayStack() // Basic function that allows the AI to make basic stacking moves
        {
            List<PlacedWord> PlacedWords = FindWordsOnBoard();
            List<PossibleWordStackPlacement> PossibleStackPlays = new List<PossibleWordStackPlacement>();
            foreach (PlacedWord p in PlacedWords)
            {
                List<PlacedWord> AttachedWords = WordsAttachedToWord(p, PlacedWords); // Get a list of all the words attached to this current word
                List<string> words = dictionary.Where(x => ContainsEnoughLetters(x, p.word)).ToList(); // All possible stack words that can be stacked on this placed word without taking attached placed words into consideration
                foreach (PlacedWord a in AttachedWords) // Cycle through all words attached to the current word so we may take them into consideration
                {
                    if (p.dir) // If p is vertical,
                    {
                        int atc = p.column, atr = a.row; // Then a is horizontal and the words are attached at p's column and a's row
                        int wdex = atr - p.row; // The index of the letter in p's string is the row of attachment minus the starting row (since we're vertical)
                        int awdex = atc - a.column; // Index of the letter in a's string is the column of attachment minus the starting column
                        words.RemoveAll(x => !NewStackWordExists(a.word, wdex, awdex, x)); // Remove words where the tile would be changed and make a non-existent word
                    }
                    else // If p is horizontal,
                    {
                        int atc = a.column, atr = p.row; // Then a is vertical and the words are attached at p's row and a's column
                        int wdex = atc - p.column;// The index of the letter in p's string is the column of attachment minus the starting column (since we're horizontal)
                        int awdex = atr - a.row;
                        words.RemoveAll(x => !NewStackWordExists(a.word, wdex, awdex, x)); // Remove words where the tile would be changed and make a non-existent word
                    }

                }

                if (p.dir) // This if statement will prevent any words that would cause a stack level higher than 5 from being played.
                {
                    for (int r = p.row; r < p.row + p.Length; r++)
                        if (stacklev[r, p.column] >= 5)
                            words.RemoveAll(x => x[r - p.row] != p.word[r - p.row]);
                }
                else
                {
                    for (int c = p.column; c < p.column + p.Length; c++)
                        if (stacklev[p.row, c] >= 5)
                            words.RemoveAll(x => x[c - p.column] != p.word[c - p.column]);
                }

                if (words.Count > 0)
                    PossibleStackPlays.Add(new PossibleWordStackPlacement(p.dir, p.row, p.column, p.word, words));
            }
            PossibleStackPlays = PossibleStackPlays.OrderByDescending(x => NumberDifferentLetters(x.oldword, x.pwords[0])).ToList(); // Orders the list so the longest stack play is at the front
            if (PossibleStackPlays.Count > 0)
            {
                AI_PlaceStack(PossibleStackPlays[0]);
                if (logboxTB.InvokeRequired)
                    logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += "STACK PLAY: " + PossibleStackPlays[0].pwords[0] + "\r\n"; }));
                else
                    logboxTB.Text += "STACK PLAY: " + PossibleStackPlays[0].oldword.Replace("Q", "QU") + " TO " + PossibleStackPlays[0].pwords[0].Replace("Q", "QU") + "\r\n";
            }
        }

        public bool NewStackWordExists(string aword, int wdex, int awdex, string checkword) // Used by AI stacking function
        {
            StringBuilder sb = new StringBuilder(aword);
            sb[awdex] = checkword[wdex];
            aword = sb.ToString();
            return dictionary.Contains(aword);
        }

        public int NumberDifferentLetters(string word1, string word2)
        {
            if (word1.Length != word2.Length)
                return -1;

            int numdiff = 0;
            for (int i = 0; i < word1.Length && i < word2.Length; i++)
                if (word1[i] != word2[i])
                    numdiff++;

            return numdiff;
        }

        /// <summary>
        /// This function determines if you can play a new word on top of an old word.
        /// </summary>
        /// <param name="word">New word we are attempting to build via a stack play</param>
        /// <param name="oldword">Old word we are stacking on top of</param>
        /// <returns></returns>
        public bool ContainsEnoughLetters(string word, string oldword) // Function allows us to tell if given n wildcards, whether or not we can play this word
        { // If using this function to determine if a new word can be played in a stack play, make sure n is less than or equal to the previous word length minus 1. You cannot cover all tiles in one play.
            int discrepencies = 0;
            if (word == oldword)
                return false;
            if (word.Length == oldword.Length)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (word[i] != oldword[i])
                    {
                        if (AI_Letters.Count(x => x == word[i]) < word.Count(x => x == word[i])) // If the AI has less tiles of this kind than is required to play the new word
                            return false; // Then we can't play the word
                        discrepencies++;
                    }
                }
            }
            else
                return false;

            return (discrepencies == oldword.Length) ? false : true; // A new stack word must contain AT LEAST 1 TILE from the old word.
        }

        private void aiplayallBUT_Click(object sender, EventArgs e)
        {
            if (!playing)
            {
                System.Threading.Thread thred = new System.Threading.Thread(PlayToEnd);
                thred.Start();
            }
        }

        private void aicontplayBUT_Click(object sender, EventArgs e)
        {
            if (!continuousplay && !playing)
            {
                System.Threading.Thread thred = new System.Threading.Thread(ContinuousPlayAll);
                thred.Start();
            }
        }
        private void ContinuousPlayAll() // Will have the AI play multiple games over and over until the user tells it to stop. This is useful for collecting statistics.
        { // If this function is directly called then the client will freeze. This function needs to be run on a new thread.
            continuousplay = true;
            while (continuousplay) // The variable continuousplay will stay true until user intervenes by pressing a button, which will set it to false.
            {
                if (!playing) // If the AI isn't currently playing a game to the end
                { // Then we will have it start playing on a new thread after resetting the game
                    ResetGame();
                    System.Threading.Thread thred = new System.Threading.Thread(PlayToEnd);
                    thred.Start();
                    thred.Join();
                }
            }

            continuousplay = false;
            return; // Required for multithreaded operations. If a thread is told to join, then it would hang until return is called
        }

        private void PlayToEnd() // Calling this function will cause the AI to play the game until it is completely unable to make any more moves.
        {
            playing = true;
            long longestplay = 0; // Holds the time of the longest play in milliseconds
            long shortestplay = long.MaxValue; // Holds the time of the shortest play in milliseconds

            if (logboxTB.InvokeRequired)
                logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text = ""; }));
            else
                logboxTB.Text = "";

            int[] pos = new int[2];
            string word = "";

            do
            {
                System.Diagnostics.Stopwatch stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                if (logboxTB.InvokeRequired)
                    logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += "TILE HAND: " + new string(AI_Letters) + "\r\n"; }));
                else
                    logboxTB.Text += "TILE HAND: " + new string(AI_Letters) + "\r\n";

                if (firstturn) // If the AI is taking the first turn
                {
                    word = FindLongestPlayableWord2(dictionary.Where(x => x.Length >= 3 && x.Length <= 6).ToArray());
                    AI_PlaceFirstWord(word);
                    firstturn = false;
                }
                else
                    word = FindNextMove2();

                if (word != BLANK_LETTER.ToString())
                {
                    if (logboxTB.InvokeRequired)
                        logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += "ATTEMPTED WORD: " + word.Replace("Q", "QU") + " (" + stpw.ElapsedMilliseconds.ToString() + " ms)\r\n"; }));
                    else
                        logboxTB.Text += "ATTEMPTED WORD: " + word.Replace("Q", "QU") + " (" + stpw.ElapsedMilliseconds.ToString() + " ms)\r\n";

                    if (stpw.ElapsedMilliseconds > longestplay)
                        longestplay = stpw.ElapsedMilliseconds;
                    else if (stpw.ElapsedMilliseconds < shortestplay)
                        shortestplay = stpw.ElapsedMilliseconds;
                }
                else
                {
                    if (logboxTB.InvokeRequired)
                        logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += "FINISHED.\r\n"; }));
                    else
                        logboxTB.Text += "FINISHED.\r\n";
                }

                GiveAITiles();
            }
            while (!word.Equals(BLANK_LETTER.ToString()));

            longestplays.Add(longestplay);
            shortestplays.Add(shortestplay);
            playing = false;

            return; // Required for multithreading operations. If this return was not here, thread.join() would stay frozen in its calling thread
        }

        private void resetgameBUT_Click(object sender, EventArgs e)
        {
            ResetGame();
        }
        private void ResetGame()
        {
            ClearAITiles(); // Clear AI tiles
            ClearGameboard(); // Clear game board
            ResetTileBag(); // Reset tile bag
            GiveAITiles(); // Hand AI tiles

            if (scoreLBL.InvokeRequired)
                scoreLBL.Invoke(new MethodInvoker(delegate { scoreLBL.Text = "Score: " + score.ToString(); }));
            else
                scoreLBL.Text = "Score: " + score.ToString();
            score = 0;
        }

        private void stopautoplayBUT_Click(object sender, EventArgs e)
        {
            continuousplay = false; // Setting this variable to false will kill the autoplay thread.
        }

        private void aiplaystackBUT_Click(object sender, EventArgs e)
        {
            AIPlayStack();
        }

        void sendgetBUT_Click(object sender, EventArgs e)
        {
            g = new GameNetworkCommuncation();
            g.RunAsync(logboxTB).GetAwaiter().OnCompleted(
                delegate()
                {
                    SetAITournamentTiles(g.myPayload.Letters);
                    SetAIBoardState(g.myPayload.Board);
                    myID = g.myPayload.ID;
                    turn = g.myPayload.Turn;
                });
        }

        private void GetGameState2()
        {
            if (!playing)
                return; // Double ensures the thread is killed
            do
            {
                System.Runtime.CompilerServices.TaskAwaiter<Payload> wait = g.GetGamestate().GetAwaiter();
                while (!wait.IsCompleted) ; // Poll for gamestate retrieval completion
                playing = g.myPayload.Success;

                if (playing)
                {
                    SetAITournamentTiles(g.myPayload.Letters);

                    if (turn != g.myPayload.Turn)
                    {
                        SetAIBoardState(g.myPayload.Board);
                        turn = g.myPayload.Turn;
                        if (tournamentscoreLBL.InvokeRequired)
                            tournamentscoreLBL.Invoke(new MethodInvoker(delegate { tournamentscoreLBL.Text = "T Score: " + g.myPayload.Score.ToString(); }));
                        else
                            tournamentscoreLBL.Text = "T Score: " + g.myPayload.Score.ToString();
                        if (logboxTB.InvokeRequired)
                            logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += "It is currently user " + g.myPayload.Turn.ToString() + "'s turn.\r\n"; }));
                        else
                            logboxTB.Text += "It is currently user " + g.myPayload.Turn.ToString() + "'s turn.\r\n";
                    }
                }
            } while ((turn != myID || myID == -1 || turn == -1) && playing); // Retrieves gamestate continuously until it is your turn.
            if(!playing)
                if (logboxTB.InvokeRequired)
                    logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += "Game over.\r\n"; }));
                else
                    logboxTB.Text += "Game over.\r\n";
            AIBestPlayChoice(); // After it is our turn we will play our best move
            AI_SendMove();
            return; // Ensures the thread is killed
        }

        private void GetGameState()
        {
            //GameNetworkCommuncation g = new GameNetworkCommuncation();
            g.GetGamestate().GetAwaiter().OnCompleted(delegate() // Gets gamestate

            //g.GetGamestateUntilTurn().GetAwaiter().OnCompleted(delegate() // Gets gamestate continuously until its your turn
            {
                SetAITournamentTiles(g.myPayload.Letters);
                SetAIBoardState(g.myPayload.Board);
                turn = g.myPayload.Turn;
                logboxTB.Text += "Game state successfully retrieved.\r\n";
                logboxTB.Text += "It is currently user " + g.myPayload.Turn.ToString() + "'s turn.\r\n";
            });
        }

        private void SetAITournamentTiles(string[] tiles)
        {
            /*if(tiles.Length < AI_Letters.Length)
            {
                string[] temp = new string[7];
                int i;
                for(i=0; i<tiles.Length; i++)
                    temp[i] = tiles[i];
                for (i = i; i < AI_Letters.Length; i++)
                    temp[i] = null;
                tiles = temp;
            }*/
            if(tiles!=null && tiles.Length > 0)
                for (int i = 0; i < AI_Letters.Length; i++) // iterate through the AI's given letters                  
                {
                    char newc = (tiles.Length == AI_Letters.Length) ? (tiles[i] != null) ? tiles[i][0] : BLANK_LETTER : BLANK_LETTER;
                    //char newc = (tiles[i] != null) ? tiles[i][0] : BLANK_LETTER;
                    AI_Letters[i] = newc;// Give the AI a random character tile between A and Z. (Tile Bag is shuffled when it is reset, so tiles are random)
                    DrawText(ref AI_tiles[i], newc, -1);
                }
        }

        private void SetAIBoardState(string[,,] board)
        {
            if (board != null)
                for (int r = 0; r < 10; r++) // Row
                    for (int c = 0; c < 10; c++) // Column
                    {
                        SetTile(new int[] { r, c }, (board[0, r, c] != null) ? board[0, r, c][0] : BLANK_LETTER, int.Parse(board[1, r, c] ?? "0"));
                        //AI_PlaceTile((board[0, r, c] != null) ? board[0, r, c][0] : BLANK_LETTER, new int[] { r, c }, true);
                        //stacklev[r, c] = int.Parse(board[1, r, c]??"0");
                    }
            if (firstturn)
                foreach (char c in tilesc)
                    if (c != BLANK_LETTER)
                    {
                        firstturn = false;
                        break;
                    }
        }

        private void sendmoveBUT_Click(object sender, EventArgs e)
        {
            playing = true;
            AI_SendMove();
        }
        private void AI_SendMove()
        {
            //playing = true;
            g.PlayMove(stacklev, tilesc).GetAwaiter().OnCompleted(
                delegate ()
                {
                    Thread t = new Thread(GetGameState2);
                    t.Start();
                });
            //.GetAwaiter().OnCompleted(delegate() { /*Thread.Sleep(100);*/ GetGameState(); });
        }

        private void getstateBUT_Click(object sender, EventArgs e)
        {
            GetGameState();
        }

        private void AIBestPlayChoice()
        {
            //WER: Using the same code as FindNextMove2 but only to retrieve the best play (called pbestreg) to compare scores later
            if (!firstturn)
            {
                List<PossibleWordPlacement> PossibleWordPlacements = new List<PossibleWordPlacement>(); // List of "PossibleWordPlacement" objects that holds information on the words we can place and where we can place them.
                List<NewPossibleWordPlacement> NewPossibleWordPlacements = new List<NewPossibleWordPlacement>(); // List of "NewPossibleWordPlacement" objects that holds information on the words we can place and where we can place them. Enables 2+ tile placements
                for (int r = 0; r < 10; r++) // Search through all of the rows
                    for (int c = 0; c < 10; c++) // Search through all of the columns
                    {
                        int rstart = -1, rend = -1, cstart = -1, cend = -1; // Used to keep track of the starting/ending position of a word
                        int[] pos = { r, c };

                        if (tilesc[r, c] != BLANK_LETTER) // We're only interested in tiles with a letter on them, because we can build words off of tiles with letters
                        {
                            List<int> rows; List<char> rschars;
                            List<int> rowe; List<char> rechars;
                            List<int> cols; List<char> cschars;
                            List<int> cole; List<char> cechars;

                            SearchUpward(pos, out rows, out rschars, out rstart); // Search in all four directions of the tile and record what we see
                            SearchDownward(pos, out rowe, out rechars, out rend); // as well as record valid data needed for word placement
                            SearchLeftward(pos, out cols, out cschars, out cstart);
                            SearchRightward(pos, out cole, out cechars, out cend);

                            if (rstart != -1 && rend != -1 && rstart != rend) // If we can make a vertical word
                            {
                                PossibleWordPlacements.Add(new PossibleWordPlacement(true, tilesc[r, c], stacklev[r, c], r, c, rstart, rend)); // Add the word to the list of possible word plays

                                List<char> lets = new List<char>(); lets.AddRange(rschars); lets.AddRange(rechars);
                                NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(true, lets, r, c, rows, rowe));
                            }
                            if (cstart != -1 && cend != -1 && cstart != cend) // If we can make a horizontal word
                            {
                                PossibleWordPlacements.Add(new PossibleWordPlacement(false, tilesc[r, c], stacklev[r, c], r, c, cstart, cend)); // Add the word to the list of possible word plays

                                List<char> lets = new List<char>(); lets.AddRange(cschars); lets.AddRange(cechars);
                                NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(false, lets, r, c, cols, cole));
                            }
                        }
                    }

                foreach (PossibleWordPlacement p in PossibleWordPlacements)
                {
                    int lpos = (p.dir) ? p.lrow : p.lcol;
                    int maxlen = p.end - p.start + 1;
                    List<string> words = new List<string>();
                    words.AddRange(dictionary.Where(x => x.Length <= maxlen && x.Contains(p.letter.ToString())).Where(x => x.IndexOf(p.letter) <= lpos && x.Length - 1 - x.IndexOf(p.letter) <= p.end - lpos && x.IndexOf(p.letter) <= lpos - p.start)); // CAP : Evaluated lambdas, figured this one would work correctly
                    words = words.OrderByDescending(x => x.Length).ToList(); // Simple explanation: These two lines save all possible playable words to a list of strings and sorts them so the longest ones come first. It is later evaluated to see which words CAN be played, based on tiles the AI has.

                    foreach (string w in words)
                        if (HasTilesForWord(w, p.letter)) // If the AI has tiles to play the word
                            p.playablewords.Add(w); // Save the word as playable word
                }

                PossibleWordPlacements = PossibleWordPlacements.Where(x => x.playablewords.Count > 0).ToList(); // Trim any possible placements with 0 playable words
                PossibleWordPlacements = PossibleWordPlacements.OrderByDescending(x => ScoreCalcReg(x)).ToList(); // Order the list so that the possible placements with the longest words are first

                int pregular;
                PossibleWordPlacement pbestreg = null;
                if (PossibleWordPlacements.Count > 0) // We cannot place any words if there are no possible placements
                {
                    pbestreg = PossibleWordPlacements[0];
                    if (pbestreg.playablewords.Count > 0) // We cannot place any words if there are no playable words (PossibleWordPlacements trims out the possible placements that had 0 playable words first)
                    {

                        pregular = ScoreCalcReg(pbestreg);
                    }
                    else
                        pregular = 0;
                }
                else
                    pregular = 0;

                //WER: Similar to regular play copied code from AIPlayStack is used to get the score of the best stack play available
                List<PlacedWord> PlacedWords = FindWordsOnBoard();
                List<PossibleWordStackPlacement> PossibleStackPlays = new List<PossibleWordStackPlacement>();
                foreach (PlacedWord p in PlacedWords)
                {
                    List<PlacedWord> AttachedWords = WordsAttachedToWord(p, PlacedWords); // Get a list of all the words attached to this current word
                    List<string> words = dictionary.Where(x => ContainsEnoughLetters(x, p.word)).ToList(); // All possible stack words that can be stacked on this placed word without taking attached placed words into consideration
                    foreach (PlacedWord a in AttachedWords) // Cycle through all words attached to the current word so we may take them into consideration
                    {
                        if (p.dir) // If p is vertical,
                        {
                            int atc = p.column, atr = a.row; // Then a is horizontal and the words are attached at p's column and a's row
                            int wdex = atr - p.row; // The index of the letter in p's string is the row of attachment minus the starting row (since we're vertical)
                            int awdex = atc - a.column; // Index of the letter in a's string is the column of attachment minus the starting column
                            words.RemoveAll(x => !NewStackWordExists(a.word, wdex, awdex, x)); // Remove words where the tile would be changed and make a non-existent word
                        }
                        else // If p is horizontal,
                        {
                            int atc = a.column, atr = p.row; // Then a is vertical and the words are attached at p's row and a's column
                            int wdex = atc - p.column;// The index of the letter in p's string is the column of attachment minus the starting column (since we're horizontal)
                            int awdex = atr - a.row;
                            words.RemoveAll(x => !NewStackWordExists(a.word, wdex, awdex, x)); // Remove words where the tile would be changed and make a non-existent word
                        }

                    }

                    if (p.dir) // This if statement will prevent any words that would cause a stack level higher than 5 from being played.
                    {
                        for (int r = p.row; r < p.row + p.Length; r++)
                            if (stacklev[r, p.column] >= 5)
                                words.RemoveAll(x => x[r - p.row] != p.word[r - p.row]);
                    }
                    else
                    {
                        for (int c = p.column; c < p.column + p.Length; c++)
                            if (stacklev[p.row, c] >= 5)
                                words.RemoveAll(x => x[c - p.column] != p.word[c - p.column]);
                    }

                    if (words.Count > 0)
                        PossibleStackPlays.Add(new PossibleWordStackPlacement(p.dir, p.row, p.column, p.word, words));
                }
                PossibleStackPlays = PossibleStackPlays.OrderByDescending(x => StackScoreCalc(x)).ToList(); // Orders stack plays by score
                int beststackscore;
                PossibleWordStackPlacement beststackselect = null;
                if (PossibleStackPlays.Count > 0)
                {
                    beststackselect = PossibleStackPlays[0];
                    beststackscore = StackScoreCalc(beststackselect);
                }
                else
                {
                    beststackscore = 0;
                }

                //Compare beststack to pregular (scores for best stack and regular play) to decide which to play
                if (beststackscore != 0 || pregular != 0)
                {
                    if (pregular < beststackscore)
                    {
                        AI_PlaceStack(beststackselect);
                        score += beststackscore;

                        if (scoreLBL.InvokeRequired)
                            scoreLBL.Invoke(new MethodInvoker(delegate { scoreLBL.Text = "Score: " + score.ToString(); }));
                        else
                            scoreLBL.Text = "Score: " + score.ToString();
                    }
                    else
                    {
                        AI_PlaceWord(pbestreg);
                    }
                }
            }
            else
            {
                string word = FindLongestPlayableWord2(dictionary.Where(x => x.Length >= 3 && x.Length <= 6).ToArray());
                AI_PlaceFirstWord(word);
                firstturn = false;
            }
        }

        private int StackScoreCalc(PossibleWordStackPlacement s) //Similar to ScoreCalcReg this calculates the score for a stack play based on the current stack level and the number of changes (increase score by 1 for each change)
        {
            int stackscore = 0;
            if (s.dir)
            {
                for (int rcount = s.r; rcount <= s.oldword.Length + s.r - 1; rcount++)
                {
                    stackscore += stacklev[rcount, s.c];
                }
            }
            else
            {
                for (int ccount = s.c; ccount <= s.oldword.Length + s.c - 1; ccount++)
                {
                    stackscore += stacklev[s.r, ccount];
                }
            }
            int changes = NumberDifferentLetters(s.oldword, s.pwords[0]);
            stackscore += (changes == 7) ? changes + 20 : changes; //Checks if the number of changes equals the number of tiles in hand (7) to determine full usage bonus
            return stackscore;
        }

        private int ScoreCalcReg(PossibleWordPlacement p)
        {
            string w = p.playablewords[0];
            int regscore = 0;
            if (p.stacklev == 1)
            {
                if (w.Length > 7)
                    regscore += (w.Contains('Q')) ? w.Length * 2 + 22 : w.Length * 2 + 20;
                else
                    regscore += (w.Contains('Q')) ? w.Length * 2 + 2 : w.Length * 2;
            }
            else
            {
                regscore += (w.Length > 7) ? w.Length - 1 + p.stacklev + 20 : w.Length - 1 + p.stacklev;
            }
            return regscore;
        }

        private void aiplaybestBUT_Click(object sender, EventArgs e)
        {
            AIBestPlayChoice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FindNextMove3();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            playing = false;
        }

        private void sendonemoveBUT_Click(object sender, EventArgs e)
        {
            g.PlayMove(stacklev, tilesc).GetAwaiter().OnCompleted(
                delegate()
                {
                    /*Thread.Sleep(100);*/
                    GetGameState();
                });
        }
    }
}
