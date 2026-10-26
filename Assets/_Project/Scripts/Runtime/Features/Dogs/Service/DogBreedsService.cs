using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Core.Network;
using TestTask.Features.Dogs.Config;
using TestTask.Features.Dogs.Data;

namespace TestTask.Features.Dogs.Service
{
    public sealed class DogBreedsService : IDogBreedsService
    {
        private readonly IRequestQueue _queue;
        private readonly IHttpClient _http;
        private readonly IJsonParser _json;
        private readonly DogsConfig _config;

        public DogBreedsService(IRequestQueue queue, IHttpClient http, IJsonParser json, DogsConfig config)
        {
            _queue = queue;
            _http = http;
            _json = json;
            _config = config;
        }

        public async UniTask<IReadOnlyList<BreedInfo>> GetBreedsAsync(CancellationToken cancellationToken)
        {
            string url = $"{_config.BaseUrl}/breeds";
            string body = await _queue.Enqueue(token => _http.GetTextAsync(url, token), cancellationToken);

            var response = _json.Parse<BreedsResponseDto>(body);
            if (response?.Data == null)
                throw new FormatException("Breeds response has no data");

            return response.Data
                .Where(item => item?.Attributes != null)
                .Take(_config.BreedsCount)
                .Select(item => new BreedInfo(item.Id, item.Attributes.Name))
                .ToList();
        }

        public async UniTask<BreedDetails> GetBreedAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Breed id is empty", nameof(id));

            string url = $"{_config.BaseUrl}/breeds/{Uri.EscapeDataString(id)}";
            string body = await _queue.Enqueue(token => _http.GetTextAsync(url, token), cancellationToken);

            var response = _json.Parse<BreedResponseDto>(body);
            BreedAttributesDto attributes = response?.Data?.Attributes;
            if (attributes == null)
                throw new FormatException($"Breed {id} response has no attributes");

            return new BreedDetails(attributes.Name, attributes.Description);
        }
    }
}
