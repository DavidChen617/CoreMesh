using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreMesh.Examples.Outbox.Data.Configuration;

public class TodoConfiguration: IEntityTypeConfiguration<Entities.Todo>
{
    public void Configure(EntityTypeBuilder<Entities.Todo> builder)
    {
        builder.ToTable("Todos");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Title)
            .IsRequired()
            .HasComment("Todo Title")
            .HasMaxLength(128);
        
        builder.Property(p => p.Description)
            .HasComment("Title Description")
            .HasMaxLength(1024);

        builder.Property(x => x.CreatedAt);
        
        builder.Property(x => x.UpdatedAt);
    }
}
