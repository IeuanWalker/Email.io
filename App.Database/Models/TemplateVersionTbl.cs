using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Database.Models
{
    [Table("TemplateVersion")]
    public class TemplateVersionTbl : BaseEntityModifiedDate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        public string? TestData { get; set; }
        public string? Html { get; set; }
        public string? Categories { get; set; }
        public bool IsActive { get; set; }
        public string? ThumbnailImage { get; set; }
        public string? PreviewImage { get; set; }

        public Guid TemplateId { get; set; }
        public TemplateTbl Template { get; set; } = null!;
    }
}