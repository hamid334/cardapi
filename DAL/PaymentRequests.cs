using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class PaymentRequests
    {
        public int Id { get; set; }
        public string BasketId { get; set; }
        public string PaidPrice { get; set; }
        public string Locale { get; set; }
        public string ConversationId { get; set; }
        public string Currency { get; set; }
        public string Price { get; set; }
    }
}
