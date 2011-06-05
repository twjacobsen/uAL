using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Jayrock;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace uAL
{
    class TorrentAPI
    {
        public string Token
        {
            get
            {
                return token;
            }
        }
        public string Cookie
        {
            get
            {
                return cookie;
            }
        }

        private string host, username, password, token, cookie;
        CredentialCache credentials;
        List<Torrent> torrentCollection;

        public TorrentAPI(string _host, string _userName, string _password)
        {
            host = "http://" + _host + "/gui/";
            username = _userName;
            password = _password;

            credentials = new CredentialCache();
            credentials.Add(new Uri(host), "Basic", new NetworkCredential(username, password));

            GetToken();
            GetTorrents(false);
        }

        private void GetToken()
        {
            HttpWebRequest getTokenRequest = (HttpWebRequest)(HttpWebRequest.Create(host + "token.html"));
            getTokenRequest.KeepAlive = false;
            getTokenRequest.Method = "GET";
            getTokenRequest.Credentials = credentials;

            HttpWebResponse response = (HttpWebResponse)getTokenRequest.GetResponse();

            cookie = response.GetResponseHeader("Set-Cookie");
            cookie = cookie.Substring(0, cookie.IndexOf(';'));

            StreamReader sr = new StreamReader(response.GetResponseStream());
            Regex r = new Regex(".*<div[^>]*id=[\"\']token[\"\'][^>]*>([^<]*)</div>.*");
            Match m = r.Match(sr.ReadToEnd());
            token = m.Result("$1");

            response.Close();
        }

        public string GetDownloadDir()
        {
            string dir = "NA";

            HttpWebRequest getProperties = (HttpWebRequest)(HttpWebRequest.Create(host + "?action=getsettings&token=" + token));
            getProperties.Credentials = credentials;
            getProperties.Headers.Add("Cookie", cookie);
            HttpWebResponse response = (HttpWebResponse)getProperties.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            dir = (string)sr.ReadToEnd();
            response.Close();

            JsonObject res = (JsonObject)JsonConvert.Import(dir);
            JsonArray settings = (JsonArray)res["settings"];
            foreach (JsonArray se in settings)
            {
                string _prop = (string)se[0];
                if(_prop == "dir_autoload")
                    dir = (string)se[2];
            }

            return dir;
        }

        private List<Torrent> GetTorrents(bool forceUpdate)
        {
            HttpWebRequest getTorrent = (HttpWebRequest)(HttpWebRequest.Create(host + "?list=1&token=" + token));
            getTorrent.Credentials = credentials;
            getTorrent.Headers.Add("Cookie", cookie);
            HttpWebResponse response = (HttpWebResponse)getTorrent.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string torrentsResponse = sr.ReadToEnd();
            response.Close();

            JsonObject res = (JsonObject)JsonConvert.Import(torrentsResponse);
            JsonArray torrents = (JsonArray)res["torrents"];

            List<Torrent> returnTorrents = new List<Torrent>();
            if (forceUpdate)
                torrentCollection = new List<Torrent>();
            foreach (JsonArray torrent in torrents)
            {
                string hash = (string)torrent[0];
                string name = (string)torrent[2];
                string label = (string)torrent[11];
                int percentage = (int)torrent[4];


                returnTorrents.Add(new Torrent { Hash = hash, Name = name, Label = label, PercentageDone = percentage });
                if(forceUpdate)
                    torrentCollection.Add(new Torrent { Hash = hash, Name = name, Label = label, PercentageDone = percentage });
            }

            return returnTorrents;
        }

        private List<Torrent> NewTorrents()
        {
            bool addTorrent = true;
            List<Torrent> newTorrents = new List<Torrent>();
            List<Torrent> torrents = GetTorrents(false);
            foreach (Torrent torrent in torrents)
            {
                addTorrent = true;
                foreach(Torrent cachedTorrent in torrentCollection)
                {
                    if (torrent.Hash == cachedTorrent.Hash)
                    {
                        addTorrent = false;
                        break;
                    }
                }
                if (addTorrent)
                    newTorrents.Add(torrent);
            }
            return newTorrents;
        }

        public bool AddTorrent(string fileName, string label)
        {
            //First, get excisting torrents from uTorrent, in case some have been added from outside this service...
            GetTorrents(true);

            Stream TorrentStream = File.OpenRead(fileName);

            HttpWebRequest PostReq = (HttpWebRequest)(HttpWebRequest.Create(host + "?action=add-file&token=" + token));
            PostReq.KeepAlive = false;
            PostReq.Credentials = credentials;
            string guid = Guid.NewGuid().ToString("N");
            PostReq.ContentType = "multipart/form-data; boundary=" + guid;
            PostReq.Method = "POST";
            PostReq.Headers.Add("Cookie", cookie);

            using (BinaryWriter Writer = new BinaryWriter(PostReq.GetRequestStream()))
            {
                byte[] FileBytes = new byte[TorrentStream.Length];
                TorrentStream.Read(FileBytes, 0, FileBytes.Length);

                Writer.Write(Encoding.ASCII.GetBytes(String.Format("--{0}\r\nContent-Disposition: form-data; name=\"torrent_file\"; filename=\"{0}\"\r\nContent-Type: application/x-bittorrent\r\n\r\n", guid, fileName)));
                Writer.Write(FileBytes, 0, FileBytes.Length);
                Writer.Write(Encoding.ASCII.GetBytes(String.Format("\r\n--{0}\r\n", guid)));
            }

            HttpWebResponse response = (HttpWebResponse)PostReq.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string t = sr.ReadToEnd();
            response.Close();

            foreach (Torrent newTorrent in NewTorrents())
            {
                if (newTorrent.Label == "")
                {
                    HttpWebRequest addLabel = (HttpWebRequest)(HttpWebRequest.Create(host + "?action=setprops&token=" + token + "&hash="+newTorrent.Hash+"&s=label&v="+label));
                    addLabel.Credentials = credentials;
                    addLabel.Headers.Add("Cookie", cookie);
                    HttpWebResponse _response = (HttpWebResponse)addLabel.GetResponse();
                    sr = new StreamReader(_response.GetResponseStream());
                    t = sr.ReadToEnd();
                    _response.Close();
                    Console.WriteLine("Added " + fileName + " with label " + label);
                    Console.WriteLine();
                    return true;
                }
            }
            return false;
        }

        public void StopTorrents()
        {

        }
    }

    public class Torrent
    {
        public string Hash;
        public string Name;
        public string Label;
        public int PercentageDone;
    }
}
