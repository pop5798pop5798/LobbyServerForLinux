using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Models
{
    public class Message
    {
        public string Sn { get; set; }
        public string Cmd { get; set; }
        public object Data { get; set; }
    }
}
