using SimpleDI.Util;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDI
{
    public abstract class Context : MonoBehaviour
    {
        public abstract DiContainer Container { get; protected set; }

        protected virtual void OnDestroy()
        {
            Container.UnbindAll();
        }

        protected virtual void InstallInternal() { }

        protected virtual void InjectInternal() { }

        protected void Install()
        {
            Container.BindFrom<DiContainer>(Container);
            Container.BindAs<InitializableManager>();
            Container.BindAs<UpdatableManager>();
            Container.BindAs<DisposableManager>();

            this.gameObject.AddComponent<MonoLifeCycle>();

            InstallInternal();

            InjectAll();
        }

        private void InjectAll()
        {
            InjectBindings();
            InjectInternal();
        }

        private void InjectBindings()
        {
            var instances = Container.GetAllInstances();
            while (instances.MoveNext())
            {
                InjectUtil.InjectWithContainer(Container, instances.Current);
            }
        }
    }
}