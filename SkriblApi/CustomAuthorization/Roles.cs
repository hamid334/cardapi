using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BasketApi.CustomAuthorization
{
    //public class RoleTypes
    //{
    //    //User,
    //    //Deliverer,
    //    //Agent,
    //    //SuperAdmin,
    //    //ApplicationAdmin
    //    public const string User = "User";
    //    public const string Deliverer = "Deliverer";
    //    public const string Agent = "Agent";
    //    public const string SuperAdmin = "SuperAdmin";
    //    public const string ApplicationAdmin = "ApplicationAdmin";
    //}

    public enum RoleTypes
    {
        User = 0,
        Deliverer = 1,
        Agent = 2,
        SuperAdmin = 3,
        ApplicationAdmin = 4,
        Guest = 5
    }
}