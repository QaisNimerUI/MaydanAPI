using Maydan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maydan.Infrastructure.Persistence.Configurations;

public class AssociationProjectSupervisorConfiguration : IEntityTypeConfiguration<AssociationProjectSupervisor>
{
    public void Configure(EntityTypeBuilder<AssociationProjectSupervisor> builder)
    {
        builder.ToTable("AssociationProjectSupervisors");

        builder.HasKey(s => s.Id);

        // Restrict on both sides, no reverse collection navigation — same shape
        // ProjectConfiguration.cs already uses for Project -> Producer/LocationManager (Project's own
        // other two User links) and WorkerConfiguration.cs uses for Worker -> Association.
        builder.HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
