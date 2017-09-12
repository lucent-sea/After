﻿using Dynamic_JSON;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Translucency.WebSockets
{
    public class WebSocketClient
    {
        public WebSocketClient(WebSocketServer Server, WebSocket Socket)
        {
            WSServer = Server;
            ClientSocket = Socket;
        }

        /// <summary>
        /// The websocket for this client.
        /// </summary>
        public WebSocket ClientSocket { get; set; }

        /// <summary>
        /// The WebSocketServer to which this client is connected.
        /// </summary>
        public WebSocketServer WSServer { get; set; }

        /// <summary>
        /// A dictionary of arbitrary data to store on the socket.
        /// </summary>
        public Dictionary<string, dynamic> Tags { get; set; } = new Dictionary<string, dynamic>();

        /// <summary>
        /// The action to perform when a string message is read.  This is performed every time, before the received string is evaluated.
        /// </summary>
        public Action<WebSocketClient, dynamic> OnMessageStringPreAction { get; set; }

        /// <summary>
        /// A dictionary of actions to be performed as a result of the string message read from the websocket.  The first property's first value of every message
        /// is used as the key for determining which action to perform.  The WebSocketClient and dynamic can be used within the Action.  The dynamic is the message
        /// received and parsed with JSON.
        /// </summary>
        public Dictionary<string, Action<WebSocketClient, dynamic>> OnMessageStringActions { get; set; } = new Dictionary<string, Action<WebSocketClient, dynamic>>();

        /// <summary>
        /// The action to perform when a string message is read.  This is performed every time, after the received string is evaluated.
        /// </summary>
        public Action<WebSocketClient> OnMessageStringPostAction { get; set; }

        /// <summary>
        /// Similar to OnMessageStringActions, only this is one action performed when a byte array message is received.
        /// </summary>
        public Action<WebSocketClient, byte[]> OnMessageByteAction { get; set; }
        /// <summary>
        /// The action to perform when the socket connecton is closed.
        /// </summary>
        public Action<WebSocketClient> OnCloseAction { get; set; }

        /// <summary>
        /// Send a JSON message to the client.
        /// </summary>
        /// <param name="JsonRequest">The message to send to the client.</param>
        public void SendJSON(dynamic JsonRequest)
        {
            var jsonRequest = JSON.Encode(JsonRequest);
            SendString(jsonRequest);
        }

        /// <summary>
        /// Send a byte array message to the client.
        /// </summary>
        /// <param name="ByteArray">The message to send to the client.</param>
        public void SendBytes(byte[] ByteArray)
        {
            var outBuffer = new ArraySegment<byte>(ByteArray);
            SendBuffer.Add(outBuffer);
            if (SendBuffer.Count > 1)
            {
                return;
            }
            while (SendBuffer.Count > 0)
            {
                var send = ClientSocket.SendAsync(SendBuffer[0], WebSocketMessageType.Text, true, CancellationToken.None);
                send.Wait();
                SendBuffer.RemoveAt(0);
            }
        }
        /// <summary>
        /// Send a string message to the client.
        /// </summary>
        /// <param name="MessageString">The message to send to the client.</param>
        public void SendString(string MessageString)
        {
            var outBuffer = Encoding.UTF8.GetBytes(MessageString);
            SendBytes(outBuffer);
        }
        /// <summary>
        /// Continuously reads data from the websocket.
        /// </summary>
        public async Task HandleSocket()
        {
            try
            {
                var buffer = new byte[WSServer.ReceiveBufferSize];
                WebSocketReceiveResult result = await ClientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                ParseMessage(result, buffer);
                while (!ClientSocket.CloseStatus.HasValue)
                {
                    buffer = new byte[WSServer.ReceiveBufferSize];
                    result = await ClientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    ParseMessage(result, buffer);
                }
                if (WSServer.ClientList.Contains(this))
                {
                    WSServer.ClientList.Remove(this);
                }
                await ClientSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch
            {
                if (WSServer.ClientList.Contains(this))
                {
                    WSServer.ClientList.Remove(this);
                }
                await ClientSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "An unhandled exception occurred.", CancellationToken.None);
            }
        }

        /// <summary>
        /// Parses the result of data read from the websocket.
        /// </summary>
        /// <param name="Result">The WebSocketReceiveResult.</param>
        /// <param name="ReadBuffer">The byte array buffer read from the socket stream.</param>
        private void ParseMessage(WebSocketReceiveResult Result, byte[] ReadBuffer)
        {
            if (!Result.EndOfMessage)
            {
                return;
            }
            if (Result.MessageType == WebSocketMessageType.Text)
            {
                var trimmedString = Encoding.UTF8.GetString(TrimBytes(ReadBuffer));
                var jsonMessage = JSON.Decode(trimmedString);
                if (OnMessageStringPreAction != null)
                {
                    OnMessageStringPreAction.Invoke(this, jsonMessage);
                }
                if (OnMessageStringActions.ContainsKey(jsonMessage?.First?.First?.ToString()))
                {
                    OnMessageStringActions[jsonMessage.First.First.ToString()].Invoke(this, jsonMessage);
                }
                if (OnMessageStringPostAction != null)
                {
                    OnMessageStringPostAction.Invoke(this);
                }
            }
            else if (Result.MessageType == WebSocketMessageType.Binary)
            {
                OnMessageByteAction(this, ReadBuffer);
            }
            else if (Result.MessageType == WebSocketMessageType.Close)
            {
                OnCloseAction(this);
            }
        }

        /// <summary>
        /// Removes trailing empty bytes in the buffer.
        /// </summary>
        /// <param name="Bytes">Byte array to trim.</param>
        /// <returns>Trimmed byte array.</returns>
        private byte[] TrimBytes(byte[] Bytes)
        {
            // Loop backwards through array until the first non-zero byte is found.
            var firstZero = 0;
            for (int i = Bytes.Length - 1; i >= 0; i--)
            {
                if (Bytes[i] != 0)
                {
                    firstZero = i + 1;
                    break;
                }
            }
            if (firstZero == 0)
            {
                throw new Exception("Byte array is empty.");
            }
            // Return non-empty bytes.
            return Bytes.Take(firstZero).ToArray();
        }

        /// <summary>
        /// The buffer to use for outgoing messages.
        /// </summary>
        private List<ArraySegment<byte>> SendBuffer { get; set; } = new List<ArraySegment<byte>>();
       
    }
}