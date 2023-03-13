using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API;
using Catalog.API.Controllers;
using Catalog.API.Infrastructure;
using Catalog.API.Model;
using Catalog.API.ViewModel;
using Catalog.UnitTests.ControllerTests.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace Catalog.UnitTests.ControllerTests;

public class CatalogControllerTest
{
    private readonly Mock<CatalogContext> _contextMock;
    private readonly Mock<CatalogSettings> _settingsMock;
    private readonly Mock<IOptionsSnapshot<CatalogSettings>> _optionsSnapshotMock;
    
    public CatalogControllerTest()
    {
        _contextMock = new Mock<CatalogContext>();
        _settingsMock = new Mock<CatalogSettings>();
        _optionsSnapshotMock = new Mock<IOptionsSnapshot<CatalogSettings>>();
    }

    [Theory]
    [InlineData(1,2,6,0,6,6,0,6)]
    [InlineData(1,2,0,0,6,0,0,0)]
    [InlineData(0,2,2,0,0,0,0,2)]
    public async Task ItemsByTypeAndBrand_ValidValues_ExpectedCountOfItemsOfCorrespondedTypeSuccessfullyReturned(
        int brandId, int typeId, int size, int index,
        int expectedCount, int expectedCountPage, int expectedIndex, int expectedSize)
    {
        // Arrange
        var catalogMoq = CatalogControllerTestData.ItemsByTypeAndBrandData(brandId, typeId).BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.ItemsByTypeIdAndBrandIdAsync(typeId, brandId, size, index);

        //Assert
        Assert.IsType<ActionResult<PaginatedItemsViewModel<CatalogItem>>>(actionResult);
        var page = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogItem>>(actionResult.Value);
        Assert.Equal(expectedIndex, page.PageIndex);
        Assert.Equal(expectedSize, page.PageSize);
        Assert.Equal(expectedCount, page.Count);
        Assert.Equal(expectedCountPage, page.Data.Count());
    }

    [Fact]
    public async Task ItemByIdAsync_InvalidId_BadRequestReturned()
    {
        //Arrange
        var invalidId = -1;
            
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.ItemByIdAsync(invalidId);

        //Assert
        Assert.IsType<BadRequestResult>(actionResult.Result);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task ItemByIdAsync_ItemWithIdDoesNotExist_NotFoundReturned(int id)
    {
        //Arrange
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData()
            .Where(x => x.Id != id)
            .BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.ItemByIdAsync(id);

        //Assert
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task ItemById_ValidValue_SingleItemOfCorrespondedTypeSuccessfullyReturned(int id)
    {
        // Arrange
        var expected = CatalogControllerTestData.ItemByIdData(id);
        
        _contextMock.Setup(x => x.CatalogItems)
            .ReturnsDbSet(CatalogControllerTestData.CatalogControllerTestCommonData());
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actual = (await orderController.ItemByIdAsync(id)).Value;

        //Assert
        Assert.IsType<CatalogItem>(actual);
        Assert.Equal(actual.Id, expected.Id);
        Assert.Equal(actual.Name, expected.Name);
        Assert.Equal(actual.PictureFileName, expected.PictureFileName);
        Assert.Equal(actual.CatalogBrandId, expected.CatalogBrandId);
        Assert.Equal(actual.CatalogTypeId, expected.CatalogTypeId);
    }
    
    
    [Theory]
    [InlineData(1,6,0,6,6,0,6)]
    [InlineData(0,6,0,0,0,0,6)]
    public async Task ItemsByBrandIdAsync_ValidValues_ExpectedCountOfItemsOfCorrespondedTypeSuccessfullyReturned(
        int brandId, int size, int index,
        int expectedCount, int expectedCountPage, int expectedIndex, int expectedSize)
    {
        // Arrange
        var catalogMoq = CatalogControllerTestData.ItemsByTypeAndBrandData(brandId, null).BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.ItemsByBrandIdAsync(brandId, size, index);

        //Assert
        Assert.IsType<ActionResult<PaginatedItemsViewModel<CatalogItem>>>(actionResult);
        var page = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogItem>>(actionResult.Value);
        Assert.Equal(expectedIndex, page.PageIndex);
        Assert.Equal(expectedSize, page.PageSize);
        Assert.Equal(expectedCount, page.Count);
        Assert.Equal(expectedCountPage, page.Data.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CatalogBrandsAsync_NoArguments_SuccessfullyReturnedExistingEntitiesAsync(int expected)
    {
        // Arrange
        var catalogBrandsMoq = CatalogControllerTestData.CatalogBrandControllerTestCommonData().Take(expected).BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogBrands)
            .Returns(catalogBrandsMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actual = await orderController.CatalogBrandsAsync();

        //Assert
        Assert.IsType<ActionResult<List<CatalogBrand>>>(actual);
        Assert.Equal(expected, actual.Value.Count);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CatalogTypesAsync_NoArguments_SuccessfullyReturnedExistingEntitiesAsync(int expected)
    {
        // Arrange
        var catalogBrandsMoq = CatalogControllerTestData.CatalogTypeControllerTestCommonData().Take(expected).BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogTypes)
            .Returns(catalogBrandsMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actual = await orderController.CatalogTypesAsync();

        //Assert
        Assert.IsType<ActionResult<List<CatalogType>>>(actual);
        Assert.Equal(expected, actual.Value.Count);
    }
    
    [Theory]
    [InlineData(6,0,6,6,0,6)]
    [InlineData(2,0,6,2,0,2)]
    public async Task ItemsAsync_ValidValues_ExpectedCountOfItemsOfCorrespondedTypeSuccessfullyReturned(
        int size, int index,
        int expectedCount, int expectedCountPage, int expectedIndex, int expectedSize)
    {
        // Arrange
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData().BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actual = await orderController.ItemsAsync(size, index);

        //Assert
        var result = Assert.IsAssignableFrom<OkObjectResult>(actual);
        var value = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogItem>>(result.Value); 
        Assert.Equal(expectedIndex, value.PageIndex);
        Assert.Equal(expectedSize, value.PageSize);
        Assert.Equal(expectedCount, value.Count);
        Assert.Equal(expectedCountPage, value.Data.Count());
    }
    
    [Theory]
    [InlineData("fake",6,0,6,6,0,6)]
    [InlineData("nonFake",6,0,0,0,0,6)]
    public async Task ItemsWithNameAsync_ValidValues_ExpectedCountOfItemsOfCorrespondedTypeSuccessfullyReturned(
        string nameStartsWith,
        int size, int index,
        int expectedCount, int expectedCountPage, int expectedIndex, int expectedSize)
    {
        // Arrange
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData()
            .BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actual = await orderController.ItemsWithNameAsync(nameStartsWith, size, index);

        //Assert
        Assert.IsType<ActionResult<PaginatedItemsViewModel<CatalogItem>>>(actual);
        var page = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogItem>>(actual.Value);
        Assert.Equal(expectedIndex, page.PageIndex);
        Assert.Equal(expectedSize, page.PageSize);
        Assert.Equal(expectedCount, page.Count);
        Assert.Equal(expectedCountPage, page.Data.Count());
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteProductAsync_InvalidId_BadRequestReturned(int id)
    {
        //Arrange
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.DeleteProductAsync(id);

        //Assert
        Assert.IsType<BadRequestResult>(actionResult);
    }
    
    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(16)]
    [InlineData(20)]
    public async Task DeleteProductAsync_ItemWithIdDoesNotExist_NotFoundReturned(int id)
    {
        //Arrange
            
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData()
            .Where(x => x.Id != id)
            .BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        
        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.DeleteProductAsync(id);

        //Assert
        Assert.IsType<NotFoundResult>(actionResult);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task DeleteProductAsync_ItemWithIdExists_NoContentResultSuccessfullyReturned(int id)
    {
        //Arrange
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData()
            .BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.DeleteProductAsync(id);

        //Assert
        _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None));
        Assert.IsType<NoContentResult>(actionResult);
    }


    [Fact]
    public async Task CreateProductAsync_ValidValues_CreatedAtActionResultSuccessfullyReturned()
    {
        var newProduct = new CatalogItem()
        {
            CatalogBrandId = 1,
            CatalogTypeId = 2,
            Description = "TestDescription",
            Name = "TestName",
            PictureFileName = "TestPictureFileName",
            Price = 100.0M
        };
        
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData()
            .BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.CreateProductAsync(newProduct);

        //Assert
        _contextMock.Verify(x => x.CatalogItems.Add(It.IsAny<CatalogItem>()));
        _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None));
        Assert.IsType<CreatedAtActionResult>(actionResult);
    }
    
    [Fact]
    public async Task UpdateProductAsync_ValidValues_CreatedAtActionResultSuccessfullyReturned()
    {
        var newProduct = new CatalogItem()
        {
            Id = 1,
            CatalogBrandId = 1,
            CatalogTypeId = 2,
            Description = "TestDescription",
            Name = "TestName",
            PictureFileName = "TestPictureFileName",
            Price = 100.0M
        };
        
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData()
            .BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.UpdateProductAsync(newProduct);

        //Assert
        _contextMock.Verify(x => x.CatalogItems.Update(It.IsAny<CatalogItem>()));
        _contextMock.Verify(x => x.SaveChangesAsync(CancellationToken.None));
        Assert.IsType<CreatedAtActionResult>(actionResult);
    }
    
    [Fact]
    public async Task UpdateProductAsync_ItemWithIdDoesNotExist_NotFoundResultReturned()
    {
        var newProduct = new CatalogItem()
        {
            Id = -1,
            CatalogBrandId = 1,
            CatalogTypeId = 2,
            Description = "TestDescription",
            Name = "TestName",
            PictureFileName = "TestPictureFileName",
            Price = 100.0M
        };
        
        var catalogMoq = CatalogControllerTestData.CatalogControllerTestCommonData()
            .BuildMock()
            .BuildMockDbSet();
        
        _contextMock.Setup(x => x.CatalogItems)
            .Returns(catalogMoq.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _settingsMock.Setup(x => x.AzureStorageEnabled)
            .Returns(false);
        _settingsMock.Setup(x => x.PicBaseUrl)
            .Returns("fakeUrl");
        
        _optionsSnapshotMock.Setup(x => x.Value)
            .Returns(_settingsMock.Object);

        //Act
        var orderController = new CatalogController(_contextMock.Object,_optionsSnapshotMock.Object);
        var actionResult = await orderController.UpdateProductAsync(newProduct);

        //Assert
        Assert.IsType<NotFoundObjectResult>(actionResult);
    }
}
