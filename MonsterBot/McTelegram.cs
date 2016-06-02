using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using MonsterBot;

namespace Telegram
{
    namespace Data
    {
        public class InfoResponse
        {
            public Nullable<bool> ok;
            public Info result;
        }

        public class Info
        {
            public int id;
            public string first_name;
            public string username;
        }

        public class User
        {
            public int id;
            public string first_name;
            public string last_name;
            public string username;
        }

        public class Chat
        {
            public int id;
            public string title;
            public string first_name;
            public string last_name;
            public string username;
        }

        public class PhotoSize
        {
            public string file_id;
            public int width;
            public int height;
            public int file_size;
        }

        public class Audio
        {
            public string file_id;
            public int duration;
            public string mime_type;
            public int file_size;
        }

        public class Document
        {
            public string file_id;
            public PhotoSize thumb;
            public string file_name;
            public string mime_type;
            public int file_size;
        }

        public class Sticker
        {
            public string file_id;
            public int width;
            public int height;
            public PhotoSize thumb;
            public int file_size;
        }

        public class Video
        {
            public string file_id;
            public int width;
            public int height;
            public int duration;
            public PhotoSize thumb;
            public string mime_type;
            public int file_size;
            public string caption;
        }

        public class Contact
        {
            public string phone_number;
            public string first_name;
            public string last_name;
            public string user_id;
        }

        public class Location
        {
            public Nullable<float> longitude;
            public Nullable<float> latitude;
        }

        public class UserProfilePhotosResult
        {
            public Nullable<bool> ok;
            public UserProfilePhotos result;
        }

        public class UserProfilePhotos
        {
            public int total_count;
            public PhotoSize[] photos;
        }

        public class UpdatesResult
        {
            public Nullable<bool> ok;
            public Update[] result;
        }

        public class Update
        {
            public int update_id;
            public Message message;
        }

        public class MessageResult
        {
            public Nullable<bool> ok;
            public Message result;
        }

        public class Message
        {
            public int message_id;
            public User from;
            public int date;
            public Chat chat;
            public User forward_from;
            public int forward_date;
            public string text;
            public Audio audio;
            public Document document;
            public PhotoSize[] photo;
            public Sticker sticker;
            public Video video;
            public Contact contact;
            public Location location;
            public User new_chat_participant;
            public User left_chat_participant;
            public string new_chat_title;
            public PhotoSize[] new_chat_photo;
            public Nullable<bool> delete_chat_photo;
            public Nullable<bool> group_chat_created;
        }
    }

    class BotClient
    {
        string ApiUrl;
        int Offset;

        // Bot info
        public readonly string Name;
        public readonly string Username;
        public readonly int BotId;

        // List of superusers
        public readonly List<int> superusers = new List<int>();

        public BotClient(string apiToken)
        {
            // Create API url
            ApiUrl = "https://api.telegram.org/bot" + apiToken;

            // Check token
            string getMeResponse = Post.SendRequest(ApiUrl + "/getMe");
            Data.InfoResponse info = JsonConvert.DeserializeObject<Data.InfoResponse>(getMeResponse);
            if (info == null)
            {
                throw new Exception("Wrong API token");
            }

            // Set bot info
            Name = info.result.first_name;
            Username = info.result.username;
            BotId = info.result.id;

            // Set offset
            string getUpdatesResponse = Post.SendRequest(ApiUrl + "/getUpdates");
            Data.UpdatesResult data = JsonConvert.DeserializeObject<Data.UpdatesResult>(getUpdatesResponse);
            if (data.result == null || data.result.Length == 0) return;
            Offset = data.result[data.result.Length - 1].update_id + 1;
            getUpdatesResponse = Post.SendRequest(ApiUrl + "/getUpdates?offset=" + Offset);
            while (data.result.Length == 100)
            {
                Offset = data.result[data.result.Length - 1].update_id + 1;
                getUpdatesResponse = Post.SendRequest(ApiUrl + "/getUpdates?offset=" + Offset);
                data = JsonConvert.DeserializeObject<Data.UpdatesResult>(getUpdatesResponse);
                if (data.result == null) return;
            }

            if (data.result.Length > 0)
            {
                Offset = data.result[data.result.Length - 1].update_id + 1;
            }
        }

        /// <summary>
        /// Get new updates
        /// </summary>
        /// <returns>New updates</returns>
        public Data.UpdatesResult GetUpdates()
        {
            string jsonResponse = Post.SendRequest(ApiUrl + "/getUpdates?offset=" + Offset);
            Data.UpdatesResult result = JsonConvert.DeserializeObject<Data.UpdatesResult>(jsonResponse);
            if (result.result.Length > 0)
            {
                Offset = result.result[result.result.Length - 1].update_id + 1;
            }
            return result;
        }

        /// <summary>
        /// Add superuser to bot
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>If user id is not exist returns true</returns>
        public bool SuperuserAdd(int user_id)
        {
            if (!superusers.Contains(user_id))
            {
                superusers.Add(user_id);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Remove superuser from bot
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>If user id is not exist returns true</returns>
        public bool SuperuserRemove(int user_id)
        {
            if (!superusers.Contains(user_id))
            {
                superusers.Remove(user_id);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if user_id is superuser
        /// </summary>
        /// <param name="user_id">User id</param>
        /// <returns>If user_id is superuser returns true</returns>
        public bool IsSuperuser(int user_id)
        {
            if (superusers.Contains(user_id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="chat_id">Chat id where to send message</param>
        /// <param name="text">Message text</param>
        /// <returns>Sent message</returns>
        public Data.Message SendMessage(int chat_id, string text)
        {
            return SendMessage(chat_id, text, false);
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="chat_id">Chat id where to send message</param>
        /// <param name="text">Message text</param>
        /// <param name="disable_web_page_preview">Disable web page preview</param>
        /// <returns>Sent message</returns>
        public Data.Message SendMessage(int chat_id, string text, bool disable_web_page_preview)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("text", text);
            postData.Add("disable_web_page_preview", disable_web_page_preview);

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendMessage", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Reply to message
        /// </summary>
        /// <param name="chat_id">Chat id where to send message</param>
        /// <param name="message_id">Message id on which to respond</param>
        /// <param name="text">Message text</param>
        /// <returns>Sent message</returns>
        public Data.Message SendReply(int chat_id, int message_id, string text)
        {
            return SendReply(chat_id, message_id, text, false);
        }

        /// <summary>
        /// Reply to message
        /// </summary>
        /// <param name="chat_id">Chat id where to send message</param>
        /// <param name="message_id">Message id on which to respond</param>
        /// <param name="text">Message text</param>
        /// <param name="disable_web_page_preview">Disable web page preview</param>
        /// <returns>Sent message</returns>
        public Data.Message SendReply(int chat_id, int message_id, string text, bool disable_web_page_preview)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("text", text);
            postData.Add("disable_web_page_preview", disable_web_page_preview);
            postData.Add("reply_to_message_id", message_id);

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendMessage", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Forward message
        /// </summary>
        /// <param name="chat_id">Chat id where to send message</param>
        /// <param name="from_chat_id">Sender chat id</param>
        /// <param name="message_id">Message id to forward</param>
        /// <returns>Sent message</returns>
        public Data.Message ForwardMessage(int chat_id, int from_chat_id, int message_id)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("from_chat_id", from_chat_id);
            postData.Add("message_id", message_id);

            string jsonResponse = Post.SendRequest(ApiUrl + "/forwardMessage", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Send photo
        /// </summary>
        /// <param name="chat_id">Chat id where to send photo</param>
        /// <param name="photo_path">Photo file path</param>
        /// <returns>Sent message</returns>
        public Data.Message SendPhoto(int chat_id, string photo_path)
        {
            if (!File.Exists(photo_path))
            {
                return null;
            }

            string extention = Path.GetExtension(photo_path);

            // Read photo data
            FileStream fs = new FileStream(photo_path, FileMode.Open, FileAccess.Read);
            byte[] photo_data = new byte[fs.Length];
            fs.Read(photo_data, 0, photo_data.Length);
            fs.Close();

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("photo", new Post.FileParameter(photo_data, "image." + extention, "image/png"));

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendPhoto", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Send audio
        /// </summary>
        /// <param name="chat_id">Chat id where to send audio</param>
        /// <param name="audio_path">Audio file path</param>
        /// <returns>Sent message</returns>
        public Data.Message SendAudio(int chat_id, string audio_path)
        {
            if (!File.Exists(audio_path))
            {
                return null;
            }

            string extention = Path.GetExtension(audio_path);

            // Read audio data
            FileStream fs = new FileStream(audio_path, FileMode.Open, FileAccess.Read);
            byte[] audio_data = new byte[fs.Length];
            fs.Read(audio_data, 0, audio_data.Length);
            fs.Close();

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("audio", new Post.FileParameter(audio_data, "audio." + extention, "audio/ogg"));

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendAudio", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Send document
        /// </summary>
        /// <param name="chat_id">Chat id where to send document</param>
        /// <param name="document_path">Document file path</param>
        /// <returns>Sent message</returns>
        public Data.Message SendDocument(int chat_id, string document_path)
        {
            if (!File.Exists(document_path))
            {
                return null;
            }

            string document_name = Path.GetFileName(document_path);

            // Read document data
            FileStream fs = new FileStream(document_path, FileMode.Open, FileAccess.Read);
            byte[] document_data = new byte[fs.Length];
            fs.Read(document_data, 0, document_data.Length);
            fs.Close();

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("document", new Post.FileParameter(document_data, document_name, "application/octet-stream"));

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendDocument", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Send sticker
        /// </summary>
        /// <param name="chat_id">Chat id where to send sticker</param>
        /// <param name="sticker_path">Webp sticker file path</param>
        /// <returns>Sent message</returns>
        public Data.Message SendSticker(int chat_id, string sticker_path)
        {
            if (!File.Exists(sticker_path))
            {
                return null;
            }

            string extention = Path.GetExtension(sticker_path);

            // Read sticker data
            FileStream fs = new FileStream(sticker_path, FileMode.Open, FileAccess.Read);
            byte[] photo_data = new byte[fs.Length];
            fs.Read(photo_data, 0, photo_data.Length);
            fs.Close();

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("sticker", new Post.FileParameter(photo_data, "sticker." + extention, "image/webp"));

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendSticker", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Send video in mp4 format
        /// </summary>
        /// <param name="chat_id">Chat id where to send video</param>
        /// <param name="video_path">Video file path</param>
        /// <returns>Sent message</returns>
        public Data.Message SendVideo(int chat_id, string video_path)
        {
            if (!File.Exists(video_path))
            {
                return null;
            }

            string extention = Path.GetExtension(video_path);

            // Read video data
            FileStream fs = new FileStream(video_path, FileMode.Open, FileAccess.Read);
            byte[] video_data = new byte[fs.Length];
            fs.Read(video_data, 0, video_data.Length);
            fs.Close();

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("video", new Post.FileParameter(video_data, "video." + extention, "image/webp"));

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendVideo", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }


        /// <summary>
        /// Send location
        /// </summary>
        /// <param name="chat_id">Chat id where to send location</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <returns>Sent message</returns>
        public Data.Message SendLocation(int chat_id, float latitude, float longitude)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("latitude", latitude);
            postData.Add("longitude", longitude);

            string jsonResponse = Post.SendRequest(ApiUrl + "/sendLocation", postData);

            Data.MessageResult messageResult = JsonConvert.DeserializeObject<Data.MessageResult>(jsonResponse);

            try
            {
                return messageResult.result;
            }
            catch
            {
                return new Data.Message();
            }
        }

        /// <summary>
        /// Send chat action
        /// </summary>
        /// <param name="chat_id">Chat id where to send action</param>
        /// <param name="action">Action, for example "send_photo"</param>
        public void SendChatAction(int chat_id, string action)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("chat_id", chat_id);
            postData.Add("action", action);

            Post.SendRequest(ApiUrl + "/sendChatAction", postData);
        }
    }
}
