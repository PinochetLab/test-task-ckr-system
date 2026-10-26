using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestTask.Features.Dogs.Data;

namespace TestTask.Features.Dogs.Service
{
    public interface IDogBreedsService
    {
        UniTask<IReadOnlyList<BreedInfo>> GetBreedsAsync(CancellationToken cancellationToken);

        UniTask<BreedDetails> GetBreedAsync(string id, CancellationToken cancellationToken);
    }
}
