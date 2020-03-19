using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SecurePipelineScan.VstsService
{
    public interface IVstsRestClient
    {
        Task<TResponse> GetAsync<TResponse>(IVstsRequest<TResponse> request);
        Task<TResponse> GetAsync<TResponse>(Uri url);
        IEnumerable<TResponse> Get<TResponse>(IEnumerableRequest<TResponse> request);
        Task<TResponse> PostAsync<TInput, TResponse>(IVstsRequest<TInput, TResponse> request, TInput body);

        Task<TResponse> PatchAsync<TInput, TResponse>(IVstsRequest<TInput, TResponse> request, TInput body);
        Task<TResponse> PutAsync<TInput, TResponse>(IVstsRequest<TInput, TResponse> request, TInput body);
        Task DeleteAsync(IVstsRequest request);
    }
}