using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp_Starter {
    class MyWebClient : WebClient {
        protected override WebRequest GetWebRequest(Uri uri) {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 2000;
            return w;
        }
    }
}
