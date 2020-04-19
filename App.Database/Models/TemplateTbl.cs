using System;
using System.ComponentModel.DataAnnotations;

namespace App.Database.Models
{
    public class TemplateTbl : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public string Template { get; set; }
        public string TestData { get; set; }

        public Guid ProjectId { get; set; }
        public ProjectTbl Project { get; set; }
    }
}
