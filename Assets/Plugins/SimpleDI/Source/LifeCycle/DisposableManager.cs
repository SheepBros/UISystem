using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SimpleDI
{
    public class DisposableManager
    {
        private List<IDisposable> _disposables = new List<IDisposable>();

        private bool _disposed = false;

        [Inject]
        public void InitInjections(IDisposable[] disposables)
        {
            if (disposables != null)
            {
                for (int i = 0; i < disposables.Length; ++i)
                {
                    if (disposables[i] != null)
                    {
                        _disposables.Add(disposables[i]);
                    }
                }
            }
        }

        public void Dispose()
        {
            Debug.Assert(!_disposed, "DisposableManager.Dispose is called twice.");
            _disposed = true;

            for (int i = 0; i < _disposables.Count; ++i)
            {
                _disposables[i].Dispose();
            }
        }
    }
}