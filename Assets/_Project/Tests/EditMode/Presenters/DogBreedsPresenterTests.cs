using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestTask.Features.Dogs.Data;
using TestTask.Features.Dogs.Presenter;

namespace TestTask.Tests
{
    public sealed class DogBreedsPresenterTests
    {
        private FakeDogBreedsView _view;
        private FakeBreedPopupView _popup;
        private FakeDogBreedsService _service;
        private DogBreedsPresenter _presenter;

        private static readonly IReadOnlyList<BreedInfo> Breeds = new List<BreedInfo>
        {
            new BreedInfo("a", "Akita"),
            new BreedInfo("b", "Beagle")
        };

        [SetUp]
        public void SetUp()
        {
            _view = new FakeDogBreedsView();
            _popup = new FakeBreedPopupView();
            _service = new FakeDogBreedsService();
            _presenter = new DogBreedsPresenter(_view, _popup, _service);
            _presenter.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
        }

        private void EnterWithBreeds()
        {
            _presenter.Enter();
            _service.ListRequests[0].Source.TrySetResult(Breeds);
        }

        [Test]
        public void Initialize_HidesViewAndPopup()
        {
            Assert.IsFalse(_view.IsVisible);
            Assert.IsFalse(_popup.IsOpen);
            Assert.AreEqual(1, _popup.HideImmediateCount);
        }

        [Test]
        public void Enter_LoadsBreedList()
        {
            _presenter.Enter();

            Assert.IsTrue(_view.IsVisible);
            Assert.IsTrue(_view.IsLoading);

            _service.ListRequests[0].Source.TrySetResult(Breeds);

            Assert.AreSame(Breeds, _view.Breeds);
            Assert.IsFalse(_view.IsLoading);
        }

        [Test]
        public void ListError_ShowsErrorMessage()
        {
            _presenter.Enter();

            _service.ListRequests[0].Source.TrySetException(new Exception("network down"));

            Assert.AreEqual("Не удалось загрузить список пород", _view.ErrorText);
            Assert.IsFalse(_view.IsLoading);
        }

        [Test]
        public void BreedClick_LoadsDetails_AndShowsPopup()
        {
            EnterWithBreeds();

            _view.ClickBreed("a");
            Assert.IsTrue(_view.ItemLoading["a"]);
            Assert.AreEqual("a", _service.DetailsRequests[0].Id);

            _service.DetailsRequests[0].Source.TrySetResult(new BreedDetails("Akita", "Large dog"));

            Assert.IsTrue(_popup.IsOpen);
            Assert.AreEqual("Akita", _popup.ShownDetails.Name);
            Assert.IsFalse(_view.ItemLoading["a"]);
        }

        [Test]
        public void ClickOnAnotherBreed_CancelsPreviousRequest()
        {
            EnterWithBreeds();

            _view.ClickBreed("a");
            _view.ClickBreed("b");

            Assert.IsTrue(_service.DetailsRequests[0].Token.IsCancellationRequested);
            Assert.IsFalse(_view.ItemLoading["a"], "Спиннер у отменённой породы должен погаснуть");

            _service.DetailsRequests[1].Source.TrySetResult(new BreedDetails("Beagle", "Hound"));
            Assert.AreEqual("Beagle", _popup.ShownDetails.Name);
        }

        [Test]
        public void DetailsError_ShowsErrorPopup()
        {
            EnterWithBreeds();

            _view.ClickBreed("a");
            _service.DetailsRequests[0].Source.TrySetException(new Exception("network down"));

            Assert.IsTrue(_popup.IsOpen);
            Assert.AreEqual("Ошибка", _popup.ShownDetails.Name);
        }

        [Test]
        public void PopupCloseRequest_HidesPopup()
        {
            EnterWithBreeds();
            _view.ClickBreed("a");
            _service.DetailsRequests[0].Source.TrySetResult(new BreedDetails("Akita", "Large dog"));

            _popup.RequestClose();

            Assert.IsFalse(_popup.IsOpen);
            Assert.AreEqual(1, _popup.HideCount);
        }

        [Test]
        public void Exit_CancelsAllRequests_AndHidesPopup()
        {
            EnterWithBreeds();
            _view.ClickBreed("a");

            _presenter.Exit();

            Assert.IsTrue(_service.ListRequests[0].Token.IsCancellationRequested);
            Assert.IsTrue(_service.DetailsRequests[0].Token.IsCancellationRequested);
            Assert.IsFalse(_view.IsVisible);
            Assert.IsFalse(_popup.IsOpen);
        }

        [Test]
        public void Exit_WhileListLoading_DoesNotTouchViewAfterCancel()
        {
            _presenter.Enter();
            Assert.IsTrue(_view.IsLoading);

            _presenter.Exit();

            // После ухода с вкладки view может быть уже уничтожена (выгрузка сцены),
            // поэтому отменённая загрузка не должна гасить индикатор.
            Assert.IsTrue(_service.ListRequests[0].Token.IsCancellationRequested);
            Assert.IsTrue(_view.IsLoading);
        }
    }
}
