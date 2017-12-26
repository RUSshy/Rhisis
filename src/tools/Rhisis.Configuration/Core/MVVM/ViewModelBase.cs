﻿using Rhisis.Configuration.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Rhisis.Configuration.Core.MVVM
{
    /// <summary>
    /// Provides a basic ViewModel abstraction.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private Window _currentWindow;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the dialog service.
        /// </summary>
        protected IDialogService DialogService { get; set; }

        /// <summary>
        /// On property changed event.
        /// </summary>
        /// <param name="propName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        /// <summary>
        /// Notify and sets the property value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage">Property reference</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        protected bool NotifyPropertyChanged<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value) == true)
                return false;

            storage = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Show this View model's interface.
        /// </summary>
        public void ShowDialog()
        {
            var window = ViewFactory.CreateInstance(this.GetType()) as Window;

            this._currentWindow = window;
            this.DialogService = new DialogService();
            window.DataContext = this;
            window.ShowDialog();
        }

        /// <summary>
        /// Close the current View model's window.
        /// </summary>
        public void Close()
        {
            this._currentWindow.Close();
        }
    }
}
