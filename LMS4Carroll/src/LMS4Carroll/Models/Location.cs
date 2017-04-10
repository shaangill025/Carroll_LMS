﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LMS4Carroll.Models
{
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Location ID")]
        public int LocationID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Room { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string NormalizedStr { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Type { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Address { get; set; }

        public virtual ICollection<ChemEquipment> ChemEquipments { get; set; }
    }
}