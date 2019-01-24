namespace DAL
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Category()
        {
            Store = new List<DAL.Store>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public short Status { get; set; }

        public string ImageUrl { get; set; }

        public int ParentCategoryId { get; set; }
        
        public bool IsDeleted { get; set; }

        public int? Sorting { get; set; }

        [NotMapped]
        public int ProductCount { get; set; }

        public List<Store> Store { get; set; }

        [NotMapped]
        public bool ImageDeletedOnEdit { get; set; }
    }
}
