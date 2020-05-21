using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MathEvent.Helpers.StatusCode
{
    public class StatusCodeResolver : IStatusCodeResolver
    {
        private readonly NavigationManager _nm;

        public StatusCodeResolver(NavigationManager nm)
        {
            _nm = nm;
        }
        public void ResolveStatusCode(HttpStatusCode statusCode, string okUrl)
        {
            if (statusCode != HttpStatusCode.OK)
            {
                _nm.NavigateTo($"error/{(int) statusCode}", true);

                return;
            }

            if (!string.IsNullOrEmpty(okUrl))
            {
                _nm.NavigateTo($"{okUrl}", true);
            }
        }
    }
}
