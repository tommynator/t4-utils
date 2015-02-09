using System.Reflection;

namespace Cmune.Core.Types
{
    public class MemberInfoMethod<T>
    {
        public T Attribute;
        public MethodInfo Method;

        public MemberInfoMethod(MethodInfo method, T attribute)
        {
            Method = method;
            Attribute = attribute;
        }

        public string Name
        {
            get { return Method.Name; }
        }
    }
}
