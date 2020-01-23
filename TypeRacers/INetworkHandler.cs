using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeRacers
{
    //interface for network handler
    interface INetworkHandler
    { 
        void SendProgressToServer(string progress);

        string GetTextFromServer();

        List<Tuple<string, Tuple<string, string>>> GetOpponents();

    }
}
