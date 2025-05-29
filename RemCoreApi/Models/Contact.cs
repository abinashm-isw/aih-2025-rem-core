using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace REM.Core.Api.Models;

[Table("CONTACTS_CONTACTS", Schema = "PMPR_929__REM")]
public partial class Contact
{
    [Key]
    [Column("ID")]
    [Precision(10)]
    public int Id { get; set; }

    [Column("PHONE1")]
    [StringLength(50)]
    public string? Phone1 { get; set; }

    [Column("PHONE2")]
    [StringLength(50)]
    public string? Phone2 { get; set; }

    [Column("MOBILE")]
    [StringLength(50)]
    public string? Mobile { get; set; }

    [Column("EMAIL")]
    [StringLength(200)]
    public string? Email { get; set; }

    [Column("FAX")]
    [StringLength(50)]
    public string? Fax { get; set; }

    [Column("NOTES", TypeName = "NVARCHAR2(16000)")]
    public string? Notes { get; set; }

    [Column("LOISSYSTEMID")]
    [StringLength(255)]
    public string? Loissystemid { get; set; }

    [Column("CONTACTSHORTNAME", TypeName = "NVARCHAR2(16000)")]
    public string? Contactshortname { get; set; }

    [Column("LA_ID", TypeName = "NVARCHAR2(16000)")]
    public string? LaId { get; set; }

    [Column("LA_ROLES", TypeName = "NVARCHAR2(16000)")]
    public string? LaRoles { get; set; }
}
