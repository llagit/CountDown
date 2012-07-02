﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CountDown.Application.ViewModels;
using System.Windows.Input;
using BigEgg.Framework.Applications;
using BigEgg.Framework.UnitTesting;

namespace CountDown.Application.Test.ViewModels
{
    [TestClass]
    public class MainViewModelTest : TestClassBase
    {
        [TestMethod]
        public void PropertiesWithNotification()
        {
            MainViewModel mainViewModel = Container.GetExportedValue<MainViewModel>();

            ICommand exitCommand = new DelegateCommand(() => { });
            AssertHelper.PropertyChangedEvent(mainViewModel, x => x.ExitCommand, () => mainViewModel.ExitCommand = exitCommand);
            Assert.AreEqual(exitCommand, mainViewModel.ExitCommand);

            ICommand settingCommand = new DelegateCommand(() => { });
            AssertHelper.PropertyChangedEvent(mainViewModel, x => x.SettingCommand, () => mainViewModel.SettingCommand = settingCommand);
            Assert.AreEqual(exitCommand, mainViewModel.ExitCommand);
        }

        [TestMethod]
        public void SelectLanguageTest()
        {
            MainViewModel mainViewModel = Container.GetExportedValue<MainViewModel>();
            Assert.IsNull(mainViewModel.NewLanguage);

            mainViewModel.ChineseCommand.Execute(null);
            Assert.AreEqual("zh-CN", mainViewModel.NewLanguage.Name);

            mainViewModel.EnglishCommand.Execute(null);
            Assert.AreEqual("en-US", mainViewModel.NewLanguage.Name);
        }
    }
}