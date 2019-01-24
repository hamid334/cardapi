using BasketApi.BindingModels;
using BasketApi.ViewModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using static BasketApi.Global;
using System.Data.Entity;
using System.Globalization;
using BasketApi.AdminViewModel;
using static BasketApi.Utility;

namespace BasketApi.Controllers
{

    [RoutePrefix("api/Subscriptions")]
    public class SubscriptionsController : ApiController
    {
        [BasketApi.Authorize("User")]
        [HttpPost]
        [Route("SubscribeVideo")]
        public async Task<IHttpActionResult> SubscribeVideo(SubscribeVideoBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (SkriblContext ctx = new SkriblContext())
                {
                    var box = ctx.Boxes.FirstOrDefault(x => x.Id == model.Box_Id && x.IsDeleted == false);
                    if (box != null)
                    {
                        switch (model.Type)
                        {
                            case (int)BoxCategoryOptions.Junior:
                                model.ExpiryDate = model.Month.AddMonths(1);
                                break;
                            case (int)BoxCategoryOptions.Monthly:
                                model.ExpiryDate = model.Month.AddMonths(1);
                                break;
                            case (int)BoxCategoryOptions.ProBox:
                                model.ExpiryDate = model.Month.AddMonths(1);
                                break;
                            case (int)BoxCategoryOptions.HallOfFame:
                                model.ExpiryDate = model.Month.AddMonths(1);
                                break;
                        }

                        UserSubscriptions subscription = new UserSubscriptions { CreatedDate = DateTime.Now, Type = box.BoxCategory_Id, Box_Id = model.Box_Id, User_Id = model.User_Id, SubscriptionDate = model.Month, ExpiryDate = model.ExpiryDate };

                        subscription.GetRandomActivationCode();

                        ctx.UserSubscriptions.Add(subscription);
                        ctx.SaveChanges();
                        return Ok(new CustomResponse<UserSubscriptions>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = subscription
                        });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "Invalid Box_Id" } });

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User")]
        [HttpGet]
        [Route("MySkriblBox")]
        public async Task<IHttpActionResult> MySkriblBox(int UserId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<MySkriblBoxViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new MySkriblBoxViewModel { Subscriptions = ctx.UserSubscriptions.Include(x => x.Box.BoxVideos).Where(x => x.User_Id == UserId && x.IsDeleted == false).ToList() }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User")]
        [HttpGet]
        [Route("GetBox")]
        public async Task<IHttpActionResult> GetBox(int Type, string month, int User_Id)
        {
            try
            {
                DateTime monthDateTime;
                DateTime.TryParse(month, out monthDateTime);

                using (SkriblContext ctx = new SkriblContext())
                {
                    var existingUserSubscription = ctx.UserSubscriptions.Include(x => x.Box).FirstOrDefault(x => x.User_Id == User_Id && x.Type == Type && x.SubscriptionDate.Month == monthDateTime.Month && x.SubscriptionDate.Year == monthDateTime.Year && x.IsDeleted == false);
                    if (existingUserSubscription != null)
                    {
                        existingUserSubscription.Box.AlreadySubscribed = true;
                        return Ok(new CustomResponse<Box>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = existingUserSubscription.Box
                        });
                    }
                    else
                    {
                        return Ok(new CustomResponse<Box>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = ctx.Boxes.FirstOrDefault(x => x.BoxCategory_Id == Type && x.ReleaseDate.Month == monthDateTime.Month && x.ReleaseDate.Year == monthDateTime.Year && x.IsDeleted == false)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User")]
        [HttpGet]
        [Route("MySubsciptions")]
        public async Task<IHttpActionResult> MySubsciptions(int UserId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<MySkriblBoxViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new MySkriblBoxViewModel
                        {
                            Subscriptions = ctx.UserSubscriptions.Include(x => x.Box).Where(x => x.User_Id == UserId && x.IsDeleted == false).ToList()
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User", "SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("SearchSubscriptions")]
        public async Task<IHttpActionResult> SearchSubscriptions(int? SubscriptionId, int? BoxId, int? UserId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    SubscriptionListViewModel returnModel = new SubscriptionListViewModel();
                    var query = @"select 
                                UserSubscriptions.Id,
                                UserSubscriptions.User_Id,
                                UserSubscriptions.SubscriptionDate,
                                UserSubscriptions.ExpiryDate,
                                UserSubscriptions.Box_Id,
                                UserSubscriptions.Type,
                                UserSubscriptions.Status,
								UserSubscriptions.ActivationCode,
                                Boxes.Name,
                                Boxes.BoxCategory_Id,
                                Boxes.Price,
                                Users.FullName,
                                Users.ProfilePictureUrl,
								Users.Email,
								Users.Phone
                                from UserSubscriptions
                                join Boxes on Boxes.Id = UserSubscriptions.Box_Id
                                join Users on Users.ID = UserSubscriptions.User_Id  ";

                    if (BoxId.HasValue && UserId.HasValue)
                    {
                        query += @"Where UserSubscriptions.User_Id=" + UserId + " AND UserSubscriptions.Box_Id=" + BoxId + "";
                    }
                    else if (BoxId.HasValue)
                    {
                        query += @"Where UserSubscriptions.Box_Id=" + BoxId + "";
                    }
                    else if (UserId.HasValue)
                    {
                        query += @"Where UserSubscriptions.User_Id=" + UserId + "";
                    }
                    else if (SubscriptionId.HasValue)
                    {
                        query += @" Where UserSubscriptions.Id=" + SubscriptionId + "";
                        returnModel.is_detail = true;

                    }
                    else
                    {

                    }
                    query += @" AND  Boxes.IsDeleted=0";
                    returnModel.Subscriptions = ctx.Database.SqlQuery<AdminSubscriptionViewModel>(query).ToList();

                    // dont need to exe for each of no data found and to prevent any other kind of exception
                    if (returnModel.Subscriptions != null)
                    {
                        foreach (var subscription in returnModel.Subscriptions)
                        {
                            subscription.BoxCategoryName = Utility.GetBoxCategoryName(subscription.BoxCategory_Id);
                        }
                    }

                    return Ok(new CustomResponse<SubscriptionListViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = returnModel
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User")]
        [HttpGet]
        [Route("ActivateBox")]
        public async Task<IHttpActionResult> ActivateBox(int UserId, int SubscriptionId, string ActivationCode)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var userSubscription = ctx.UserSubscriptions.FirstOrDefault(x => x.Id == SubscriptionId && x.User_Id == UserId && x.ActivationCode == ActivationCode && x.IsDeleted == false);
                    if (userSubscription != null)
                    {
                        userSubscription.Status = (int)SubscriptionStatus.Active;
                        ctx.SaveChanges();
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                    }
                    else
                    {
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "Invalid Subscription" } });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User")]
        [HttpGet]
        [Route("GetBoxesByType")]
        public async Task<IHttpActionResult> GetBoxesByType(int Type, int UserId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    List<Box> boxes = new List<Box>();
                    if (Type == (int)BoxCategoryOptions.HallOfFame)
                    {
                        boxes = ctx.Boxes.Where(x => x.IsDeleted == false && x.ReleaseDate.Month < DateTime.Now.Month).ToList();
                    }
                    else
                        boxes = ctx.Boxes.Where(x => x.IsDeleted == false && x.BoxCategory_Id == Type).ToList();

                    var userSubscriptions = ctx.UserSubscriptions.Where(x => x.User_Id == UserId && x.IsDeleted == false).ToList();

                    if (userSubscriptions != null && userSubscriptions.Count > 0)
                    {
                        foreach (var box in boxes)
                        {
                            box.AlreadySubscribed = userSubscriptions.Any(x => x.Box_Id == box.Id);
                        }
                    }

                    return Ok(new CustomResponse<SearchBoxesViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new SearchBoxesViewModel { Boxes = boxes }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
    }
}
