using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Net;
using System.Net.Http; // Used for tournament communication
using System.Net.Http.Formatting;
using System.Net.Http.Headers; // Used for tournament communication
using System.Web;
using System.Web.Http;
using System.Threading; // Used for multithreaded tournament communication operations
using System.Threading.Tasks;


namespace UpwordsAI
{
    public class Payload
    {
        public int ID { get; set; }
        public string Hash { get; set; }
        public string[] Letters { get; set; }
        public string[, ,] Board { get; set; }
        public int Turn { get; set; }

        public int Score { get; set; }
        public bool Success { get; set; }
    }
    public class Move
    {
        public string Board { get; set; }
        public string Letters { get; set; }
        public Move(string board, string letters)
        {
            Board = board;
            Letters = letters;

        }
    }
    public class GameNetworkCommuncation
    {

        public HttpClient client = new HttpClient();
        public Payload myPayload = new Payload();

        //this function will join the game for the first time and get the ID and Hash
        async Task<Uri> JoinGame()
        {
            //sends a get request to http://localhost:62027/api/user to join game
            HttpResponseMessage response = await client.GetAsync(client.BaseAddress.AbsoluteUri + "api/user");

            //makes sure the action was performed correctly
            response.EnsureSuccessStatusCode();

            //if it was performed correctly, write the response into the data structure.
            if (response.IsSuccessStatusCode)
            {
                myPayload = await response.Content.ReadAsAsync<Payload>();
            }

            //return location
            return (response.Headers.Location);
        }

        //gets the updated game state from the server and will update the board, letters, and turn
        public async Task<Payload> GetGamestate()
        {
            Payload tempPayload = new Payload();
            client.DefaultRequestHeaders.Clear();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(client.BaseAddress.AbsoluteUri + "api/game/" + myPayload.ID),
                Method = HttpMethod.Get,
            };

            client.DefaultRequestHeaders.Add("Hash", myPayload.Hash);

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string resp = await response.Content.ReadAsStringAsync();
                if (resp.Contains("The game has ended. Final Scores:"))
                {
                    myPayload.Success = false;
                    tempPayload.Success = false;
                }
                tempPayload = await response.Content.ReadAsAsync<Payload>();
                myPayload.Success = true;
                tempPayload.Success = true;
            }

            updatePayload(tempPayload, myPayload);

            return myPayload;

        }

        public async Task<Payload> GetGamestateUntilTurn()
        {
            do await GetGamestate();
            while (myPayload.Turn != myPayload.ID); // Poll for our turn
            return myPayload;
        }

        //helper function to update the payload variable if the data exists
        void updatePayload(Payload a, Payload b)
        {
            if (a.ID != 0)
                b.ID = a.ID;
            if (a.Hash != null)
                b.Hash = a.Hash;
            if (a.Letters != null)
                b.Letters = a.Letters;
            if (a.Board != null)
                b.Board = a.Board;
            if (a.Turn != 0)
                b.Turn = a.Turn;
            if (a.Score != 0)
                b.Score = a.Score;
        }

        //will send the move to the server
        async Task<Payload> SendMove()
        {
            //initialization
            Payload tempPayload = new Payload();
            client.DefaultRequestHeaders.Clear();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(client.BaseAddress.AbsoluteUri + "api/game/" + myPayload.ID),
                Method = HttpMethod.Post,
            };

            //adds data to header
            string board = JsonConvert.SerializeObject(myPayload.Board);
            string letters = JsonConvert.SerializeObject(myPayload.Letters);
            Move move = new Move(board, letters);
            string jmove = JsonConvert.SerializeObject(move);
            client.DefaultRequestHeaders.Add("Hash", myPayload.Hash);
            client.DefaultRequestHeaders.Add("Move", jmove);

            //sends data to server
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                tempPayload = await response.Content.ReadAsAsync<Payload>();
            }

            //update the payload variable
            updatePayload(tempPayload, myPayload);
            return myPayload;

        }

        //send letter exchange to server 
        async Task<Payload> SendExchangeLetters()
        {
            Payload tempPayload = new Payload();

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(client.BaseAddress.AbsoluteUri + "api/game/" + myPayload.ID),
                Method = HttpMethod.Post,
            };

            string Move = JsonConvert.SerializeObject(myPayload.Letters);
            client.DefaultRequestHeaders.Add("Hash", myPayload.Hash);
            client.DefaultRequestHeaders.Add("Move", Move);


            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                tempPayload = await response.Content.ReadAsAsync<Payload>();
            }

            updatePayload(tempPayload, myPayload);

            return myPayload;

            // Deserialize the updated product from the response body.

        }

        public async Task RunAsync(TextBox logboxTB)
        {
            //set up the client to communicate with server
            client.BaseAddress = new Uri("http://localhost:62027/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                //join the game and get ID and Hash
                await JoinGame();

                logboxTB.Text += "I am user number " + myPayload.ID + "\r\nWith hash code " + myPayload.Hash + "\r\n";
                await GetGamestate();

                while (myPayload.Turn != myPayload.ID)
                {
                    await GetGamestate();
                    //Thread.Sleep(100);
                }
                logboxTB.Text += "I got the board state and letters.\r\n";
                logboxTB.Text += "It is currently user " + myPayload.ID.ToString() + "'s turn.\r\n";

                //await SendMove();
            }
            catch (Exception e)
            {
                logboxTB.Text += e.Message + "\r\n";
            }

            Console.ReadLine();
        }

        public async Task PlayMove(int[,] stacklev, char[,] boardlet)
        {
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                {
                    myPayload.Board[1, r, c] = stacklev[r, c].ToString();
                    myPayload.Board[0, r, c] = (boardlet[r, c] == '~') ? null : (boardlet[r, c] == 'Q') ? "Qu" : boardlet[r, c].ToString();
                }
                
            await SendMove();

        }

        public async Task PlayMove(Gameboard gameboard)
        {
            for(int r=0; r<10; r++)
                for(int c=0; c<10; c++)
                {
                    myPayload.Board[1, r, c] = gameboard.board[r, c].stack_value.ToString();
                    myPayload.Board[0, r, c] = (gameboard.board[r, c].IsBlank) ? null : (gameboard.board[r, c].letter_value == 'Q') ? "Qu" : gameboard.board[r, c].letter_value.ToString();
                }

            await SendMove();
        }
    }
}
