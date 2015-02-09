using System.Reflection;

namespace Cmune.Core.Types
{
    public class MemberInfoProperty<T>
    {
        public T Attribute;
        public PropertyInfo Property;

        public MemberInfoProperty(PropertyInfo field, T attribute)
        {
            Property = field;
            Attribute = attribute;
        }

        public string Name
        {
            get { return Property.Name; }
        }
    }

}
