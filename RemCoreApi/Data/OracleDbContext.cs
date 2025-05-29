using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RemCoreApi.Models;

namespace RemCoreApi.Data;

public class OracleDbContext : DbContext
{
    public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
    {
    }

    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Contact> Contacts { get; set; }    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Oracle-specific settings
        modelBuilder.HasDefaultSchema("PMPR_929__REM");        // Configure Contract entity
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("CONTRACTS_CONTRACT", "DEV_RAY2__REM");
            entity.HasKey(e => e.Id);
            
            // Configure indexes
            entity.HasIndex(e => e.Contractedpartyid, "IDX_CONTRACT_CONTRACTEDPARTY");
            entity.HasIndex(e => e.Currencyid, "IDX_CONTRACT_CURRENCY");
            entity.HasIndex(e => e.Contracttypeid, "IDX_CONTRACT_TYPE");
            entity.HasIndex(e => e.Vendorid, "IDX_CONTRACT_VENDOR");
            entity.HasIndex(e => e.Entityid, "UQ_CONTRACT_ENTITYID").IsUnique();
            
            // Configure NUMBER(1) fields as integers (no boolean conversion to avoid Oracle type mapping issues)
            entity.Property(e => e.Isarchived)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int?>();
            entity.Property(e => e.Isbroken)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int?>();
            entity.Property(e => e.Isinholdover)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int?>();
            entity.Property(e => e.Ispartialbuilding)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int?>();
            entity.Property(e => e.Isreceivable)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int?>();
            entity.Property(e => e.LeaseaccountingEoltakeownership)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int?>();
            entity.Property(e => e.LeaseaccountingForcereview)
                .HasColumnType("NUMBER(1)")
                .HasConversion<int?>();
            
            // Configure other numeric properties
            entity.Property(e => e.LeaseaccountingManualoverride)
                .HasColumnType("NUMBER(10)");

            // Configure decimal properties for Oracle
            entity.Property(e => e.Netequivalentfactor)
                .HasColumnType("NUMBER(18,8)");
            entity.Property(e => e.LeaseaccountingOriginalpurchaseprice)
                .HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.LeaseaccountingInitialprepayment)
                .HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.LeaseaccountingCalculatedrestoringrate)
                .HasColumnType("NUMBER(18,8)");
            entity.Property(e => e.Terminationcost)
                .HasColumnType("NUMBER(16,2)");

            // Configure date properties
            entity.Property(e => e.Makegooddateofobligation)
                .HasColumnType("DATE");
            entity.Property(e => e.LeaseaccountingStartdate)
                .HasColumnType("DATE");
            entity.Property(e => e.Archiveddate)
                .HasColumnType("DATE");
            entity.Property(e => e.Holdoverstartdate)
                .HasColumnType("DATE");
            entity.Property(e => e.Terminationdate)
                .HasColumnType("DATE");

            // Configure large text properties
            entity.Property(e => e.Notes)
                .HasColumnType("NCLOB");
        });        // Configure Contact entity
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("CONTACTS_CONTACTS", "PMPR_929__REM");
            entity.HasKey(e => e.Id);

            // Configure large text properties
            entity.Property(e => e.Notes)
                .HasColumnType("NVARCHAR2(16000)");
            entity.Property(e => e.Contactshortname)
                .HasColumnType("NVARCHAR2(16000)");
            entity.Property(e => e.LaId)
                .HasColumnType("NVARCHAR2(16000)");
            entity.Property(e => e.LaRoles)
                .HasColumnType("NVARCHAR2(16000)");
        });
    }
}
