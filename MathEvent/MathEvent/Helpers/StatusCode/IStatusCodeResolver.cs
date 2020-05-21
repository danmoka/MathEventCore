using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MathEvent.Helpers.StatusCode
{
    interface IStatusCodeResolver
    {
        void ResolveStatusCode(HttpStatusCode statusCode, string okUrl);
    }
}
