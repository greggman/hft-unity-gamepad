using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace HappyFunTimes {
    public class HFTWebServerUtils {

        public HFTWebServerUtils(string gamePath)
        {
            m_gamePath = gamePath;
        }

        public void SendJsonBytes(HttpListenerResponse res, byte[] data)
        {
            res.StatusCode = (int)HttpStatusCode.OK;
            res.ContentType = "application/json";
            res.ContentEncoding = System.Text.Encoding.UTF8;
            res.AddHeader("Access-Control-Allow-Origin", "*");
            res.WriteContent(data);
        }

        public void SendContent(HttpListenerResponse res, string path, string content)
        {
            SendContent(res, path, System.Text.Encoding.UTF8.GetBytes(content));
        }

        public void SendContent(HttpListenerResponse res, string path, byte[] content)
        {
            string mimeType = HFTMimeType.GetMimeType(path);
            res.ContentType = mimeType;
            if (mimeType.StartsWith("text/") ||
                mimeType == "application/javascript")
            {
                res.ContentEncoding = System.Text.Encoding.UTF8;
            }
            res.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            res.AddHeader("Pragma",        "no-cache");                            // HTTP 1.0.
            res.AddHeader("Expires",       "0");                                   // Proxies.
            res.WriteContent(content);
        }

        public bool GetGameFile(string path, out byte[] content)
        {
            bool found = HFTWebFileDB.GetInstance().GetFile(path, out content);
            if (!found)
            {
                if (path.StartsWith(m_gamePath))
                {
                    path = path.Substring(m_gamePath.Length - 1);
                    found = HFTWebFileDB.GetInstance().GetFile(path, out content);
                }
            }
            return found;
        }

        public bool SendFile(string path, HttpListenerRequest req, HttpListenerResponse res)
        {
            byte[] content = null;
            if (!GetGameFile(path, out content))
            {
                return false;
            }

            SendContent(res, path, content);
            return true;
        }

        private string m_gamePath;
    }
}
