namespace DAL
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    public partial class OfferDistance
    {
        public OfferDistance()
        {
            DistanceKM = new float();
        }
        public virtual float DistanceKM { get; set; }
        public virtual int ID { get; set; }
    }
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            Favourites = new HashSet<Favourite>();
            Offer_Products = new HashSet<Offer_Products>();
            //Order_Items = new HashSet<Order_Items>();
            Package_Products = new HashSet<Package_Products>();
            ProductRatings = new HashSet<ProductRating>();
           
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public double Price { get; set; }

        public double? DiscountPrice { get; set; }

        public double? DiscountPercentage { get; set; }

        //[Required]
        public string Description { get; set; }
        //[Required]
       // public float DistanceKM { get; set; }

        [NotMapped]
        public string Weight { get; set; }

        public int WeightUnit { get; set; }

        public double WeightInGrams { get; set; }

        public double WeightInKiloGrams { get; set; }

        public string ImageUrl { get; set; }

        public string VideoUrl { get; set; }

        public short Status { get; set; }

        //public int SubCategory_Id { get; set; }


        [ForeignKey("Store")]
        public int Store_Id { get; set; }

        public bool IsDeleted { get; set; }

        public string Size { get; set; }

        [ForeignKey("Category")]
        public int? Category_Id { get; set; }

        [NotMapped]
        public bool IsFavourite { get; set; }

        public virtual Category Category { get; set; }

        [NotMapped]
        public double? DistanceKM { get; set; }

         [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Favourite> Favourites { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Offer_Products> Offer_Products { get; set; }

        //[JsonIgnore]
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<Order_Items> Order_Items { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Package_Products> Package_Products { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductRating> ProductRatings { get; set; }

        public virtual Store Store { get; set; }

        [NotMapped]
        public bool ImageDeletedOnEdit { get; set; }

        //[NotMapped]
        //public string ImageUrl { get; set; }

        //public string ImagePath { get; set; }

        //[JsonIgnore]
        //public virtual SubCategory SubCategory { get; set; }

        public int OrderedCount { get; set; }

        public double AverageRating { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
