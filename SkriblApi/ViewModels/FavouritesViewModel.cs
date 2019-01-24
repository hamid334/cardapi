using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BasketApi.ViewModels
{
    public class FavouritesViewModel
    {
        public FavouritesViewModel()
        {
            Favourites = new List<Favourite>();
        }
        public List<Favourite> Favourites { get; set; }
        public int TotalRecords { get; set; }
    }
}