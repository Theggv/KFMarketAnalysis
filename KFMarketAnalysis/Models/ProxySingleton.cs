using KFMarketAnalysis.Models.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models
{
    public class ProxySingleton
    {
        private static ProxySingleton instance;

        private static List<Address> proxyList = new List<Address>();

        private static int index;
        private static bool isUse = false;

        private static string dirPath = "temp/proxy";
        private static string proxyListPath = $"{dirPath}/proxy_list.txt";
        private static string actualProxyPrimaryListPath = $"{dirPath}/actual_proxy_primary.txt";
        private static string actualProxySecondaryListPath = $"{dirPath}/actual_proxy_secondary.txt";


        public event EventHandler OnProxyTested;
        public event EventHandler OnTestStarted;
        public event EventHandler OnTestCompleted;
        public event EventHandler OnListLoading;
        public event EventHandler OnListLoaded;


        public WebProxy Proxy { get; set; }

        public int Count => proxyList.Count;

        public int TestedCount => proxyList.Where(x => x.IsValid != null).Count();

        public bool CanUse
        {
            get => IsTesting ? 
                false : (Count > 0 ? isUse : false);
            set => isUse = value;
        }

        public bool IsTesting { get; set; }


        private ProxySingleton()
        {
            Proxy = new WebProxy();

            CreateTempDirectory();

            LoadProxyList();
        }


        public static ProxySingleton GetInstance()
        {
            if (instance == null)
            {
                instance = new ProxySingleton();

                instance.ProxyTest();
            }

            if (proxyList.Count == 0)
                instance.CanUse = false;
            else
                instance.Proxy = new WebProxy(proxyList[index].Ip, proxyList[index].Port);

            return instance;
        }

        public static ProxySingleton GetInstanceNext()
        {
            if (instance == null)
            {
                instance = new ProxySingleton();

                instance.ProxyTest();
            }

            if (proxyList.Count == 0)
                instance.CanUse = false;
            else
            {
                if (index < 0)
                    index = 0;

                instance.Proxy = new WebProxy(proxyList[index].Ip, proxyList[index].Port);

                index++;

                if (index >= proxyList.Count)
                    index = 0;
            }

            return instance;
        }


        public void RemoveCurrent()
        {
            proxyList.RemoveAt(index);

            if (index >= proxyList.Count)
                index = 0;

            instance.Proxy = new WebProxy(proxyList[index].Ip, proxyList[index].Port);
        }

        private void LoadProxyList()
        {
            OnListLoading?.Invoke(this, new EventArgs());

            HttpWebRequest request = (HttpWebRequest)WebRequest
                      .Create("https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list.txt");

            HttpWebResponse response = RequestsUtil.GetResponse(request);

            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);

            string content = streamReader.ReadToEnd();

            if (!File.Exists(proxyListPath))
            {
                FileStream f = new FileStream(proxyListPath, FileMode.Create);
                StreamWriter sw = new StreamWriter(f);

                sw.Write(content);

                sw.Close();
                f.Close();
            }
            else
            {
                FileStream f = new FileStream(proxyListPath, FileMode.Open);
                StreamReader sr = new StreamReader(f);

                var fileContent = sr.ReadToEnd();

                sr.Close();
                
                if (fileContent != content)
                {
                    f = new FileStream(proxyListPath, FileMode.Create);
                    StreamWriter sw = new StreamWriter(f);

                    sw.Write(content);

                    sw.Close();
                    f.Close();
                }
                else
                {
                    LoadActualProxy();
                    return;
                }
            }

            var ipPattern = @"(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}:\d{1,5})\s.{2,7}\s\+";
            var regex = new Regex(ipPattern);

            var match = regex.Match(content);

            var matches = new List<string>();

            while (match.Success)
            {
                matches.Add(match.Groups[1].Value);

                match = match.NextMatch();
            }

            foreach (var item in matches)
            {
                var buf = item.Split(':');

                proxyList.Add(new Address(buf[0], int.Parse(buf[1])));
            }

            OnListLoaded?.Invoke(this, new EventArgs());
        }

        private void ProxyTest()
        {
            return;

            string address;

            bool isPrimary = !File.Exists(actualProxyPrimaryListPath);

            if (isPrimary)
                address = "https://steamcommunity.com/";
            else
                address = RequestBuilder.PriceRequest("Street%20Punks%20Bandana%20|%20Precious");

            Task.Run(() =>
            {
                OnTestStarted?.Invoke(this, new EventArgs());
                IsTesting = true;

                Parallel.ForEach(proxyList, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 10
                }, (item) =>
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest
                        .Create(address);

                    request.Proxy = new WebProxy(item.Ip, item.Port);
                    request.Timeout = 5000;

                    HttpWebResponse response;

                    try
                    {
                        response = (HttpWebResponse)request.GetResponse();
                    }
                    catch(Exception e)
                    {
                        item.IsValid = false;

                        OnProxyTested?.Invoke(this, new EventArgs());

                        return;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                        item.IsValid = false;
                    else
                        item.IsValid = true;

                    response.Close();

                    OnProxyTested?.Invoke(this, new EventArgs());
                });

                proxyList = proxyList.Where(item => item.IsValid == true).ToList();

                // save actual proxy list
                SaveActualProxy(isPrimary);

                IsTesting = false;
                OnTestCompleted?.Invoke(this, new EventArgs());

                if (isPrimary)
                    ProxyTest();
            });
        }

        private void LoadActualProxy()
        {
            FileStream f;

            if (!File.Exists(actualProxySecondaryListPath))
                f = new FileStream(actualProxyPrimaryListPath, FileMode.Open);
            else
                f = new FileStream(actualProxySecondaryListPath, FileMode.Open);
            
            StreamReader sr = new StreamReader(f);

            string[] address;

            while (!sr.EndOfStream)
            {
                address = sr.ReadLine().Split(':');

                proxyList.Add(new Address(address[0], int.Parse(address[1])));
            }

            sr.Close();
            f.Close();
        }

        private void SaveActualProxy(bool isPrimary)
        {
            FileStream f = new FileStream(isPrimary ? 
                actualProxyPrimaryListPath : actualProxySecondaryListPath, FileMode.Create);

            StreamWriter sw = new StreamWriter(f);

            foreach (var item in proxyList)
            {
                sw.WriteLine($"{item.Ip}:{item.Port}");
            }

            sw.Close();
            f.Close();
        }
        
        private void CreateTempDirectory()
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        sealed class Address
        {
            public string Ip { get; set; }

            public int Port { get; set; }

            public bool? IsValid { get; set; } = null;

            public Address(string ip, int port)
            {
                Ip = ip;
                Port = port;
            }
        }
    }
}
