using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMDynamic;

namespace Assets.MVVM.Demos
{
    [OptIn]
    //Now PropertyChanged event will be invoked 
    //only for members with RiseChangedEvent(true) attribute
    interface ITestViewModel : IViewModel
    {
        int Test1 { get; set; }

        [RiseChangedEvent(true)]
        string Test2 { get; set; }

        [RiseChangedEvent(true)]
        long Test3 { get; set; }
    }

    class Test
    {
        public Test()
        {
            ITestViewModel viewModel = TypeEmitter.Instance.CreateViewModel<ITestViewModel>();
            viewModel.Update(
                () =>
                {
                    viewModel.Test1 = 15;
                    viewModel.Test2 = "Test text";
                    viewModel.Test3 = 1234567890;
                });

            viewModel.Update(
                vm =>
                {
                    vm.Test1 = 15;
                    vm.Test2 = "Test text";
                    vm.Test3 = 1234567890;
                });
        } 
    }
}
