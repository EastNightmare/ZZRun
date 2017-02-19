using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MVVMDynamic.Internal
{
    public static class ReflectionExtensions
    {
        private static Type[] _allTypes;
        public static Type[] AllTypes
        {
            get
            {
                if (_allTypes == null)
                {
                    _allTypes = Assembly.GetAssembly(typeof(ReflectionExtensions)).GetTypes();
                }

                return _allTypes;
            }
        }
        static public T GetCustomAttribute<T>(this MemberInfo element) where T : Attribute
        {
            return Attribute.GetCustomAttribute(element, typeof(T)) as T;
        }
        public static MemberInfo MemberInfo(Expression expression)
        {
            var lambda = expression as LambdaExpression;
            UnaryExpression unaryExpression = lambda.Body as UnaryExpression;
            MethodCallExpression methodCallExpression = lambda.Body as MethodCallExpression;

            MemberExpression memberExpression;
            if (unaryExpression != null)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
                return memberExpression.Member;
            }
            if (methodCallExpression != null)
            {
                return methodCallExpression.Method;
            }
            memberExpression = lambda.Body as MemberExpression;
            return memberExpression.Member;
        }
    }
}
