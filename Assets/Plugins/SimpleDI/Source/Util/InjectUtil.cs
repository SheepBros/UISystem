using System;
using System.Reflection;

namespace SimpleDI.Util
{
    public static class InjectUtil
    {
        public static void InjectWithContainer(DiContainer container, object instance)
        {
            if (GetInjectMethod(instance, out MethodInfo methodInfo))
            {
                InvokeInjectMethod(instance, methodInfo, container);
            }
        }

        public static bool GetInjectMethod(object instance, out MethodInfo methodInfo)
        {
            Type type = instance.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<InjectAttribute>() != null)
                {
                    methodInfo = method;
                    return true;
                }
            }

            methodInfo = null;
            return false;
        }

        public static void InvokeInjectMethod(object instance, MethodInfo methodInfo, DiContainer container)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] args = null;
            if (parameters.Length > 0)
            {
                args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    ParameterInfo parameter = parameters[i];
                    Type parameterType = parameter.ParameterType;
                    if (parameterType.IsArray)
                    {
                        args[i] = container.GetInstances(parameterType);
                    }
                    else
                    {
                        args[i] = container.GetInstance(parameterType);
                    }
                }
            }

            methodInfo.Invoke(instance, args);
        }
    }
}