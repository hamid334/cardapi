using BasketApi.Areas.Admin.ViewModels;
using BasketApi.ViewModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using WebApplication1.Areas.Admin.ViewModels;
using WebApplication1.BindingModels;
using System.Data.Entity;


namespace BasketApi.Areas.Agent.Controllers
{
    //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "User", "Guest")]
    [RoutePrefix("api")]
    public class ProductController : ApiController
    {

        [HttpGet]
        [Route("GetOfferByOfferId")]
        public async Task<IHttpActionResult> GetOfferByOfferId(int Offer_Id, int? User_Id = 0)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {

                    // var offer = ctx.Products.Include(x => x.Store).FirstOrDefault(x => x.Id == Offer_Id && x.IsDeleted == false);
                       var offer = ctx.Products.Include(x => x.Store).FirstOrDefault(x => x.Id == Offer_Id && x.IsDeleted == false);
                  
                    if (offer != null)
                    {
                        if (User_Id.HasValue && User_Id != 0)
                            offer.IsFavourite = ctx.Favourites.Any(x => x.User_ID == User_Id && x.Product_Id == offer.Id && x.IsFavourite);
                        CustomResponse<Product> response = new CustomResponse<Product>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = ctx.Products.FirstOrDefault(x => x.Id == Offer_Id)
                    };
                        return Ok(response);
                    }
                    else
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "NotFound",
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Result = new Error { ErrorMessage = "Invalid Offer Id" }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        public double GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2) 
        {
            var R = 6367.45; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);  // deg2rad below
            var dLon = Deg2Rad(lon2 - lon1);
            var a =
              Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
              Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
              Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
            var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
            var d = R * c; // Distance in km
            
            return d;
        }

        public double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180.0d);
        }
       
        [Route("ConfirmOffer")]
        public async Task<IHttpActionResult> ConfirmOffer(int UserId, double Amount)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var userSavings = ctx.Savings.FirstOrDefault(x => x.User_Id == UserId);
                    if (userSavings == null)
                    {
                        ctx.Savings.Add(new Savings
                        {
                            CreatedDate = DateTime.Now,
                            User = ctx.Users.Find(UserId),
                            SavingsAmount = Amount.ToString(),
                            User_Id = UserId,
                            IsDeleted = false
                        });

                   
                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        double savings;
                        if (double.TryParse(userSavings.SavingsAmount, out savings))
                        {
                            savings += Amount;
                            userSavings.SavingsAmount = savings.ToString();
                        }
                        else
                        {
                            userSavings.SavingsAmount = Amount.ToString();
                        }

                        ctx.Entry(userSavings).State = EntityState.Modified;

                        await ctx.SaveChangesAsync();
                    }
                    return Ok(new CustomResponse<Object>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = "Success"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }

        }
        //[HttpGet]
        //[Route("GetProductCount")]
        //public async Task<IHttpActionResult> GetProductCount()
        //{
        //    try
        //    {
        //        using (SkriblContext ctx = new SkriblContext())
        //        {
        //            ProductCountViewModel model = new ProductCountViewModel { TotalProducts = ctx.Products.Count(x => x.IsDeleted == false) };
        //            CustomResponse<ProductCountViewModel> response = new CustomResponse<ProductCountViewModel>
        //            {
        //                Message = Global.ResponseMessages.Success,
        //                StatusCode = (int)HttpStatusCode.OK,
        //                Result = model
        //            };
        //            return Ok(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}
      
        [HttpGet]
        [Route("SearchOffers")]
        public async Task<IHttpActionResult> GetAllProducts(string Location = "", string SearchString = "", int? User_Id = 0, int? Page = 0, int? Items = 6, int? Category_Id = 0, double? Distance = 0, double? Latitude = 0, double? Longitude = 0)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext()) 
                {

                    var query = "SELECT distinct (select top 1 Id from Products p where p.Category_Id=Products.Category_Id and p.Price=Products.Price " +
                                " and p.DiscountPercentage = Products.DiscountPercentage and p.DiscountPrice = Products.DiscountPrice and p.Name = Products.Name)Id, [dbo].[DistanceKM] ((select top 1 s.Latitude from stores s where s.Id =((select top 1 p.Store_Id from Products p where p.Category_Id= Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice " +
                                " and p.Name= Products.Name))),'0',(select top 1 s.Latitude from stores s where s.Id =((select top 1 p.Store_Id from Products p where p.Category_Id= Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice " +
                                " and p.Name= Products.Name))),'0') DistanceKM, Products.Name,Products.Price, Products.DiscountPrice, Products.DiscountPercentage, Products.Description, Products.WeightUnit, Products.WeightInGrams, Products.WeightInKiloGrams,  " +
                                " (select top 1 p.ImageUrl from Products p where p.Category_Id=Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice " +
                                " and p.Name= Products.Name)ImageUrl, Products.VideoUrl, Products.Status, Products.IsDeleted, Products.Size, Products.Category_Id, Products.OrderedCount,  " +
                                " Products.AverageRating, (select top 1 p.Store_Id from Products p where p.Category_Id=Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice and p.Name= Products.Name)Store_Id, (select top 1 p.CreatedDate from Products p where p.Category_Id=Products.Category_Id and p.Price= Products.Price " +
                                "and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice and p.Name= Products.Name)CreatedDate FROM Products join Stores ON Products.Store_Id=Stores.Id Where Products.IsDeleted=0  ";
                    //                    var query = "SELECT distinct(select top 1 Id from Products p where p.Category_Id=Products.Category_Id" +
                    //" and p.Price = Products.Price" +
                    //" and p.DiscountPercentage = Products.DiscountPercentage" +
                    //" and p.DiscountPrice = Products.DiscountPrice" +
                    //" and p.Name = Products.Name)Id, [dbo].[DistanceKM] (Stores.Latitude,'" + Latitude.Value + "', Stores.Longitude ,'" + Longitude.Value + "') DistanceKM, Products.Name, Products.Price, Products.DiscountPrice, Products.DiscountPercentage, Products.Description, Products.WeightUnit, " +
                    //                         "Products.WeightInGrams, Products.WeightInKiloGrams, Products.ImageUrl, Products.VideoUrl, Products.Status, Products.IsDeleted, Products.Size, Products.Category_Id, Products.OrderedCount, Products.AverageRating, " +
                    //                         "Products.Store_Id, Products.CreatedDate FROM Products join Stores ON Products.Store_Id=Stores.Id Where Products.IsDeleted=0 ";

                    if (!string.IsNullOrEmpty(Location))
                    {
                        query += "And Stores.Address LIKE '%" + Location + "%'";
                    }

                    if (Category_Id != 0)
                    {
                        if (!string.IsNullOrEmpty(Location))
                        {
                            query += "  ";
                        }

                        query += "And Products.Category_Id=" + Category_Id.Value;
                    }

                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        if (Category_Id != 0 || !string.IsNullOrEmpty(Location))
                        {
                            query += "";
                        }

                        query += " And (Products.Name LIKE '%" + SearchString + "%'";
                        query += " OR Products.DiscountPercentage LIKE '%" + SearchString + "%'";
                        query += " OR Products.Description LIKE '%" + SearchString + "%'";
                        query += " OR Stores.Name LIKE '%" + SearchString + "%')";
                    }
                    if (Longitude != 0 && Latitude != 0 && Distance != 0) 
                    {
                        if (!string.IsNullOrEmpty(Location))
                        {
                            query += "";
                        }
                        query += " And [dbo].[DistanceKM] (Stores.Latitude," + Latitude.Value + ", Stores.Longitude ," + Longitude.Value + ")<=" + Distance.Value;
                       // query += "And Stores.Longitude=" + Longitude.Value;
                    }
                    //if (Distance != 0)
                    //{
                    //    if (!string.IsNullOrEmpty(Location))
                    //    {
                    //        query += " And ";
                    //    }

                    //    query += " Stores.Distance =" + Distance.Value;
                    //}
                    query += " ORDER BY Products.Category_Id desc OFFSET " + Page.Value * Items.Value + " ROWS FETCH NEXT " + Items.Value + " ROWS ONLY ";


                    var Offers = ctx.Database.SqlQuery<Product>(query).Distinct().ToList();
                   
                    if (Offers != null)
                    {
                        if (User_Id.HasValue && User_Id != 0)
                        {
                            foreach (var offer in Offers)
                            {
                                offer.IsFavourite = ctx.Favourites.Any(x => x.User_ID == User_Id && x.Product_Id == offer.Id && !x.IsFavourite);
                            }
                        }
                        foreach (var offer in Offers)
                        {
                            offer.Store = ctx.Stores.FirstOrDefault(x => x.Id == offer.Store_Id);
                           offer.DistanceKM = GetDistanceFromLatLonInKm(offer.Store.Latitude,offer.Store.Longitude, Latitude.Value,  Longitude.Value);
                        }

                    }


                    return Ok(new CustomResponse<ProductsViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new ProductsViewModel
                        {
                            Count = Offers.Count(x => x.IsDeleted == false),
                            Products = Offers
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [HttpPost]
        [Route("MarkOfferAsFavourite")]
        public async Task<IHttpActionResult> MarkOfferAsFavourite(FavouriteBindingModel model)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    Favourite favourite = new Favourite();
                    favourite = ctx.Favourites.FirstOrDefault(x => x.User_ID == model.User_Id && x.Product_Id == model.Offer_Id);
                    if (favourite != null)
                    {
                        favourite.IsFavourite = model.IsFavourite;
                    }
                    else
                    {
                        favourite = ctx.Favourites.Add(new Favourite
                        {
                            IsDeleted = false,
                            IsFavourite = true,
                            Product_Id = model.Offer_Id,
                            User_ID = model.User_Id
                        });
                    }
                    ctx.SaveChanges();


                    CustomResponse<Favourite> response = new CustomResponse<Favourite>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = favourite
                    };
                    return Ok(response);


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetMyFavouriteOffers")]
        public async Task<IHttpActionResult> GetMyFavouriteOffers(int User_Id, int? Page = 0, int? Items = 10)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    FavouritesViewModel returnModel = new FavouritesViewModel();

                    returnModel.Favourites = ctx.Favourites.Where(x => x.User_ID == User_Id && x.IsFavourite && !x.IsDeleted).Include(x => x.Product).OrderByDescending(x => x.Id).Skip(Page.Value * Items.Value).Take(Items.Value).ToList();
                    returnModel.TotalRecords = ctx.Favourites.Count(x => x.User_ID == User_Id && x.IsFavourite && !x.IsDeleted);

                    CustomResponse<FavouritesViewModel> response = new CustomResponse<FavouritesViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = returnModel
                    };
                    return Ok(response);


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("UnFavourite")]
        public async Task<IHttpActionResult> UnFavourite(int User_Id, int Id)
        {
            try
            {

                //public async Task<IHttpActionResult> UnFavourite(int User_Id, int Id)

                using (SkriblContext ctx = new SkriblContext())
                {
                    var fav = await ctx.Favourites.FindAsync(Id);
                    if (fav != null)
                    {
                        fav.IsFavourite = false;
                        ctx.Entry(fav).State = EntityState.Modified;

                        ctx.SaveChanges();
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });

                    }
                    else
                    {
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.NotFound });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetLatestOffers")]
        public async Task<IHttpActionResult> GetLatestOffers(int User_Id, double? Latitude = 0, double? Longitude = 0, int? Page = 0, int? Items = 10)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    ProductsViewModel returnModel = new ProductsViewModel();
                    var query = "SELECT distinct (select top 1 Id from Products p where p.Category_Id=Products.Category_Id and p.Price=Products.Price " +
                                " and p.DiscountPercentage = Products.DiscountPercentage and p.DiscountPrice = Products.DiscountPrice and p.Name = Products.Name)Id, [dbo].[DistanceKM] ((select top 1 s.Latitude from stores s where s.Id =((select top 1 p.Store_Id from Products p where p.Category_Id= Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice " +
                                " and p.Name= Products.Name))),'0',(select top 1 s.Latitude from stores s where s.Id =((select top 1 p.Store_Id from Products p where p.Category_Id= Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice " +
                                " and p.Name= Products.Name))),'0') DistanceKM, Products.Name,Products.Price, Products.DiscountPrice, Products.DiscountPercentage, Products.Description, Products.WeightUnit, Products.WeightInGrams, Products.WeightInKiloGrams,  " +
                                " (select top 1 p.ImageUrl from Products p where p.Category_Id=Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice " +
                                " and p.Name= Products.Name)ImageUrl, Products.VideoUrl, Products.Status, Products.IsDeleted, Products.Size, Products.Category_Id, Products.OrderedCount,  " +
                                " Products.AverageRating, (select top 1 p.Store_Id from Products p where p.Category_Id=Products.Category_Id and p.Price= Products.Price and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice and p.Name= Products.Name)Store_Id, (select top 1 p.CreatedDate from Products p where p.Category_Id=Products.Category_Id and p.Price= Products.Price " +
                                " and p.DiscountPercentage= Products.DiscountPercentage and p.DiscountPrice= Products.DiscountPrice and p.Name= Products.Name)CreatedDate FROM Products join Stores ON Products.Store_Id=Stores.Id Where Products.IsDeleted=0  ";
                    var products = ctx.Database.SqlQuery<Product>(query).ToList();
                    if (products != null)
                    {
                        //returnModel.Products = products.Where(x => x.CreatedDate.Date == DateTime.Now.Date).OrderByDescending(x => x.Id).Skip(Page.Value * Items.Value).Take(Items.Value).ToList();
                        returnModel.Products = products.Where(x => !x.IsDeleted).OrderByDescending(x => x.Id).Take(Items.Value).ToList();

                        foreach (var item in returnModel.Products)
                        {
                            item.Store = ctx.Stores.FirstOrDefault(x => x.Id == item.Store_Id);
                            item.IsFavourite = ctx.Favourites.Any(x => x.User_ID == User_Id && x.Product_Id == item.Id && !x.IsFavourite);
                            item.DistanceKM = GetDistanceFromLatLonInKm(item.Store.Latitude, item.Store.Longitude, Latitude.Value, Longitude.Value);
                        }

                        returnModel.Count = products.Count(x => x.CreatedDate.Date == DateTime.Now.Date);

                    }
                    CustomResponse<ProductsViewModel> response = new CustomResponse<ProductsViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = returnModel
                    };
                    return Ok(response);


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetAllOffers")]
        public async Task<IHttpActionResult> GetAllOffers()
        {
            try
            {
                bool flage = true;
                using (SkriblContext ctx = new SkriblContext())
                {
                    ProductsViewModel returnModel = new ProductsViewModel();
                    if (ctx.Admins.Any(x => x.Email == User.Identity.Name))
                    {
                        returnModel.Products = ctx.Products.Where(x => !x.IsDeleted).ToList();

                    }
                    else
                    {
                        var stores= ctx.Stores.Where(x => x.MerchantEmail == User.Identity.Name).ToList();
                        if (stores != null) 
                        {
var prod=new                            List<Product>();
                            foreach (var item in stores)
                            {
                                var product = ctx.Products.Where(x => !x.IsDeleted && x.Store_Id == item.Id).FirstOrDefault();
                                if (product!=null)
                                {
                                    prod.Add(product);
                                }
                               
                                //ctx.Products.Where(x => !x.IsDeleted && x.Store_Id == item.Id).ToList();
                            }  returnModel.Products = prod;

                        }
                        else
                        {
                            returnModel.Products = new List<Product>();
                        }
                        //foreach (var item in returnModel.Products)
                        //{
                        //    item.Store = ctx.Stores.Where(x => x.Id == item.Store_Id && x.MerchantEmail == User.Identity.Name).FirstOrDefault();
                        //    if (item.Store == null) 
                        //    {
                                
                        //        flage = false;
                        //    }
                        //}
                    }
                   
                    CustomResponse<ProductsViewModel> response = new CustomResponse<ProductsViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = returnModel
                    };
                    return Ok(response);


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        //[HttpGet]
        //[Route("GetPopularProducts")]
        //public async Task<IHttpActionResult> GetPopularProducts(int Count)
        //{
        //    try
        //    {
        //        using (SkriblContext ctx = new SkriblContext())
        //        {
        //            return Ok(new CustomResponse<ProductsViewModel>
        //            {
        //                Message = Global.ResponseMessages.Success,
        //                StatusCode = (int)HttpStatusCode.OK,
        //                Result = new ProductsViewModel { Products = ctx.Products.Where(x => x.IsDeleted == false).OrderByDescending(x => x.OrderedCount).Take(Count).ToList() }
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}
    }
}
