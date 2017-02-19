using System;

namespace MVVMDynamic
{

    /// <summary>
    /// Declares member behavior in Opt-in mode.
    /// In Opt-in mode PropertyChanged event will be invoked only for members with RiseChangedEvent(true) attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    sealed class RiseChangedEventAttribute : Attribute
    {
        public readonly bool Rise;
        public RiseChangedEventAttribute(bool rise)
        {
            Rise = rise;
        }
    }

    /// <summary>
    /// Opt-in convention.
    /// PropertyChanged event will be invoked only for members with RiseChangedEvent(true) attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    sealed class OptInAttribute : Attribute
    {
       
    }
}
