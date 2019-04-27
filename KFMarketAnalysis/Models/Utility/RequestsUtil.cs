using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models.Utility
{
    public static class RequestsUtil
    {
        private const string imageUrl = "https://steamcommunity-a.akamaihd.net/economy/image";

        public static void OpenInBrowser(IMarketItem item)
        {
            Process.Start($"https://steamcommunity.com/market/listings/232090/{item.Name}");
        }

        public static BitmapImage GetImageLootBox(string code, string name)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    var path = NormalizeName(name);

                    if(!File.Exists(path))
                        client.DownloadFile(new Uri($"{imageUrl}/{code}"), path);

                    var imageUri = new Uri($"{Environment.CurrentDirectory}/{path}", UriKind.Absolute);

                    var bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = imageUri;
                    bitmapImage.EndInit();

                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
            catch (Exception e)
            {
                Logging.AddToLog(nameof(GetImageItem), e);
            }

            return null;
        }

        public static BitmapImage GetImageItem(string code, string name, string lootBoxName)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    var path = NormalizeName(name, lootBoxName);

                    if (!File.Exists(path))
                        client.DownloadFile(new Uri($"{imageUrl}/{code}"), path);

                    var imageUri = new Uri($"{Environment.CurrentDirectory}/{path}", UriKind.Absolute);

                    var bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = imageUri;
                    bitmapImage.EndInit();

                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
            catch (Exception e)
            {
                Logging.AddToLog(nameof(GetImageItem), e);
            }

            return null;
        }

        public static double GetPrice(string name)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest
                  .Create(RequestBuilder.PriceRequest(name));

            if (ProxySingleton.GetInstance().CanUse)
                request.Proxy = ProxySingleton.GetInstanceNext().Proxy;

            HttpWebResponse response = GetResponse(request);

            if (response == null)
                return -2;

            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);

            string content = streamReader.ReadToEnd();

            response.Close();

            JSONObject json = JSONParser.Parse(content);

            if (json.GetValue("success").ToString() == "false")
                return -2;

            return Converters.ConvertToDouble(json.GetValue("lowest_price")?.ToString());
        }

        public static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception e)
            {
                if (ProxySingleton.GetInstance().CanUse)
                    ProxySingleton.GetInstance().RemoveCurrent();

                // Add to log file exception data
                Logging.AddToLog(nameof(GetPrice), e);

                return null;
            }

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            return response;
        }

        public static async Task<HttpWebResponse> GetResponseAsync(HttpWebRequest request)
        {
            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch (Exception e)
            {
                // Add to log file exception data
                Logging.AddToLog(nameof(GetPrice), e);

                return null;
            }

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            return response;
        }

        private static string NormalizeName(string name)
        {
            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");

            if (!Directory.Exists("temp/img"))
                Directory.CreateDirectory("temp/img");

            char[] forgottenChars = { '.', '|', '#', '<', '>', '/', '\\', '\"', ':', '*', '?' };

            foreach (var ch in forgottenChars)
            {
                name = name.Replace(ch.ToString(), "");
            }

            return $"temp/img/{name.Replace(" ", "_")}.png";
        }

        private static string NormalizeName(string name, string lootBox)
        {
            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");

            if (!Directory.Exists("temp/img"))
                Directory.CreateDirectory("temp/img");

            char[] forgottenChars = { '.', '|', '#', '<', '>', '/', '\\', '\"', ':', '*', '?' };

            foreach (var ch in forgottenChars)
            {
                name = name.Replace(ch.ToString(), "");
                lootBox = lootBox.Replace(ch.ToString(), "");
            }

            lootBox = lootBox.Replace(" ", "_");

            if(!Directory.Exists($"temp/img/{lootBox}"))
                Directory.CreateDirectory($"temp/img/{lootBox}");

            return $"temp/img/{lootBox}/{name.Replace(" ", "_")}.png";
        }
    }
}
