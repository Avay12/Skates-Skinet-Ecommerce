using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specification
{
    public class TypeListSpecification : BaseSpecification<Product, string>
    {
        public TypeListSpecification() {
            AddSelect(x => x.Type);
            ApplyDistinct();
        }
    }
}
