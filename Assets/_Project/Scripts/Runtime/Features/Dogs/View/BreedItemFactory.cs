using System;

namespace TestTask.Features.Dogs.View
{
    /// <summary>Строки списка пород из пула: новые встают в конец списка.</summary>
    public sealed class BreedItemFactory : IBreedItemFactory
    {
        private readonly BreedItemView.Pool _pool;

        public BreedItemFactory(BreedItemView.Pool pool)
        {
            _pool = pool;
        }

        public IBreedItemView Spawn()
        {
            BreedItemView item = _pool.Spawn();
            item.transform.SetAsLastSibling();
            return item;
        }

        public void Despawn(IBreedItemView item)
        {
            if (item is not BreedItemView view)
                throw new ArgumentException($"Expected {nameof(BreedItemView)}", nameof(item));

            // Строка могла быть уничтожена вместе со сценой — возвращать в пул уже нечего.
            if (view != null)
                _pool.Despawn(view);
        }
    }
}
