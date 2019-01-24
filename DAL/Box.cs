using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Box
    {
        public Box()
        {

        }

        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("Category")]
        public int Category_Id { get; set; }

        public int MerchantId { get; set; }

        public virtual Category Category { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Description { get; set; }

        public short Status { get; set; }

    }
}
