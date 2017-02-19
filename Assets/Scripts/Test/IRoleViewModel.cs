using JetBrains.Annotations;
using MVVMDynamic;
using System;
using System.Runtime.Serialization;

namespace Assets.Scripts.Test
{
    public interface IRoleViewModel : IViewModel
    {
        int hp { get; set; }

        void Attack();

        void Jump();
    }
}