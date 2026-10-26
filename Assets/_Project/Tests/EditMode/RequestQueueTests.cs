using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestTask.Core.Network;

namespace TestTask.Tests
{
    public sealed class RequestQueueTests
    {
        private RequestQueue _queue;

        [SetUp]
        public void SetUp()
        {
            _queue = new RequestQueue();
        }

        [TearDown]
        public void TearDown()
        {
            _queue.Dispose();
        }

        [Test]
        public void Requests_RunOneByOne_InOrder()
        {
            var started = new List<int>();
            var first = new UniTaskCompletionSource<int>();
            var second = new UniTaskCompletionSource<int>();

            UniTask<int> task1 = _queue.Enqueue(_ => { started.Add(1); return first.Task; }, CancellationToken.None);
            UniTask<int> task2 = _queue.Enqueue(_ => { started.Add(2); return second.Task; }, CancellationToken.None);

            CollectionAssert.AreEqual(new[] { 1 }, started);
            Assert.AreEqual(1, _queue.PendingCount);

            first.TrySetResult(10);
            CollectionAssert.AreEqual(new[] { 1, 2 }, started);
            Assert.AreEqual(UniTaskStatus.Succeeded, task1.Status);

            second.TrySetResult(20);
            Assert.AreEqual(UniTaskStatus.Succeeded, task2.Status);
            Assert.IsFalse(_queue.IsBusy);
        }

        [Test]
        public void CancelPending_RemovesFromQueue_AndNeverStarts()
        {
            var first = new UniTaskCompletionSource<int>();
            bool secondStarted = false;
            var cts = new CancellationTokenSource();

            _queue.Enqueue(_ => first.Task, CancellationToken.None);
            UniTask<int> task2 = _queue.Enqueue(_ => { secondStarted = true; return UniTask.FromResult(2); }, cts.Token);

            cts.Cancel();

            Assert.AreEqual(0, _queue.PendingCount);
            Assert.AreEqual(UniTaskStatus.Canceled, task2.Status);

            first.TrySetResult(1);
            Assert.IsFalse(secondStarted);
        }

        [Test]
        public void CancelActive_CancelsRequestToken_AndQueueContinues()
        {
            var cts = new CancellationTokenSource();
            var running = new UniTaskCompletionSource<int>();
            bool secondStarted = false;

            UniTask<int> task1 = _queue.Enqueue(token =>
            {
                token.Register(() => running.TrySetCanceled(token));
                return running.Task;
            }, cts.Token);

            UniTask<int> task2 = _queue.Enqueue(_ => { secondStarted = true; return UniTask.FromResult(2); }, CancellationToken.None);

            Assert.IsFalse(secondStarted);

            cts.Cancel();

            Assert.AreEqual(UniTaskStatus.Canceled, task1.Status);
            Assert.IsTrue(secondStarted);
            Assert.AreEqual(UniTaskStatus.Succeeded, task2.Status);
        }

        [Test]
        public void AlreadyCancelledToken_ReturnsCancelledTask()
        {
            bool started = false;
            var cts = new CancellationTokenSource();
            cts.Cancel();

            UniTask<int> task = _queue.Enqueue(_ => { started = true; return UniTask.FromResult(1); }, cts.Token);

            Assert.AreEqual(UniTaskStatus.Canceled, task.Status);
            Assert.IsFalse(started);
        }

        [Test]
        public void FailedRequest_DoesNotBlockQueue()
        {
            UniTask<int> failed = _queue.Enqueue<int>(_ => throw new System.InvalidOperationException("boom"), CancellationToken.None);
            UniTask<int> next = _queue.Enqueue(_ => UniTask.FromResult(5), CancellationToken.None);

            Assert.AreEqual(UniTaskStatus.Faulted, failed.Status);
            Assert.Throws<System.InvalidOperationException>(() => failed.GetAwaiter().GetResult());
            Assert.AreEqual(UniTaskStatus.Succeeded, next.Status);
        }
    }
}
