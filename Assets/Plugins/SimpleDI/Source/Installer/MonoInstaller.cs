using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDI
{
    public abstract class MonoInstaller : MonoBehaviour
    {
        protected DiContainer _container { get; private set; }

        public void Initialize(DiContainer container)
        {
            _container = container;
        }

        public abstract void InstallBindings();
    }
}