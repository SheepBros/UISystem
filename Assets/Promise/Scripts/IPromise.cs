using System;
using System.Collections;

namespace SB.Async
{
    public interface IBasePromise : IEnumerator
    {
        PromiseState State { get; }

        /// <summary>
		/// Report the promise failed.
		/// </summary>
        void Fail(Exception ex);

        /// <summary>
		/// Register a callback called when the promise is failed.
		/// </summary>
        IBasePromise Catch(Action<Exception> callback);

        /// <summary>
		/// Register a callback called when the promise is done.
		/// Note that this callback is called after Then or Fail.
		/// </summary>
        IBasePromise Finally(Action callback);
    }

    public interface IPromise : IBasePromise
    {
        /// <summary>
		/// Report the promise fulfilled.
		/// </summary>
        void Resolve();

        /// <summary>
		/// Register a callback called when the promise is resolved.
		/// </summary>
        IPromise Then(Action callback);
    }

    public interface IPromise<TParam1> : IBasePromise
    {
        /// <inheritdoc cref="IPromise.Resolve"/>
        void Resolve(TParam1 param1);

        /// <inheritdoc cref="IPromise.Then"/>
        IPromise<TParam1> Then(Action<TParam1> callback);
    }

    public interface IPromise<TParam1, TParam2> : IBasePromise
    {
        /// <inheritdoc cref="IPromise.Resolve"/>
        void Resolve(TParam1 param1, TParam2 param2);

        /// <inheritdoc cref="IPromise.Then"/>
        IPromise<TParam1, TParam2> Then(Action<TParam1, TParam2> callback);
    }

    public interface IPromise<TParam1, TParam2, TParam3>
    {
        /// <inheritdoc cref="IPromise.Resolve"/>
        void Resolve(TParam1 param1, TParam2 param2, TParam3 param3);

        /// <inheritdoc cref="IPromise.Then"/>
        IPromise<TParam1, TParam2, TParam3> Then(Action<TParam1, TParam2, TParam3> callback);
    }
}