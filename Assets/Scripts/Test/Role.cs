using MVVMDynamic;
using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace Assets.Scripts.Test
{
    public class Role : VMMonoBehaviour<IRoleViewModel>
    {
        public int hp
        {
            get { return viewModel.hp; }
        }
    }
}