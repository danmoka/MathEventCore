using Microsoft.AspNetCore.Components;
using System.Net;

namespace MathEvent.Helpers.StatusCode
{
    public class StatusCodeService : IStatusCodeResolver
    {
        private readonly NavigationManager _nm;

        public StatusCodeService(NavigationManager nm)
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
