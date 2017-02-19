using System;
using System.Linq.Expressions;
using System.Reflection;
using MVVMDynamic.Internal;

namespace MVVMDynamic
{
    public class Binder
    {
        private MultiDictionary<IViewModel, PropertyChangedEventHandler> _delegates =
            new MultiDictionary<IViewModel, PropertyChangedEventHandler>();

        /// <summary>
        /// Binds property to specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <typeparam name="TPrp">Type of property</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="propertyExpression">lambda with property member. (vm => vm.Property) </param>
        /// <param name="action">Action that should be called on property changed</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindProperty<TVm, TPrp>(TVm viewModel, Expression<Func<TVm, TPrp>> propertyExpression,
            Action<TVm> action, bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            PropertyInfo prop = ReflectionExtensions.MemberInfo(propertyExpression) as PropertyInfo;
            string name = prop.Name;
            BindNameArgs(viewModel, name, args => action(viewModel), initialUpdateCall);
        }

        /// <summary>
        /// Binds multiple properties to the same specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <typeparam name="TPrp">Type of property</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="propertyExpression1">lambda with property member. (vm => vm.Property) </param>
        /// <param name="propertyExpression2">lambda with property member. (vm => vm.Property) </param>
        /// <param name="action">Action that should be called on property changed</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindMultiProperty<TVm, TPrp>(TVm viewModel,
            Expression<Func<TVm, TPrp>> propertyExpression1,
            Expression<Func<TVm, TPrp>> propertyExpression2,
            Action<TVm> action, bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            BindProperty(viewModel, propertyExpression1, action, initialUpdateCall);
            BindProperty(viewModel, propertyExpression2, action, initialUpdateCall);
        }

        /// <summary>
        /// Binds multiple properties to the same specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <typeparam name="TPrp">Type of property</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="propertyExpression1">lambda with property member. (vm => vm.Property) </param>
        /// <param name="propertyExpression2">lambda with property member. (vm => vm.Property) </param>
        /// <param name="propertyExpression3">lambda with property member. (vm => vm.Property) </param>
        /// <param name="action">Action that should be called on property changed</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindMultiProperty<TVm, TPrp>(TVm viewModel,
            Expression<Func<TVm, TPrp>> propertyExpression1,
            Expression<Func<TVm, TPrp>> propertyExpression2,
            Expression<Func<TVm, TPrp>> propertyExpression3,
            Action<TVm> action, bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            BindProperty(viewModel, propertyExpression1, action, initialUpdateCall);
            BindProperty(viewModel, propertyExpression2, action, initialUpdateCall);
            BindProperty(viewModel, propertyExpression3, action, initialUpdateCall);
        }

        /// <summary>
        /// Binds view model action to specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="actionExpression">lambda with view model action member. (vm => vm.Action) </param>
        /// <param name="action">Action that should be called on view model action called</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindAction<TVm>(TVm viewModel, Expression<Action<TVm>> actionExpression, Action<TVm> action,
            bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            MemberInfo memberInfo = ReflectionExtensions.MemberInfo(actionExpression);
            string name = memberInfo.Name;
            BindNameArgs(viewModel, name, args => action(viewModel), initialUpdateCall);
        }

        /// <summary>
        /// Binds multiple view model actions to the same specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="actionExpression1">lambda with view model action member. (vm => vm.Action) </param>
        /// <param name="actionExpression2">lambda with view model action member. (vm => vm.Action) </param>
        /// <param name="action">Action that should be called on view model action called</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindMultiAction<TVm>(TVm viewModel,
            Expression<Action<TVm>> actionExpression1, Expression<Action<TVm>> actionExpression2,
            Action<TVm> action,
            bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            BindAction(viewModel, actionExpression1, action, initialUpdateCall);
            BindAction(viewModel, actionExpression2, action, initialUpdateCall);
        }


        /// <summary>
        /// Binds multiple view model actions to the same specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="actionExpression1">lambda with view model action member. (vm => vm.Action) </param>
        /// <param name="actionExpression2">lambda with view model action member. (vm => vm.Action) </param>
        /// <param name="actionExpression3">lambda with view model action member. (vm => vm.Action) </param>
        /// <param name="action">Action that should be called on view model action called</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindMultiAction<TVm>(TVm viewModel,
            Expression<Action<TVm>> actionExpression1, Expression<Action<TVm>> actionExpression2,
            Expression<Action<TVm>> actionExpression3,
            Action<TVm> action,
            bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            BindAction(viewModel, actionExpression1, action, initialUpdateCall);
            BindAction(viewModel, actionExpression2, action, initialUpdateCall);
            BindAction(viewModel, actionExpression3, action, initialUpdateCall);
        }

        /// <summary>
        /// Binds property to specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// Note that this method passes PropertyChangedEventArgs casted to TPrp to the action instead of viewmodel
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <typeparam name="TPrp">Type of property</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="propertyExpression">lambda with property member. (vm => vm.Property) </param>
        /// <param name="action">Action that should be called on property changed</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindPropertyArgs<TVm, TPrp>(TVm viewModel, Expression<Func<TVm, TPrp>> propertyExpression,
            Action<PropertyChangedEventArgs<TPrp>> action, bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            PropertyInfo prop = ReflectionExtensions.MemberInfo(propertyExpression) as PropertyInfo;
            string name = prop.Name;
            BindNameArgs(viewModel, name, args => action(args.Of<TPrp>()), initialUpdateCall);
        }

        /// <summary>
        /// Binds view model PropertyChanged event with specified property name to specified action.
        /// Binder subscribes to NotifyPropertyChanged event of the passed viewModel and calls the action when such event occur.
        /// Note that this method passes raw PropertyChangedEventArgs to the action instead of viewmodel
        /// </summary>
        /// <typeparam name="TVm">Type of view model</typeparam>
        /// <param name="viewModel">View Model</param>
        /// <param name="name">member name</param>
        /// <param name="action">Action that should be called on view model action called</param>
        /// <param name="initialUpdateCall">Call action immediately after binding. Useful for initial update of the view</param>
        public void BindNameArgs<TVm>(TVm viewModel, string name, Action<PropertyChangedEventArgs> action,
            bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            if (Object.ReferenceEquals(viewModel, null))
            {
                throw new ArgumentNullException("viewModel", "ViewModel can't be null");
            }

            PropertyChangedEventHandler lambdaDelegate = (sender, args) =>
            {
                if (args.PropertyName == name)
                {
                    action(args);
                }
            };

            _delegates.Add(viewModel, lambdaDelegate);
            viewModel.PropertyChanged += lambdaDelegate;

            if (initialUpdateCall)
            {
                var value = typeof (TVm).GetProperty(name).GetValue(viewModel, null);
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(name, value, value);
                action(args);
            }
        }

        public void BindActionPlain<TVm>(TVm viewModel, Action viewModelAction, Action callbackAction,
            bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            string name = viewModelAction.Method.Name;
            BindNameArgs(viewModel, name, args => callbackAction());
        }

        public void BindActionPlain<TVm, T1>(TVm viewModel, Action<T1> viewModelAction, Action<T1> callbackAction,
            bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            string name = viewModelAction.Method.Name;
            BindNameArgs(viewModel, name, args => callbackAction((T1) args.Argument1));
        }

        public void BindActionPlain<TVm, T1, T2>(TVm viewModel, Action<T1, T2> viewModelAction,
            Action<T1, T2> callbackAction,
            bool initialUpdateCall = false)
            where TVm : IViewModel
        {
            string name = viewModelAction.Method.Name;
            BindNameArgs(viewModel, name, args => callbackAction((T1) args.Argument1, (T2) args.Argument1));
        }

        /// <summary>
        /// Unbinds everything.
        /// Binder unbinds all properties and actions that were bound by this binder.
        /// </summary>
        public void Reset()
        {
            foreach (var pair in _delegates)
            {
                foreach (var item in pair.Value)
                {
                    pair.Key.PropertyChanged -= item;
                }
            }
        }

        /// <summary>
        /// Unbinds everything from viewModel.
        /// Binder unbinds all properties and actions of viewModel that were bound by this binder.
        /// </summary>
        /// <param name="viewModel">view model</param>
        public void Reset(IViewModel viewModel)
        {
            if (_delegates.ContainsKey(viewModel))
            {
                foreach (var item in _delegates[viewModel])
                {
                    viewModel.PropertyChanged -= item;
                }
            }
        }
    }
}