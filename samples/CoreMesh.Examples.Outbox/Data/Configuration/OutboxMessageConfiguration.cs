using CoreMesh.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreMesh.Examples.Outbox.Data.Configuration;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.Property(x => x.ClaimId);

        builder.Property(x => x.ProcessingStartedAt);

        builder.HasIndex(x => new { x.Status, x.NextRetryAtUtc, x.OccurredAtUtc });
        builder.HasIndex(x => x.ClaimId).IsUnique(false);
        builder.HasIndex(x => new { x.Status, x.ProcessingStartedAt });
    }
}
