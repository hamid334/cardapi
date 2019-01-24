using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Areas.Admin.ViewModels
{
    public class StoresViewModel
    {
        public StoresViewModel()
        {
            Stores = new List<Store>();
        }
        public List<Store> Stores { get; set; }
        public int TotalRecords { get; set; }
    }
}