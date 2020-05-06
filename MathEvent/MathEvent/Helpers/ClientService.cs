using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace MathEvent.Helpers
{
    public class ClientService
    {
        public ClientService(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; set; }
    }
}
