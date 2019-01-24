using BasketApi;
using DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication1.BindingModels;

namespace WebApplication1.Controllers
{
    [RoutePrefix("api/Settings")]
    public class SettingsController : ApiController
    {


        [HttpGet]
        [Route("GetSettings")]
        public async Task<IHttpActionResult> GetSettings()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var settings = ctx.Settings.FirstOrDefault();
                    if (settings == null)
                    {
                        return Ok(new CustomResponse<Settings>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = new Settings { DeliveryFee = 0, AboutUs = "", BannerImage = "", Currency = "", FreeDeliveryThreshold = 0, HowItWorksDescription = "", HowItWorksUrl = "" }
                        });
                    }
                    else
                    {
                        return Ok(new CustomResponse<Settings>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = ctx.Settings.FirstOrDefault()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [HttpPost]
        [Route("SetSettings")]
        public async Task<IHttpActionResult> SetSettings()
        {
            try
            {
                Settings model = new Settings();
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;



                if (httpRequest.Params["Id"] != null)
                    model.Id = Convert.ToInt32(httpRequest.Params["Id"]);

                //if (httpRequest.Params["ImageDeletedOnEdit"] != null)
                //    model.BannerImage = Convert.ToBoolean(httpRequest.Params["ImageDeletedOnEdit"]);
                if (httpRequest.Params["Currency"] != null)
                    model.Currency = httpRequest.Params["Currency"];
                if (httpRequest.Params["DeliveryFee"] != null)
                    model.DeliveryFee = Convert.ToDouble(httpRequest.Params["DeliveryFee"]);
                if (httpRequest.Params["FreeDeliveryThreshold"] != null)
                    model.FreeDeliveryThreshold = Convert.ToDouble(httpRequest.Params["FreeDeliveryThreshold"]);



                Validate(model);

                #region Validations

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (httpRequest.Files.Count > 0)
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "UnsupportedMediaType",
                            StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                            Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                        });
                    }
                    else if (httpRequest.Files.Count > 2)
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "UnsupportedMediaType",
                            StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                            Result = new Error { ErrorMessage = "Multiple images are not supported, please upload upto two image" }
                        });
                    }
                }
                #endregion



                //model.Phone = httpRequest.Params["Phone"];
                //model.Role = Convert.ToInt16(httpRequest.Params["Role"]);
                //model.Password = httpRequest.Params["Password"];
                //model.Status = (int)Global.StatusCode.NotVerified;



                using (SkriblContext ctx = new SkriblContext())
                {
                    Settings returnModel = new Settings();
                    returnModel = ctx.Settings.FirstOrDefault();
                    // in case on showing subscription table 
                    if (string.IsNullOrEmpty(model.BannerImage) && string.IsNullOrEmpty(model.Currency) && model.DeliveryFee == 0)
                    {
                        return Ok(new CustomResponse<Settings>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = returnModel
                        });

                    }

                    string fileExtension = string.Empty;
                    HttpPostedFile postedFile;
                    #region ImageSaving
                    foreach (var file in httpRequest.Files)
                    {
                        postedFile = httpRequest.Files[file.ToString()];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                            var ext = Path.GetExtension(postedFile.FileName);
                            fileExtension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(fileExtension))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload image of type .jpg,.gif,.png" }
                                });
                            }
                            else if (postedFile.ContentLength > Global.MaximumImageSize)
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload a file upto " + Global.ImageSize }
                                });
                            }
                            else
                            {

                            }
                        }
                        var guid = Guid.NewGuid();
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["BannerImageFolerPath"] + model.Id + "_" + guid + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        if (Path.GetFileNameWithoutExtension(postedFile.FileName) == "Banner")
                        {
                            model.BannerImage = ConfigurationManager.AppSettings["BannerImageFolerPath"] + model.Id + "_" + guid + fileExtension;
                        }
                        else if (Path.GetFileNameWithoutExtension(postedFile.FileName) == "Instagram")
                        {
                            model.InstagramImage = ConfigurationManager.AppSettings["BannerImageFolerPath"] + model.Id + "_" + guid + fileExtension;
                        }
                    }
                    #endregion

                    if (returnModel != null)
                    {
                        if (model.BannerImage != "")
                        {
                            returnModel.BannerImage = model.BannerImage;
                        }
                        if (model.InstagramImage != null)
                        {
                            returnModel.InstagramImage = model.InstagramImage;
                        }
                        returnModel.Currency = model.Currency;
                        returnModel.DeliveryFee = model.DeliveryFee;
                        returnModel.FreeDeliveryThreshold = model.FreeDeliveryThreshold;
                        ctx.SaveChanges();

                    }
                    else
                    {
                        ctx.Settings.Add(model);
                        ctx.SaveChanges();
                    }

                    BasketSettings.LoadSettings();

                    return Ok(new CustomResponse<Settings>
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




    }
}
