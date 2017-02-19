using System;

namespace MVVMDynamic
{
    public static class ViewModelExtensions
    {
        public static void Update(this IViewModel viewModel, Action action)
        {
            ViewModelBase viewModelBase = viewModel as ViewModelBase;
            if (viewModelBase == null) return;
            viewModelBase.HaltEvents();
            action();
            viewModelBase.DropEvents();
        }

        public static void Update<T>(this T viewModel, Action<T> action) where T : IViewModel
        {
            Update(viewModel, () => action(viewModel));
        }

        private static void UnsubscribeAll(this IViewModel viewModel)
        {
            ViewModelBase viewModelBase = viewModel as ViewModelBase;
            if (viewModelBase == null) return;
            viewModelBase.UnsubsribeAll();
        }

        public static PropertyChangedEventArgs<T> Of<T>(this PropertyChangedEventArgs args)
        {
            return new PropertyChangedEventArgs<T>(args.PropertyName, (T) args.OldValue, (T) args.NewValue);
        }
    }
}