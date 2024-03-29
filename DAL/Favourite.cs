namespace DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Favourite
    {
        public int Id { get; set; }

        public int Product_Id { get; set; }

        public int User_ID { get; set; }

        public bool IsFavourite { get; set; }

        public bool IsDeleted { get; set; }

        public virtual Product Product { get; set; }

        public virtual User User { get; set; }
    }
}
