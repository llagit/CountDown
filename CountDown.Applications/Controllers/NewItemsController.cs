﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using BigEgg.Framework.Applications;
using BigEgg.Framework.Foundation;
using CountDown.Applications.Domain;
using CountDown.Applications.Models;
using CountDown.Applications.Properties;
using CountDown.Applications.Services;
using CountDown.Applications.ViewModels;
using CountDown.Applications.ViewModels.NewItemViewModels;
using CountDown.Applications.Views.NewItemViews;

namespace CountDown.Applications.Controllers
{
    [Export]
    public class NewItemsController : Controller
    {
        private readonly CompositionContainer container;
        private readonly IDataService dataService;
        private readonly DelegateCommand addItemCommand;

        private readonly Dictionary<INewItemView, NewItemModelBase> viewDictionary;
        private INewItemModel activeNewItemModel;
        private NewItemsViewModel newItemsViewModel;

        [ImportingConstructor]
        public NewItemsController(CompositionContainer container, IDataService dataService)
        {
            this.container = container;
            this.dataService = dataService;

            this.viewDictionary = new Dictionary<INewItemView, NewItemModelBase>();

            this.addItemCommand = new DelegateCommand(AddItemExcute, CanAddItemExcute);
        }

        #region Methods
        protected override void OnInitialize()
        {
            this.newItemsViewModel = container.GetExportedValue<NewItemsViewModel>();
            AddWeakEventListener(this.newItemsViewModel, NewItemsViewModelPropertyChanged);

            IAlertAtTimeView alertAtTimeView = this.container.GetExportedValue<IAlertAtTimeView>();
            AlertAtTimeViewModel alertAtTimeViewModel = new AlertAtTimeViewModel(alertAtTimeView, this.dataService);
            alertAtTimeView.Title = alertAtTimeViewModel.Title;

            this.newItemsViewModel.NewItemViews.Add(alertAtTimeView);
            this.viewDictionary.Add(alertAtTimeView, alertAtTimeViewModel.NewItem);

            ICountDownAlertView countDownAlertView = this.container.GetExportedValue<ICountDownAlertView>();
            CountDownAlertViewModel countDownAlertViewModel = new CountDownAlertViewModel(countDownAlertView, this.dataService);
            countDownAlertView.Title = countDownAlertViewModel.Title;

            this.newItemsViewModel.NewItemViews.Add(countDownAlertView);
            this.viewDictionary.Add(countDownAlertView, countDownAlertViewModel.NewItem);

            this.newItemsViewModel.AddItemCommand = this.addItemCommand;

            if (string.IsNullOrEmpty(Settings.Default.SelectNewItem))
            {
                this.newItemsViewModel.ActiveNewItemView = this.newItemsViewModel.NewItemViews.First();
            }
            else
            {
                foreach (object view in this.newItemsViewModel.NewItemViews)
                {
                    INewItemViewModel viewModel = ViewHelper.GetViewModel(view as IView) as INewItemViewModel;
                    if (viewModel.Title == Settings.Default.SelectNewItem)
                    {
                        this.newItemsViewModel.ActiveNewItemView = view;
                    }
                }
            }

        }

        public void Shutdown()
        {
            this.viewDictionary.Clear();

            INewItemViewModel viewModel = ViewHelper.GetViewModel(this.newItemsViewModel.ActiveNewItemView as IView) as INewItemViewModel;
            if (viewModel != null) { Settings.Default.SelectNewItem = viewModel.Title; }
            else { Settings.Default.SelectNewItem = string.Empty; }
        }


        #region Command Methods
        private bool CanAddItemExcute()
        {
            INewItemView view = this.newItemsViewModel.ActiveNewItemView as INewItemView;
            if (view == null) { return false; }
            NewItemModelBase model = this.viewDictionary[view];
            if (model == null) { return false; }

            if (string.IsNullOrEmpty(model.Validate())) { return true; }
            else { return false; }
        }

        private void AddItemExcute()
        {
            INewItemView view = this.newItemsViewModel.ActiveNewItemView as INewItemView;
            NewItemModelBase model = this.viewDictionary[view];

            AlertItem item = model.ToAlertItem(Settings.Default.DefaultAlertBeforeMinutes);
            this.dataService.Items.Add(item);

            int index = this.dataService.Branches.IndexOf(model.NoticeBranch);
            if (index > 0)
            {
                this.dataService.Branches.Move(index, 0);
            }
            else if (index < 0)
            {
                this.dataService.Branches.Add(model.NoticeBranch);
            }

            if (Settings.Default.ResetCountDownData) { model.Clean(); }
        }
        #endregion


        private void NewItemsViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveNewItemView")
            {
                if (this.activeNewItemModel != null) { RemoveWeakEventListener(this.activeNewItemModel as INotifyPropertyChanged, ActiveNewItemModelPropertyChanged); }

                INewItemView view = this.newItemsViewModel.ActiveNewItemView as INewItemView;
                this.activeNewItemModel = this.viewDictionary[view];

                if (this.activeNewItemModel != null) { AddWeakEventListener(this.activeNewItemModel as INotifyPropertyChanged, ActiveNewItemModelPropertyChanged); }

                UpdateCommand();
            }
        }

        private void ActiveNewItemModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "NoticeBranch") || (e.PropertyName == "Notice"))
            {
                foreach (NewItemModelBase model in this.viewDictionary.Values)
                {
                    model.NoticeBranch = this.activeNewItemModel.NoticeBranch;
                    model.Notice = this.activeNewItemModel.Notice;
                }
            }

            UpdateCommand();
        }

        private void UpdateCommand()
        {
            this.addItemCommand.RaiseCanExecuteChanged();
        }
        #endregion
    }
}
