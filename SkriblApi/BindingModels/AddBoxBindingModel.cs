using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BasketApi.BindingModels
{
    public class AddBoxBindingModel
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int Category_Id { get; set; }
    }

    public class Video
    {
        public string VideoUrl { get; set; }
    }
}