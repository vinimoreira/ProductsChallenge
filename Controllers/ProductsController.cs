using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Data;
using Products.Entities;
using System.Net.Mime;

namespace Products.Controllers
{
    /// <summary>
    /// API for managing products
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDBContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductDBContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all products
        /// </summary>
        /// <returns>List of products</returns>
        /// <response code="200">Returns the list of products</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Getting all products");
            return await _context.Products.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Gets a specific product by ID
        /// </summary>
        /// <param name="id">The ID of the product to get</param>
        /// <returns>The requested product</returns>
        /// <response code="200">Returns the requested product</response>
        /// <response code="404">If the product is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }

        /// <summary>
        /// Creates a new product
        /// </summary>
        /// <param name="product">The product to create</param>
        /// <returns>The created product</returns>
        /// <response code="201">Returns the newly created product</response>
        /// <response code="400">If the product is invalid</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product data: {Errors}", 
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created product with ID: {ProductId}", product.Id);

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        /// <summary>
        /// Updates an existing product
        /// </summary>
        /// <param name="id">The ID of the product to update</param>
        /// <param name="product">The updated product data</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the product was updated successfully</response>
        /// <response code="400">If the ID doesn't match the product ID or the product is invalid</response>
        /// <response code="404">If the product is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            if (id != product.Id)
            {
                _logger.LogWarning("ID mismatch: {RouteId} vs {ProductId}", id, product.Id);
                return BadRequest(new { message = "ID in URL does not match product ID" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product data: {Errors}", 
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated product with ID: {ProductId}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(e => e.Id == id))
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for update", id);
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }
                else
                {
                    _logger.LogError("Concurrency error updating product with ID: {ProductId}", id);
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a specific product
        /// </summary>
        /// <param name="id">The ID of the product to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the product was deleted successfully</response>
        /// <response code="404">If the product is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            var product = await _context.Products.FindAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted product with ID: {ProductId}", id);

            return NoContent();
        }
    }
}
