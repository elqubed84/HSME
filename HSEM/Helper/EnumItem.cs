using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Helper
{
    public class EnumItem<T>
    {
        public T Value { get; set; }
        public string Name { get; set; }

        public EnumItem(T value, string name)
        {
            Value = value;
            Name = name;
        }

        public override string ToString() => Name;
    }

}
