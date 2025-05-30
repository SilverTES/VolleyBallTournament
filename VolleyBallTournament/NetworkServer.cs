﻿using LiteNetLib;
using LiteNetLib.Utils;
using Mugen.Core;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text.RegularExpressions;
using static Mugen.GUI.Gui;

namespace VolleyBallTournament
{
    // Définition des types de messages
    //enum MessageType : byte
    //{
    //    Position,
    //    Chat,
    //    Connexion
    //}

    //// Envoi d'un message
    //void SendPosition(NetPeer peer, Vector3 position)
    //{
    //    NetDataWriter writer = new NetDataWriter();
    //    writer.Put((byte)MessageType.Position); // Identifiant de type
    //    writer.Put(position.X);
    //    writer.Put(position.Y);
    //    writer.Put(position.Z);
    //    peer.Send(writer, DeliveryMethod.Unreliable);
    //}

    //// Réception d'un message
    //void OnReceive(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
    //{
    //    MessageType messageType = (MessageType)reader.GetByte();
    //    switch (messageType)
    //    {
    //        case MessageType.Position:
    //            Vector3 position = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    //            // Traiter la position
    //            break;
    //        case MessageType.Chat:
    //            string message = reader.GetString();
    //            // Traiter le message de chat
    //            break;
    //            // ...
    //    }
    //}

    public enum MessageType : byte
    {
        Unknown = 0,
        Message,
        AddPoint,
        Update,
        UpdatePhase,
    }

    public class NetworkServer
    {
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _server;
        public Dictionary<int, NetPeer> Clients => _connectedPeers;
        private readonly Dictionary<int, NetPeer> _connectedPeers;

        public Dictionary<int, int> ClientControlCourts = [];

        ScreenPlay _screenPlay;
        public NetworkServer(ScreenPlay screenPlay)
        {
            Console.WriteLine("Initialisation du serveur...");
            _listener = new EventBasedNetListener();
            _server = new NetManager(_listener);
            _connectedPeers = new Dictionary<int, NetPeer>();

            // Associer les événements
            _listener.ConnectionRequestEvent += OnConnectionRequest;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _listener.NetworkReceiveEvent += OnNetworkReceive;
            _listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;
            _screenPlay = screenPlay;

            ClientControlCourts[0] = 0;
            ClientControlCourts[1] = 1;
            ClientControlCourts[2] = 2;
        }
        public void StartServer()
        {
            try
            {
                Misc.Log("Avant activation des messages non connectés...");
                _server.UnconnectedMessagesEnabled = true; // Pour les broadcasts
                Misc.Log("Messages non connectés activés.");
                _server.BroadcastReceiveEnabled = true;   // Activer la réception broadcast
                Misc.Log("Démarrage du serveur...");
                _server.Start(9050);
                Misc.Log("Serveur démarré sur le port 9050.");
            }
            catch (Exception ex)
            {
                Misc.Log($"Erreur au démarrage : {ex.Message}");
                Misc.Log(ex.StackTrace);
            }
        }
        // Réception des messages broadcast (non connectés)
        private void OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Misc.Log($"Message non connecté reçu de {remoteEndPoint}, type : {messageType}");
            if (messageType == UnconnectedMessageType.Broadcast)
            {
                string message = reader.GetString();
                if (message == "DISCOVER_REQUEST")
                {
                    Misc.Log($"Requête de découverte reçue de {remoteEndPoint}");
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put("DISCOVER_RESPONSE");
                    writer.Put("VolleyBall Tournament"); // Nom du serveur (personnalisable)
                    _server.SendUnconnectedMessage(writer, remoteEndPoint);
                    Misc.Log($"Réponse envoyée à {remoteEndPoint}");
                }
                reader.Recycle();
            }
        }

        // Gérer les demandes de connexion
        private void OnConnectionRequest(ConnectionRequest request)
        {
            if (_server.ConnectedPeersCount < 10) // Limite à 10 joueurs
                request.AcceptIfKey("MugenKey"); // Clé de connexion
            else
                request.Reject();
        }

        // Quand un client se connecte
        private void OnPeerConnected(NetPeer peer)
        {
            _connectedPeers[peer.Id] = peer;
            ClientControlCourts[peer.Id] = 0;

           Misc.Log($"Client connecté : {peer.Address} (ID: {peer.Id})");

            // Envoyer un message de bienvenue
            NetDataWriter writer = new NetDataWriter();
            writer.Put("Bienvenue sur le serveur VolleyBall Tournament !");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        // Quand un client se déconnecte
        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _connectedPeers.Remove(peer.Id);
            Misc.Log($"Client déconnecté : {peer.Address} (Raison : {disconnectInfo.Reason})");
        }

        // Réception des messages des clients
        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            MessageType messageType = (MessageType)reader.GetByte();

            switch (messageType)
            {
                case MessageType.Unknown:

                    break;

                case MessageType.Message:

                    ProcessMessage(peer, reader);

                    break;

                case MessageType.AddPoint:

                    ProcessAddPoint(peer, reader);

                    break;
                case MessageType.Update:

                    ProcessRequestUpdate(peer, reader);

                    break;

                default:
                    break;
            }



            reader.Recycle();
        }

        private void ProcessMessage(NetPeer peer, NetPacketReader reader)
        {
            string message = reader.GetString();
            Misc.Log($"Message reçu de {peer.Address} : {message}");
                    
            // Répondre au client (exemple)
            NetDataWriter writer = new NetDataWriter();
            writer.Put($"Echo: {message}");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        private void ProcessAddPoint(NetPeer peer, NetPacketReader reader)
        {
            int points = reader.GetInt();
            int matchIndex = reader.GetInt();
            int team = reader.GetInt();

            var match = _screenPlay.PhasePool1.GetMatch(matchIndex);

            if (team == 0) match.AddPointA(points);
            if (team == 1) match.AddPointB(points);

            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)MessageType.Update); // Identifiant de type
            writer.Put(match.TeamA.Stats.ScorePoint);
            writer.Put(match.TeamB.Stats.ScorePoint);
            writer.Put(match.TeamA.Stats.TeamName);
            writer.Put(match.TeamB.Stats.TeamName);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        private void ProcessRequestUpdate(NetPeer peer, NetPacketReader reader)
        {
            int matchIndex = reader.GetInt();

            ClientControlCourts[peer.Id] = matchIndex;

            Match match = null;

            if (_screenPlay.Phase == Phases.Pool1) match = _screenPlay.PhasePool1.GetMatch(matchIndex);
            if (_screenPlay.Phase == Phases.Pool2) match = _screenPlay.PhasePool2.GetMatch(matchIndex);
            if (_screenPlay.Phase == Phases.DemiFinal) match = _screenPlay.PhaseDemiFinal.GetMatch(matchIndex);

            
            SendUpdateTo(peer, match);

            //NetDataWriter writer = new NetDataWriter();
            //writer.Put((byte)MessageType.Update); // Identifiant de type
            //writer.Put(match.TeamA.Stats.ScorePoint);
            //writer.Put(match.TeamB.Stats.ScorePoint);
            //writer.Put(match.TeamA.Stats.TeamName);
            //writer.Put(match.TeamB.Stats.TeamName);
            //peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        public void SendUpdateTo(NetPeer peer, Match match)
        {
            if (match == null) return;

            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)MessageType.Update); // Identifiant de type
            writer.Put(match.TeamA.Stats.ScorePoint);
            writer.Put(match.TeamB.Stats.ScorePoint);
            writer.Put(match.TeamA.Stats.TeamName);
            writer.Put(match.TeamB.Stats.TeamName);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        public void SendUpdateToAll(List<Match> matchs)
        {
            for (int i = 0; i < matchs.Count; i++)
            {
                if (Static.Server.Clients.ContainsKey(i))
                    Static.Server.SendUpdateTo(Static.Server.Clients[i], matchs[Static.Server.ClientControlCourts[Static.Server.Clients[i].Id]]);
            }
        }
        public void SendUpdatePhaseToAll()
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)MessageType.UpdatePhase);
            writer.Put((int)_screenPlay.Phase);

            for (int i = 0; i < _connectedPeers.Count; i++)
            {
                if (_connectedPeers.ContainsKey(i))
                {
                    var peer = _connectedPeers[i];
                    peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
            }

        }

        // Mettre à jour le serveur (appelé dans Game.Update)
        public void Update()
        {
            _server.PollEvents();
        }

        // Arrêter le serveur proprement
        public void Stop()
        {
            _server.Stop();
            Misc.Log("Serveur arrêté.");
        }
    }
}