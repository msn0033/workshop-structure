using MechanicShop.Domain.Workorders;
using MechanicShop.Domain.Workorders.Billing;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(w => w.Id).IsClustered(false);



        builder.HasOne(i => i.Invoice)
            .WithOne(w => w.WorkOrder)
            .HasForeignKey<Invoice>(i => i.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(w => w.State).HasConversion<string>().IsRequired();

        builder.Property(w => w.StartAtUtc).IsRequired();

        builder.Property(w => w.EndAtUtc).IsRequired();

        builder.Property(w => w.Tax).HasPrecision(18, 2);
        builder.Property(w => w.Discount).HasPrecision(18, 2);

        builder.Ignore(w => w.Total);
        builder.Ignore(w => w.TotalLaborCost);
        builder.Ignore(w => w.TotalPartsCost);

       
  
        builder.HasIndex(w => w.State);
        builder.HasIndex(a => new { a.StartAtUtc, a.EndAtUtc });

       
    }
}