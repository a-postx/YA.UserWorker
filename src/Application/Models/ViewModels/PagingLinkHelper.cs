using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Delobytes.AspNetCore;

namespace YA.TenantWorker.Application.Models.ViewModels
{
    public class PagingLinkHelper : IPagingLinkHelper
    {
        public PagingLinkHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
        }

        private readonly IUrlHelper _urlHelper;

        public string GetLinkValue<T>(PageResult<T> page, string routeNames) where T : class
        {
            List<string> values = new List<string>(4);

            if (page.HasNextPage)
            {
                values.Add(GetLinkValueItem("next", page.Page + 1, page.Count, routeNames));
            }

            if (page.HasPreviousPage)
            {
                values.Add(GetLinkValueItem("previous", page.Page - 1, page.Count, routeNames));
            }

            values.Add(GetLinkValueItem("first", 1, page.Count, routeNames));
            values.Add(GetLinkValueItem("last", page.TotalPages, page.Count, routeNames));

            return string.Join(", ", values);
        }

        private string GetLinkValueItem(string rel, int page, int count, string routeNames)
        {
            string url = _urlHelper.AbsoluteRouteUrl(routeNames, new PageOptions { Page = page, Count = count });
            return $"<{url}>; rel=\"{rel}\"";
        }
    }
}
