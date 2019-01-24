using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CardRequest
    {
        public int Id { get; set; }

        public string DeliveryAddress { get; set; }

        public string DeliveryState { get; set; }

        public string DeliveryCity { get; set; }

        public double DeliveryZipCode { get; set; }

        public string NomineeName { get; set; }

        public DateTime? NomineeDOB { get; set; }

        public string NomineeGender { get; set; }

        public string NomineeRelationship { get; set; }

        public string CardNumber { get; set; }

        public string CVV { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string Signature { get; set; }

        [ForeignKey("User")]
        public int User_Id { get; set; }

        public User User { get; set; }

        public bool IsDeleted { get; set; }
    }
}
