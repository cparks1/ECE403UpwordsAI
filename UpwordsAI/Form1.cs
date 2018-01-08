#define simulation // Undefine this if the AI is competing
#define WORK_COMPUTER

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

// TO-DO : 
//         Update move making algorithms to become able to make moves off of more than just 1 tile at a time.
//         Verify scoring calculator
//

namespace UpwordsAI
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// List of longest times required to make a play. Utilized during "Continuous Play" mode.
        /// </summary>
        List<long> longestplays = new List<long>();

        /// <summary>
        /// List of shortest times required to make a play. Utilized during "Continuous Play" mode.
        /// </summary>
        List<long> shortestplays = new List<long>();

        /// <summary>
        /// AI's current calculated score.
        /// </summary>
        int score = 0;

        volatile bool playing = false; // Variable used to determine if the AI is currently playing a game til the end
        volatile bool continuousplay = false; // Variable used to determine if the AI should keep playing games over and over

        int myID = -1; // Variable used to remember the AI's player ID
        volatile int turn = -1; // Variable used to remember the current player turn

        const char BLANK_LETTER = '~';

        ComputerPlayer AI = new ComputerPlayer();

#if   SCHOOL_COMPUTER
        const string DICTIONARY_PATH = "Z:\\dictionary.txt";
#elif WORK_COMPUTER
        const string DICTIONARY_PATH = "C:\\users\\cparks\\Documents\\dictionary.txt";
#elif HOME_COMPUTER
        const string DICTIONARY_PATH = "C:\\Users\\Christopher\\Documents\\dictionary.txt"; // "Z:\\dictionary.txt";
#endif

        LocalTileBag tile_bag = new LocalTileBag(); // LocalTileBag object that will contain all the tiles that can be handed to the AI (not for tournament use)

        string[] dictionary = new string[0]; // String array that will hold the entire dictionary.

        GraphicTile[,] gameboard = new GraphicTile[10, 10]; // 10x10 array of tiles visible to humans that stores data on the character and stack level of that tile.

        bool firstturn = true; // Boolean that determines if the AI will follow special rules for placing the first word.

        Random rand = new Random(); // Random number generator used in first move type turns to create unpredictable first turns

        GameNetworkCommuncation g; // Object used for communications with the tournament adjudicator program

        public Form1()
        {
            InitializeComponent();
            LoadDictionary(); // Loads up the dictionary given in DICTIONARY_PATH into a string array named dictionary
            InitializeBoard(); // Draws the board and sets all tiles to blanks
            InitializeAITiles(); // Draws the AI's held tiles and sets them all to blanks
        }

        public void InitializeBoard()
        {
            int ypos = 13;
            for (int r = 0; r < 10; r++) // This for loop iterates through each tile on the gameboard and creates the picturebox that will show the tile information
            { // It also draws a blank tile onto each tile, and sets the machine readable tile as a blank tile
                int xpos = 13;
                for (int c = 0; c < 10; c++)
                {
                    Point location = new Point(xpos, ypos);
                    gameboard[r, c] = new GraphicTile(location, $"({r.ToString()},{c.ToString()})", true); // (r,c)
                    Controls.Add(gameboard[r, c].tile_box); // Add graphic tile's picturebox to form1's visible controls
                    gameboard[r, c].DrawTile(BLANK_LETTER, 0); // Set this tile to a blank tile, and draw it.
                    xpos += 33;
                }
                ypos += 33;
            }
        }

        /// <summary>
        /// Initializes the value of each tile held by the AI and then draws them on the game board.
        /// </summary>
        public void InitializeAITiles()
        {
            int xpos = gameboard[9, 1].tile_box.Left + 16, ypos = gameboard[9, 1].tile_box.Bottom + 32;
            for (int i = 0; i < AI.tileset.Length; i++)
            {
                Point location = new Point(xpos, ypos);
                AI.tileset[i] = new GraphicTile(location, i.ToString(), false);
                Controls.Add(AI.tileset[i].tile_box);
                AI.tileset[i].DrawTile(BLANK_LETTER, 0);
                xpos += 33;
            }

        }

        public void LoadDictionary()
        {
            try
            { dictionary = System.IO.File.ReadAllLines(DICTIONARY_PATH); FixDictionary(); }
            catch (Exception exc)
            { MessageBox.Show(exc.ToString(), "Unable to load dictionary!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        /// <summary>
        /// Removes words that the AI cannot play from the dictionary that's been loaded.
        /// Modifies words in the dictionary that the AI can play, so it can understand them.
        /// For example, QUACK is changed to QACK.
        /// NIQAB is removed as it is unplayable in UpWords.
        /// </summary>
        public void FixDictionary()
        {
            List<string> tempd = new List<string>(); // Temporary dictionary
            foreach (string word in dictionary)
            {
                string modified_word = "";
                bool playable_word = true;

                for (int i = 0; i < word.Length; i++) // Look through all characters in the word
                {
                    char c = char.ToUpper(word[i]); // AI only handles tiles in upper case.
                    if(c=='Q') // If it's a Q verify the next letter is a U
                    {
                        if(i + 1 == word.Length) // Q is the last letter in the word
                        {
                            playable_word = false;
                        }
                        else if(char.ToUpper(word[i+1]) != 'U') // Q MUST have a U following it, or the word is unplayable.
                        { // If 'U' is not next, remove the word by not adding it to the tempd list
                            playable_word = false;
                            break;
                        }
                        else // If it is next, change it from QU to Q
                        {
                            modified_word += 'Q';
                            i++; // Skip the U in the word. For loop will increment again at the end of this loop.
                        }
                    }
                    else
                    {
                        modified_word += c; // Add the letter.
                    }
                }

                if(playable_word) // Addthe word to the modified dictionary
                {
                    tempd.Add(modified_word);
                }
            }
            dictionary = tempd.ToArray(); // Replace the loaded dictionary with the new fixed dictionary.
        }

        /// <summary>
        /// Function under construction that will attempt to use Regex to select playable words.
        /// Also needs to bridge words together if possible.
        /// </summary>
        private void FindNextMove3() // This function finds possible moves the AI can make after the first turn and plays them, where a move is considered to be building an entirely new word off of one or multiple words.
        {
            List<string> regexes=new List<string>();
            string regex = "";
            for(int r=0; r<10; r++) // Cycle through all rows
            {
                regex = "";

                for (int c = 0; c < 10; c++)
                    regex += (gameboard[r, c].letter_value == BLANK_LETTER) ? '.' : gameboard[r, c].letter_value; // Generate a regex capable of search

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
                    regex += (gameboard[r, c].letter_value == BLANK_LETTER) ? '.' : gameboard[r, c].letter_value; // Generate a regex capable of search
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

            if (!gameboard[r, c].IsBlank) // We're only interested in tiles with a letter on them, because we can build words off of tiles with letters
            {
                if (r - 1 >= 0) // If we can check ABOVE the tile we're currently searching (We do this check to avoid an out of bounds exception)
                {
                    if (gameboard[r - 1, c].IsBlank) // This is part of the process of checking if we're able to build an entirely new vertical word.
                    {
                        rstart = r; // Set the initial starting row
                        for (int i = r - 1; i >= 0; i--) // Search all the rows behind
                        {
                            letters.Add(gameboard[i, c].letter_value); // Add the letter we may be building off of

                            if (gameboard[i, c].IsBlank) // Need a blank tile to place a word.
                            {
                                if (c + 1 <= 9) // If we can search to the right
                                    if (!gameboard[i, c + 1].IsBlank) // Verify we're not building next to other tiles CAP: Modify this so the function will check if it's a valid word
                                        break; // If we are break before rstart is set

                                if (c - 1 >= 0) // If we can search to the left
                                    if (!gameboard[i, c - 1].IsBlank) // Verify we're not building next to other tiles
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
                if (gameboard[r + 1, c].IsBlank) // This is part of the process of checking if we're able to build an entirely new vertical word.
                {
                    rend = r; // Set the initial ending row
                    for (int i = r + 1; i <= 9; i++) // Search all the rows ahead
                    {
                        letters.Add(gameboard[i, c].letter_value); // Add the letter we may be building off of
                        if (gameboard[i, c].IsBlank) // Need a blank tile to place a word.
                        {
                            if (c + 1 <= 9) // If we can search to the right
                                if (!gameboard[i, c + 1].IsBlank) // Verify we're not building next to other tiles CAP: Modify this function to allow us to build next to other tiles if possible
                                    break; // If we are break before rstart is set
                            if (c - 1 >= 0) // If we can search to the left
                                if (!gameboard[i, c - 1].IsBlank) // Verify we're not building next to other tiles
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
                if (gameboard[r, c - 1].IsBlank) // If the tile to the left of it is a blank letter
                {
                    cstart = c; // Set the initial starting column
                    for (int i = c - 1; i >= 0; i--) // Search all the columns behind
                    {
                        letters.Add(gameboard[r, i].letter_value); // Add the letter we may be building off of
                        if (gameboard[r, i].IsBlank) // Need a blank tile to place a word.
                        {
                            if (r + 1 <= 9) // If we can search to the right
                                if (!gameboard[r + 1, i].IsBlank) // Verify we're not building next to other tiles
                                    break; // If we are break before rstart is set
                            if (r - 1 >= 0) // If we can search to the left
                                if (!gameboard[r - 1, i].IsBlank) // Verify we're not building next to other tiles
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
                if (gameboard[r, c + 1].IsBlank) // If the tile to the right of it is a blank letter
                {
                    cend = c;
                    for (int i = c + 1; i <= 9; i++) // Search all the columns ahead
                    {
                        letters.Add(gameboard[r, i].letter_value);
                        if (gameboard[r, i].IsBlank) // Need a blank tile to place a word.
                        {
                            if (r + 1 <= 9) // If we can search to the right
                                if (!gameboard[r + 1, i].IsBlank) // Verify we're not building next to other tiles
                                    break; // If we are break before rstart is set
                            if (r - 1 >= 0) // If we can search to the left
                                if (!gameboard[r - 1, i].IsBlank) // Verify we're not building next to other tiles
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

        private PossibleWordPlacement FindNextMove2() // This function finds the next possible move (After the first) move that the AI can make, where a move is considered to be building an entirely new word off of an old one
        {
            List<PossibleWordPlacement> PossibleWordPlacements = new List<PossibleWordPlacement>(); // List of "PossibleWordPlacement" objects that holds information on the words we can place and where we can place them.
            List<NewPossibleWordPlacement> NewPossibleWordPlacements = new List<NewPossibleWordPlacement>(); // List of "NewPossibleWordPlacement" objects that holds information on the words we can place and where we can place them. Enables 2+ tile placements
            for (int r = 0; r < 10; r++) // Search through all of the rows
                for (int c = 0; c < 10; c++) // Search through all of the columns
                {
                    int rstart = -1, rend = -1, cstart = -1, cend = -1; // Used to keep track of the starting/ending position of a word
                    int[] pos = { r, c };

                    if (!gameboard[r, c].IsBlank) // We're only interested in tiles with a letter on them, because we can build words off of tiles with letters
                    {
                        SearchUpward(pos, out List<int> rows, out List<char> rschars, out rstart); // Search in all four directions of the tile and record what we see
                        SearchDownward(pos, out List<int> rowe, out List<char> rechars, out rend); // as well as record valid data needed for word placement
                        SearchLeftward(pos, out List<int> cols, out List<char> cschars, out cstart);
                        SearchRightward(pos, out List<int> cole, out List<char> cechars, out cend);

                        if (rstart != -1 && rend != -1 && rstart != rend) // If we can make a vertical word
                        {
                            PossibleWordPlacements.Add(new PossibleWordPlacement(true, gameboard[r, c].letter_value, r, c, rstart, rend)); // Add the word to the list of possible word plays

                            List<char> lets = new List<char>(); lets.AddRange(rschars); lets.AddRange(rechars);
                            NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(true, lets, r, c, rows, rowe));
                        }
                        if (cstart != -1 && cend != -1 && cstart != cend) // If we can make a horizontal word
                        {
                            PossibleWordPlacements.Add(new PossibleWordPlacement(false, gameboard[r, c].letter_value, r, c, cstart, cend)); // Add the word to the list of possible word plays

                            List<char> lets = new List<char>(); lets.AddRange(cschars); lets.AddRange(cechars);
                            NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(false, lets, r, c, cols, cole));
                        }
                    }
                }

            foreach (PossibleWordPlacement p in PossibleWordPlacements)
            {
                p.SetBestWord(AI, dictionary);
            }

            return PossibleWordPlacements.Aggregate(PossibleWordPlacements[0], (max, cur) => max.longest_word.Length > cur.longest_word.Length ? max : cur); // Return the word placement with the longest word
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
                        if (gameboard[r, c].IsBlank) // If there is a blank space for the AI to place a word
                            blanktiles++;           // Count the number of blank spaces available
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
                        if (gameboard[r, c].IsBlank)
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

        /// <summary>
        /// Function that returns the longest word in the dictionary that the AI has tiles for. Considers a Q tile to be length of '2'.
        /// </summary>
        /// <param name="dict">Dictionary of words to look through.</param>
        /// <param name="min_len">Minimum length, inclusive.</param>
        /// <param name="max_len">Maximum length, inclusive.</param>
        /// <returns>Word found if function found a playable word, BLANK_LETTER.ToString() if it couldn't find any.</returns>
        private string FindLongestPlayableWord2(string[] dict, int min_len, int max_len) // Finds longest playable word in the dictionary by looking at the AI's tileset.
        { // CAP : This function is faster than a human, but terrible in terms of speed when matched against computers.
            string longest_word = "";       // Keep track of the longest word
            int longest_word_true_len = 0;  // Used to keep track of the longest word's "true" length, where a Q tile is considered to be worth more.

            foreach (string word in dict) // Cycle through each word in the dictionary
            {
                if (word.Length >= min_len && word.Length <= max_len)
                {
                    int word_true_len = word.Length + (word.Contains("Q") ? 1 : 0);
                    if (word_true_len > longest_word_true_len && AI.HasTilesForWord(word, BLANK_LETTER))
                    {
                        longest_word = word;
                        longest_word_true_len = word_true_len;
                    }
                }
            }

            // If there was no valid word available, then
            // return a blank letter to notify there was no choice.
            return longest_word != "" ? longest_word : BLANK_LETTER.ToString();
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

                AI.PlaceWord(word, new int[] { r, c }, dir, gameboard);

                score += (word.Contains('Q')) ? word.Length * 2 + 2 : word.Length * 2;
                Set_score_lbl_score(score);
            }
            else if (word.Length > 2)
            { // If the word is 5 letters or shorter, and at least 3 letters, then we don't need to worry about constraints.
                bool dir = (rand.Next(int.MaxValue) % 2 == 0) ? true : false; // Choose word placement direction randomly
                int[] pos = new int[] { ((rand.Next(int.MaxValue) % 2 == 0) ? 4 : 5), ((rand.Next(int.MaxValue) % 2 == 0) ? 4 : 5) }; // Choose word tile start randomly

                AI.PlaceWord(word, pos, dir, gameboard);

                score += (word.Contains('Q')) ? word.Length * 2 + 2 : word.Length * 2;
                Set_score_lbl_score(score);
            }
            else
                MessageBox.Show("The AI could not make a first turn!");
        }

        private void giveaitilesBUT_Click(object sender, EventArgs e)
        {
            AI.GrabTiles(tile_bag);
        }

        // NOTE: There's a bug in this function where it is not placing words.
        private void aiplacewordBUT_Click(object sender, EventArgs e)
        { // When the AI places tiles, the program must then set the tiles used to "blank character".
            string word;

            if (firstturn) // If the AI is taking the first turn
            {
                word = FindLongestPlayableWord2(dictionary, 3, 6);
                AI_PlaceFirstWord(word);
                firstturn = false;
            }
            else
            {
                PossibleWordPlacement next_word_placement = FindNextMove2();
                word = next_word_placement.longest_word;
                AI.PlaceWord(next_word_placement, gameboard);

                score += next_word_placement.Score();
                Set_score_lbl_score(score);
            }

            if (word != BLANK_LETTER.ToString())
            {
                logboxTB.Text += $"PLAYED: {word.Replace("Q", "QU")}.\r\n";
            }
            else
            {
                MessageBox.Show("The AI was unable to play a word.");
            }
        }

        private void clearaitilesBUT_Click(object sender, EventArgs e)
        { AI.ResetTileHand(); }

        private void clearboardBUT_Click(object sender, EventArgs e)
        { ClearGameboard(); }

        private void ClearGameboard()
        {
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    gameboard[r, c].DrawTile(BLANK_LETTER, 0);
            firstturn = true;
        }

        /// <summary>
        /// Public accessor for working with the mouseposLBL's text
        /// </summary>
        public string MouseposLBL_text
        { get { return mouseposLBL.Text; } set { mouseposLBL.Text = value; } }

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
                        if (!gameboard[r, c].IsBlank && (!searched[0, r, c] || !searched[1, r, c])) // If the tile is not a blank letter and hasn't yet been searched then we will search it
                        { // The tile is not blank and hasn't been searched yet (either vertically or horizontally)
                            if (ca && !searched[0, r, c]) // If we're on a tile where we can search above it (row != 0)
                            { // If we can search the tiles above this and this tile hasn't yet been searched vertically
                                if (!gameboard[r - 1, c].IsBlank && !searched[0, r - 1, c]) // If the tile above it is not a blank letter
                                {
                                    for (int i = r - 1; i >= 0; i--) // Go up in rows until you hit the end of the word
                                    {
                                        if (!gameboard[i, c].IsBlank) // A letter tile has been placed here
                                        {
                                            searched[0, i, c] = true; // We will want to mark it as searched
                                            rstart = i; // And then set the starting row
                                            rstring = rstring.Insert(0, gameboard[i, c].letter_value.ToString()); // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }
                                }
                            }

                            if (cb && !searched[0, r, c]) // If we're on a tile where we can search below it (row != 9)
                            {
                                if (!gameboard[r + 1, c].IsBlank && !searched[0, r + 1, c]) // If the tile below it is not a blank letter and hasn't been searched
                                {
                                    rstart = r;
                                    for (int i = r + 1; i <= 9; i++)
                                    {
                                        if (!gameboard[i, c].IsBlank) // A letter tile has been placed here
                                        {
                                            searched[0, i, c] = true;                // We will want to mark it as searched
                                            rend = i;                                // And then set the ending row
                                            rstring += gameboard[i, c].letter_value; // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }
                                }
                            }

                            searched[0, r, c] = true;
                            rstring = rstring.Insert(0, gameboard[r, c].letter_value.ToString());

                            if (cl && !searched[1, r, c]) // If we're on a tile where we can search to the left of it (column != 0)
                            {
                                if (!gameboard[r, c - 1].IsBlank && !searched[1, r, c - 1]) // If the tile to the left of it is not a blank letter
                                {
                                    cstart = c;
                                    for (int i = c - 1; i >= 0; i++)
                                    {
                                        if (!gameboard[r, i].IsBlank) // A letter tile has been placed here
                                        {
                                            searched[1, r, i] = true; // We will want to mark it as searched
                                            cstart = i; // And then set the starting column
                                            cstring = cstring.Insert(0, gameboard[r, i].letter_value.ToString()); // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }

                                }
                            }

                            if (cr && !searched[1, r, c]) // If we're on a tile where we can search to the right of it (column != 9)
                            {
                                if (!gameboard[r, c + 1].IsBlank && !searched[1, r, c + 1]) // If the tile to the right of it is not a blank letter
                                {
                                    cstart = c;
                                    for (int i = c + 1; i <= 9; i++)
                                    {
                                        if (!gameboard[r, i].IsBlank) // A letter tile has been placed here
                                        {
                                            searched[1, r, i] = true; // We will want to mark it as searched
                                            cend = i; // And then set the ending column
                                            cstring += gameboard[r, i].letter_value; // Build the string
                                        }
                                        else break; // If we've hit a blank tile then we want to break this for loop
                                    }
                                }
                            }

                            searched[1, r, c] = true; cstring = cstring.Insert(0, gameboard[r, c].letter_value.ToString());

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        List<PlacedWord> WordsAttachedToTile(int[] pos) // Array pos assumes form of { row, column }.
        {
            int r = pos[0], c = pos[1];
            return FindWordsOnBoard().Where(x => (x.dir) ? ((x.column == c) && (x.row + x.Length - 1 >= r) && (x.row <= r)) : ((x.row == r) && (x.column + x.Length - 1 >= c) && (x.column <= c))).ToList(); // Gets a list of the words attached to the current searched tile
        }

        /// <summary>
        /// Finds all words in the given list that are attached to a tile at the given position.
        /// </summary>
        /// <param name="words">List of words placed on the game board.</param>
        /// <param name="pos">Integer array, requires form of {row, column}.</param>
        /// <returns>A list of placed words that are attached to the tile at the given position, taken from the given list of placed words.</returns>
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
                bool can_search_left = word.column - 1 >= 0, can_search_right = word.column + 1 < 10; // Whether we can search left/right of the tile
                int c = word.column;
                for (int r = word.row; r < word.row + word.Length; r++) // Search through all tiles attached to word
                {
                    bool found = false; // If we found a word attached to the tile we don't want to check twice by checking left AND right

                    if (can_search_left) // If we can search left (if we can't, there is no word attached to the left)
                    {
                        if (!gameboard[r, c - 1].IsBlank) // Doing this one comparison saves us time by checking if there is a word attached before trying to get it
                        {
                            WordsAttached.AddRange(WordsAttachedToTile(WordsOnBoard, new int[] { r, c }).Where(x => x != word));
                            found = true;
                        }
                    }
                    if (can_search_right && !found) // If we can search right (if we can't, there is no word attached to the right) and haven't yet found a word
                    {
                        if (!gameboard[r, c + 1].IsBlank) // Doing this one comparison saves us time
                        {
                            WordsAttached.AddRange(WordsAttachedToTile(WordsOnBoard, new int[] { r, c }).Where(x => x != word));
                        }
                    }
                }
            }
            else
            {
                bool can_search_above = word.row - 1 >= 0, can_search_below = word.row + 1 < 10; // Whether we can search above/below of the tile
                int r = word.row;
                for (int c = word.column; c < word.column + word.Length; c++) // Search through all tiles attached to word
                {
                    bool found = false; // If we found a word attached to the tile we don't want to check twice by checking above AND below

                    if (can_search_above)     // If we can search above
                    {
                        if (!gameboard[r - 1, c].IsBlank)   // If there's a letter on this tile
                        {
                            WordsAttached.AddRange(WordsAttachedToTile(WordsOnBoard, new int[] { r, c }).Where(x => x != word));
                            found = true;
                        }
                    }
                    if (can_search_below && !found)   // If we can search below, and we haven't found a word attached to the tile yet
                    {
                        if (!gameboard[r + 1, c].IsBlank)   // If there's a letter on this tile
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
                        // NOTE: Why are they being added in the first place?
                    }
                    else // If p is horizontal,
                    {
                        int atc = a.column, atr = p.row; // Then a is vertical and the words are attached at p's row and a's column
                        int wdex = atc - p.column;// The index of the letter in p's string is the column of attachment minus the starting column (since we're horizontal)
                        int awdex = atr - a.row;
                        words.RemoveAll(x => !NewStackWordExists(a.word, wdex, awdex, x)); // Remove words where the tile would be changed and make a non-existent word
                        // NOTE: Why are they being added in the first place?
                    }

                }

                // NOTE: I have a note asking why words with a stack level > 5 would be added. It seems this may be preventing that from actually happening, which means I can remove a redundant check.
                if (p.dir) // This if statement will prevent any words that would cause a stack level higher than 5 from being played.
                {
                    for (int r = p.row; r < p.row + p.Length; r++)
                        if (gameboard[r, p.column].stack_value >= 5)
                            words.RemoveAll(x => x[r - p.row] != p.word[r - p.row]);
                }
                else
                {
                    for (int c = p.column; c < p.column + p.Length; c++)
                        if (gameboard[p.row, c].stack_value >= 5)
                            words.RemoveAll(x => x[c - p.column] != p.word[c - p.column]);
                }

                if (words.Count > 0)
                    PossibleStackPlays.Add(new PossibleWordStackPlacement(p.dir, p.row, p.column, p.word, words));
            }
            PossibleStackPlays = PossibleStackPlays.OrderByDescending(x => Utilities.NumberDifferentLetters(x.oldword, x.pwords[0])).ToList(); // Orders the list so the longest stack play is at the front
            if (PossibleStackPlays.Count > 0)
            {
                AI.PlaceStack(PossibleStackPlays[0], gameboard);
                score += PossibleStackPlays[0].Score(gameboard);
                Set_score_lbl_score(score);
                Log_Box_Post_Message($"STACK PLAY: {PossibleStackPlays[0].pwords[0]}\r\n");
            }
        }

        public bool NewStackWordExists(string aword, int wdex, int awdex, string checkword) // Used by AI stacking function
        {
            StringBuilder sb = new StringBuilder(aword);
            sb[awdex] = checkword[wdex];
            aword = sb.ToString();
            return dictionary.Contains(aword);
        }

        /// <summary>
        /// This function determines if you can play a new word on top of an old word.
        /// </summary>
        /// <param name="word">New word we are attempting to build via a stack play</param>
        /// <param name="oldword">Old word we are stacking on top of</param>
        /// <returns></returns>
        public bool ContainsEnoughLetters(string word, string oldword) // Function allows us to tell if given n wildcards, whether or not we can play this word
        { // If using this function to determine if a new word can be played in a stack play, make sure n is less than or equal to the previous word length minus 1. You cannot cover all tiles in one play.
            int discrepencies = 0;  // Number of character differences between word and oldword

            if (word == oldword) // To play a stack move, the words must be different.
            {
                return false;
            }

            if (word.Length == oldword.Length) // Word lengths must be the same to play word on top of oldword
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (word[i] != oldword[i])
                    {
                        if (AI.tileset.Count(x => x.letter_value == word[i]) < word.Count(x => x == word[i])) // If the AI has less tiles of this kind than is required to play the new word
                            return false; // Then we can't play the word

                        discrepencies++;
                    }
                }
            }
            else
            {
                return false;
            }

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
            bool tiles_left = true;

            string play_message = "";

            Clear_Log_Box();

            do
            {
                System.Diagnostics.Stopwatch stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                // Post tile hand to logging text box
                Log_Box_Post_Message($"TILE HAND: {new string(AI.tileset.Select(x=>x.letter_value).ToArray())}\r\n");

                play_message = AIBestPlayChoice();
                if (play_message != "NO PLAY")
                {
                    Log_Box_Post_Message($"{play_message} ({stpw.ElapsedMilliseconds.ToString()} ms)\r\n");

                    if (stpw.ElapsedMilliseconds > longestplay)
                        longestplay = stpw.ElapsedMilliseconds;
                    else if (stpw.ElapsedMilliseconds < shortestplay)
                        shortestplay = stpw.ElapsedMilliseconds;
                }
                else
                {
                    Log_Box_Post_Message($"FINISHED.\r\nLONGEST PLAY: {longestplay}\r\nSHORTEST PLAY:{shortestplay}\r\n");
                }

                if (tiles_left)
                {
                    tiles_left = AI.GrabTiles(tile_bag);
                }
            }
            while (!play_message.Equals("NO PLAY"));

            longestplays.Add(longestplay);
            shortestplays.Add(shortestplay);
            playing = false;

            return; // Required for multithreading operations. If this return was not here, thread.join() would stay frozen in its calling thread
        }

        private void Clear_Log_Box()
        {
            if (logboxTB.InvokeRequired)
                logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text = ""; }));
            else
                logboxTB.Text += "";
        }

        private void Log_Box_Post_Message(string message)
        {
            if (logboxTB.InvokeRequired)
                logboxTB.Invoke(new MethodInvoker(delegate { logboxTB.Text += message; }));
            else
                logboxTB.Text += message;
        }

        private void resetgameBUT_Click(object sender, EventArgs e)
        {
            ResetGame();
        }

        private void ResetGame()
        {
            AI.ResetTileHand(); // Clear the AI's tile hand
            ClearGameboard(); // Clear game board
            tile_bag.Reset(); // Reset tile bag
            AI.GrabTiles(tile_bag); // Hand AI tiles

            score = 0;
            Set_score_lbl_score(score);
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
                        Log_Box_Post_Message($"It is currently user {g.myPayload.Turn.ToString()}'s turn.\r\n");
                    }
                }
            } while ((turn != myID || myID == -1 || turn == -1) && playing); // Retrieves gamestate continuously until it is your turn.

            if (!playing)
            {
                Log_Box_Post_Message("Game over.\r\n");
            }

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
            if(tiles!=null && tiles.Length > 0)
                for (int i = 0; i < AI.tileset.Length; i++) // iterate through the AI's given letters                  
                {
                    char newc = (tiles.Length == AI.tileset.Length) ? (tiles[i] != null) ? tiles[i][0] : BLANK_LETTER : BLANK_LETTER; // Handle any possible errors in transmission
                    AI.tileset[i].DrawTile(newc, -1); // Give the AI the tile we're currently looking at, and draw it.
                }
        }

        /// <summary>
        /// Function that translates the Tournament Adjudicator's board to our own board format, and then sets the board state.
        /// </summary>
        /// <param name="board">Tournament Adjudicator formatted board.
        ///                     Should be a 3D array where the first dimension switches between letter (0) and stack (1).
        ///                     The other two dimensions correspond to board position.
        /// </param>
        private void SetAIBoardState(string[,,] board)
        {
            if (board != null) // If we've received a valid board
            {
                for (int r = 0; r < 10; r++) // Loop through all rows
                {
                    for (int c = 0; c < 10; c++) // Loop through each column for each row
                    {
                        char letter = (board[0, r, c] != null) ? board[0, r, c][0] : BLANK_LETTER;  // If the letter is NULL, set it to BLANK. Otherwise set it to the letter it represents.
                        sbyte stack_level = sbyte.Parse(board[1, r, c] ?? "0"); // If the stack level is NULL, set it to 0. Else, parse whatever it is and set it.
                        gameboard[r, c].DrawTile(letter, stack_level);  // Set tile letter and stack level, draw the tile.
                    }
                }
            }

            if (firstturn) // If we believe it's our first turn, then verify it truly is.
            {
                foreach (GraphicTile tile in gameboard)
                {
                    if (!tile.IsBlank)   // If any tiles are filled
                    {
                        firstturn = false; // Then we are not actually making the first move!
                        break;
                    }
                }
            }
        }

        private void sendmoveBUT_Click(object sender, EventArgs e)
        {
            playing = true;
            AI_SendMove();
        }

        private void AI_SendMove()
        {
            g.PlayMove(gameboard).GetAwaiter().OnCompleted(
                delegate ()
                {
                    Thread t = new Thread(GetGameState2);
                    t.Start();
                });
        }

        private void getstateBUT_Click(object sender, EventArgs e)
        {
            GetGameState();
        }

        /// <summary>
        /// Function that will play the best move the AI can think of.
        /// </summary>
        /// <returns>A message indicating the type of move made and the new word formed.</returns>
        private string AIBestPlayChoice()
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

                        if (!gameboard[r, c].IsBlank) // We're only interested in tiles with a letter on them, because we can build words off of tiles with letters
                        {
                            SearchUpward(pos, out List<int> rows, out List<char> rschars, out rstart); // Search in all four directions of the tile and record what we see
                            SearchDownward(pos, out List<int> rowe, out List<char> rechars, out rend); // as well as record valid data needed for word placement
                            SearchLeftward(pos, out List<int> cols, out List<char> cschars, out cstart);
                            SearchRightward(pos, out List<int> cole, out List<char> cechars, out cend);

                            if (rstart != -1 && rend != -1 && rstart != rend) // If we can make a vertical word
                            {
                                PossibleWordPlacements.Add(new PossibleWordPlacement(true, gameboard[r, c].letter_value, gameboard[r, c].stack_value, r, c, rstart, rend)); // Add the word to the list of possible word plays

                                List<char> lets = new List<char>(); lets.AddRange(rschars); lets.AddRange(rechars);
                                NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(true, lets, r, c, rows, rowe));
                            }

                            if (cstart != -1 && cend != -1 && cstart != cend) // If we can make a horizontal word
                            {
                                PossibleWordPlacements.Add(new PossibleWordPlacement(false, gameboard[r, c].letter_value, gameboard[r, c].stack_value, r, c, cstart, cend)); // Add the word to the list of possible word plays

                                List<char> lets = new List<char>(); lets.AddRange(cschars); lets.AddRange(cechars);
                                NewPossibleWordPlacements.Add(new NewPossibleWordPlacement(false, lets, r, c, cols, cole));
                            }
                        }
                    }

                // NOTE: Known issue with this routine: doesn't account for Q being worth 2 more points
                foreach (PossibleWordPlacement p in PossibleWordPlacements)
                {
                    p.SetBestWord(AI, dictionary);
                }

                PossibleWordPlacements = PossibleWordPlacements.Where(x => x.longest_word != "").OrderByDescending(x => x.longest_word.Length).ToList(); // Trim any possible placements with no playable word

                int best_word_play_score;
                PossibleWordPlacement pbestreg = null;
                if (PossibleWordPlacements.Count > 0) // We cannot place any words if there are no possible placements
                {
                    pbestreg = PossibleWordPlacements[0];
                    if (pbestreg.longest_word != "") // Verify the longest_word is a valid word
                    {
                        best_word_play_score = pbestreg.Score();
                    }
                    else
                    {
                        best_word_play_score = 0;
                    }
                }
                else
                {
                    best_word_play_score = 0;
                }

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
                    {           // NOTE: Why are any words that would cause a stack level >= 5 even being added in the first place?!?!
                        for (int r = p.row; r < p.row + p.Length; r++)
                            if (gameboard[r, p.column].stack_value >= 5)
                                words.RemoveAll(x => x[r - p.row] != p.word[r - p.row]);
                    }
                    else
                    {
                        for (int c = p.column; c < p.column + p.Length; c++)
                            if (gameboard[p.row, c].stack_value >= 5)
                                words.RemoveAll(x => x[c - p.column] != p.word[c - p.column]);
                    }

                    if (words.Count > 0)
                        PossibleStackPlays.Add(new PossibleWordStackPlacement(p.dir, p.row, p.column, p.word, words));
                }
                PossibleStackPlays = PossibleStackPlays.OrderByDescending(x => x.Score(gameboard)).ToList(); // Orders stack plays by score
                int best_stack_play_score;
                PossibleWordStackPlacement beststackselect = null;
                if (PossibleStackPlays.Count > 0)
                {
                    beststackselect = PossibleStackPlays[0];
                    best_stack_play_score = beststackselect.Score(gameboard);
                }
                else
                {
                    best_stack_play_score = 0;
                }

                //Compare beststack to pregular (scores for best stack and regular play) to decide which to play
                if (best_stack_play_score != 0 || best_word_play_score != 0)
                {
                    if (best_word_play_score < best_stack_play_score)
                    {
                        AI.PlaceStack(beststackselect, gameboard);

                        score += best_stack_play_score;
                        Set_score_lbl_score(score);

                        return $"STACK PLAY: {beststackselect.oldword} -> {beststackselect.pwords[0]} ({best_stack_play_score})";
                    }
                    else
                    {
                        AI.PlaceWord(pbestreg, gameboard);

                        score += best_word_play_score;
                        Set_score_lbl_score(score);

                        return $"WORD PLAY: {pbestreg.longest_word} ({best_word_play_score})";
                    }
                }
                else
                {
                    return "NO PLAY";
                }
            }
            else
            {
                // Limit word choices to at least 3 tiles because the first move MUST be at least 3 tiles.
                // Limit word choices to a maximum of 6 tiles because 7-10 tiles cannot fit in the first play.
                string word = FindLongestPlayableWord2(dictionary, 3, 6);
                AI_PlaceFirstWord(word);
                firstturn = false;
                return $"WORD PLAY: {word} ({word.Length * 2 + (word.Contains("Q") ? 2 : 0)})";
            }
        }

        public void Set_score_lbl_score(int new_score)
        {
            if (scoreLBL.InvokeRequired)
                scoreLBL.Invoke(new MethodInvoker(delegate { scoreLBL.Text = "Score: " + new_score.ToString(); }));
            else
                scoreLBL.Text = "Score: " + new_score.ToString();
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
            g.PlayMove(gameboard).GetAwaiter().OnCompleted(
                delegate ()
                {
                    //Thread.Sleep(100);
                    GetGameState();
                });
        }
    }
}
