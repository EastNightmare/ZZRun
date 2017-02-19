using System;
using System.Collections.Generic;

namespace MVVMDynamic
{
    public abstract class ViewModelBase : IViewModel
    {
        private Dictionary<string, PropertyChangedEventArgs> _events =
            new Dictionary<string, PropertyChangedEventArgs>();

        private bool _isUpdating;

        public void NotifyPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            _events[propertyName] = new PropertyChangedEventArgs(propertyName, oldValue, newValue);
            if (!_isUpdating)
            {
                DropEvents();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void HaltEvents()
        {
            _isUpdating = true;
        }

        public void DropEvents()
        {
            if (PropertyChanged != null)
            {
                foreach (var pair in _events)
                {
                    PropertyChanged(this, pair.Value);
                }
            }

            _events.Clear();
            _isUpdating = false;
        }

        public void UnsubsribeAll()
        {
            if (PropertyChanged != null)
            {
                foreach (Delegate @delegate in PropertyChanged.GetInvocationList())
                {
                    PropertyChangedEventHandler handler = @delegate as PropertyChangedEventHandler;
                    PropertyChanged -= handler;
                }
            }
        }
    }
}