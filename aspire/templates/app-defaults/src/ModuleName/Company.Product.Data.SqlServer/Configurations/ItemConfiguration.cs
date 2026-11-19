using Company.Product.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Product.Data.SqlServer.Configurations;

internal sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).HasMaxLength(Item.MaxNameLength).IsRequired();
        builder.Property(item => item.Version).IsConcurrencyToken();
    }
}
