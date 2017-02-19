using MVVMDynamic;
using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace Assets.Scripts.Test
{
    public class VMMonoBehaviour<T> : MonoBehaviour where T : IViewModel
    {
        private readonly T m_ViewModel = TypeEmitter.Instance.CreateViewModel<T>();
        protected Binder m_Binder = new Binder();

        public T viewModel
        {
            get
            {
                return m_ViewModel;
            }
        }

        protected virtual void Unbind()
        {
            m_Binder.Reset(m_ViewModel);
        }
    }
}