using System.Collections.Generic;

namespace SimpleDI
{
    public class UpdatableManager
    {
        private List<IFixedUpdatable> _fixedUpdatables = new List<IFixedUpdatable>();

        private List<IUpdatable> _updatables = new List<IUpdatable>();

        private List<ILateUpdatable> _lateUpdatables = new List<ILateUpdatable>();

        [Inject]
        public void InitInjections(IFixedUpdatable[] fixedUpdatables, IUpdatable[] updatables, ILateUpdatable[] lateUpdatables)
        {
            if (fixedUpdatables != null)
            {
                for (int i = 0; i < fixedUpdatables.Length; ++i)
                {
                    if (fixedUpdatables[i] != null)
                    {
                        _fixedUpdatables.Add(fixedUpdatables[i]);
                    }
                }
            }

            if (updatables != null)
            {
                for (int i = 0; i < updatables.Length; ++i)
                {
                    if (updatables[i] != null)
                    {
                        _updatables.Add(updatables[i]);
                    }
                }
            }

            if (lateUpdatables != null)
            {
                for (int i = 0; i < fixedUpdatables.Length; ++i)
                {
                    if (fixedUpdatables[i] != null)
                    {
                        _lateUpdatables.Add(lateUpdatables[i]);
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            for (int i = 0; i < _fixedUpdatables.Count; ++i)
            {
                _fixedUpdatables[i].FixedUpdate();
            }
        }

        public void Update()
        {
            for (int i = 0; i < _updatables.Count; ++i)
            {
                _updatables[i].Update();
            }
        }

        public void LateUpdate()
        {
            for (int i = 0; i < _lateUpdatables.Count; ++i)
            {
                _lateUpdatables[i].LateUpdate();
            }
        }
    }
}