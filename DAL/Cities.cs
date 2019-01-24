using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Cities
    {
        public int Id { get; set; }

        public string CityName { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool IsDeleted { get; set; }
    }
}
