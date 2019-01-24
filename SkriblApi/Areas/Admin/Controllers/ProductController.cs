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

namespace BasketApi.Areas.SubAdmin.Controllers
{
    //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin", "User", "Guest")]
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
                        return NotFound();
                    }
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

                    return Ok();
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
        public string GetCardNumber()
        {
            Random randm = new Random();
           

            string rand_card_value = "1234 5678 9123 4567";
            using (SkriblContext ctx = new SkriblContext())
            {
                do
                {
                    string rand_card_value1 = randm.Next(1000, 9999).ToString();
                    string rand_card_value2 = randm.Next(1000, 9999).ToString();
                    string rand_card_value3 = randm.Next(1000, 9999).ToString();
                    string rand_card_value4 = randm.Next(1000, 9999).ToString();
                    rand_card_value = rand_card_value1 + " " + rand_card_value2 + " " + rand_card_value3 + " " + rand_card_value4;

                } while (ctx.CardRequest.Any(x => x.CardNumber == rand_card_value));

            }

            return rand_card_value;
        }
        //public string GetCardNumber()
        //{
        //    Random randm = new Random();
        //    int rand_month = randm.Next(1, 13);
        //    int rand_dice = randm.Next(1, 9);
        //    for (int i = 0; i < 10; i++)
        //    {
        //        string rand_card_value1 = randm.Next(1000, 9999).ToString();
        //        string rand_card_value2 = randm.Next(1000, 9999).ToString();
        //        string rand_card_value3 = randm.Next(1000, 9999).ToString();
        //        string rand_card_value4 = randm.Next(1000, 9999).ToString();
        //        string rand_card_value = rand_card_value1 + " " + rand_card_value2 + " " + rand_card_value3 + " " + rand_card_value4;

        //        using (SkriblContext ctx = new SkriblContext())
        //        {
        //            if (ctx.CardRequest.Any(x => x.CardNumber == rand_card_value))
        //            {

        //            }
        //            else
        //            { return rand_card_value; }
        //        }
        //    }
        //    return "";
        //}
        [HttpGet]
        [Route("SearchOffers")]
        public async Task<IHttpActionResult> GetAllProducts(string Location, string SearchString = "", int? User_Id = 0, int? Page = 0, int? Items = 6, int? Category_Id = 0, double? Distance = 0, double? Latitude = 0, double? Longitude = 0)
        {
            try
            {
                string number = GetCardNumber();
                using (SkriblContext ctx = new SkriblContext())
                {

                    var query = "SELECT  Products.* FROM Products join Stores ON Products.Store_Id=Stores.Id Where Products.IsDeleted=0 And";

                    if (!string.IsNullOrEmpty(Location))
                    {
                        query += " Stores.Address LIKE '%" + Location + "%'";
                    }

                    if (Category_Id != 0)
                    {
                        if (!string.IsNullOrEmpty(Location))
                        {
                            query += " And ";
                        }

                        query += " Products.Category_Id=" + Category_Id.Value;
                    }

                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        if (Category_Id != 0 || !string.IsNullOrEmpty(Location))
                        {
                            query += " And (";
                        }

                        query += "Products.Name LIKE '%" + SearchString + "%'";
                        query += " OR Products.DiscountPercentage LIKE '%" + SearchString + "%'";
                        query += " OR Products.Description LIKE '%" + SearchString + "%'";
                        query += " OR Stores.Name LIKE '%" + SearchString + "%')";
                    }
                    if (Longitude != 0 && Latitude != 0 && Distance != 0) 
                    {
                        if (!string.IsNullOrEmpty(Location))
                        {
                            query += " And ";
                        }
                        query += "[dbo].[DistanceKM] (Stores.Latitude," + Latitude.Value + ", Stores.Longitude ," + Longitude.Value + ")<=" + Distance.Value;
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
                    query += " ORDER BY Products.Id desc OFFSET " + Page.Value * Items.Value + " ROWS FETCH NEXT " + Items.Value + " ROWS ONLY ";


                    var Offers = ctx.Database.SqlQuery<Product>(query).ToList();

                    if (Offers != null)
                    {
                        if (User_Id.HasValue && User_Id != 0)
                        {
                            foreach (var offer in Offers)
                            {
                                offer.IsFavourite = ctx.Favourites.Any(x => x.User_ID == User_Id && x.Product_Id == offer.Id && !x.IsFavourite);
                            }
                        }

                       

                        //foreach (var item in Offers)
                        //{
                        //    item.Store = ctx.Stores.FirstOrDefault(x => x.Id == item.Store_Id);
                        //}

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
        public async Task<IHttpActionResult> GetLatestOffers(int User_Id, int? Page = 0, int? Items = 10)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    ProductsViewModel returnModel = new ProductsViewModel();

                    var products = ctx.Products.Where(x => !x.IsDeleted).ToList();
                    if (products != null)
                    {
                        //returnModel.Products = products.Where(x => x.CreatedDate.Date == DateTime.Now.Date).OrderByDescending(x => x.Id).Skip(Page.Value * Items.Value).Take(Items.Value).ToList();
                        returnModel.Products = products.Where(x => !x.IsDeleted).OrderByDescending(x => x.Id).Take(Items.Value).ToList();

                        foreach (var item in returnModel.Products)
                        {
                            item.Store = ctx.Stores.FirstOrDefault(x => x.Id == item.Store_Id);
                            item.IsFavourite = ctx.Favourites.Any(x => x.User_ID == User_Id && x.Product_Id == item.Id && !x.IsFavourite);
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
                using (SkriblContext ctx = new SkriblContext())
                {
                    ProductsViewModel returnModel = new ProductsViewModel();

                    returnModel.Products = ctx.Products.Where(x => !x.IsDeleted).ToList();

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
