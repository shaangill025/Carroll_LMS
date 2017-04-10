using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LMS4Carroll.Models
{
    public class BioEquipment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "BioEquipment ID")]
        public int BioEquipmentID { get; set; }

        [ForeignKey("Order")]
        public int OrderID { get; set; }
        public virtual Order Order { get; set; }

        [ForeignKey("Location")]
        public int LocationID { get; set; }
        public virtual Location Location { get; set; }

        [StringLength(50)]
        [Display(Name = "Equipment Type")]
        public string Type { get; set; }

        [StringLength(50)]
        [Display(Name = "Attribute Name")]
        public string AttributeName { get; set; }

        [StringLength(50)]
        [Display(Name = "Equipment Name")]
        public string EquipmentName { get; set; }

        [StringLength(50)]
        [Display(Name = "Equipment Model")]
        public string EquipmentModel { get; set; }
    }
}
