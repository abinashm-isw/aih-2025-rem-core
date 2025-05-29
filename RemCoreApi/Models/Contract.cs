using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RemCoreApi.Models;

[Table("CONTRACTS_CONTRACT", Schema = "DEV_RAY2__REM")]
[Index("Contractedpartyid", Name = "IDX_CONTRACT_CONTRACTEDPARTY")]
[Index("Currencyid", Name = "IDX_CONTRACT_CURRENCY")]
[Index("Contracttypeid", Name = "IDX_CONTRACT_TYPE")]
[Index("Vendorid", Name = "IDX_CONTRACT_VENDOR")]
[Index("Entityid", Name = "UQ_CONTRACT_ENTITYID", IsUnique = true)]
public partial class Contract
{
    [Key]
    [Column("ID")]
    [Precision(10)]
    public int Id { get; set; }

    [Column("CONTRACTTYPEID")]
    [Precision(10)]
    public int? Contracttypeid { get; set; }

    [Column("DESCRIPTION")]
    [StringLength(200)]
    public string? Description { get; set; }

    [Column("VENDORID")]
    [Precision(10)]
    public int? Vendorid { get; set; }

    [Column("CONTRACTEDPARTYID")]
    [Precision(10)]
    public int? Contractedpartyid { get; set; }

    [Column("CURRENCYID")]
    [Precision(10)]
    public int? Currencyid { get; set; }    [Column("ISRECEIVABLE")]
    [Precision(1)]
    public int? Isreceivable { get; set; }

    [Column("ISARCHIVED")]
    [Precision(1)]
    public int? Isarchived { get; set; }

    [Column("ISINHOLDOVER")]
    [Precision(1)]
    public int? Isinholdover { get; set; }

    [Column("ENTITYID")]
    public Guid? Entityid { get; set; }

    [Column("DISCRIMINATOR")]
    [StringLength(128)]
    public string? Discriminator { get; set; }    [Column("ISBROKEN")]
    [Precision(1)]
    public int? Isbroken { get; set; }

    [Column("NETEQUIVALENTFACTOR", TypeName = "NUMBER(18,8)")]
    public decimal? Netequivalentfactor { get; set; }

    [Column("LEASEACCOUNTING_ORIGINALPURCHASEPRICE", TypeName = "NUMBER(18,2)")]
    public decimal? LeaseaccountingOriginalpurchaseprice { get; set; }    [Column("LEASEACCOUNTING_EOLTAKEOWNERSHIP")]
    [Precision(1)]
    public int? LeaseaccountingEoltakeownership { get; set; }

    [Column("LEASEACCOUNTING_INITIALPREPAYMENT", TypeName = "NUMBER(18,2)")]
    public decimal? LeaseaccountingInitialprepayment { get; set; }

    [Column("LEASEACCOUNTING_USEFULLIFE")]
    [Precision(10)]
    public int? LeaseaccountingUsefullife { get; set; }

    [Column("LEASEACCOUNTING_CALCULATEDRESTORINGRATE", TypeName = "NUMBER(18,8)")]
    public decimal? LeaseaccountingCalculatedrestoringrate { get; set; }

    [Column("LEASEACCOUNTING_LEASETYPE")]
    [StringLength(255)]
    public string? LeaseaccountingLeasetype { get; set; }

    [Column("LEASEACCOUNTING_ASSETCATEGORYTYPE")]
    [StringLength(255)]
    public string? LeaseaccountingAssetcategorytype { get; set; }

    [Column("LEASEACCOUNTING_LEDGERSYSTEM")]
    [StringLength(255)]
    public string? LeaseaccountingLedgersystem { get; set; }

    [Column("MAKEGOODDATEOFOBLIGATION", TypeName = "DATE")]
    public DateTime? Makegooddateofobligation { get; set; }

    [Column("LEASEACCOUNTING_STARTDATE", TypeName = "DATE")]
    public DateTime? LeaseaccountingStartdate { get; set; }

    [Column("LEASEACCOUNTING_MANUALOVERRIDE")]
    [Precision(10)]
    public int? LeaseaccountingManualoverride { get; set; }

    [Column("ARCHIVEDDATE", TypeName = "DATE")]
    public DateTime? Archiveddate { get; set; }

    [Column("HOLDOVERSTARTDATE", TypeName = "DATE")]
    public DateTime? Holdoverstartdate { get; set; }    [Column("LEASEACCOUNTING_FORCEREVIEW")]
    [Precision(1)]
    public int? LeaseaccountingForcereview { get; set; }

    [Column("TREASURYAPPROVERID")]
    [Precision(10)]
    public int? Treasuryapproverid { get; set; }    [Column("ISPARTIALBUILDING")]
    [Precision(1)]
    public int? Ispartialbuilding { get; set; }

    [Column("LIFECYCLE_STATE")]
    [StringLength(100)]
    public string? LifecycleState { get; set; }

    [Column("CLONEDFROMCONTRACTID")]
    [Precision(10)]
    public int? Clonedfromcontractid { get; set; }

    [Column("LEASEACCOUNTING_ACCOUNTINGCODE")]
    [StringLength(100)]
    public string? LeaseaccountingAccountingcode { get; set; }

    [Column("NOTES", TypeName = "NCLOB")]
    public string? Notes { get; set; }

    [Column("REFERENCENO")]
    [StringLength(200)]
    public string? Referenceno { get; set; }

    [Column("STATUS")]
    [StringLength(100)]
    public string? Status { get; set; }

    [Column("TERMINATIONCOST", TypeName = "NUMBER(16,2)")]
    public decimal? Terminationcost { get; set; }

    [Column("TERMINATIONDATE", TypeName = "DATE")]
    public DateTime? Terminationdate { get; set; }
}
