using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDI
{
    [ExecutionOrderAttribute(-9999)]
    public class PersistentContext : Context
    {
        private static PersistentContext _instance;
        public static PersistentContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    Instatiate();
                }

                return _instance;
            }
        }

        public override DiContainer Container { get; protected set; }

        [SerializeField]
        public List<MonoInstaller> _monoInstallers;

        private static void Instatiate()
        {
            GameObject prefab = Resources.Load<GameObject>("PersistentContext");
            if (prefab == null)
            {
                GameObject instance = new GameObject("PersistentContext");
                _instance = instance.AddComponent<PersistentContext>();
            }
            else
            {
                GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                _instance = instance.GetComponent<PersistentContext>();
            }

            DontDestroyOnLoad(_instance);

            _instance.Container = new DiContainer();
            _instance.Install();
        }

        public void MakeSureItsReady() { }

        protected override void InstallInternal()
        {
            MonoLifeCycle lifeCycle = GetComponent<MonoLifeCycle>();
            Container.BindFrom<MonoLifeCycle>(lifeCycle);

            foreach (MonoInstaller installer in _monoInstallers)
            {
                installer.Initialize(Container);
                installer.InstallBindings();
            }
        }
    }
}