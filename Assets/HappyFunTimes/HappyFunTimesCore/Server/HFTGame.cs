﻿using DeJson;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;

namespace HappyFunTimes
{
    public class HFTGame
    {
        public string id
        {
            get {
                return id_;
            }
        }
        public delegate void UntypedCmdEventHandler(Deserializer deserializer, string id, object data);
        public delegate void TypedCmdEventHandler<T>(string id, T eventArgs);
        private delegate bool CmdEventHandler(Deserializer deserializer, string id, object data);

        private class CmdConverter<T>
        {
            public CmdConverter(HFTLog log, TypedCmdEventHandler<T> handler)
            {
                m_log = log;
                m_handler = handler;
            }

            public bool Callback(Deserializer deserializer, string id, object data)
            {
                try
                {
                    T t = deserializer.Deserialize<T>(data);
                    m_handler(id, t);
                    return true;
                }
                catch (System.Exception ex)
                {
                    m_log.Error(ex.ToString());
                    return false;
                }
            }

            HFTLog m_log;
            TypedCmdEventHandler<T> m_handler;
        }

        private class UntypedCmdConverter
        {
            public UntypedCmdConverter(HFTLog log, UntypedCmdEventHandler handler)
            {
                m_log = log;
                m_handler = handler;
            }

            public bool Callback(Deserializer deserializer, string id, object data)
            {
                try
                {
                    m_handler(deserializer, id, data);
                    return true;
                }
                catch (System.Exception ex)
                {
                    m_log.Error(ex.ToString());
                    return false;
                }
            }

            HFTLog m_log;
            UntypedCmdEventHandler m_handler;
        }

        public void RegisterCmdHandler<T>(string name, TypedCmdEventHandler<T> callback)
        {
            CmdConverter<T> converter = new CmdConverter<T>(log_, callback);
            messageHandlers_[name] = converter.Callback;
        }

        public HFTGame(string id, HFTGameGroup group, HFTRuntimeOptions options)
        {
            id_ = id;
            gameGroup_ = group;
            options_ = options;

            SetGameId();

            log_ = new HFTLog("Game " + gameId_);
            log_.Info("created game");
        }

        public string GetControllerUrl(string baseUrl)
        {
//            return runtimeInfo_ ? baseUrl + "/games/" + runtimeInfo_.info.happyFunTimes.gameId + "/index.html" : null;
            return "fix this!";
        }

        public void SetGameId()
        {
            //this.gameId = (this.runtimeInfo ? this.runtimeInfo.info.happyFunTimes.gameId : "") + " id=" + this.id;
            gameId_ = " id=" + id_;
        }

        public bool HasClient()
        {
            return client_ != null;
        }

        public bool ShowInList()
        {
            return options_.showInList;
        }

        public int GetNumPlayers()
        {
            return players_.Count;
        }

        public void AddPlayer(HFTPlayer player, object data)
        {
            log_.Info("add player " + player.id + " to game ");
            HFTPlayer oldPlayer = null;
            if (players_.TryGetValue(player.id, out oldPlayer))
            {
                log_.Error("player " + player.id + " is already member of game");
                return;
            }

            player.SetGame(this);
            players_[player.id] = player;
            Send(null, new HFTRelayToGameMessage("start", player.id, data));
        }

        public void RemovePlayer(HFTPlayer player)
        {
            log_.Info("remove player " + player.id);
            bool removed = players_.Remove(player.id);
            if (!removed)
            {
                log_.Error("player " + player.id + " is not a member of game");
                return;
            }

            // remove queued messages from this player
            sendQueue_.RemoveAll(item => item.Key != null && item.Key.id == player.id);

            Send(null, new HFTRelayToGameMessage("remove", player.id, null));
        }

        public void Send(HFTPlayer owner, HFTRelayToGameMessage msg)
        {
            if (client_ != null)
            {
                try
                {
                    client_.Send(msg);
                } catch (System.Exception ex)
                {
                    log_.Warn("Attempt to send message to game failed: " + ex);
                }
            }
            else
            {
                sendQueue_.Add(new KeyValuePair<HFTPlayer, HFTRelayToGameMessage>(owner, msg));
            }
        }

        public void AssignClient(HFTSocket client, HFTRuntimeOptions data)
        {
            if (client_ != null)
            {
                log_.Error("this game already has a client!");
                client_.Close();
            }

            client_ = client;

            client.OnMessageEvent += OnMessage;
            client.OnCloseEvent += OnDisconnect;

            RegisterCmdHandler<object>("client", SendMessageToPlayer);
            RegisterCmdHandler<object>("broadcast", Broadcast);
            RegisterCmdHandler<MessageSwitchGame>("switchGame", SwitchGame);
            RegisterCmdHandler<object>("peer", SendMessageToGame);
            RegisterCmdHandler<object>("bcastToGames", BroadcastToGames);

            // Tell the game it's id
            var gs = new MessageGameStart();
            gs.id = id_;
            gs.gameId = ""; //FIX!
            client.Send(new HFTRelayToGameMessage("gamestart", "", gs));

            // start each player
            foreach (var pair in players_)
            {
                client.Send(new HFTRelayToGameMessage("start", pair.Value.id, null));
            }

            // Not sure why I even have a sendQueue
            // as the game should be running before anyone
            // joins but it seems to be useful for debugging
            // since contollers start and often immediately
            // send a name and color cmd.
            foreach (var pair in sendQueue_)
            {
                client.Send(pair.Value);
            }
            sendQueue_.Clear();

        }

        public void SendMessageToPlayer(string id, object message)  // ???????????????????????????????????????????????????????????
        {
            HFTPlayer player = null;
            if (!players_.TryGetValue(id, out player))
            {
                log_.Error("SendMessageToPlayer no player " + id);
                return;
            }

            player.Send(message);
        }

        public void Broadcast(string id, object message)  // ??????????????????????????????????????
        {
            foreach (KeyValuePair<string, HFTPlayer>p in players_)
            {
                p.Value.Send(message);
            }
        }

        public void SwitchGame(string playerId, MessageSwitchGame data)  // ?????????????????????????????????????????
        {
            HFTPlayer player = null;
            if (!players_.TryGetValue(playerId, out player))
            {
                log_.Error("SwitchGame player " + playerId);
                return;
            }

            RemovePlayer(player);
            gameGroup_.AddPlayerToGame(player, data.gameId, data.data);
        }

        public void SendMessageToGame(string destId, object data)
        {
            gameGroup_.SendMessageToGame(id_, destId, data);
        }

        public void BroadcastToGames(string id, object data)
        {
            this.gameGroup_.BroadcastMessageToGames(id_, id, data);
        }

        public void OnMessage(object sender, WebSocketSharp.MessageEventArgs message)
        {
            HFTRelayFromPlayerMessage msg = deserializer_.Deserialize<HFTRelayFromPlayerMessage>(message.Data);
            CmdEventHandler handler = null;
            if (!messageHandlers_.TryGetValue(msg.cmd, out handler))
            {
                log_.Error("unknown game message: " + msg.cmd + " for :" + gameId_);
                return;
            }

            handler(deserializer_, msg.id, msg.data);
        }

        public void OnDisconnect(object sender, WebSocketSharp.CloseEventArgs e)
        {
            HandleClose();
        }

        void HandleClose()
        {
            log_.Info("closing:" + gameId_);
            if (client_ != null)
            {
                var client = client_;
                client_ = null;
                log_.Info("closing client:" + gameId_);
                client.Close();
            }
            gameGroup_.RemoveGame(this);
            if (options_.disconnectPlayersIfGameDisconnects)
            {
                while (players_.Count > 0)
                {
                    IEnumerator<KeyValuePair<string, HFTPlayer>> it = players_.GetEnumerator();
                    it.MoveNext();
                    HFTPlayer player = it.Current.Value;
                    RemovePlayer(player);
                    player.Disconnect();
                }
            }
        }

        public void Close()
        {
            HandleClose();
        }

        // report error back to game
        void Error(string msg)
        {
            if (client_ != null)
            {
                client_.Send(new MessageCmd("log", new MessageLog("error", msg)));
            }
        }

        void SendSystemCmd(string cmd, object data)
        {
            Send(null, new HFTRelayToGameMessage("system", "-1", new HFTCmdMessage(cmd, data)));
        }

        public void SendQuit()
        {
            SendSystemCmd("exit", null);
        }

        public void SendGameDisconnect(HFTGame otherGame)
        {
            if (client_ != null)  // this check is needed because in GameGroup.assignClient the new game has been added to games but not yet assigned a client
            {
                client_.Send(
                    new HFTRelayToGameMessage(
                        "upgame",
                        otherGame.id,
                        new MessageCmd("gamedisconnect",
                                       new HFTMessageGameDisconnect(otherGame.id))));
            }
        }

        string id_; // id of this individual game. On a game like Tonde-Iko there are multiple games
        string gameId_; // id of this game in general eg "tonde-iko", "boomboom", etc..
        Deserializer deserializer_ = new Deserializer();
        HFTGameGroup gameGroup_;
        HFTSocket client_;
        Dictionary<string, CmdEventHandler> messageHandlers_ = new Dictionary<string, CmdEventHandler>();
        Dictionary<string, HFTPlayer> players_ = new Dictionary<string, HFTPlayer>();
        HFTRuntimeOptions options_;
        List<KeyValuePair<HFTPlayer, HFTRelayToGameMessage>> sendQueue_ = new List<KeyValuePair<HFTPlayer, HFTRelayToGameMessage>>();
        HFTLog log_;

    }

}  // namespace HappyFunTimes

