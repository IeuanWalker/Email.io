using System;

namespace App.Database.Models
{
    public class BaseEntity
    {
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}