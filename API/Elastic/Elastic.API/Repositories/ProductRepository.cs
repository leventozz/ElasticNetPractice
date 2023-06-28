﻿using Elastic.API.DTOs;
using Elastic.API.Model;
using Nest;
using System.Collections.Immutable;

namespace Elastic.API.Repositories
{
    public class ProductRepository
    {
        private readonly ElasticClient _client;
        private const string indexName = "newproducts";

        public ProductRepository(ElasticClient client)
        {
            _client = client;
        }
        public async Task<Product> SaveAsync(Product newProduct)
        {
            newProduct.Created = DateTime.Now;

            var response = await _client.IndexAsync(newProduct, x => x.Index(indexName).Id(Guid.NewGuid().ToString()));

            if (!response.IsValid) return null;

            newProduct.Id = response.Id;
            return newProduct;
        }

        public async Task<ImmutableList<Product>> GetAllAsync()
        {
            var result = await _client.SearchAsync<Product>(s => s.Index(indexName).Query(q=>q.MatchAll()));

            foreach (var item in result.Hits)
            {
                item.Source.Id = item.Id;
            }

            return result.Documents.ToImmutableList();
        }

        public async Task<Product> GetByIdAsync(string id)
        {
            var result = await _client.GetAsync<Product>(id, x => x.Index(indexName));

            if (!result.IsValid) return null;

            result.Source.Id = result.Id;

            return result.Source;
        }

        public async Task<bool> UpdateAsync(ProductUpdateDto updateProduct)
        {
            var response = await _client.UpdateAsync<Product, ProductUpdateDto>
                (updateProduct.Id,x=>x.Index(indexName).Doc(updateProduct));

            return response.IsValid;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _client.DeleteAsync<Product>(id, x=>x.Index(indexName)); 
            return response.IsValid;
        }
    }
}
