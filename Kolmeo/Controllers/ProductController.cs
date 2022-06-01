using Kolmeo.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolmeo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of products
        /// </summary>
        /// <returns>List of existing products</returns>
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            try
            {
                // Navigate & read to the text file
                string filePath = string.Format("{0}Data/Products.txt", System.AppDomain.CurrentDomain.BaseDirectory);
                if (System.IO.File.Exists(filePath))
                {
                    var productLines = System.IO.File.ReadAllLines(filePath);

                    // Get products list
                    var productList = new List<string[]>();
                    foreach (var productLine in productLines)
                    {
                        productList.Add(productLine.Split(';'));
                    }

                    // First line contains data field
                    var fields = productLines[0].Split(';');

                    // Product dictionary
                    var dictProducts = new List<Dictionary<string, string>>();
                    for (int i = 1; i < productLines.Length; i++)
                    {
                        var dictProduct = new Dictionary<string, string>();
                        for (int j = 0; j < fields.Length; j++)
                        {
                            dictProduct.Add(fields[j], j == 3 ? string.Format("${0:n}", productList[i][j]) : productList[i][j]);
                        }

                        dictProducts.Add(dictProduct);
                    }

                    return await Task.Run(() => JsonConvert.DeserializeObject<List<Product>>(JsonConvert.SerializeObject(dictProducts)));
                }
                else
                {
                    return BadRequest();
                }                
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error encountered getting a list of products.");
            }
        }

        /// <summary>
        /// Create a new product details
        /// </summary>
        /// <param name="productName">Product Name</param>
        /// <param name="productDescription">Product Description</param>
        /// <param name="productPrice">Product Pride</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(string productName, string productDescription, double productPrice)
        {
            try
            {
                if (string.IsNullOrEmpty(productName))
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Product name is null");
                }

                if (string.IsNullOrEmpty(productDescription))
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Product description is null");
                }

                double price = 0;
                if (!double.TryParse(productPrice.ToString(), out price))
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Product price is null");
                }

                var newProduct = await Task.Run(() => AddProduct(productName, productDescription, price));

                if (newProduct == null)
                {
                    return BadRequest();
                }

                return newProduct;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new employee record");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private Product AddProduct(string name, string description, double price)
        {
            // Navigate & read to the text file
            string filePath = string.Format("{0}Data/Products.txt", System.AppDomain.CurrentDomain.BaseDirectory);
            if (System.IO.File.Exists(filePath))
            {
                var productLines = System.IO.File.ReadAllLines(filePath);

                var newProduct = new Product()
                {
                    ID = productLines.Length, // First line contains the fields so should be ok
                    Name = name,
                    Description = description,
                    Price = string.Format("{0:n}", price)
                };

                var productLine = string.Format("{0};{1};{2};{3}",
                    newProduct.ID,
                    newProduct.Name,
                    newProduct.Description,
                    newProduct.Price);

                System.IO.File.AppendAllText(filePath, Environment.NewLine + productLine);

                return newProduct;
            }
            else
            {
                return null;
            }
        }
    }
}
