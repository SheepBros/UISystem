using System;

namespace SimpleDI
{
    /// <summary>
    /// This is only working for classes in SimpleDi namespace.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExecutionOrderAttribute : Attribute
    {
        public bool FixedOrder { get; set; } = true;

        public int Order { get; }

        public ExecutionOrderAttribute(int order)
        {
            Order = order;
        }
    }
}