// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BronnBot
{
    public class RecommendationDialog : CancelAndHelpDialog
    {
        private readonly MusicRecognizer _luisRecognizer;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private static Dictionary<string, List<Song>> songs;

        // Lists that hold song information from each genre
        private List<Song> rock;
        private List<Song> metal;
        private List<Song> classical;
        private List<Song> jazz;
        private List<Song> pop;
        private List<Song> electronic;

        public RecommendationDialog(MusicRecognizer luisRecognizer, UserState userState)
            : base(nameof(RecommendationDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _luisRecognizer = luisRecognizer;

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                GenreStepAsync,
                RecommendationStepAsync,
                ConfirmationStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

            // Initialize song dictionary to retrive songs based on genre.
            songs = new Dictionary<string, List<Song>>() {
                {"rock", new List<Song>()},
                {"metal", new List<Song>()},
                {"classical", new List<Song>()},
                {"jazz", new List<Song>()},
                {"pop", new List<Song>()},
                {"electronic", new List<Song>()}
            };

            // Initialize song data
            rock = new List<Song> {
                    new Song (
                        "Tame Impala",
                        "Lucidity",
                        "Innerspeaker",
                        "https://upload.wikimedia.org/wikipedia/en/d/dc/Tame_Impala_-_Innerspeaker.png"
                    ),
                    new Song (
                        "King Gizzard and the Lizard Wizard",
                        "Rattlesnake",
                        "Flying Microtonal Banana",
                        "https://img.discogs.com/Dsx2WfNSZ0G3SWPY9wduXsKblqs=/fit-in/300x300/filters:strip_icc():format(jpeg):mode_rgb():quality(40)/discogs-images/R-9885063-1488706851-7952.jpeg.jpg"
                    ),
                    new Song (
                        "Temples",
                        "Sun Structures",
                        "Sun Structures",
                        "https://upload.wikimedia.org/wikipedia/en/5/58/Temples_-_Sun_Structures.png"
                    )
                };
            // Add rock songs
            foreach(Song s in rock) {
                songs["rock"].Add(s);
            }

            // Metal data
            metal = new List<Song> {
                    new Song (
                        "Megadeth",
                        "Lucretia",
                        "Rust in Peace",
                        "https://upload.wikimedia.org/wikipedia/en/d/dc/Megadeth-RustInPeace.jpg"
                    ),
                    new Song (
                        "Annihilator",
                        "Alison Hell",
                        "Alice in Hell",
                        "https://upload.wikimedia.org/wikipedia/en/thumb/3/3a/AnnihilatorAliceInHell.jpg/220px-AnnihilatorAliceInHell.jpg"
                    ),
                    new Song (
                        "Metallica",
                        "One",
                        "...And Justice for All",
                        "https://upload.wikimedia.org/wikipedia/en/b/bd/Metallica_-_...And_Justice_for_All_cover.jpg"
                    )
                };
            // Add metal songs
            foreach(Song s in metal) {
                songs["metal"].Add(s);
            }

            // Classical data
            classical = new List<Song> {
                    new Song (
                        "Chopin",
                        "Etude Op.10 No.3 in E Major",
                        "",
                        "https://m.media-amazon.com/images/I/71DCffFhbYL._SS500_.jpg"
                    ),
                    new Song (
                        "Antonio Vivaldi",
                        "Four Seasons",
                        "",
                        "https://www.baroquemusic.org/19Large.jpg"
                    ),
                    new Song (
                        "Camille Saint-Saëns",
                        "Danse Macabre",
                        "",
                        "https://images-na.ssl-images-amazon.com/images/I/71C2-Lw5wUL._SX355_.jpg"
                    )
                };
            // Add classical songs
            foreach(Song s in classical) {
                songs["classical"].Add(s);
            }

            // Jazz data
            jazz = new List<Song> {
                    new Song (
                        "McCoy Tyner",
                        "When Sunny Gets Blue",
                        "Today and Tomorrow",
                        "https://upload.wikimedia.org/wikipedia/en/e/ec/Today_and_Tomorrow.jpg"
                    ),
                    new Song (
                        "Beegie Adair",
                        "What A Difference A Day Makes",
                        "A Time For Love: Jazz Piano Romance",
                        "https://images-na.ssl-images-amazon.com/images/I/711WxGESziL._SX355_.jpg"
                    ),
                    new Song (
                        "Jack Jezzro",
                        "Wave",
                        "Cocktail Party Bossa Nova",
                        "https://images-na.ssl-images-amazon.com/images/I/81GP9iBusPL._SY355_.jpg"
                    )
                };
            // Add jazz songs
            foreach(Song s in jazz) {
                songs["jazz"].Add(s);
            }

            // Pop data
            pop = new List<Song> {
                    new Song (
                        "Michelle Branch",
                        "Game of Love",
                        "Shaman",
                        "https://upload.wikimedia.org/wikipedia/en/b/b2/Santana_-_Shaman_-_CD_album_cover.jpg"
                    ),
                    new Song (
                        "Khriz y Angel",
                        "Ven Bailalo",
                        "Ven Bailalo (Reggaeton Mix)",
                        "https://m.media-amazon.com/images/I/61YsVHHg-NL._SS500_.jpg"
                    ),
                    new Song (
                        "Foster the People",
                        "Helena Beat",
                        "Torches",
                        "https://upload.wikimedia.org/wikipedia/en/d/d3/Torches_foster_the_people.jpg"
                    )
                };
            // Add pop songs
            foreach(Song s in pop) {
                songs["pop"].Add(s);
            }

            // Electronic data
            electronic = new List<Song> {
                    new Song (
                        "Linea Aspera",
                        "Synapse",
                        "Linea Aspera",
                        "https://img.discogs.com/XDDumr9vpZBh1lcBmxx6BB8yetM=/fit-in/600x600/filters:strip_icc():format(jpeg):mode_rgb():quality(90)/discogs-images/R-3831443-1346116401-7418.jpeg.jpg"
                    ),
                    new Song (
                        "New Order",
                        "Blue Monday",
                        "Substance",
                        "https://images-na.ssl-images-amazon.com/images/I/31RRJ84EK6L.jpg"
                    ),
                    new Song (
                        "Kraftwerk",
                        "Numbers",
                        "Computer World",
                        "https://upload.wikimedia.org/wikipedia/en/a/a6/Kraftwerk_-_Computer_World.png"
                    )
                };
            // Add pop songs
            foreach(Song s in electronic) {
                songs["electronic"].Add(s);
            }
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Console.WriteLine("Beginning new dialog");
            Dictionary<string, string> a = (Dictionary<string, string>)stepContext.Options;
            Console.WriteLine(stepContext.Options);

            if (!_luisRecognizer.IsConfigured || (stepContext.Options != null && a.Count > 0))
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
            
            // Initialize the genre to be empty, then greet the user.
            stepContext.Values["genre"] = "";
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("How are you doing today?"),
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> GenreStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Check whether the user greeted back or specified an emotion/genre of interest.
            var luisResult = await _luisRecognizer.RecognizeAsync<RecognizerResult>(stepContext.Context, cancellationToken);
            JObject sentiment = (JObject)luisResult.Properties["sentiment"];
            string label = sentiment.GetValue("label").ToString();
            string genre = "";
            Dictionary<string, string> a = (Dictionary<string, string>)stepContext.Options;

            if (stepContext.Options != null && !a["g"].Equals(""))
            {
                stepContext.Values["genre"] = a["g"];
                return await stepContext.NextAsync(null, cancellationToken);
            } else  {
                stepContext.Values["genre"] = "";
            }

            // If the user gives back a neutral greeting
            if (luisResult.GetTopScoringIntent().intent.Equals("Recommendation")) 
            {
                if (luisResult.Entities.ContainsKey("genre"))
                {
                    Console.WriteLine("The found genre is: " + luisResult.Entities["genre"].First);
                    // If the user expresses an adjective, translate it into a valid dictionary key
                    switch (luisResult.Entities["genre"].First.ToString()) {
                        case "upbeat": 
                            genre = "pop";
                            break;
                        case "relaxing":
                            genre = "jazz";
                            break;
                        case "calm":
                            genre = "classical";
                            break;
                        default:
                            genre = luisResult.Entities["genre"].First.ToString();
                            break;
                    }
                    Console.WriteLine("After processing: " + genre);
                }
                else 
                {
                    // If the user requested a recommendation but did not specify a genre.
                    // Use the computed sentiment to display appropiate message
                    if (label.Equals("positive"))
                    {
                        string messageText = "That's great to hear!";
                        var message = MessageFactory.Text(messageText);
                        await stepContext.Context.SendActivityAsync(message, cancellationToken);
                        genre = "pop";
                    }
                    else
                    {
                        string messageText = "Sorry to hear that. Let me try to help.";
                        var message = MessageFactory.Text(messageText);
                        await stepContext.Context.SendActivityAsync(message, cancellationToken);
                        genre = "jazz";
                    }
                }

                // Set the genre and go to recommendation step
                stepContext.Values["genre"] = genre;
                return await stepContext.NextAsync(null, cancellationToken);
            }
            // If the user did not specify a genre, prompt them for one.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What kind of music would you like me to recommend?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "rock", "metal", "classical", "jazz", "pop", "electronic"}),
                }, cancellationToken);
        }
        private static Attachment CreateAdaptiveCardAttachment(string artist, string name, string album, string imgUrl)
        {
            string path = Path.Combine(".", "Resources", "songCard.json");
            var adaptiveCardJson = File.ReadAllText(path);
            dynamic card = JObject.Parse(adaptiveCardJson);

            // Update the template card with the song data
            card.body[0].columns[0].items[0].text = "Artist: " + artist;
            card.body[0].columns[0].items[1].text = "Song: " + name;
            card.body[0].columns[0].items[2].text = "Album: " + album;
            card.body[0].columns[1].items[0].url = imgUrl;

            //  Update the link to display song info in the browser
            string[] artistSplit = artist.Split(' ');
            string[] songSplit = name.Split(' ');
            string base_url = "https://www.google.com/search?q=";

            // Concatenate song info to base Google search URL
            foreach(string w in artistSplit) {
                base_url += w + "+";
            }
            foreach(string w in songSplit) {
                base_url += w + "+";
            }
            card.body[0].columns[0].items[3].items[0].actions[0].url = base_url.Substring(0, base_url.Length - 2);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };
            return adaptiveCardAttachment;
        }
        private static async Task<DialogTurnResult> RecommendationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string Genre;
            // Check if the genre has been set, if not, retrieve the choice prompt result
            if (stepContext.Values["genre"].Equals("")) {
                Genre = ((FoundChoice)stepContext.Result).Value;
                stepContext.Values["genre"] = Genre;
            } else {
                Genre = stepContext.Values["genre"].ToString();
            }

            // Retrieve random song from the given genre
            Random rand = new Random();
            int genreCount = rand.Next(songs[Genre].Count);
            Song song = songs[Genre][genreCount];

            string messageText = "I recommend listening to:";
            var message = MessageFactory.Text(messageText);
            await stepContext.Context.SendActivityAsync(message, cancellationToken);
            
            // // Show card for this song
            var cardAttachment = CreateAdaptiveCardAttachment(song.artist, song.name, song.album, song.url);
            var response = MessageFactory.Attachment(cardAttachment);

            await stepContext.Context.SendActivityAsync(response, cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Did you enjoy my recommendation?") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
            Dictionary<string, string> options;
            if ((bool) stepContext.Result) {
                string messageText = "Glad you enjoyed it!";
                await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(messageText),
                }, cancellationToken);

                //return await stepContext.ReplaceDialogAsync(nameof(RecommendationDialog));
                options = new Dictionary<string, string>() {
                    {"g", stepContext.Values["genre"].ToString()}
                };

                return await stepContext.BeginDialogAsync(nameof(RecommendationDialog), options, cancellationToken);
            } 
            else
            {
                string messageText = "Sorry to hear that.";
                await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(messageText),
                }, cancellationToken);

                //return await stepContext.ReplaceDialogAsync(nameof(RecommendationDialog));
                options = new Dictionary<string, string>() {
                    {"g", ""}
                };
                return await stepContext.BeginDialogAsync(nameof(RecommendationDialog), options, cancellationToken);
            }
        }
    }
}
