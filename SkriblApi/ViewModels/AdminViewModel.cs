using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.ViewModels
{
    public class AdminViewModel
    {
        public AdminViewModel()
        {
            Agents = new List<AgentViewModel>();
        }
        public List<AgentViewModel> Agents { get; set; }
    }
    public class AgentViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public short Role { get; set; }
        public string ImageUrl { get; set; }
        public bool IsDeleted { get; set; }
    }
}