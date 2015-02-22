using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.data.model
{
    [Table("Quotes")]
    public class Quote
    {
        [Column("QuoteId"), Key]
        public int Id { get; set; }
        [Column("Quote"), Required]
        public string Text { get; set; }
        [Column("AddedBy"), Required]
        public string AddedBy { get; set; }
    }
}
