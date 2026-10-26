using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace TestTask.Tests
{
    /// <summary>
    /// Планировщик с ручным временем: задачи выполняются только в AdvanceBy.
    /// Позволяет проверять таймеры без ожидания реального времени.
    /// </summary>
    internal sealed class ManualScheduler : IScheduler
    {
        private sealed class Item
        {
            public DateTimeOffset Due;
            public long Order;
            public Action Action;
        }

        private readonly List<Item> _items = new List<Item>();
        private long _order;

        public DateTimeOffset Now { get; private set; } = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public IDisposable Schedule(Action action)
        {
            return Schedule(TimeSpan.Zero, action);
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action)
        {
            var item = new Item
            {
                Due = Now + (dueTime < TimeSpan.Zero ? TimeSpan.Zero : dueTime),
                Order = _order++,
                Action = action
            };

            _items.Add(item);
            return Disposable.Create(() => _items.Remove(item));
        }

        public void AdvanceBy(TimeSpan time)
        {
            DateTimeOffset target = Now + time;

            while (true)
            {
                Item next = _items
                    .Where(item => item.Due <= target)
                    .OrderBy(item => item.Due)
                    .ThenBy(item => item.Order)
                    .FirstOrDefault();

                if (next == null)
                    break;

                _items.Remove(next);
                Now = next.Due;
                next.Action();
            }

            Now = target;
        }

        public void AdvanceBySeconds(double seconds)
        {
            AdvanceBy(TimeSpan.FromSeconds(seconds));
        }
    }
}
