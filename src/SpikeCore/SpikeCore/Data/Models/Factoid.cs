using System;
using System.ComponentModel.DataAnnotations;

namespace SpikeCore.Data.Models
{
    public class Factoid
    {
        [Required]
        public long Id { get; set; }
                              
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Type { get; set; }
        
        [Required]
        [MaxLength(4000)]
        public string Description { get; set; }
        
        [Required]
        public DateTime CreationDate { get; set; }
        
        [MaxLength(255)]
        public string CreatedBy { get; set; }
    }
}