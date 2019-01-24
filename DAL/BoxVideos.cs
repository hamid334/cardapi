using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class BoxVideos
    {
        public int Id { get; set; }

        public string VideoUrl { get; set; }

        public bool IsDeleted { get; set; }
        
        public int Box_Id { get; set; }

        public Box Box { get; set; }
    }
}
