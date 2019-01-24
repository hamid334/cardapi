using BasketApi.ViewModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static BasketApi.Global;
using System.Data.Entity;

namespace BasketApi
{
    public static class ExtensionMethods
    {
        public static void CalculateAverageRating(this Store store)
        {
            try
            {
                if (store.StoreRatings.Count > 0)
                {
                    store.AverageRating = store.StoreRatings.Average(x => x.Rating);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void SetOrderItem(this Order_Items orderItem, CartItemViewModel model)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {

                    switch (model.ItemType)
                    {
                        case (int)CartItemTypes.Product:
                            orderItem.Product_Id = model.ItemId;
                            var product = ctx.Products.FirstOrDefault(x => x.Id == model.ItemId && x.IsDeleted == false);
                            orderItem.Name = product.Name;
                            orderItem.Price = product.Price * model.Qty;
                            orderItem.Description = product.Description;
                            product.OrderedCount = product.OrderedCount + model.Qty;
                            ctx.SaveChanges();
                            break;
                        case (int)CartItemTypes.Package:
                            orderItem.Package_Id = model.ItemId;
                            var package = ctx.Packages.FirstOrDefault(x => x.Id == model.ItemId && x.IsDeleted == false);
                            orderItem.Name = package.Name;
                            orderItem.Price = package.Price * model.Qty;
                            orderItem.Description = package.Description;
                            break;
                        case (int)CartItemTypes.Offer_Product:
                            orderItem.Offer_Product_Id = model.ItemId;
                            var offerProduct = ctx.Offer_Products.Include(x => x.Product).FirstOrDefault(x => x.Id == model.ItemId && x.IsDeleted == false);
                            orderItem.Name = offerProduct.Product.Name;
                            orderItem.Price = offerProduct.DiscountedPrice * model.Qty;
                            orderItem.Description = offerProduct.Description;
                            break;
                        case (int)CartItemTypes.Offer_Package:
                            orderItem.Offer_Package_Id = model.ItemId;
                            var offerPackage = ctx.Offer_Packages.Include(x => x.Package).FirstOrDefault(x => x.Id == model.ItemId && x.IsDeleted == false);
                            orderItem.Name = offerPackage.Package.Name;
                            orderItem.Price = offerPackage.DiscountedPrice * model.Qty;
                            orderItem.Description = offerPackage.Description;
                            break;
                        default:
                            throw new Exception("Invalid CartItemType");
                    }
                }

                orderItem.Qty = model.Qty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void AddNewStoreOrder(this Order order, CartItemViewModel model)
        {
            try
            {
                StoreOrder storeOrder = new StoreOrder();
                storeOrder.Store_Id = model.StoreId;
                storeOrder.OrderNo = Guid.NewGuid().ToString("N").ToUpper();
                storeOrder.AddNewOrderItem(model);
                order.StoreOrders.Add(storeOrder);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void AddNewOrderItem(this StoreOrder storeOrder, CartItemViewModel model)
        {
            try
            {
                Order_Items orderItem = new Order_Items();
                orderItem.SetOrderItem(model);
                storeOrder.Order_Items.Add(orderItem);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SetPaymentDetails(this Order order, OrderViewModel model)
        {
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SetOrderDetails(this Order order, OrderViewModel model)
        {
            try
            {
                order.OrderNo = Guid.NewGuid().ToString("N").ToUpper();
                order.OrderDateTime = DateTime.Now;
                order.Status = (int)OrderStatuses.Initiated;
                order.DeliveryTime_From = model.DeliveryDateTime_From;
                order.DeliveryTime_To = model.DeliveryDateTime_To;
                order.PaymentMethod = model.PaymentMethodType;
                order.User_ID = model.UserId;
                order.DeliveryAddress = model.DeliveryAddress;
                order.AdditionalNote = model.AdditionalNote;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void MakeOrder(this Order order, OrderViewModel model)
        {
            try
            {
                order.User_ID = model.UserId;
                foreach (var cartItem in model.Cart.CartItems)
                {
                    if (order.StoreOrders.Count == 0)
                    {
                        order.AddNewStoreOrder(cartItem);
                    }
                    else
                    {
                        var existingStoreOrder = order.StoreOrders.FirstOrDefault(x => x.Store_Id == cartItem.StoreId);

                        if (existingStoreOrder == null)
                        {
                            order.AddNewStoreOrder(cartItem);
                        }
                        else
                        {
                            existingStoreOrder.AddNewOrderItem(cartItem);
                        }
                    }
                }

                if (model.PaymentMethodType != (int)PaymentMethods.CashOnDelivery)
                {
                    order.SetPaymentDetails(model);
                }

                order.SetOrderDetails(model);

                order.CalculateSubTotal();
                order.CalculateTotal();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void CalculateSubTotal(this StoreOrder storeOrder)
        {
            storeOrder.Subtotal = storeOrder.Order_Items.Sum(x => x.Price);
        }

        public static void CalculateSubTotal(this Order order)
        {
            foreach (var storeOrder in order.StoreOrders)
            {
                storeOrder.CalculateSubTotal();
            }
            order.Subtotal = order.StoreOrders.Sum(x => x.Subtotal);
        }

        public static void CalculateTotal(this Order order)
        {
            order.CalculateSubTotal();
            order.ServiceFee = 0;
            order.DeliveryFee = order.Subtotal >= BasketSettings.FreeDeliveryThreshold ? 0 : BasketSettings.DeliveryFee;

            order.Total = order.Subtotal + order.DeliveryFee;
        }

        public static async Task GetRandomActivationCode(this UserSubscriptions userSubscription)
        {
            var crypto = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var bytes = new byte[5];
            crypto.GetBytes(bytes);
            userSubscription.ActivationCode = BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
    }
}