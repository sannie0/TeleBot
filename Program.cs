using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chat_Bot
{
    class Program
    {
        private static string token { get; set; } = "6213746023:AAHTrekEfyeIgUT211RR83OjqTo4OEb-a9k";
        private static TelegramBotClient botClient;

        static async Task Main(string[] args)
        {
            botClient = new TelegramBotClient(token);
            //new comment
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Bot id: {me.Id}. Bot name: {me.FirstName}.");

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            //Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

            botClient.StopReceiving();
        }

        async private static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                var chatId = e.Message.Chat.Id;
                var message = e.Message.Text;
                if (message.StartsWith("/start"))
                {
                    await botClient.SendTextMessageAsync(chatId,
                        "Привет, я могу посоветовать книги на любой вкус📚\nДля того чтобы начать, отправь жанр той книги, которую ты хочешь прочитать🤓");
                    return;
                }

                var books = await SearchBooks(message);
                if (books.Count == 0)
                {
                    await botClient.SendTextMessageAsync(chatId, $"По запросу '{message}' книг не найдено.");
                    return;
                }

                var reply = "Вот несколько книг, которые могут тебе понравиться:\n\n";
                await botClient.SendTextMessageAsync(chatId, reply);
                var count = 0;
                foreach (var book in books)
                {
                    foreach (var i in book.authors)
                    {
                        if (i.name.ToLower().Contains(message.ToLower()))
                        {
                            var formts = book.formats;
                            await botClient.SendTextMessageAsync(chatId, $"✔️{book.title}\nНачни читать прямо сейчас:\n{formts.textplain}\n{formts.imagejpeg}\n\n");
                            count = 1;
                        }
                    }
                } 
                if (count == 0)
                {
                    await botClient.SendTextMessageAsync(chatId, $"По запросу '{message}' книг не найдено.");
                    return;
                }
                
            }
        }

        private static async Task<List<Result>> SearchBooks(string query)
        {
            var person = await "https://gutendex.com/"
                .AppendPathSegment("books")
                .SetQueryParams(new { languages = "en, fr" })
                .GetJsonAsync<Root>();
            var books = person.results;
            return books;
        }

        public class Author
        {
            public string name { get; set; }
            public int birth_year { get; set; }
            public int death_year { get; set; }
        }

        public class Formats
        {
            [JsonProperty("application/x-mobipocket-ebook")]
            public string applicationxmobipocketebook { get; set; }

            [JsonProperty("application/epub+zip")] public string applicationepubzip { get; set; }

            [JsonProperty("text/html")] public string texthtml { get; set; }

            [JsonProperty("application/octet-stream")]
            public string applicationoctetstream { get; set; }

            [JsonProperty("image/jpeg")] public string imagejpeg { get; set; }

            [JsonProperty("text/plain")] public string textplain { get; set; }

            [JsonProperty("text/plain; charsetus-ascii")]
            public string textplaincharsetusascii { get; set; }

            [JsonProperty("application/rdf+xml")] public string applicationrdfxml { get; set; }

            [JsonProperty("text/html; charsetutf-8")]
            public string texthtmlcharsetutf8 { get; set; }

            [JsonProperty("text/plain; charsetutf-8")]
            public string textplaincharsetutf8 { get; set; }

            [JsonProperty("text/html; charsetiso-8859-1")]
            public string texthtmlcharsetiso88591 { get; set; }

            [JsonProperty("text/plain; charsetiso-8859-1")]
            public string textplaincharsetiso88591 { get; set; }
        }
        
public class Result
        {
            public int id { get; set; }
            public string title { get; set; }
            public List<Author> authors { get; set; }
            public List<Translator> translators { get; set; }
            public List<string> subjects { get; set; }
            public List<string> bookshelves { get; set; }
            public List<string> languages { get; set; }
            public bool copyright { get; set; }
            public string media_type { get; set; }
            public Formats formats { get; set; }
            public int download_count { get; set; }
        }

        public class Root
        {
            public int count { get; set; }
            public string next { get; set; }
            public object previous { get; set; }
            public List<Result> results { get; set; }
        }

        public class Translator
        {
            public string name { get; set; }
            public int? birth_year { get; set; }
            public int? death_year { get; set; }
        }


    }
}