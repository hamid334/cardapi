using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public partial class VerificaionCodes
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public DateTime CreatedDate { get; set; }

        public int User_Id { get; set; }

        public User User { get; set; }

        public bool IsDeleted { get; set; }
    }
}
