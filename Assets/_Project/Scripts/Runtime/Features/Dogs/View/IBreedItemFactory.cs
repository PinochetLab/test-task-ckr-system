namespace TestTask.Features.Dogs.View
{
    /// <summary>Создаёт строки списка пород и возвращает их обратно (в игре — через пул).</summary>
    public interface IBreedItemFactory
    {
        IBreedItemView Spawn();

        void Despawn(IBreedItemView item);
    }
}
