using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestTask.Features.Dogs.Config;
using TestTask.Features.Dogs.Data;
using TestTask.Features.Dogs.Presenter;
using UnityEngine;

namespace TestTask.Tests
{
    public sealed class DogBreedsPresenterTests
    {
        private DogsConfig _config;
        private FakeDogBreedsView _view;
        private FakeBreedPopupView _popup;
        private FakeBreedItemFactory _items;
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
            _config = ScriptableObject.CreateInstance<DogsConfig>();
            _view = new FakeDogBreedsView();
            _popup = new FakeBreedPopupView();
            _items = new FakeBreedItemFactory();
            _service = new FakeDogBreedsService();
            _presenter = new DogBreedsPresenter(_view, _popup, _items, _service, _config);
            _presenter.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
            UnityEngine.Object.DestroyImmediate(_config);
        }

        private void EnterWithBreeds()
        {
            _presenter.Enter();
            _service.ListRequests[0].Source.TrySetResult(Breeds);
        }

        private FakeBreedItemView Item(int index) => _items.Created[index];

        [Test]
        public void Initialize_HidesViewAndPopup()
        {
            Assert.IsFalse(_view.IsVisible);
            Assert.IsFalse(_popup.IsOpen);
            Assert.AreEqual(1, _popup.HideImmediateCount);
        }

        [Test]
        public void Enter_LoadsBreedList_AndBuildsNumberedItems()
        {
            _presenter.Enter();

            Assert.IsTrue(_view.IsVisible);
            Assert.IsTrue(_view.IsLoading);

            _service.ListRequests[0].Source.TrySetResult(Breeds);

            Assert.AreEqual(2, _items.Created.Count);
            Assert.AreEqual(string.Format(_config.ItemFormat, 1, "Akita"), Item(0).Label);
            Assert.AreEqual(string.Format(_config.ItemFormat, 2, "Beagle"), Item(1).Label);
            Assert.IsFalse(_view.IsLoading);
        }

        [Test]
        public void ReEnter_ReturnsOldItemsToFactory()
        {
            EnterWithBreeds();
            _presenter.Exit();

            _presenter.Enter();

            Assert.IsTrue(Item(0).IsDespawned);
            Assert.IsTrue(Item(1).IsDespawned);
        }

        [Test]
        public void ListError_ShowsErrorMessageFromConfig()
        {
            _presenter.Enter();

            _service.ListRequests[0].Source.TrySetException(new Exception("network down"));

            Assert.AreEqual(_config.ListErrorMessage, _view.ErrorText);
            Assert.IsFalse(_view.IsLoading);
        }

        [Test]
        public void ItemClick_LoadsDetails_AndShowsPopup()
        {
            EnterWithBreeds();

            Item(0).Click();
            Assert.IsTrue(Item(0).IsLoading);
            Assert.AreEqual("a", _service.DetailsRequests[0].Id);

            _service.DetailsRequests[0].Source.TrySetResult(new BreedDetails("Akita", "Large dog"));

            Assert.IsTrue(_popup.IsOpen);
            Assert.AreEqual("Akita", _popup.Title);
            Assert.AreEqual("Large dog", _popup.Description);
            Assert.IsFalse(Item(0).IsLoading);
        }

        [Test]
        public void EmptyDescription_IsReplacedWithTextFromConfig()
        {
            EnterWithBreeds();

            Item(0).Click();
            _service.DetailsRequests[0].Source.TrySetResult(new BreedDetails("Akita", string.Empty));

            Assert.AreEqual(_config.EmptyDescription, _popup.Description);
        }

        [Test]
        public void ClickOnAnotherBreed_CancelsPreviousRequest()
        {
            EnterWithBreeds();

            Item(0).Click();
            Item(1).Click();

            Assert.IsTrue(_service.DetailsRequests[0].Token.IsCancellationRequested);
            Assert.IsFalse(Item(0).IsLoading, "Спиннер у отменённой породы должен погаснуть");

            _service.DetailsRequests[1].Source.TrySetResult(new BreedDetails("Beagle", "Hound"));
            Assert.AreEqual("Beagle", _popup.Title);
        }

        [Test]
        public void DetailsError_ShowsErrorPopupFromConfig()
        {
            EnterWithBreeds();

            Item(0).Click();
            _service.DetailsRequests[0].Source.TrySetException(new Exception("network down"));

            Assert.IsTrue(_popup.IsOpen);
            Assert.AreEqual(_config.DetailsErrorTitle, _popup.Title);
            Assert.AreEqual(_config.DetailsErrorMessage, _popup.Description);
        }

        [Test]
        public void PopupCloseRequest_HidesPopup()
        {
            EnterWithBreeds();
            Item(0).Click();
            _service.DetailsRequests[0].Source.TrySetResult(new BreedDetails("Akita", "Large dog"));

            _popup.RequestClose();

            Assert.IsFalse(_popup.IsOpen);
            Assert.AreEqual(1, _popup.HideCount);
        }

        [Test]
        public void Exit_CancelsAllRequests_AndHidesPopup()
        {
            EnterWithBreeds();
            Item(0).Click();

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
