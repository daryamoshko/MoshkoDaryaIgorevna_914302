namespace Catalog.API;

public class CatalogSettings
{
    public virtual string PicBaseUrl { get; set; }

    public virtual bool UseCustomizationData { get; set; }

    public virtual bool AzureStorageEnabled { get; set; }
}
