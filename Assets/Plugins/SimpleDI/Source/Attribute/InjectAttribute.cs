using System;

namespace SimpleDI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class InjectAttribute : Attribute
    {
    }
}