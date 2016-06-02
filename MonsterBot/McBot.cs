using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Telegram.Data;
using Google.Search;
using Google.Image;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;

namespace MonsterBot
{
    class McBot
    {
        static Telegram.BotClient botClient;

        struct Id
        {
            public int chat_id;
            public int from_id;
        }

        class Dialogue
        {
            public Thread thread;
            public List<Telegram.Data.Message> messages;
        }

        public class KonachanItem
        {
            public string file_url;
        }

        // Active sessions array
        static Dictionary<Id, Dialogue> Sessions = new Dictionary<Id, Dialogue>();

        bool StopBot = false;

        public McBot(string token)
        {
            botClient = new Telegram.BotClient(token);
            Console.WriteLine("Bot info:\n");
            Console.WriteLine("Name \t\t{0}", botClient.Name);
            Console.WriteLine("Username \t{0}", botClient.Username);
            Console.WriteLine("Bot id \t\t{0}\n", botClient.BotId);
        }

        public void Start()
        {
            // Your user id
            botClient.SuperuserAdd(80667864);

            MainLoop();
        }

        void MainLoop()
        {
            while (!StopBot)
            {
                // Read updates every 100 ms
                Thread.Sleep(100);
                UpdatesResult updates = botClient.GetUpdates();

                // Skip if there is no new updates
                if (updates.result.Length == 0)
                {
                    continue;
                }

                foreach (Update update in updates.result)
                {
                    Id id = new Id();
                    id.chat_id = update.message.chat.id;
                    id.from_id = update.message.from.id;

                    if (Sessions.ContainsKey(id))
                    {
                        try
                        {
                            Sessions[id].messages.Add(update.message);
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            Dialogue dialogue = new Dialogue();
                            dialogue.messages = new List<Telegram.Data.Message> { update.message };
                            Sessions.Add(id, dialogue);
                            Sessions[id].thread = new Thread(() => ParseMessages(id));
                            Sessions[id].thread.Start();
                        }
                        catch
                        {
                            try
                            {
                                if (Sessions.ContainsKey(id))
                                {
                                    Sessions.Remove(id);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            Environment.Exit(0);
        }

        void LogMessage(Message message, string text)
        {
            string time = DateTime.Now.ToString();
            string username = (message.from.username != null) ? message.from.username : "Null";

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.Write(time);
            Console.Write("] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(message.chat.id);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(":");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(message.from.id);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(username);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(":");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(message.from.first_name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" Cmd:");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(text);
            Console.WriteLine();
            Console.ResetColor();

            string toLog = String.Format("[{0}] {1}:{2} {3}:{4} Cmd: {5}\n", time, message.chat.id, message.from.id, username, message.from.first_name, text);

            bool logged = false;

            while (!logged)
            {
                try
                {
                    File.AppendAllText("log.txt", toLog);
                    logged = true;
                }
                catch
                {
                    // File in use
                }
            }
        }

        void ParseMessages(Id id)
        {
            if (!Sessions.ContainsKey(id))
            {
                return;
            }
            if (Sessions[id].messages[0].text == null)
            {
                Sessions.Remove(id);
                return;
            }

            bool log = true;

            Dialogue dialogue = Sessions[id];
            string message_text = Sessions[id].messages[0].text;
            int space_index = message_text.IndexOf(' ');

            if (!message_text.StartsWith("/"))
            {
                Sessions.Remove(id);
                return;
            }

            if (space_index < message_text.Length && space_index != -1)
            {
                string[] args = SplitString(message_text);
                args[0] = args[0].Replace("@" + botClient.Username, "").ToLower();

                // Handle commands with additional arguments
                switch (args[0])
                {
                    case "/echo": Echo(id, args[1]); break;
                    case "/google": Google(id, args[1]); break;
                    case "/youtube": YouTube(id, args[1]); break;
                    case "/image": Image(id, args[1]); break;
                    case "/math": Math(id, args[1]); break;
                    case "/getfile": GetFile(id, args[1]); break;
                    default:
                        if (id.chat_id > 0)
                        {
                            botClient.SendMessage(id.chat_id, "Unknown command. Use /help to find commands.");
                        }
                        log = false;
                        break;
                }
                if (log)
                {
                    LogMessage(dialogue.messages[0], args[0]);
                }
            }
            else
            {
                string cmd = message_text.Replace("@" + botClient.Username, "").ToLower();

                // Handle commands without arguments
                switch (cmd)
                {
                    case "/ping": Ping(id); break;
                    case "/echo": Echo(id); break;
                    case "/google": Google(id); break;
                    case "/youtube": YouTube(id); break;
                    case "/image": Image(id); break;
                    case "/anime": Anime(id); break;
                    case "/hentai": Hentai(id); break;
                    case "/math": Math(id); break;
                    case "/scr": Scr(id); break;
                    case "/getfile": GetFile(id); break;
                    case "/feedback": Feedback(id); break;
                    case "/info": Info(id); break;
                    case "/source": Source(id); break;
                    case "/help": Help(id); break;
                    case "/start": Start(id); break;
                    case "/stop": Stop(id);  break;
                    default:
                        if (id.chat_id > 0)
                        {
                            botClient.SendMessage(id.chat_id, "Unknown command. Use /help");
                        }
                        log = false;
                        break;
                }
                if (log)
                {
                    LogMessage(dialogue.messages[0], cmd);
                }
            }
            // Close current user-chat session
            Sessions.Remove(id);
        }

        /// <summary>
        /// Gets last message_id from dialogue
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns>Message id</returns>
        int MessageId(Dialogue dialogue)
        {
            return dialogue.messages[dialogue.messages.Count - 1].message_id;
        }

        string[] SplitString(string source)
        {
            int index = source.IndexOf(' ');
            string s1 = "", s2 = "";
            if (index != -1)
            {
                s1 = source.Substring(0, source.IndexOf(' ')).ToLower();
                s2 = source.Substring(index + 1, source.Length - index - 1);
            }
            return new string[] { s1, s2 };
        }

        // Check cancelation
        bool IsCancel(Message message)
        {
            if (message.text != null)
            {
                if (message.text.StartsWith("/cancel"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Wait for new message from user
        void WaitNewMessage(Dialogue dialogue, int index)
        {
            while (dialogue.messages.Count < (index + 1))
            {
                Thread.Sleep(100);
            }
        }

        void ReportCancel(int chat_id)
        {
            botClient.SendMessage(chat_id, "Operation was canceled");
        }

        void ReportAccessError(Id id)
        {
            Dialogue dialogue = Sessions[id];
            botClient.SendReply(id.chat_id, MessageId(dialogue), "You are not superuser");
        }

        string GetCommand(string command)
        {
            return command.Replace("@" + botClient.Username, "").ToLower();
        }

        void Ping(Id id)
        {
            Dialogue dialogue = Sessions[id];
            botClient.SendReply(id.chat_id, dialogue.messages[0].message_id, "pong");
        }

        void Echo(Id id)
        {
            Dialogue dialogue = Sessions[id];
            botClient.SendMessage(id.chat_id, "Type text to repeat:");
            WaitNewMessage(Sessions[id], 1);
            if (IsCancel(dialogue.messages[1]))
            {
                ReportCancel(id.chat_id);
            }
            else
            {
                if (dialogue.messages[1].text != null)
                {
                    Echo(id, dialogue.messages[1].text);
                }
                else
                {
                    botClient.SendReply(id.chat_id, dialogue.messages[1].message_id, "I can repeat text messages only");
                }
            }
        }

        void Echo(Id id, string text)
        {
            botClient.SendMessage(id.chat_id, text);
        }

        void Feedback(Id id)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendMessage(id.chat_id, "Enter your feedback:");
            WaitNewMessage(Sessions[id], 1);
            if (IsCancel(dialogue.messages[1]))
            {
                ReportCancel(id.chat_id);
            }
            else
            {
                botClient.SendMessage(id.chat_id, "Thanks for your feedback!");

                if (botClient.superusers.Count == 0) return;

                foreach (int superuser in botClient.superusers)
                {
                    botClient.SendMessage(superuser, String.Format("You have new feedback from {0}(id{1})", dialogue.messages[0].from.first_name, id.from_id));
                    botClient.ForwardMessage(superuser, id.chat_id, dialogue.messages[1].message_id);
                }
            }
        }

        void Google(Id id)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendMessage(id.chat_id, "What to search:");
            WaitNewMessage(Sessions[id], 1);
            if (IsCancel(dialogue.messages[1]))
            {
                ReportCancel(id.chat_id);
            }
            else
            {
                Google(id, dialogue.messages[1].text);
            }
        }

        void Google(Id id, string text)
        {
            Dialogue dialogue = Sessions[id];

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/web?v=1.0&q=" + text);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string jsonResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();

                GwebResponse gwebResponse = JsonConvert.DeserializeObject<GwebResponse>(jsonResponse);
                if (gwebResponse.responseData.results.Length == 0)
                {
                    botClient.SendReply(id.chat_id,
                    dialogue.messages[dialogue.messages.Count - 1].message_id,
                    String.Format("I can't find this", text));
                }

                string message = string.Empty;
                foreach (GwebResult result in gwebResponse.responseData.results)
                {
                    message += result.titleNoFormatting + Environment.NewLine;
                    message += result.unescapedUrl + Environment.NewLine;
                }
                botClient.SendReply(id.chat_id, MessageId(dialogue), message, true);
            }
            catch
            {
                botClient.SendReply(id.chat_id,
                    dialogue.messages[dialogue.messages.Count - 1].message_id,
                    String.Format("I can't find \"{0}\"", text));
            }
        }

        void Image(Id id)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendMessage(id.chat_id, "What to search:");
            WaitNewMessage(Sessions[id], 1);
            if (IsCancel(dialogue.messages[1]))
            {
                ReportCancel(id.chat_id);
            }
            else
            {
                Image(id, dialogue.messages[1].text);
            }
        }

        void Image(Id id, string text)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendChatAction(id.chat_id, "upload_photo");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/search/images?v=1.0&rsz=5&q=" + text);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string jsonResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();

                GimResponse gimResponse = JsonConvert.DeserializeObject<GimResponse>(jsonResponse);

                if (gimResponse.responseData.results != null)
                {
                    if (!System.IO.Directory.Exists("temp"))
                    {
                        System.IO.Directory.CreateDirectory("temp");
                    }
                    string imgName = "temp\\google_img" + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + ".jpg";
                    int imgPos = new Random().Next(0, gimResponse.responseData.results.Length);
                    var client = new WebClient();
                    client.DownloadFile(gimResponse.responseData.results[imgPos].unescapedUrl, imgName);
                    botClient.SendPhoto(id.chat_id, imgName);
                    File.Delete(imgName);
                }
            }
            catch
            {
                botClient.SendReply(id.chat_id,
                    dialogue.messages[dialogue.messages.Count - 1].message_id,
                    String.Format("I can't find \"{0}\"", text));
            }
        }

        // Show user and chat info
        void Info(Id id)
        {
            Dialogue dialogue = Sessions[id];

            string info = string.Empty;

            if (dialogue.messages[0].from.username != null)
            {
                info += "Username: " + dialogue.messages[0].from.username + Environment.NewLine;
            }
            info += "Name: " + dialogue.messages[0].from.first_name + Environment.NewLine;
            info += "User id: " + id.from_id + Environment.NewLine;
            info += "Chat id: " + id.chat_id + Environment.NewLine;

            botClient.SendReply(id.chat_id, MessageId(dialogue), info);
        }

        void Source(Id id)
        {
            Dialogue dialogue = Sessions[id];
            string response = @"Github link: https://github.com/FloodCode/MonsterBot";
            botClient.SendReply(id.chat_id, MessageId(dialogue), response);
        }

        void Help(Id id)
        {
            Dialogue dialogue = Sessions[id];
            string help =
                @"
Available commands:
/google - search in google
/youtube - search in youtube
/image - search images
/anime - random manga picture
/hentai - random hentai picture (18+)
/math - solve math expression
/info - show info about user
/feedback - leave feedback
/source - get bot sources
/help - show this help";
            botClient.SendReply(id.chat_id, MessageId(dialogue), help);
        }

        void Stop(Id id)
        {
            Dialogue dialogue = Sessions[id];

            if (botClient.IsSuperuser(id.from_id))
            {
                botClient.SendReply(id.chat_id, MessageId(dialogue), "Goodbye!");
                StopBot = true;
            }
            else
            {
                ReportAccessError(id);
            }
        }

        void Konachan(Id id, string tags)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendChatAction(id.chat_id, "upload_photo");

            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("tags", tags);
            postParameters.Add("page", new Random().Next(1, 50));

            string json = Post.SendRequest("https://konachan.com/post.json", postParameters);

            KonachanItem[] items = JsonConvert.DeserializeObject<KonachanItem[]>(json);

            if (items.Length < 1)
            {
                botClient.SendReply(id.chat_id, MessageId(dialogue), "No results");
                return;
            }
            else
            {
                int pos = new Random().Next(0, items.Length);
                if (items[pos].file_url != null)
                {
                    using (var client = new WebClient())
                    {
                        if (!Directory.Exists("temp"))
                        {
                            Directory.CreateDirectory("temp");
                        }
                        string imgName = "temp\\konachan_img" + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + ".jpg";
                        client.DownloadFile(items[pos].file_url, imgName);
                        botClient.SendPhoto(id.chat_id, imgName);
                        if (File.Exists(imgName))
                        {
                            File.Delete(imgName);
                        }
                    }
                }
                else
                {
                    botClient.SendReply(id.chat_id, MessageId(dialogue), "No results");
                    return;
                }
            }
        }

        void Hentai(Id id)
        {
            Konachan(id, "rating:explicit");
        }

        void Anime(Id id)
        {
            Konachan(id, "rating:safe");
        }

        void Start(Id id)
        {
            Dialogue dialogue = Sessions[id];
            botClient.SendReply(id.chat_id, MessageId(dialogue),
                String.Format("Hello, {0}, im {1}! Use /help to check all commands", dialogue.messages[0].from.first_name, botClient.Name));
        }

        void Math(Id id)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendMessage(id.chat_id, "Enter expression:");
            WaitNewMessage(Sessions[id], 1);
            if (IsCancel(dialogue.messages[1]))
            {
                ReportCancel(id.chat_id);
            }
            else
            {
                Math(id, dialogue.messages[1].text);
            }
        }

        void Math(Id id, string text)
        {
            Dialogue dialogue = Sessions[id];

            try
            {
                var result = new DataTable().Compute(text, null);
                botClient.SendReply(id.chat_id, MessageId(dialogue), result.ToString());
            }
            catch
            {
                botClient.SendReply(id.chat_id, MessageId(dialogue), "I can't solve this");
            }
        }

        void Scr(Id id)
        {
            if (!botClient.IsSuperuser(id.from_id))
            {
                ReportAccessError(id);
                return;
            }
            //Create a new bitmap.
            Bitmap bitmap = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                                           System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            Graphics graphics = Graphics.FromImage(bitmap);

            // Take the screenshot from the upper left corner to the right bottom corner.
            graphics.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X,
                                        System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            // Save the screenshot to the specified path that the user has chosen.
            if (!System.IO.Directory.Exists("temp"))
            {
                System.IO.Directory.CreateDirectory("temp");
            }
            bitmap.Save("temp\\scr.png", ImageFormat.Png);

            // Send photo to chat_id
            botClient.SendDocument(id.chat_id, "temp\\scr.png");
        }

        void GetFile(Id id, string path)
        {
            Dialogue dialogue = Sessions[id];

            if (!botClient.IsSuperuser(id.from_id))
            {
                ReportAccessError(id);
                return;
            }
            if (!File.Exists(path))
            {
                botClient.SendReply(id.chat_id, MessageId(dialogue), "File is not exist");
                return;
            }
            else if (((new FileInfo(path).Length) / 1024 / 1024) > 64)
            {
                botClient.SendReply(id.chat_id, MessageId(dialogue), "File is too big. Maximal file size is 64MB");
                return;
            }
            else
            {
                botClient.SendDocument(id.chat_id, path);
            }
        }

        void GetFile(Id id)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendMessage(id.chat_id, "Enter path:");
            WaitNewMessage(Sessions[id], 1);
            if (IsCancel(dialogue.messages[1]))
            {
                ReportCancel(id.chat_id);
            }
            else
            {
                GetFile(id, dialogue.messages[1].text);
            }
        }

        void YouTube(Id id, string query)
        {
            Dialogue dialogue = Sessions[id];

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCP1xdM7xG3u63mChJkHHBrddZ0sXbeds4",
                ApplicationName = "MonsterBot"
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = query; // Replace with your search term.
            searchListRequest.MaxResults = 10;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = searchListRequest.Execute();

            string video_result = string.Empty;

            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    video_result = searchResult.Snippet.Title + "\nhttps://www.youtube.com/watch?v=" + searchResult.Id.VideoId;
                }
            }

            if (video_result != String.Empty)
            {
                botClient.SendReply(id.chat_id, MessageId(dialogue), video_result);
            }
            else
            {
                botClient.SendReply(id.chat_id, MessageId(dialogue), "I can't find this");
            }
        }

        void YouTube(Id id)
        {
            Dialogue dialogue = Sessions[id];

            botClient.SendMessage(id.chat_id, "What to search:");
            WaitNewMessage(Sessions[id], 1);
            if (IsCancel(dialogue.messages[1]))
            {
                ReportCancel(id.chat_id);
            }
            else
            {
                YouTube(id, dialogue.messages[1].text);
            }
        }
    }
}
