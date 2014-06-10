using System;
using MrCMS.Entities.Multisite;

namespace MrCMS.Web.Apps.Ecommerce.Helpers
{
    public class UrlHelper : IUrlHelper
    {
        private readonly Site _site;

        public UrlHelper(Site site)
        {
            _site = site;
        }

        public string ToAbsolute(string urlSegment)
        {
            const string scheme = "http://";

            if (string.IsNullOrWhiteSpace(urlSegment) || urlSegment.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string authority = _site.BaseUrl;

            if (authority.EndsWith("/"))
                authority = authority.TrimEnd('/');

            if (urlSegment.StartsWith("/"))
                urlSegment = urlSegment.TrimStart('/');

            return string.Format("{0}{1}/{2}", scheme, authority, urlSegment);
        }
    }
}