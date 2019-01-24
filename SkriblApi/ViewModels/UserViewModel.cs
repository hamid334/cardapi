using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BasketApi.ViewModels
{
    public class EditUserProfileBindingModel
    {
        [Required]
        public int ID { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        
        public string StreetAddress { get; set; }

        public string Area { get; set; }

        public string City { get; set; }
        
        public string InstagramUrl { get; set; }
    }
    public class SavingsViewModel
    {
        public SavingsViewModel()
        {
            Savings = new List<DAL.Savings>();
        }
        public List<Savings> Savings { get; set; }
        public int TotalRecords { get; set; }
    }
    public class UserViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
        
        public string AccountType { get; set; }

        public string ZipCode { get; set; }

        public string DateofBirth { get; set; }

        public short? SignInType { get; set; }

        public string UserName { get; set; }

        public short? Status { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool PhoneConfirmed { get; set; }
    }
}