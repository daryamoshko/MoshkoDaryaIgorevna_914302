using Catalog.API.Infrastructure.EntityConfigurations;
using Catalog.API.Model;

namespace Catalog.API.Infrastructure;

public class CatalogContext : DbContext
{
    public CatalogContext()
    {
    }
    
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
    {
    }
    public virtual DbSet<CatalogItem> CatalogItems { get; set; }
    public virtual DbSet<CatalogBrand> CatalogBrands { get; set; }
    public virtual DbSet<CatalogType> CatalogTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Код демонстрирует применение DRY
        // Вместо того, чтобы каждый раз при добавлении новой модели в БД дополнительно писать и регистрировать
        // конфигурацию добавленной модели в ORM как указано ниже, мы используем универсальный метод сбора этих конфигураций
        // с использованием Reflection, сканируя сборку на наличие классов, реализующих интерфейс IEntityTypeConfiguration<>
        // и добавляем все существующие конфигурации в ModelBuilder (строитель модели ORM). 
        
        // builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        // builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        // builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
        
        var implementedConfigTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => !t.IsAbstract
                        && !t.IsGenericTypeDefinition
                        && t.GetTypeInfo().ImplementedInterfaces.Any(i =>
                            i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));

        foreach (var configType in implementedConfigTypes)
        {
            dynamic config = Activator.CreateInstance(configType);
            builder.ApplyConfiguration(config);
        }
    }
}


public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
{
    public CatalogContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer("Server=.;Initial Catalog=CatalogDb;Integrated Security=true");

        return new CatalogContext(optionsBuilder.Options);
    }
}
