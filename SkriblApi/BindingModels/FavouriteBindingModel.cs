using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.BindingModels
{
    public class FavouriteBindingModel
    {
        public int User_Id { get; set; }

        public int Offer_Id { get; set; }

        public bool IsFavourite { get; set; }
    }
}