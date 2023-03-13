using System;
using System.Collections.Generic;
using System.Linq;
using Catalog.API.Model;

namespace Catalog.UnitTests.ControllerTests.Data;

public static class CatalogControllerTestData
{
    public static List<CatalogItem> CatalogControllerTestCommonDataCustom(Action<CatalogItem>? customizeAction)
    {
        var data = CatalogControllerTestCommonData();

        if (customizeAction is not null)
        {
            data.ForEach(x => customizeAction(x));
        }

        return data;
    }
    public static CatalogItem ItemByIdData(int? id)
     => CatalogControllerTestCommonData().First(x => x.Id == id);
        
    public static List<CatalogItem> ItemsByTypeAndBrandData(int? brandId, int? typeId)
     => CatalogControllerTestCommonData().Where(x => brandId is null || x.CatalogBrandId == brandId).Where(x => typeId is null || x.CatalogTypeId == typeId).ToList();
    

    public static List<CatalogItem> CatalogControllerTestCommonData()
    {
        var plainCatalog = new List<CatalogItem>()
        {
            new()
            {
                Id = 1,
                Name = "fakeItemA",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemA.png"
            },
            new()
            {
                Id = 2,
                Name = "fakeItemB",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemB.png"
            },
            new()
            {
                Id = 3,
                Name = "fakeItemC",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemC.png"
            },
            new()
            {
                Id = 4,
                Name = "fakeItemD",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemD.png"
            },
            new()
            {
                Id = 5,
                Name = "fakeItemE",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemE.png"
            },
            new()
            {
                Id = 6,
                Name = "fakeItemF",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemF.png"
            }
        };

        return plainCatalog;
    }

    public static List<CatalogBrand> CatalogBrandControllerTestCommonData()
    {
        var data = new List<CatalogBrand>
        {
            new()
            {
                Id = 1,
                Brand = "Brand1",
            },

            new()
            {
                Id = 2,
                Brand = "Brand2",
            },
            new()
            {
                Id = 3,
                Brand = "Brand3",
            },
        };

        return data;
    }
    
    
    public static List<CatalogType> CatalogTypeControllerTestCommonData()
    {
        var data = new List<CatalogType>
        {
            new()
            {
                Id = 1,
                Type = "Type1",
            },

            new()
            {
                Id = 2,
                Type = "Type2",
            },
            new()
            {
                Id = 3,
                Type = "Type3",
            },
        };

        return data;
    }
}