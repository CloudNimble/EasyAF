using CloudNimble.Breakdance.AspNetCore;
using Microsoft.Restier.Breakdance;
using Microsoft.Restier.Core;
using Simple.OData.Client;
using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Restier.Breakdance
{

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public class EasyAFRestierTestBase<T> : RestierBreakdanceTestBase<T>
    //    where T : ApiBase
    //{

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    public async Task<ODataBatch> GetAuthenticatedODataBatch() => new(new ODataClientSettings(await GetAuthenticatedHttpClient().ConfigureAwait(false)));

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    public async Task<ODataClient> GetAuthenticatedODataClient() => new(new ODataClientSettings(await GetAuthenticatedHttpClient().ConfigureAwait(false)));

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="settings"></param>
    //    /// <returns></returns>
    //    public ODataClient GetAuthenticatedODataClient(ODataClientSettings settings) => new(settings);

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    public ODataBatch GetODataBatch() => new(new ODataClientSettings(GetHttpClient()));

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    public ODataClient GetODataClient() => new(new ODataClientSettings(GetHttpClient()));

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="settings"></param>
    //    /// <returns></returns>
    //    public ODataClient GetODataClient(ODataClientSettings settings) => new(settings);

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    public async Task<HttpClient> GetAuthenticatedHttpClient() => GetHttpClient(new AuthenticationHeaderValue("Bearer", await GetAccessToken().ConfigureAwait(false)), $"{WebApiConstants.RoutePrefix}/");

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <returns></returns>
    //    /// <exception cref="Exception"></exception>
    //    public async Task<string> GetAccessToken()
    //    {
    //        var tokenHelper = GetScopedService<TokenHelper>();
    //        var apiToken = await tokenHelper.GetClientCredentialsTokenAsync().ConfigureAwait(false);
    //        return apiToken is null ? throw new Exception("Unable to get a token from the identity provider.") : apiToken.AccessToken;
    //    }

    //}

}
