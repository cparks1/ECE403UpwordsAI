# Upwords AI
This repository contains the source code for an Artificial Intelligence capable of playing Hasbro's board game "Upwords". It was created for a project in Lance Fiondella's undergraduate Artificial Intelligence class (ECE 403) at UMass Dartmouth.

The project is pure C# and uses GET/POST requests with JSON encoded data for communications with the tournament adjudicator program. The tournament adjudicator can be found at https://github.com/cparks1/TournamentAdjudicator.

As of now (12/31/16) the code is disorganized and hard to follow. I am now (as of 12/11/17) making some effort to make the code more user friendly. If you have any questions, feel free to contact me.


Opening the AI:

• In Form1.cs, you must change the dictionary path to a valid path to a text file containing words that the AI will consider valid play.
Dictionary format is as such: Words must be capitalized, must contain ONLY letters, and must be separated by new line.
If this is not done, the program will throw an exception for an invalid path to the dictionary file.

Using the AI:

• The 10x10 matrix of boxes in the left hand side of the GUI is the game board. It will contain tiles placed and tell you their stack level. The letter placed is drawn in black. The stack level is drawn in red.

• The row of 7 boxes under the game board is the AI's tile hand. It will contain tiles given to you that can be used for play.

• Clicking on the boxes in the game board or AI tile hand will open a dialogue asking for you to set the tile. Typing a letter and clicking OK will change the tile. If you do this on the game board, the new tile will have a stack level of 0. Clicking cancel will cancel the operation. This change only occurs on client side.

• "Give AI Tiles" button is a client sided operation that will assign your AI tiles from a local tile bag. This is for debugging, not tournament use.

• "Clear AI Tiles" button is a client sided operation that will clear your AI tile hand.

• "AI Place Word" will cause the AI to place a word.

• "AI Play Stack" will cause the AI to make a stack play.

• "AI Play Best" will cause the AI to make the best move it can, whether that be a word placement or a stack play.

• "AI Play All" button will cause the AI to continuously play against itself locally using local methods, using only word placements for 
moves. This is not for tournament use.

• "Reset Game" button will reset the game on the AI's client side. This is not for tournament use.

• "Clear Board" button will clear the board on the AI's client side. This is not for tournament use.

• "AI Cont Play" will cause the AI to call the AI Play All function, wait until the game is played through using only word placements for moves, reset the game, and then call the play all function again continuously.

• "STOP" button will stop AI Cont Play.

• "Join Game" will cause the AI to join a tournament adjudicated game on localhost, at port 62027. The textbox and number selector for URI and Port do not currently do anything.

• "Get State" will cause the AI to request the game state from the tournament adjudicator. The AI will automatically request the game state when joining a game until it is their turn.

• "Send ONE Move" will cause the AI to send the move played to the tournament adjudicator. If doing this by hand, you should first click "AI Place Word", "AI Play Stack", or "AI Play Best", then click "Send ONE Move"

• "Send Move" will cause the AI to send the move played to the tournament adjudicator, and then continuously play using "AI Best Play" until the game has ended. When doing this, you should first click "AI Place Word", "AI Play Stack", or "AI Play Best", then sit back and allow the AI to play til the end of game.

• "TRIGGERED" was used to cause a breakpoint in a function I was trying to write to fire. It currently does nothing.

When using the AI with the tournament adjudicator, you must have at least 2 clients connect to the adjudicator program. Once at least 2 clients have joined, the adjudicator will wait for a cetain amount of time for any new clients to join and then begin the game.

Once the game has begun, the AI will not display its retrieved game state until it is the AI's turn.

Once it is the AI's turn, you may choose which route to go: fully autonomous or semi-autonomous.

At the end of the game, to begin a new game you must close the tournament adjudicator and then close all connected clients. You can then open a new tournament adjudicator and begin the connection process again.


CREDITS:

Credit to Christopher Parks (cparks13@live.com) for AI play logic, GUI, Tournament Adjudicator communications, and other functions.

Credit to William Ryan for scoring logic.

Credit to Samuel Freitas for Tournament Adjudicator communications
