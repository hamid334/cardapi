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
using BasketApi;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Configuration;
using BasketApi.Models;
using Iyzipay;

namespace WebApplication1.Controllers
{
    //[RoutePrefix("api/Payment")]
    public class PaymentController : ApiController
    {
        // GET: Payment
        [HttpPost]
        [Route("api/GeneratePayment")]
        public async Task<IHttpActionResult> GeneratePayment()
        {
            try
            {
                Iyzipay.Model.PaymentCard payment = new Iyzipay.Model.PaymentCard();

                var httpRequest = System.Web.HttpContext.Current.Request;

          //    payment.CardAlias = httpRequest.Params["CardAlias"];
                payment.CardHolderName = httpRequest.Params["CardHolderName"];
                payment.CardNumber = httpRequest.Params["CardNumber"].Trim();
                payment.Cvc = httpRequest.Params["Cvc"].Trim();
                payment.ExpireMonth = httpRequest.Params["ExpireMonth"].Trim();
                payment.ExpireYear = httpRequest.Params["ExpireYear"].Trim();
                
                payment.RegisterCard = 0;

                Validate(payment);

                #region Validations
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                #endregion

               

                string paymentId = Guid.NewGuid().ToString();
                CreatePaymentRequest request = new CreatePaymentRequest();
                request.Locale = Locale.TR.ToString();
                request.ConversationId = paymentId;
                request.Price = httpRequest.Params["Price"];
                request.PaidPrice = httpRequest.Params["PaidPrice"];
                request.Currency = Currency.TRY.ToString();
                request.Installment = 1;
                request.BasketId = paymentId;
                PaymentChannel outputval;
                Enum.TryParse(httpRequest.Params["PaymentChannel"].ToString(), out  outputval);
                request.PaymentChannel = outputval.ToString();// Enum.GetName(typeof(PaymentChannel), httpRequest.Params["PaymentChannel"].ToString());
                request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
                request.PaymentCard = payment;

                Buyer buyer = new Buyer();
                buyer.Id = paymentId;
                buyer.Name = httpRequest.Params["FirstName"] + " " + httpRequest.Params["LastName"];
                buyer.Surname = httpRequest.Params["LastName"]; 
                buyer.GsmNumber = "+905350000000";
                buyer.Email = httpRequest.Params["Email"];
                buyer.IdentityNumber = paymentId;
                buyer.LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                buyer.RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                buyer.RegistrationAddress = "N/A";
                buyer.Ip = httpRequest.Params["IpAddress"]; ;
                buyer.City = httpRequest.Params["City"];
                buyer.Country = "Turkey";
                buyer.ZipCode = "34732";
                request.Buyer = buyer;

                Address shippingAddress = new Address();
                shippingAddress.ContactName = httpRequest.Params["FirstName"] + " " + httpRequest.Params["LastName"];
                shippingAddress.City = httpRequest.Params["City"];
                shippingAddress.Country = "Turkey";
                shippingAddress.Description = "N/A";
                shippingAddress.ZipCode = "34742";
                request.ShippingAddress = shippingAddress;
                request.BillingAddress = shippingAddress;
                List<BasketItem> basketItems = new List<BasketItem>();
                BasketItem firstBasketItem = new BasketItem();
                firstBasketItem.Id = "AladdinCard101";
                firstBasketItem.Name = "Aladdin Card Payment";
                firstBasketItem.Category1 = "Aladdin Card";
                firstBasketItem.Category2 = "Aladdin Card Online Purchase";
                firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
                firstBasketItem.Price = httpRequest.Params["Price"];
                basketItems.Add(firstBasketItem);

                request.BasketItems = basketItems;
                Options op = new Options();
                op.ApiKey = ConfigurationManager.AppSettings["PaymentApiKey"].ToString();
                op.SecretKey = ConfigurationManager.AppSettings["PaymentApiSecretKey"].ToString();
                op.BaseUrl = ConfigurationManager.AppSettings["PaymentApiBaseUrl"].ToString();
                Validate(request);

                #region Validations
                if (httpRequest.Params.Count != 12)
                {
                   // return BadRequest(ModelState);
                }
               
                #endregion
                Payment GeneratePayment = Payment.Create(request, op);
                string status = GeneratePayment.Status;
                string errormessage = GeneratePayment.ErrorMessage == "Kart numarası geçersizdir" ? "Card number is invalid" : GeneratePayment.ErrorMessage;
                CustomResponse<Object> response = new CustomResponse<Object>
                {
                    Message = Global.ResponseMessages.Success,
                    StatusCode = (int)HttpStatusCode.OK,
                    Result = status
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }

        }
    }
}