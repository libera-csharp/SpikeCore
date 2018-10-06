using System.ComponentModel.DataAnnotations;

namespace SpikeCore.Data.Models
{
    public class KarmaItem
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [Required]
        public long Karma { get; set; }
    }
}