using Core.Entities;

namespace Core.Specification
{
    public class BrandListSpecification: BaseSpecification<Product, string>
    {
        public BrandListSpecification() 
        {

            AddSelect(x => x.Brand); // Projects only the Brand property
            ApplyDistinct(); // Ensures distinct brands
        }
    }
}
