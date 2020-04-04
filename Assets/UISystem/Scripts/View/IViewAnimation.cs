using System;

namespace SB.UI
{
    /// <summary>
    /// UI animation interface.
    /// </summary>
    public interface IViewAnimation
    {
        void Animate(Action finished);

        void Stop();
    }
}