namespace MVVMDynamic
{
    public interface INotifyPropertyChanged
    {
        void NotifyPropertyChanged(string propertyName, object oldValue, object newValue);
        event PropertyChangedEventHandler PropertyChanged;
    }

    public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);

    public class PropertyChangedEventArgs
    {
        public string PropertyName { get; private set; }
        public object OldValue { get; private set; }
        public object NewValue { get; private set; }
        public object Argument1 { get; private set; }
        public object Argument2 { get; private set; }

        public PropertyChangedEventArgs(string name, object oldValue, object newValue)
        {
            PropertyName = name;
            OldValue = oldValue;
            NewValue = newValue;
            Argument1 = OldValue;
            Argument2 = NewValue;
        }
    }

    public delegate void PropertyChangedEventHandler<T>(object sender, PropertyChangedEventArgs<T> e);

    public class PropertyChangedEventArgs<T> : System.ComponentModel.PropertyChangedEventArgs
    {
        public T OldValue { get; private set; }
        public T NewValue { get; private set; }

        public PropertyChangedEventArgs(string name, T oldValue, T newValue)
            : base(name)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public class ActionEventArgs<T1, T2> : System.ComponentModel.PropertyChangedEventArgs
    {
        public T1 Argument1 { get; private set; }
        public T2 Argument2 { get; private set; }

        public ActionEventArgs(string name, T1 arg1, T2 arg2)
            : base(name)
        {
            Argument1 = arg1;
            Argument2 = arg2;
        }
    }
}