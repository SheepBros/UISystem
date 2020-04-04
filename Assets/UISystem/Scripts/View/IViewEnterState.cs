namespace SB.UI
{
    /// <summary>
    /// Interface class to be notified when the view is enabled.
    /// </summary>
    public interface IViewEnterState
    {
        void EnterState(object args);
    }
}