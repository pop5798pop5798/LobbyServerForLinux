using LobbyServerForLinux.Model;
using LobbyServerForLinux.Models;
using LobbyServerForLinux.Services;
using MhCore.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyServerForLinux
{
    /// <summary> WebSocket連線主服務 </summary>
    public class WebsocketService
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public WebsocketService(RequestDelegate next,ILoggerFactory loggerFactory)        
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<WebsocketService>();               
        }
        /// <summary> 調用webScoket </summary>
        /// <param name="context"> 傳入資料 </param>
        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                string clientId = Guid.NewGuid().ToString(); ;
                var wsClient = new WebsocketClient
                {
                    Id = clientId,
                    WebSocket = webSocket
                };
                try
                {
                    await Handle(wsClient);
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync("closed");
                }
            }
            else
            {
                await _next(context);
                //context.Response.StatusCode = 404;
            }


        }
        /// <summary> 交握處理 </summary>
        /// <param name="webSocket"> Scoket資料 </param>
        private async Task Handle(WebsocketClient webSocket)
        {
            WsCollectionService.Add(webSocket);
            _logger.LogInformation($"Websocket client added.");

            WebSocketReceiveResult result = null;
            do
            {
                var buffer = new byte[1024 * 1];
                result = await webSocket.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
                {
                    var msgString = Encoding.UTF8.GetString(buffer);
                    _logger.LogInformation($"Websocket client ReceiveAsync message {msgString}.");
                    var message = JsonConvert.DeserializeObject<Message>(msgString);
                    message.Sn = webSocket.Id;
                    MessageRoute(message);
                }
            }
            while (!result.CloseStatus.HasValue);
            WsCollectionService.Remove(webSocket);
            _logger.LogInformation($"Websocket client closed.");
        }
        /// <summary> 資料Rout </summary>
        /// <param name="message"> 回傳資料訊息 </param>
        private void MessageRoute(Message message)
        {
            var client = WsCollectionService.Get(message.Sn);
            Message m = new Message();
            var jsonStr = "";
            switch (message.Cmd)
            {
                case "User_Accept": return;
                case "LoginServer":                   
                    m.Cmd = "LoginServer";
                    m.Data = "{}";
                    CmdJSon_Login_Receive loginData = JsonConvert.DeserializeObject<CmdJSon_Login_Receive>(message.Data.ToString());
                    DataNet_Login Logininfo = new DataNet_Login(1003, message.Sn, loginData.Key);
                    DataServer_Login result = DBAdapter.Do.UserLogin(Logininfo);
                    //bgEnd_UserLogin((DataServer_Login)info);
                    CmdJSon_Login_Send Datainfo = new CmdJSon_Login_Send();
                    Datainfo.NickName = result.data.nick_name;
                    Datainfo.Plunid = result.data.user_id;
                    Datainfo.State = 1;
                    Datainfo.VPoints = result.data.user_point;
                    message.Data = Datainfo;
                    jsonStr = JsonConvert.SerializeObject(message);
                    client.SendMessageAsync(jsonStr);
                    break;
                case "GetLobbyInfo":
                    m.Cmd = "GetLobbyInfo";
                    m.Data = "{}";
                    jsonStr = JsonConvert.SerializeObject(m);
                    client.SendMessageAsync(jsonStr);
                    break;
                case "GetGameList":
                    jsonStr = JsonConvert.SerializeObject(message);
                    client.SendMessageAsync(jsonStr);
                    break;
                case "ClientGetPing":
                    CmdJson_ClientGetPing_Send pinginfo = new CmdJson_ClientGetPing_Send();
                    message.Data = pinginfo;
                    jsonStr = JsonConvert.SerializeObject(message);
                    client.SendMessageAsync(jsonStr);
                    break;
                case "SystemCheck":
                    CmdJSon_SystemCheck_Send sysinfo = new CmdJSon_SystemCheck_Send();
                    message.Data = sysinfo;
                    jsonStr = JsonConvert.SerializeObject(message);
                    client.SendMessageAsync(jsonStr);
                    break;
                default:
                    break;
            }
        }
    }
}
