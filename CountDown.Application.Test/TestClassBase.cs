﻿using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using CountDown.Application.Controllers;
using CountDown.Application.Services;
using CountDown.Application.Test.Services;
using CountDown.Application.Test.Views;
using CountDown.Application.Test.Views.Dialogs;
using CountDown.Application.ViewModels;
using CountDown.Application.ViewModels.Dialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CountDown.Application.Test
{
    [TestClass]
    public abstract class TestClassBase
    {
        private readonly CompositionContainer container;


        protected TestClassBase()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new TypeCatalog(
                typeof(ApplicationController), typeof(DataController),
                typeof(ShellService), typeof(DataService),
                typeof(MainViewModel), typeof(ShellViewModel), typeof(AlertDialogViewModel), typeof(AboutDialogViewModel)
            ));
            catalog.Catalogs.Add(new TypeCatalog(
                typeof(MockPresentationService),
                typeof(MockMessageService), typeof(MockFileDialogService),
                typeof(MockShellView), typeof(MockMainView),
                typeof(MockAboutDialogView), typeof(MockAlertDialogView)
            ));
            container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);
        }


        protected CompositionContainer Container { get { return container; } }


        [TestInitialize]
        public void TestInitialize()
        {
            OnTestInitialize();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            OnTestCleanup();
        }

        protected virtual void OnTestInitialize() { }

        protected virtual void OnTestCleanup() { }
    }
}
