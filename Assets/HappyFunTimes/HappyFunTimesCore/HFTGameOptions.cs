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

using System;
using System.Collections.Generic;
using DeJson;

namespace HappyFunTimes
{

    public class HFTGameOptions
    {
        ///<summary>
        /// there's generally no need to set this.
        ///</summary>
        public string gameId;

        /// <summary>
        /// Name of game (shown if more than one game running on WiFi)
        /// </summary>
        public string name;

        ///<summary>
        /// id used for multi-player games. Can be set from command line with --hft=id=someid
        ///</summary>
        public string id;

        /// <summary>
        /// As the user for a name when they start
        /// </summary>
        public bool askUserForName = true;

        ///<summary>
        ///Deprecated and not used.
        ///</summary>
        public string controllerUrl;

        ///<summary>
        ///true allows multiple games to run as the same id. Default: false
        ///
        ///normally when a second game connects the first game will be disconnected
        ///as it's assumed the first game probably crashed or for whatever reason did
        ///not disconnect and this game is taking over. Setting this to true doesn't
        ///disconnect the old game. This is needed for multi-machine games.
        ///</summary>
        public bool allowMultipleGames;   // allow multiple games

        ///<summary>
        ///For a multiple machine game designates this game as the game where players start.
        ///Default: false
        ///Can be set from command line with --hft-master
        ///</summary>
        public bool master;

        ///<summary>
        ///The URL of HappyFunTimes
        ///
        ///Normally you don't need to set this as HappyFunTimes is running on the same machine
        ///as the game. But, for multi-machine games you'd need to tell each machine the address
        ///of the machine running HappyFunTimes. Example: "ws://192.168.2.9:18679".
        ///
        ///Can be set from the command line with --hft-url=someurl
        ///</summary>
        public string url;

        ///<summary>
        ///Normally if a game disconnets all players are also disconnected. This means
        ///they'll auto join the next game running.
        ///Default: true
        ///</summary>
        public bool disconnectPlayersIfGameDisconnects;

        ///<summary>
        ///Prints all the messages in and out to the console.
        ///</summary>
        public bool showMessages;

        /// <summary>
        /// Uploads the controller files from Unity to HappyFunTimes.
        /// Normally the controller files are served from disk read
        /// directly by HappyFunTimes
        /// </summary>
        public bool uploadControllerFiles = false;

        ///<summary>
        /// don't set this. it will be set automatically
        ///</summary>
        public string cwd;

        /// <summary>
        /// Files for the Controller.
        /// The key is the path to the file relative to
        /// `Assets/WebPlayerTemplates/HappyFunTimes` so
        /// for example
        ///
        ///     options.files["controller.html"] = controllerHTMLContent;
        ///     options.files["scripts/controller.js"] = controllerJSContent;
        ///     options.files["css/controller.css"] = controllerCSSContent;
        ///
        /// This allows the game to provide all the files HappyFunTimes
        /// to server to the controller so that no external files are
        /// needed.
        /// </summary>
        public Dictionary<string, string> files = new Dictionary<string, string>();

        /// <summary>
        /// whether or not to show in list of games
        /// </summary>
        public bool showInList = true;

        /// <summary>
        /// Whether or not to start the server
        ///
        /// By default it's true unless HFT_URL is set then it's false;
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool startServer = true;

        /// <summary>
        /// URL of rendezvous server. Defaults to
        /// http://happyfuntimes.net/api/inform2. If you're running your
        /// own server you'd change this.
        /// </summary>
        public string rendezvousUrl;

        /// <summary>
        /// Port to run server on
        /// </summary>
        public string serverPort = "";

        public string getGameId()
        {
            return String.IsNullOrEmpty(gameId) ? "HFTUnity" : gameId;
        }
    }

}  // namespace HappyFunTimes