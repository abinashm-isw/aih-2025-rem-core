using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LA.Entities;

[Table("CONTRACTS_CONTRACT", Schema = "PMPR_929__REM")]
[Index("Contractedpartyid", Name = "IDX_CONTRACT_CONTRACTEDPARTY")]
[Index("Currencyid", Name = "IDX_CONTRACT_CURRENCY")]
[Index("Contracttypeid", Name = "IDX_CONTRACT_TYPE")]
[Index("Vendorid", Name = "IDX_CONTRACT_VENDOR")]
[Index("Entityid", Name = "UQ_CONTRACT_ENTITYID", IsUnique = true)]
public partial class ContractsContract
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
    public int? Currencyid { get; set; }

    [Column("ISRECEIVABLE")]
    [Precision(1)]
    public bool? Isreceivable { get; set; }

    [Column("ISARCHIVED")]
    [Precision(1)]
    public bool? Isarchived { get; set; }

    [Column("ISINHOLDOVER")]
    [Precision(1)]
    public bool? Isinholdover { get; set; }

    [Column("ENTITYID")]
    public Guid? Entityid { get; set; }

    [Column("DISCRIMINATOR")]
    [StringLength(128)]
    public string? Discriminator { get; set; }

    [Column("ISBROKEN")]
    [Precision(1)]
    public bool? Isbroken { get; set; }

    [Column("NETEQUIVALENTFACTOR", TypeName = "NUMBER(18,8)")]
    public decimal? Netequivalentfactor { get; set; }

    [Column("LEASEACCOUNTING_ORIGINALPURCHASEPRICE", TypeName = "NUMBER(18,2)")]
    public decimal? LeaseaccountingOriginalpurchaseprice { get; set; }

    [Column("LEASEACCOUNTING_EOLTAKEOWNERSHIP")]
    [Precision(1)]
    public bool? LeaseaccountingEoltakeownership { get; set; }

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
    public DateTime? Holdoverstartdate { get; set; }

    [Column("LEASEACCOUNTING_FORCEREVIEW")]
    [Precision(1)]
    public bool? LeaseaccountingForcereview { get; set; }

    [Column("TREASURYAPPROVERID")]
    [Precision(10)]
    public int? Treasuryapproverid { get; set; }

    [Column("ISPARTIALBUILDING")]
    [Precision(1)]
    public bool? Ispartialbuilding { get; set; }

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

    [ForeignKey("Contractedpartyid")]
    [InverseProperty("ContractsContractContractedparties")]
    public virtual ContactsContact? Contractedparty { get; set; }

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsAssetschedule> ContractsAssetschedules { get; set; } = new List<ContractsAssetschedule>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsBreakclause> ContractsBreakclauses { get; set; } = new List<ContractsBreakclause>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsClause> ContractsClauses { get; set; } = new List<ContractsClause>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsContractAgreedvaluereview> ContractsContractAgreedvaluereviews { get; set; } = new List<ContractsContractAgreedvaluereview>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsContractGuarantee> ContractsContractGuarantees { get; set; } = new List<ContractsContractGuarantee>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsContractTerm> ContractsContractTerms { get; set; } = new List<ContractsContractTerm>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsExitcost> ContractsExitcosts { get; set; } = new List<ContractsExitcost>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsIncentive> ContractsIncentives { get; set; } = new List<ContractsIncentive>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsInitialcost> ContractsInitialcosts { get; set; } = new List<ContractsInitialcost>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsMakegoodcost> ContractsMakegoodcosts { get; set; } = new List<ContractsMakegoodcost>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsRatereview> ContractsRatereviews { get; set; } = new List<ContractsRatereview>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsSubcontractmapping> ContractsSubcontractmappingContracts { get; set; } = new List<ContractsSubcontractmapping>();

    [InverseProperty("Parentcontract")]
    public virtual ICollection<ContractsSubcontractmapping> ContractsSubcontractmappingParentcontracts { get; set; } = new List<ContractsSubcontractmapping>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsSynchronisationevent> ContractsSynchronisationevents { get; set; } = new List<ContractsSynchronisationevent>();

    [InverseProperty("Contract")]
    public virtual ICollection<ContractsVendorhistory> ContractsVendorhistories { get; set; } = new List<ContractsVendorhistory>();

    [ForeignKey("Contracttypeid")]
    [InverseProperty("ContractsContracts")]
    public virtual ContractsContracttype? Contracttype { get; set; }

    [ForeignKey("Currencyid")]
    [InverseProperty("ContractsContracts")]
    public virtual LocaleCurrency? Currency { get; set; }

    [InverseProperty("Contract")]
    public virtual ICollection<InvoiceInvoice> InvoiceInvoices { get; set; } = new List<InvoiceInvoice>();

    [InverseProperty("Contract")]
    public virtual ICollection<InvoiceInvoicetemplate> InvoiceInvoicetemplates { get; set; } = new List<InvoiceInvoicetemplate>();

    [InverseProperty("Contract")]
    public virtual ICollection<LeaseaccountingManualoverridehistory> LeaseaccountingManualoverridehistories { get; set; } = new List<LeaseaccountingManualoverridehistory>();

    [InverseProperty("Contract")]
    public virtual ICollection<LeaseaccountingReview> LeaseaccountingReviews { get; set; } = new List<LeaseaccountingReview>();

    [ForeignKey("Vendorid")]
    [InverseProperty("ContractsContractVendors")]
    public virtual ContactsContact? Vendor { get; set; }
}
