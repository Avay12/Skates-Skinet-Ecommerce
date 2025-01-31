using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Core.Specification;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{     
    public class ProductsController(IUnitOfWork _unit) : BaseApiController
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery]ProductSpecParams specParams
            )
        {
            var spec = new ProductSpecification(specParams);

            return await CreatePagedResult(_unit.Repository<Product>(), spec, specParams.PageIndex, specParams.PageSize);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _unit.Repository<Product>().GetByIdAsync(id);

            if (product == null) return NotFound();
            
            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _unit.Repository<Product>().Add(product);

            if (!await _unit.Complete()) return BadRequest("Problem creating product");

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            if (product.Id != id || !ProductExists(id))
                return BadRequest("Cannot update this Product");

            _unit.Repository<Product>().Update(product);

           if(!await _unit.Complete())
            {
                return BadRequest("Problem updating the product");
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _unit.Repository<Product>().GetByIdAsync(id);

            if (product == null) return NotFound();

            _unit.Repository<Product>().Remove(product);

            if (!await _unit.Complete()) return BadRequest("Problem deleting the product");

            return NoContent();
            
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            var spec = new BrandListSpecification();

            var brands = await _unit.Repository<Product>().ListAsync(spec);
            return Ok(brands);
        }
        
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            var spec = new TypeListSpecification();
            return Ok(await _unit.Repository<Product>().ListAsync(spec));
        }


        private bool ProductExists(int id)
        {
            return _unit.Repository<Product>().Exists(id);
        }
    }
}
