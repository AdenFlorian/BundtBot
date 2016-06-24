using System;
using System.Net;

namespace DiscordSharp_Starter {
    class MyWebClient : WebClient {
        protected override WebRequest GetWebRequest(Uri uri) {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 2000;
            return w;
        }
    }
}
