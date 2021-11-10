using System.Dynamic;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using YA.UserWorker.Application.Interfaces;
using YA.UserWorker.Application.Models.HttpQueryParams;
using YA.UserWorker.Application.Models.ViewModels;

namespace YA.UserWorker.Application.Services;

/// <summary>
/// Фабрика модели результата постраничного вывода
/// </summary>
public class PaginatedResultFactory : IPaginatedResultFactory
{
    public PaginatedResultFactory(IActionContextAccessor actionCtx, LinkGenerator linkGenerator)
    {
        _actionCtx = actionCtx ?? throw new ArgumentNullException(nameof(actionCtx));
        _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
    }

    private readonly IActionContextAccessor _actionCtx;
    private readonly LinkGenerator _linkGenerator;

    private const string ApiVersionQueryKey = "api-version";

    public PaginatedResultVm<T> GetCursorPaginatedResult<T>(PageOptionsCursor pageOptions, bool hasNextPage, bool hasPreviousPage,
        int totalCount, string startCursor, string endCursor, string routeName, ICollection<T> items) where T : class
    {
        if (pageOptions == null)
        {
            throw new ArgumentNullException(nameof(pageOptions));
        }

        if (string.IsNullOrEmpty(routeName))
        {
            throw new ArgumentNullException(nameof(routeName));
        }

        ICollection<T> resultItems = items ?? new List<T>();

        Tuple<ExpandoObject, ExpandoObject> baseQueryParams = GetCursorUniqueQueryParams(pageOptions);

        PageInfoVm pageInfo = new PageInfoVm()
        {
            Count = items.Count,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage,
            NextPageUrl = hasNextPage ? GetCursorNextPageUri(routeName, baseQueryParams, pageOptions, endCursor) : null,
            PreviousPageUrl = hasPreviousPage ? GetCursorPreviousPageUri(routeName, baseQueryParams, pageOptions, startCursor) : null,
            FirstPageUrl = GetCursorFirstPageUri(routeName, baseQueryParams, pageOptions),
            LastPageUrl = GetCursorLastPageUri(routeName, baseQueryParams, pageOptions),
        };

        PaginatedResultVm<T> result = new PaginatedResultVm<T>(totalCount, pageInfo, resultItems);

        return result;
    }

    private Tuple<ExpandoObject, ExpandoObject> GetCursorUniqueQueryParams(PageOptionsCursor pageOptions)
    {
        ExpandoObject uniqueParams = new ExpandoObject();
        ExpandoObject apiVersionParam = new ExpandoObject();

        foreach (KeyValuePair<string, StringValues> item in _actionCtx.ActionContext.HttpContext.Request.Query)
        {
            string key = item.Key.ToLowerInvariant();

            if (key != nameof(pageOptions.First).ToLowerInvariant()
                && key != nameof(pageOptions.Last).ToLowerInvariant()
                && key != nameof(pageOptions.Before).ToLowerInvariant()
                && key != nameof(pageOptions.After).ToLowerInvariant())
            {
                if (key == ApiVersionQueryKey)
                {
                    apiVersionParam.TryAdd(key, item.Value);
                }
                else
                {
                    uniqueParams.TryAdd(key, item.Value);
                }
            }
        }

        return new Tuple<ExpandoObject, ExpandoObject>(uniqueParams, apiVersionParam);
    }

    private static ExpandoObject CopyUniqueQueryParams(ExpandoObject original)
    {
        ExpandoObject clone = new ExpandoObject();
        IDictionary<string, object> convertedClone = clone;

        foreach (KeyValuePair<string, object> kvp in original)
        {
            convertedClone.Add(kvp);
        }

        return clone;
    }

    private Uri GetCursorNextPageUri(string routeName, Tuple<ExpandoObject, ExpandoObject> baseUrlParams, PageOptionsCursor pageOptions, string endCursor)
    {
        ExpandoObject baseParams = CopyUniqueQueryParams(baseUrlParams.Item1);

        baseParams.TryAdd(nameof(pageOptions.First), pageOptions.First ?? pageOptions.Last);
        baseParams.TryAdd(nameof(pageOptions.After), endCursor);

        return GenerateLink(routeName, baseParams, baseUrlParams.Item2);
    }

    private Uri GetCursorPreviousPageUri(string routeName, Tuple<ExpandoObject, ExpandoObject> baseUrlParams, PageOptionsCursor pageOptions, string startCursor)
    {
        ExpandoObject baseParams = CopyUniqueQueryParams(baseUrlParams.Item1);

        baseParams.TryAdd(nameof(pageOptions.Last), pageOptions.First ?? pageOptions.Last);
        baseParams.TryAdd(nameof(pageOptions.Before), startCursor);

        return GenerateLink(routeName, baseParams, baseUrlParams.Item2);
    }

    private Uri GetCursorFirstPageUri(string routeName, Tuple<ExpandoObject, ExpandoObject> baseUrlParams, PageOptionsCursor pageOptions)
    {
        ExpandoObject baseParams = CopyUniqueQueryParams(baseUrlParams.Item1);

        baseParams.TryAdd(nameof(pageOptions.First), pageOptions.First ?? pageOptions.Last);

        return GenerateLink(routeName, baseParams, baseUrlParams.Item2);
    }

    private Uri GetCursorLastPageUri(string routeName, Tuple<ExpandoObject, ExpandoObject> baseUrlParams, PageOptionsCursor pageOptions)
    {
        ExpandoObject baseParams = CopyUniqueQueryParams(baseUrlParams.Item1);

        baseParams.TryAdd(nameof(pageOptions.Last), pageOptions.First ?? pageOptions.Last);

        return GenerateLink(routeName, baseParams, baseUrlParams.Item2);
    }

    private Uri GenerateLink(string routeName, ExpandoObject queryParams, IDictionary<string, object> apiKvp)
    {
        if (apiKvp.TryGetValue(ApiVersionQueryKey, out object versionValue))
        {
            queryParams.TryAdd(ApiVersionQueryKey, versionValue);
        }

        Uri link = new Uri(_linkGenerator.GetUriByRouteValues(_actionCtx.ActionContext.HttpContext, routeName, queryParams));

        return link;
    }

    public PaginatedResultVm<T> GetOffsetPaginatedResult<T>(PageOptionsOffset pageOptions, int totalCount, string routeName, ICollection<T> items) where T : class
    {
        if (string.IsNullOrEmpty(routeName))
        {
            throw new ArgumentNullException(nameof(routeName));
        }

        ICollection<T> resultItems = items ?? new List<T>();

        Tuple<ExpandoObject, ExpandoObject> baseQueryParams = GetOffsetUniqueQueryParams(pageOptions);

        double totalPages = ((double)totalCount / (double)pageOptions.PageSize);
        int roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));

        PageInfoVm pageInfo = new PageInfoVm()
        {
            Count = items.Count,
            NextPageUrl = pageOptions.PageNumber >= 1 && pageOptions.PageNumber < roundedTotalPages
                ? GetPageUri(baseQueryParams, new PageOptionsOffset(pageOptions.PageNumber + 1, pageOptions.PageSize), routeName)
                : null,
            PreviousPageUrl = pageOptions.PageNumber - 1 >= 1 && pageOptions.PageNumber <= roundedTotalPages
                ? GetPageUri(baseQueryParams, new PageOptionsOffset(pageOptions.PageNumber - 1, pageOptions.PageSize), routeName)
                : null,
            FirstPageUrl = GetPageUri(baseQueryParams, new PageOptionsOffset(1, pageOptions.PageSize), routeName),
            LastPageUrl = GetPageUri(baseQueryParams, new PageOptionsOffset(roundedTotalPages, pageOptions.PageSize), routeName)
        };

        pageInfo.HasNextPage = pageInfo.NextPageUrl != null;
        pageInfo.HasPreviousPage = pageInfo.PreviousPageUrl != null;

        PaginatedResultVm<T> result = new PaginatedResultVm<T>(totalCount, pageInfo, resultItems);

        return result;
    }

    private Tuple<ExpandoObject, ExpandoObject> GetOffsetUniqueQueryParams(PageOptionsOffset pageOptions)
    {
        ExpandoObject uniqueParams = new ExpandoObject();
        ExpandoObject apiVersionParam = new ExpandoObject();

        foreach (KeyValuePair<string, StringValues> item in _actionCtx.ActionContext.HttpContext.Request.Query)
        {
            string key = item.Key.ToLowerInvariant();

            if (key != nameof(pageOptions.PageNumber).ToLowerInvariant()
                && key != nameof(pageOptions.PageSize).ToLowerInvariant())
            {
                if (key == ApiVersionQueryKey)
                {
                    apiVersionParam.TryAdd(key, item.Value);
                }
                else
                {
                    uniqueParams.TryAdd(key, item.Value);
                }
            }
        }

        return new Tuple<ExpandoObject, ExpandoObject>(uniqueParams, apiVersionParam);
    }

    private Uri GetPageUri(Tuple<ExpandoObject, ExpandoObject> baseUrlParams, PageOptionsOffset pageOptions, string routeName)
    {
        ExpandoObject baseParams = CopyUniqueQueryParams(baseUrlParams.Item1);

        baseParams.TryAdd(nameof(pageOptions.PageSize), pageOptions.PageSize);
        baseParams.TryAdd(nameof(pageOptions.PageNumber), pageOptions.PageNumber);

        return GenerateLink(routeName, baseParams, baseUrlParams.Item2);
    }
}
