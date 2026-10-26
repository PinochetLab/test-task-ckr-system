using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestTask.Core.Content
{
    public interface ISpriteLoader
    {
        UniTask<Sprite> LoadAsync(string url, CancellationToken cancellationToken);
    }
}
