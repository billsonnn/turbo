using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Game.Catalog;
using Turbo.Core.Game.Catalog.Constants;
using Turbo.Core.Game.Furniture;
using Turbo.Database.Entities.Catalog;
using Turbo.Database.Repositories.Catalog;

namespace Turbo.Catalog.Factories
{
    public class CatalogFactory(
        IFurnitureManager _furnitureManager,
        IServiceProvider _provider) : ICatalogFactory
    {
        public ICatalog CreateCatalog(string catalogType)
        {
            return ActivatorUtilities.CreateInstance<Catalog>(_provider, catalogType);
        }

        public ICatalogPage CreateRoot()
        {
            return ActivatorUtilities.CreateInstance<CatalogRoot>(_provider);
        }

        public ICatalogPage CreatePage(CatalogPageEntity entity)
        {
            return ActivatorUtilities.CreateInstance<CatalogPage>(_provider, entity);
        }

        public ICatalogOffer CreateOffer(CatalogOfferEntity entity)
        {
            return ActivatorUtilities.CreateInstance<CatalogOffer>(_provider, entity);
        }

        public ICatalogProduct CreateProduct(CatalogProductEntity entity)
        {
            var product = ActivatorUtilities.CreateInstance<CatalogProduct>(_provider, entity);

            if (product.ProductType.Equals(ProductTypeEnum.Floor) || product.ProductType.Equals(ProductTypeEnum.Wall))
            {
                var definition = _furnitureManager.GetFurnitureDefinition(product.FurnitureDefinitionId);

                if (definition != null) product.SetFurnitureDefinition(definition);
            }

            return product;
        }
    }
}