using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beehive.data.model
{
    [Table("CustomCommands")]
    public class CustomCommand
    {
        [Column("CommandId"), Key]
        public int Id { get; set; }
        [Column("Command"), Required]
        public string Command { get; set; }
        [Column("Response")]
        public string Response { get; set; }
    }
}
