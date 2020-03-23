using System;

namespace SB.UI
{
    public interface IViewAnimation
    {
        void Animate(Action finished);

        void Stop();
    }
}