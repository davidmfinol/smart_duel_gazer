﻿namespace Dpoch.SocketIO {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UnityEngine;
    using WebSocketSharp;

    public class SocketIOConnection {
        class EngineIOHandshakeData {
            public int pingTimeout = 60000;
            public int pingInterval = 25000;
        }

        Uri uri;
        WebSocket ws;
        Thread thread;
        volatile bool shouldRun = false;
        int connectTimeoutMS;
        Queue<Action> synchronizationQueue = new Queue<Action>();
        object synchronizationQueueLock = new object();

        public bool IsAlive {
            get {
                return thread != null && thread.IsAlive;
            }
        }

        public event Action OnOpen;
        public event Action OnConnectFailed;
        public event Action OnClose;
        public event Action<SocketIOException> OnError;
        public event Action<Packet> OnEvent;
        public event Action<Packet> OnAck;

        public SocketIOConnection(Uri uri, int connectTimeoutMS = 30000) {
            if (uri.Scheme != "ws" && uri.Scheme != "wss") throw new ArgumentException("Protocol " + uri.Scheme + " is invalid");
            this.uri = uri;
            this.connectTimeoutMS = connectTimeoutMS;
        }

        public void Connect() {
            if (IsAlive) {
                Debug.LogError("Connection is already alive");
                return;
            }
            synchronizationQueue.Clear();
            shouldRun = true;
            thread = new Thread(RunSocketThread);
            thread.Start();
        }

        public void Close() {
            lock (synchronizationQueueLock) {
                synchronizationQueue.Enqueue(() => {
                    shouldRun = false;
                });
            }
        }

        public void Send(Packet packet) {
            lock (synchronizationQueueLock) {
                synchronizationQueue.Enqueue(() => {
                    ws.Send(packet.Encode());
                    foreach (var attachment in packet.Attachments) {
                        ws.Send(Packet.BinaryPacket(attachment));
                    }
                });
            }
        }

        void RunSocketThread() {
            ws = new WebSocket(uri.ToString());
            ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            bool receivedSocketHandshake = false;
            EngineIOHandshakeData engineHandshakeData = null;
            DateTime lastPingSent = DateTime.Now;
            DateTime lastPongReceived = DateTime.Now;

            Action<Packet> handleMessage = (Packet packet) => {
                switch (packet.SocketPacketType) {
                    case Packet.SocketIOPacketType.EVENT:
                    case Packet.SocketIOPacketType.BINARY_EVENT:
                        if (OnEvent != null) OnEvent(packet);
                        break;
                    case Packet.SocketIOPacketType.ACK:
                    case Packet.SocketIOPacketType.BINARY_ACK:
                        if (OnAck != null) OnAck(packet);
                        break;
                    case Packet.SocketIOPacketType.CONNECT:
                        if (!receivedSocketHandshake) {
                            receivedSocketHandshake = true;
                            if (OnOpen != null) OnOpen();
                        }
                        break;
                    case Packet.SocketIOPacketType.DISCONNECT:
                        shouldRun = false;
                        break;
                    case Packet.SocketIOPacketType.ERROR:
                        if (OnError != null) OnError(new SocketIOException(packet.Data.ToString()));
                        break;
                }
            };

            Action<Packet> handlePacket = (packet) => {
                switch (packet.EnginePacketType) {
                    case Packet.EngineIOPacketType.OPEN:
                        engineHandshakeData = packet.Data.ToObject<EngineIOHandshakeData>();
                        lastPingSent = DateTime.Now - TimeSpan.FromMilliseconds(engineHandshakeData.pingInterval);
                        lastPongReceived = DateTime.Now;
                        break;
                    case Packet.EngineIOPacketType.CLOSE:
                        shouldRun = false;
                        break;
                    case Packet.EngineIOPacketType.PONG:
                        lastPongReceived = DateTime.Now;
                        break;
                    case Packet.EngineIOPacketType.MESSAGE:
                        handleMessage(packet);
                        break;
                }
            };

            ws.OnError += (sender, e) => {
                if (OnError != null) OnError(new SocketIOException("WebSocket Error: " + e.Message));
            };

            ws.OnClose += (sender, e) => {
                shouldRun = false;
            };

            Packet currentPacket = null;
            ws.OnMessage += (sender, e) => {
                if (e.IsText) {
                    try {
                        var packet = Packet.Parse(e.Data);
                        if (packet.IsComplete) {
                            handlePacket(packet);
                        } else {
                            currentPacket = packet;
                        }
                    } catch (SocketIOException ex) {
                        if (OnError != null) OnError(ex);
                    }
                } else if (e.IsBinary && currentPacket != null) {
                    currentPacket.Attachments.Add(e.RawData.Skip(1).ToArray());
                    if (currentPacket.IsComplete) {
                        handlePacket(currentPacket);
                        currentPacket = null;
                    }
                }
            };

            ws.ConnectAsync();
            var startTime = DateTime.Now;

            while (shouldRun) {
                if (engineHandshakeData != null) {
                    //stop if we didn't receive a pong in <pingTimeout>
                    if (DateTime.Now.Subtract(lastPongReceived).TotalMilliseconds >= engineHandshakeData.pingTimeout) {
                        break;
                    }

                    //ping the server every <pingInterval> if we receive an engine handshake
                    if (DateTime.Now.Subtract(lastPingSent).TotalMilliseconds >= engineHandshakeData.pingInterval) {
                        ws.Send(Packet.Ping().Encode());
                        lastPingSent = DateTime.Now;
                    }
                }

                //send queued packets if we received a socketio handshake
                if (receivedSocketHandshake) {
                    lock (synchronizationQueueLock) {
                        while (synchronizationQueue.Count > 0 && shouldRun) {
                            synchronizationQueue.Dequeue()();
                        }
                    }
                } else if (DateTime.Now.Subtract(startTime).TotalMilliseconds >= connectTimeoutMS) {
                    //stop if we didn't receive a socket handshake in <connectTimeoutMS>
                    break;
                }

                Thread.Sleep(10);
            }

            ws.CloseAsync();
            thread = null;

            if (receivedSocketHandshake) {
                if (OnClose != null) OnClose();
            }else {
                if (OnConnectFailed != null) OnConnectFailed();
            }
        }
    }
}