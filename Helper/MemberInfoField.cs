using System.Reflection;

namespace Cmune.Core.Types
{
    public class MemberInfoField<T>
    {
        public T Attribute;
        public FieldInfo Field;

        public MemberInfoField(FieldInfo field, T attribute)
        {
            Field = field;
            Attribute = attribute;
        }

        public string Name
        {
            get { return Field.Name; }
        }
    }

}
