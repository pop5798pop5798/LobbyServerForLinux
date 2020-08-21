using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MhCore.SocketAdapter.WebSocketAdapter;

namespace LobbyServerForLinux.Model
{
    public class CommandStruct
    {

    }

    // 登入部份.
    public class CmdJSon_Login_Receive : CmdJsonData
    {
        public String Account { get; set; } = "";     // 玩家帳號.
        public String Key { get; set; } = "";         // 登入金鑰.

    }

    public class CmdJSon_Login_Send : CmdJsonData
    {

        public int State { get; set; } = 0;
        public int Plunid { get; set; } = 0;
        public int GameMode { get; set; } = 0;
        public Decimal VPoints { get; set; } = 0;
        public String NickName { get; set; } = "";
        //public string Version = Program.Version;

    }

    // 系統檢測.
    public class CmdJSon_SystemCheck_Send : CmdJsonData
    {
        public int State = 0;
        public int MemberID = 0;
        public string LoginDate = string.Empty;
    }

    // Client要求連線品質
    public class CmdJson_ClientGetPing_Send : CmdJsonData
    {

    }
}
