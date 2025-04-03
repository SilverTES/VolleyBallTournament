using LiteNetLib;
using LiteNetLib.Utils;
using Mugen.Core;
using System;
using System.Collections.Generic;

namespace VolleyBallTournament
{
    public class NetworkServer
    {
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _server;
        private readonly Dictionary<int, NetPeer> _connectedPeers;

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
            this._screenPlay = screenPlay;
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
                    writer.Put("Mon Serveur Awesome"); // Nom du serveur (personnalisable)
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
            string message = reader.GetString();
            Misc.Log($"Message reçu de {peer.Address} : {message}");

            // Répondre au client (exemple)
            NetDataWriter writer = new NetDataWriter();
            writer.Put($"Echo: {message}");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            reader.Recycle();
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