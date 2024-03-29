﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Database.Models
{
    [Table("Template")]
    public class TemplateTbl : BaseEntityModifiedDate
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        public Guid ProjectId { get; set; }
        public ProjectTbl Project { get; set; }
        public ICollection<TemplateVersionTbl> Versions { get; set; }
    }
}