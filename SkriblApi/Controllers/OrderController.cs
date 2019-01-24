using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using BasketApi.ViewModels;
using BasketApi.DomainModels;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using static BasketApi.Global;
using Newtonsoft.Json;
using BasketApi.CustomAuthorization;

namespace BasketApi.Controllers
{

    [RoutePrefix("api/Order")]
    public class OrderController : ApiController
    {
        [BasketApi.Authorize("User", "Deliverer")]
        /// <summary>
        /// Delete user order
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DeleteOrderFromHistory")]
        public async Task<IHttpActionResult> DeleteOrderFromHistory(int UserId, int OrderId, int StoreOrderId, int SignInType, bool StoreOrder)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    if (SignInType == (int)RoleTypes.User)
                    {
                        if (ctx.Users.Any(x => x.Id == UserId))
                        {
                            if (StoreOrder)
                            {
                                var order = ctx.Orders.Include(x => x.StoreOrders).FirstOrDefault(x => x.Id == OrderId);

                                order.StoreOrders.FirstOrDefault(x => x.Id == StoreOrderId).RemoveFromUserHistory = true;

                                order.RemoveFromUserHistory = !(order.StoreOrders.Any(x => x.RemoveFromUserHistory == false));
                            }
                            else
                            {
                                var order = ctx.Orders.FirstOrDefault(x => x.Id == OrderId);
                                order.RemoveFromUserHistory = true;
                            }

                            ctx.SaveChanges();
                            return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                        }
                        else
                            return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.NotFound, StatusCode = (int)HttpStatusCode.NotFound, Result = new Error { ErrorMessage = "Invalid UserId" } });
                    }
                    else
                    {
                        if (ctx.DeliveryMen.Any(x => x.Id == UserId))
                        {
                            if (StoreOrder)
                            {
                                var order = ctx.Orders.Include(x => x.StoreOrders).FirstOrDefault(x => x.Id == OrderId);

                                order.StoreOrders.FirstOrDefault(x => x.Id == StoreOrderId).RemoveFromUserHistory = true;

                                order.RemoveFromUserHistory = !(order.StoreOrders.Any(x => x.RemoveFromUserHistory == false));
                            }
                            else
                            {
                                var order = ctx.Orders.FirstOrDefault(x => x.Id == OrderId);
                                order.RemoveFromDelivererHistory = true;
                            }

                            ctx.SaveChanges();

                            return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                        }
                        else
                            return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.NotFound, StatusCode = (int)HttpStatusCode.NotFound, Result = new Error { ErrorMessage = "Invalid UserId" } });
                    }

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User", "Deliverer")]
        /// <summary>
        /// Get User's previous orders
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetOrdersHistory")]
        public async Task<IHttpActionResult> GetOrdersHistory(int UserId, int SignInType, bool IsCurrentOrder, int PageSize, int PageNo)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    BasketApi.AppsViewModels.OrdersHistoryViewModel orderHistory = new AppsViewModels.OrdersHistoryViewModel();

                    if (SignInType == (int)RoleTypes.User)
                    {
                        if (IsCurrentOrder)
                            orderHistory.Count = ctx.Orders.Count(x => x.User_ID == UserId && x.IsDeleted == false && x.Status != (int)OrderStatuses.Completed && x.RemoveFromUserHistory == false);
                        else
                            orderHistory.Count = ctx.Orders.Count(x => x.User_ID == UserId && x.IsDeleted == false && x.Status == (int)OrderStatuses.Completed && x.RemoveFromUserHistory == false);
                    }
                    else
                    {
                        if (IsCurrentOrder)
                            orderHistory.Count = ctx.Orders.Count(x => x.DeliveryMan_Id == UserId && x.IsDeleted == false && x.Status != (int)OrderStatuses.Completed && x.RemoveFromDelivererHistory == false);
                        else
                            orderHistory.Count = ctx.Orders.Count(x => x.DeliveryMan_Id == UserId && x.IsDeleted == false && x.Status == (int)OrderStatuses.Completed && x.RemoveFromDelivererHistory == false);
                    }

                    if (orderHistory.Count == 0)
                    {
                        return Ok(new CustomResponse<AppsViewModels.OrdersHistoryViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = orderHistory });
                    }

                    #region OrderQuery
                    string orderQuery = String.Empty;
                    if (SignInType == (int)RoleTypes.User)
                    {
                        orderQuery = @"
                        SELECT *, Users.FullName as UserFullName FROM Orders 
                        join Users on Users.ID = Orders.User_ID
                        where Orders.User_Id = " + UserId + @" and Orders.IsDeleted = 0 and Users.IsDeleted = 0 and Orders.RemoveFromUserHistory = 0 and " + (IsCurrentOrder ? " Orders.Status <> " + (int)OrderStatuses.Completed : " Orders.Status = " + (int)OrderStatuses.Completed) +
                        @"ORDER BY Orders.id desc OFFSET " + PageNo * PageSize + " ROWS FETCH NEXT " + PageSize + " ROWS ONLY;";
                    }
                    else
                    {
                        orderQuery = @"
                        SELECT *, Users.FullName as UserFullName FROM Orders 
                        join Users on Users.ID = Orders.User_ID
                        join DeliveryMen on DeliveryMen.ID = Orders.DeliveryMan_Id
                        where Orders.DeliveryMan_Id = " + UserId + @" and Orders.IsDeleted = 0 and Orders.RemoveFromDelivererHistory = 0 and " + (IsCurrentOrder ? " Orders.Status <> " + (int)OrderStatuses.Completed : " Orders.Status = " + (int)OrderStatuses.Completed) +
                        @"ORDER BY Orders.id desc OFFSET " + PageNo * PageSize + " ROWS FETCH NEXT " + PageSize + " ROWS ONLY;";
                    }

                    #endregion

                    orderHistory.orders = ctx.Database.SqlQuery<BasketApi.AppsViewModels.OrderViewModel>(orderQuery).ToList();
                    if (orderHistory.orders.Count == 0)
                    {
                        return Ok(new CustomResponse<AppsViewModels.OrdersHistoryViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = orderHistory });
                    }
                    var orderIds = string.Join(",", orderHistory.orders.Select(x => x.Id.ToString()));

                    #region StoreOrderQuery
                    string storeOrderQuery = String.Empty;
                    if (SignInType == (int)RoleTypes.User)
                    {
                        storeOrderQuery = @"
select
StoreOrders.*,
Stores.Name as StoreName,
Stores.ImageUrl from StoreOrders 
join Stores on Stores.Id = StoreOrders.Store_Id
where 
Order_Id in (" + orderIds + @")
and StoreOrders.RemoveFromUserHistory = 0
";

                    }
                    else
                    {
                        storeOrderQuery = @"
select
StoreOrders.*,
Stores.Name as StoreName,
Stores.ImageUrl from StoreOrders 
join Stores on Stores.Id = StoreOrders.Store_Id
where 
Order_Id in (" + orderIds + @")
and StoreOrders.RemoveFromDelivererHistory = 0
";
                    }

                    #endregion

                    var storeOrders = ctx.Database.SqlQuery<BasketApi.AppsViewModels.StoreOrderViewModel>(storeOrderQuery).ToList();

                    if (storeOrders.Count == 0)
                    {
                        orderHistory.orders.Clear();
                        return Ok(new CustomResponse<AppsViewModels.OrdersHistoryViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = orderHistory });
                    }

                    var storeOrderIds = string.Join(",", storeOrders.Select(x => x.Id.ToString()));

                    #region OrderItemsQuery
                    var orderItemsQuery = @"
SELECT
  CASE
    WHEN ISNULL(Order_Items.Product_Id, 0) <> 0 THEN Products.Id
    WHEN ISNULL(Order_Items.Package_Id, 0) <> 0 THEN Packages.Id
    WHEN ISNULL(Order_Items.Offer_Product_Id, 0) <> 0 THEN Offer_Products.Id
    WHEN ISNULL(Order_Items.Offer_Package_Id, 0) <> 0 THEN Offer_Packages.Id
  END AS ItemId,
  Order_Items.Name AS Name,
  Order_Items.Price AS Price,
  CASE
    WHEN ISNULL(Order_Items.Product_Id, 0) <> 0 THEN Products.ImageUrl
    WHEN ISNULL(Order_Items.Package_Id, 0) <> 0 THEN Packages.ImageUrl
    WHEN ISNULL(Order_Items.Offer_Product_Id, 0) <> 0 THEN Offer_Products.ImageUrl
    WHEN ISNULL(Order_Items.Offer_Package_Id, 0) <> 0 THEN Offer_Packages.ImageUrl
  END AS ImageUrl,
  Order_Items.Id,
  Order_Items.Qty,
  ISNULL(Products.WeightInGrams,0) as WeightInGrams,
  ISNULL(Products.WeightInKiloGrams,0) as WeightInKiloGrams,
  Order_Items.StoreOrder_Id
FROM Order_Items
LEFT JOIN products
  ON products.Id = Order_Items.Product_Id
LEFT JOIN Packages
  ON Packages.Id = Order_Items.Package_Id
LEFT JOIN Offer_Products
  ON Offer_Products.Id = Order_Items.Offer_Product_Id
LEFT JOIN Offer_Packages
  ON Offer_Packages.Id = Order_Items.Offer_Package_Id
WHERE StoreOrder_Id IN (" + storeOrderIds + ")";
                    #endregion

                    var orderItems = ctx.Database.SqlQuery<BasketApi.AppsViewModels.OrderItemViewModel>(orderItemsQuery).ToList();

                    var userFavourites = ctx.Favourites.Where(x => x.User_ID == UserId && x.IsDeleted == false).ToList();

                    foreach (var orderItem in orderItems)
                    {
                        orderItem.Weight = Convert.ToString(orderItem.WeightInGrams) + " gm";

                        if (userFavourites.Any(x => x.Product_Id == orderItem.Id))
                            orderItem.IsFavourite = true;
                        else
                            orderItem.IsFavourite = false;
                    }

                    foreach (var orderItem in orderItems)
                    {
                        storeOrders.FirstOrDefault(x => x.Id == orderItem.StoreOrder_Id).OrderItems.Add(orderItem);
                    }

                    foreach (var storeOrder in storeOrders)
                    {
                        orderHistory.orders.FirstOrDefault(x => x.Id == storeOrder.Order_Id).StoreOrders.Add(storeOrder);
                    }

                    return Ok(new CustomResponse<AppsViewModels.OrdersHistoryViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = orderHistory });

                    #region CommentedOldLogic
                    //OrdersHistoryViewModel ordersHistoryViewModel = new OrdersHistoryViewModel();

                    //if (IsCurrentOrder)
                    //{
                    //    if (SignInType == (int)RoleTypes.User)
                    //    {
                    //        ordersHistoryViewModel.Count = ctx.Orders.Count(x => x.User_ID == UserId && x.IsDeleted == false && x.Status != (int)OrderStatuses.Completed);

                    //        ordersHistoryViewModel.orders = ctx.Orders.Include(x => x.StoreOrders.Select(x1 => x1.Order_Items.Select(x2 => x2.Product.Store)))
                    //            .Where(x => x.User_ID == UserId && x.IsDeleted == false && x.Status != (int)OrderStatuses.Completed).OrderBy(x => x.Id).Page(PageSize, PageNo).ToList();
                    //    }
                    //    else
                    //    {
                    //        ordersHistoryViewModel.Count = ctx.Orders.Count(x => x.DeliveryMan_Id == UserId && x.IsDeleted == false && x.Status != (int)OrderStatuses.Completed);

                    //        ordersHistoryViewModel.orders = ctx.Orders.Include(x => x.StoreOrders.Select(x1 => x1.Order_Items.Select(x2 => x2.Product.Store)))
                    //         .Where(x => x.DeliveryMan_Id == UserId && x.IsDeleted == false && x.Status != (int)OrderStatuses.Completed).OrderBy(x => x.Id).Page(PageSize, PageNo).ToList();
                    //    }
                    //}
                    //else
                    //{
                    //    if (SignInType == (int)RoleTypes.User)
                    //    {
                    //        ordersHistoryViewModel.Count = ctx.Orders.Count(x => x.User_ID == UserId && x.IsDeleted == false && x.Status == (int)OrderStatuses.Completed);

                    //        ordersHistoryViewModel.orders = ctx.Orders.Include(x => x.StoreOrders.Select(x1 => x1.Order_Items.Select(x2 => x2.Product.Store)))
                    //            .Where(x => x.User_ID == UserId && x.IsDeleted == false && x.Status == (int)OrderStatuses.Completed).OrderBy(x => x.Id).Page(PageSize, PageNo).ToList();
                    //    }
                    //    else
                    //    {
                    //        ordersHistoryViewModel.Count = ctx.Orders.Count(x => x.DeliveryMan_Id == UserId && x.IsDeleted == false && x.Status == (int)OrderStatuses.Completed);

                    //        ordersHistoryViewModel.orders = ctx.Orders.Include(x => x.StoreOrders.Select(x1 => x1.Order_Items.Select(x2 => x2.Product.Store)))
                    //            .Where(x => x.DeliveryMan_Id == UserId && x.IsDeleted == false && x.Status == (int)OrderStatuses.Completed).OrderBy(x => x.Id).Page(PageSize, PageNo).ToList();
                    //    }
                    //}
                    //return Ok(new CustomResponse<OrdersHistoryViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ordersHistoryViewModel }); 
                    #endregion
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User")]
        [HttpGet]
        [Route("EditOrderScheduling")]
        public async Task<IHttpActionResult> EditOrderScheduling(int OrderId, string From, string To, string AdditionalNote)
        {
            try
            {
                DateTime FromDateTime;
                DateTime ToDateTime;
                DateTime.TryParse(From, out FromDateTime);
                DateTime.TryParse(To, out ToDateTime);

                if (FromDateTime == DateTime.MinValue || ToDateTime == DateTime.MinValue)
                    return Ok(new CustomResponse<Error> { Message = "BadRequest", StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "Invalid startdate or enddate" } });

                using (SkriblContext ctx = new SkriblContext())
                {
                    var order = ctx.Orders.FirstOrDefault(x => x.Id == OrderId);
                    if (order != null)
                    {
                        order.DeliveryTime_From = FromDateTime;
                        order.DeliveryTime_To = ToDateTime;
                        order.AdditionalNote = AdditionalNote;
                        ctx.SaveChanges();
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.NotFound, StatusCode = (int)HttpStatusCode.NotFound, Result = new Error { ErrorMessage = Global.ResponseMessages.GenerateInvalid("OrderId") } });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("User")]
        [HttpPost]
        [Route("InsertOrder")]
        public async Task<IHttpActionResult> InsertOrder(OrderViewModel model)
        {
            try
            {
                Order order;

                if (System.Web.HttpContext.Current.Request.Params["PaymentCard"] != null)
                    model.PaymentCard = JsonConvert.DeserializeObject<PaymentCardViewModel>(System.Web.HttpContext.Current.Request.Params["PaymentCard"]);

                if (System.Web.HttpContext.Current.Request.Params["Cart"] != null)
                    model.Cart = JsonConvert.DeserializeObject<CartViewModel>(System.Web.HttpContext.Current.Request.Params["Cart"]);

                Validate(model);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (model.Cart.CartItems.Count() > 0)
                {
                    order = new Order();
                    order.MakeOrder(model);

                    using (SkriblContext ctx = new SkriblContext())
                    {
                        ctx.Orders.Add(order);
                        await ctx.SaveChangesAsync();
                    }

                    return Ok(new CustomResponse<OrderSummaryViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new OrderSummaryViewModel(order) });
                }
                else
                    return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "No items in the cart." } });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize("User", "Deliverer")]
        [HttpGet]
        [Route("GetOrderByOrderId")]
        public async Task<IHttpActionResult> GetOrderByOrderId(int OrderId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    #region OrderQuery
                    var orderQuery = @"
SELECT *, Users.FullName as UserFullName FROM Orders 
join Users on Users.ID = Orders.User_ID
where Orders.Id = " + OrderId + @" and Orders.IsDeleted = 0 ";
                    #endregion

                    var order = ctx.Database.SqlQuery<BasketApi.AppsViewModels.OrderViewModel>(orderQuery).First();

                    #region storeOrderQuery
                    var storeOrderQuery = @"
select
StoreOrders.*,
Stores.Name as StoreName,
Stores.ImageUrl from StoreOrders 
join Stores on Stores.Id = StoreOrders.Store_Id
where 
Order_Id = " + order.Id + @"
";
                    #endregion
                    var UserQuery = @"
select Users.Id , 
Users.FirstName,
Users.LastName,
Users.Email,
Users.Phone,
Users.ProfilePictureUrl
from 
Users Where Users.Id=" + order.User_ID + "";
                    var user = ctx.Database.SqlQuery<BasketApi.ViewModels.UserViewModel>(UserQuery).FirstOrDefault();


                    var storeOrders = ctx.Database.SqlQuery<BasketApi.AppsViewModels.StoreOrderViewModel>(storeOrderQuery).ToList();

                    var storeOrderIds = string.Join(",", storeOrders.Select(x => x.Id.ToString()));

                    #region OrderItemsQuery
                    var orderItemsQuery = @"
SELECT
  CASE
    WHEN ISNULL(Order_Items.Product_Id, 0) <> 0 THEN Products.Id
    WHEN ISNULL(Order_Items.Package_Id, 0) <> 0 THEN Packages.Id
    WHEN ISNULL(Order_Items.Offer_Product_Id, 0) <> 0 THEN Offer_Products.Id
    WHEN ISNULL(Order_Items.Offer_Package_Id, 0) <> 0 THEN Offer_Packages.Id
  END AS ItemId,
  Order_Items.Name AS Name,
  Order_Items.Price AS Price,
  CASE
    WHEN ISNULL(Order_Items.Product_Id, 0) <> 0 THEN Products.ImageUrl
    WHEN ISNULL(Order_Items.Package_Id, 0) <> 0 THEN Packages.ImageUrl
    WHEN ISNULL(Order_Items.Offer_Product_Id, 0) <> 0 THEN Offer_Products.ImageUrl
    WHEN ISNULL(Order_Items.Offer_Package_Id, 0) <> 0 THEN Offer_Packages.ImageUrl
  END AS ImageUrl,
  Order_Items.Id,
  Order_Items.Qty,
  ISNULL(Products.WeightInGrams,0) as WeightInGrams,
  ISNULL(Products.WeightInKiloGrams,0) as WeightInKiloGrams,
  Order_Items.StoreOrder_Id
FROM Order_Items
LEFT JOIN products
  ON products.Id = Order_Items.Product_Id
LEFT JOIN Packages
  ON Packages.Id = Order_Items.Package_Id
LEFT JOIN Offer_Products
  ON Offer_Products.Id = Order_Items.Offer_Product_Id
LEFT JOIN Offer_Packages
  ON Offer_Packages.Id = Order_Items.Offer_Package_Id
WHERE StoreOrder_Id IN (" + storeOrderIds + ")";
                    #endregion

                    var orderItems = ctx.Database.SqlQuery<BasketApi.AppsViewModels.OrderItemViewModel>(orderItemsQuery).ToList();

                    //var userFavourites = ctx.Favourites.Where(x => x.User_ID == UserId && x.IsDeleted == false).ToList();

                    //foreach (var orderItem in orderItems)
                    //{
                    //    orderItem.Weight = Convert.ToString(orderItem.WeightInGrams) + " gm";

                    //    if (userFavourites.Any(x => x.Product_Id == orderItem.Id))
                    //        orderItem.IsFavourite = true;
                    //    else
                    //        orderItem.IsFavourite = false;
                    //}

                    foreach (var orderItem in orderItems)
                    {
                        storeOrders.FirstOrDefault(x => x.Id == orderItem.StoreOrder_Id).OrderItems.Add(orderItem);
                    }

                    foreach (var storeOrder in storeOrders)
                    {
                        order.StoreOrders.Add(storeOrder);
                    }
                    order.User = user;
                    return Ok(new CustomResponse<AppsViewModels.OrderViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = order });

                }

            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


    }
}
