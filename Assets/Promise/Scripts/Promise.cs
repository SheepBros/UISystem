using System;

namespace SB.Async
{
    public class BasePromise : IBasePromise
    {
        public object Current => null;

        public PromiseState State { get; protected set; }

        protected Action<Exception> _fail;

        protected Action _finally;

        protected Exception _exception;

        public BasePromise()
        {
            State = PromiseState.Waiting;
        }

        /// <inheritdoc cref="IPromise.Fail"/>
        public void Fail(Exception ex)
        {
            _exception = ex;
            State = PromiseState.Failed;
            _fail?.Invoke(ex);
            _finally?.Invoke();
        }

        /// <inheritdoc cref="IPromise.Catch"/>
        public IBasePromise Catch(Action<Exception> callback)
        {
            if (State == PromiseState.Failed)
            {
                callback?.Invoke(_exception);
                return this;
            }

            _fail += callback;
            return this;
        }

        /// <inheritdoc cref="IPromise.Finally"/>
        public IBasePromise Finally(Action callback)
        {
            if (State != PromiseState.Waiting)
            {
                callback?.Invoke();
                return this;
            }

            _finally += callback;
            return this;
        }

        public bool MoveNext()
        {
            return State == PromiseState.Waiting;
        }

        public void Reset()
        {
        }
    }

    public class Promise : BasePromise, IPromise
    {
        private Action _then;

        /// <inheritdoc cref="IPromise.Resolve"/>
        public void Resolve()
        {
            State = PromiseState.Resolved;
            _then?.Invoke();
            _finally?.Invoke();
        }

        /// <inheritdoc cref="IPromise.Then"/>
        public IPromise Then(Action callback)
        {
            if (State == PromiseState.Resolved)
            {
                callback?.Invoke();
                return this;
            }

            _then += callback;
            return this;
        }
    }

    public class Promise<TParam1> : BasePromise, IPromise<TParam1>
    {
        private Action<TParam1> _then;

        private TParam1 _param1;

        /// <inheritdoc cref="IPromise.Resolve"/>
        public void Resolve(TParam1 param1)
        {
            _param1 = param1;
            State = PromiseState.Resolved;
            _then?.Invoke(param1);
            _finally?.Invoke();
        }

        /// <inheritdoc cref="IPromise.Then"/>
        public IPromise<TParam1> Then(Action<TParam1> callback)
        {
            if (State == PromiseState.Resolved)
            {
                callback?.Invoke(_param1);
                return this;
            }

            _then += callback;
            return this;
        }
    }

    public class Promise<TParam1, TParam2> : BasePromise, IPromise<TParam1, TParam2>
    {
        private Action<TParam1, TParam2> _then;

        private TParam1 _param1;

        private TParam2 _param2;

        /// <inheritdoc cref="IPromise.Resolve"/>
        public void Resolve(TParam1 param1, TParam2 param2)
        {
            _param1 = param1;
            _param2 = param2;
            State = PromiseState.Resolved;
            _then?.Invoke(param1, param2);
            _finally?.Invoke();
        }

        /// <inheritdoc cref="IPromise.Then"/>
        public IPromise<TParam1, TParam2> Then(Action<TParam1, TParam2> callback)
        {
            if (State == PromiseState.Resolved)
            {
                callback?.Invoke(_param1, _param2);
                return this;
            }

            _then += callback;
            return this;
        }
    }

    public class Promise<TParam1, TParam2, TParam3> : BasePromise, IPromise<TParam1, TParam2, TParam3>
    {
        private Action<TParam1, TParam2, TParam3> _then;

        private TParam1 _param1;

        private TParam2 _param2;

        private TParam3 _param3;

        /// <inheritdoc cref="IPromise.Resolve"/>
        public void Resolve(TParam1 param1, TParam2 param2, TParam3 param3)
        {
            _param1 = param1;
            _param2 = param2;
            _param3 = param3;
            State = PromiseState.Resolved;
            _then?.Invoke(param1, param2, param3);
            _finally?.Invoke();
        }

        /// <inheritdoc cref="IPromise.Then"/>
        public IPromise<TParam1, TParam2, TParam3> Then(Action<TParam1, TParam2, TParam3> callback)
        {
            if (State == PromiseState.Resolved)
            {
                callback?.Invoke(_param1, _param2, _param3);
                return this;
            }

            _then += callback;
            return this;
        }
    }
}