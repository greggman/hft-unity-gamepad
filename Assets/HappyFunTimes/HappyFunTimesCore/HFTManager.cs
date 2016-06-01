/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using DeJson;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace HappyFunTimes
{
    public class HFTManager {

        public event EventHandler<EventArgs> OnReady;
        public event EventHandler<EventArgs> OnFail;

        public HFTManager()
        {
        }

        public void Start(HFTRuntimeOptions options, GameObject gameObject)
        {
            m_options = options;
            m_gameObject = gameObject;

            if (options.startServer)
            {
                StartServer();
            }

            StartCheck();
        }

        void StartCheck()
        {
            m_check = m_gameObject.AddComponent<HFTCheck>();
            m_check.Init(m_options.url, Ready, Failed);
        }

        void CleanupCheck()
        {
            if (m_check != null)
            {
                Component.Destroy(m_check);
                m_check = null;
            }
        }

        public void Stop()
        {
            StopServer();
        }

        void Ready()
        {
            CleanupCheck();

            EventHandler<EventArgs> handler = OnReady;
            if (handler != null) {
                handler(this, new EventArgs());
            }
        }

        void Failed()
        {
            CleanupCheck();
            EventHandler<EventArgs> handler = OnFail;
            if (handler != null) {
                handler(this, new EventArgs());
            }
        }

        #if UNITY_STANDALONE_OSX
        bool FileExistsAndSame(string filename, string shaFilename, string sha256)
        {
            if (System.IO.File.Exists(filename))
            {
                if (System.IO.File.Exists(shaFilename))
                {
                    string oldSha = HFTUtil.ReadText(shaFilename);
                    return sha256.Equals(oldSha);
                }
            }
            return false;
        }

        TextAsset LoadTextAsset(string name)
        {
            TextAsset asset = Resources.Load<TextAsset>(name);
            if (asset == null)
            {
                m_log.Tell("missing resource: " + name + ".bytes");
            }
            return asset;
        }

        void StartExternalServer(bool admin)
        {
            TextAsset serverData = LoadTextAsset("HFTOSXServer");
            TextAsset shaData = LoadTextAsset("HFTOSXServer.sha256");
            if (serverData == null || shaData == null)
            {
                return;
            }

            string serverPath = Application.persistentDataPath + "/hft-server";
            string shaPath = Application.persistentDataPath + "/hft-server.sha";

            if (!FileExistsAndSame(serverPath, shaPath, shaData.text))
            {
                HFTUtil.WriteBytes(serverPath, serverData.bytes);
                HFTUtil.WriteText(shaPath, shaData.text);
                m_log.Info("wrote new webserver: " + serverPath + ", size: " + serverData.bytes.Length);
            }

string cmdString = @"-e '
set myFile to quoted form of ""%(serverPath)s""
do shell script ""chmod -R 770 "" & myFile
do shell script myFile %(admin)s
'";
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["serverPath"] = serverPath;
            dict["admin"] = admin ? "with administrator privileges" : "";
            string script = HFTUtil.ReplaceParamsFlat(cmdString, dict);

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo psi = p.StartInfo;
            psi.EnvironmentVariables.Add("HFT_ARGS", Serializer.Serialize(m_options));
            psi.UseShellExecute = false;
            psi.FileName = "/usr/bin/osascript";
            psi.Arguments = script;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    m_log.Tell("webserver: stderr: " + e.Data);
                }
            });
            p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    m_log.Info("webserver: stdout: " + e.Data);
                }
            });
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            if (p.WaitForExit(2000)) {
                // If it exited there was an error
                m_log.Tell("error starting webserver: exit code = " + p.ExitCode);
            }
            m_webServerProcess = p;
        }
        #endif

        public void StartServer()
        {
            m_log.Info("Start Server");
            // Where should this be checked?
            string controllerPath = "/" + m_options.controllerFilename;
            if (!HFTWebFileDB.GetInstance().FileExists(controllerPath))
            {
                throw new System.ArgumentException(
                    "\"Assets/WebPlayerTemplates/HappyFunTimes" + controllerPath + "\" does not exist. Did you forget to set \"controllerFilename\" in your \"PlayerSpawner\" or \"PlayerConnector\"?");
            }

            #if UNITY_STANDALONE_OSX
            // TODO make 2 classes, one for running internal server, one for external?
            if (m_options.dns || m_options.installationMode)
            {
                StartExternalServer(true);
                return;
            }
            #endif

            List<string> addresses = new List<string>();
            addresses.Add("http://[::0]:18679");
//            addresses.Add("http://0.0.0.0:18679");

            if (m_options.installationMode)
            {
                addresses.Add("http://[::0]:80");
//                addresses.Add("http://0.0.0.0:80");
            }
            else
            {
                var hftOptions = new HFTSite.Options();
                //hftOptions.port = ??
                HFTUtil.SetIfNotNullOrEmpty(m_options.rendezvousUrl, ref hftOptions.rendezvousUrl);
                HFTUtil.SetIfNotNullOrEmpty(m_options.serverPort, ref hftOptions.port);
                m_hftSite = m_gameObject.AddComponent<HFTSite>();
                m_hftSite.Init(hftOptions);
            }

            string ipv4Address = HFTIpUtils.GetLocalIPv4Address();
            string ipv6Address = HFTIpUtils.GetLocalIPv6Address();

            m_webServer = new HFTWebServer(m_options, addresses.ToArray());
            m_webServer.Start();

            if (m_options.dns || m_options.installationMode)
            {
                m_dnsRunner = new HFTDnsRunner();
                m_dnsRunner.Start(ipv4Address, ipv6Address, 53);
            }
        }

        public void StopServer()
        {
            CleanupCheck();
            if (m_webServerProcess != null)
            {
                m_webServerProcess.Kill();
            }
            if (m_hftSite != null)
            {
                m_hftSite.Stop();
                Component.Destroy(m_hftSite);
                m_hftSite = null;
            }
            if (m_webServer != null)
            {
                m_webServer.Stop();
                m_webServer = null;
            }
            if (m_dnsRunner != null)
            {
                m_dnsRunner.Stop();
                m_dnsRunner = null;
            }
        }

        HFTLog m_log = new HFTLog("HFTManager");
        HFTRuntimeOptions m_options;
        GameObject m_gameObject;
        HFTSite m_hftSite;
        HFTCheck m_check;
        HFTWebServer m_webServer;
        HFTDnsRunner m_dnsRunner;
        System.Diagnostics.Process m_webServerProcess;
    }
}  // namaspace HappyFunTimes
