using BasketApi;
using BasketApi.Areas.Admin.ViewModels;
using BasketApi.Areas.Agent.Models;
using BasketApi.CustomAuthorization;
using BasketApi.Models;
using BasketApi.ViewModels;
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
using System.Web.Security;
using WebApplication1.Areas.Admin.ViewModels;
using System.Data.Entity;
using BasketApi.BindingModels;
using static BasketApi.Utility;
using Newtonsoft.Json;
using static BasketApi.Global;
using System.Globalization;
using System.Data.Entity.Core.Objects;
using System.Web.Hosting;
using WebApplication1.ViewModels;

namespace WebApplication1.Areas.Admin.Controllers
{
    [RoutePrefix("api/Admin")]
    public class AdminController : ApiController
    {
        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        /// <summary>
        /// Add admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddAdmin")]
        public async Task<IHttpActionResult> AddAdmin()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                DAL.Admin model = new DAL.Admin();
                DAL.Admin existingAdmin = new DAL.Admin();

                if (httpRequest.Params["Id"] != null)
                    model.Id = Convert.ToInt32(httpRequest.Params["Id"]);

                if (httpRequest.Params["ImageDeletedOnEdit"] != null)
                    model.ImageDeletedOnEdit = Convert.ToBoolean(httpRequest.Params["ImageDeletedOnEdit"]);

                model.FirstName = httpRequest.Params["FirstName"];
                model.LastName = httpRequest.Params["LastName"];
                model.Email = httpRequest.Params["Email"];
                model.Phone = httpRequest.Params["Phone"];
                model.Role = Convert.ToInt16(httpRequest.Params["Role"]);
                model.Password = httpRequest.Params["Password"];
                model.Status = (int)Global.StatusCode.NotVerified;

                if (httpRequest.Params["Store_Id"] != null)
                    model.Store_Id = Convert.ToInt32(httpRequest.Params["Store_Id"]);

                Validate(model);

                #region Validations

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image" }
                    });
                }
                #endregion

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (model.Id == 0)
                    {

                        if (ctx.Admins.Any(x => x.Email == model.Email && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Admin with same email already exists" }
                            });
                        }
                    }
                    else
                    {
                        existingAdmin = ctx.Admins.FirstOrDefault(x => x.Id == model.Id);
                        model.Password = existingAdmin.Password;
                        if (existingAdmin.Email.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase) == false || existingAdmin.Store_Id != model.Store_Id)
                        {
                            if (ctx.Admins.Any(x => x.IsDeleted == false && x.Store_Id == model.Store_Id && x.Email.Equals(model.Email.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Admin with same email already exists" }
                                });
                            }
                        }
                    }

                    string fileExtension = string.Empty;
                    HttpPostedFile postedFile = null;
                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
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
                                //int count = 1;
                                //fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                                //newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["AdminImageFolderPath"] + postedFile.FileName);

                                //while (File.Exists(newFullPath))
                                //{
                                //    string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                //    newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["AdminImageFolderPath"] + tempFileName + extension);
                                //}
                                //postedFile.SaveAs(newFullPath);
                            }
                        }
                        //model.ImageUrl = ConfigurationManager.AppSettings["AdminImageFolderPath"] + Path.GetFileName(newFullPath);
                    }
                    #endregion

                    if (model.Id == 0)
                    {
                        ctx.Admins.Add(model);
                        ctx.SaveChanges();
                        var guid = Guid.NewGuid();
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["AdminImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        model.ImageUrl = ConfigurationManager.AppSettings["AdminImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        ctx.SaveChanges();

                    }
                    else
                    {
                        //existingProduct = ctx.Products.FirstOrDefault(x => x.Id == model.Id);
                        if (httpRequest.Files.Count == 0)
                        {
                            // Check if image deleted
                            if (model.ImageDeletedOnEdit == false)
                            {
                                model.ImageUrl = existingAdmin.ImageUrl;
                            }
                        }
                        else
                        {
                            Utility.DeleteFileIfExists(existingAdmin.ImageUrl);
                            var guid = Guid.NewGuid();
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["AdminImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                            postedFile.SaveAs(newFullPath);
                            model.ImageUrl = ConfigurationManager.AppSettings["AdminImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        }

                        ctx.Entry(existingAdmin).CurrentValues.SetValues(model);
                        ctx.SaveChanges();
                    }

                    await model.GenerateToken(Request);

                    CustomResponse<DAL.Admin> response = new CustomResponse<DAL.Admin>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = model
                    };

                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "User")]
        /// <summary>
        /// Add category with image. This is multipart request
        /// </summary>
        /// <returns></returns>
        [Route("AddCategory")]
        public async Task<IHttpActionResult> AddCategoryWithImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                Category model = new Category();
                Category existingCategory = new Category();

                if (httpRequest.Params["Id"] != null)
                {
                    model.Id = Convert.ToInt32(httpRequest.Params["Id"]);
                }
                if (httpRequest.Params["ParentCategoryId"] != null)
                {
                    model.ParentCategoryId = Convert.ToInt32(httpRequest.Params["ParentCategoryId"]);
                }
                if (httpRequest.Params["ImageDeletedOnEdit"] != null)
                {
                    model.ImageDeletedOnEdit = Convert.ToBoolean(httpRequest.Params["ImageDeletedOnEdit"]);
                }
                model.Name = httpRequest.Params["Name"];
                model.Description = httpRequest.Params["Description"];
                //model.Store_Id = Convert.ToInt32(httpRequest.Params["Store_Id"]);

                Validate(model);

                #region Validations
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image" }
                    });
                }
                #endregion

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (model.Id == 0)
                    {
                        if (ctx.Categories.Any(x => x.Name.Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase) && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Category already exist under same store" }
                            });
                        }
                    }
                    else
                    {
                        existingCategory = ctx.Categories.FirstOrDefault(x => x.Id == model.Id);
                        if (existingCategory.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            if (ctx.Categories.Any(x => x.IsDeleted == false && x.Name.Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Category with same name already exist under same store" }
                                });
                            }
                        }

                        if (existingCategory.Id == model.ParentCategoryId)
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Parent category name and child category name must be different" }
                            });
                        }
                    }

                    HttpPostedFile postedFile = null;
                    string fileExtension = string.Empty;

                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
                        if (postedFile != null && postedFile.ContentLength > 0 && Request.Content.IsMimeMultipartContent())
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
                                //int count = 1;
                                //fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                                //newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["CategoryImageFolderPath"] + postedFile.FileName);

                                //while (File.Exists(newFullPath))
                                //{
                                //    string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                //    newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["CategoryImageFolderPath"] + tempFileName + extension);
                                //}
                                //postedFile.SaveAs(newFullPath);
                            }
                        }
                        //model.ImageUrl = ConfigurationManager.AppSettings["CategoryImageFolderPath"] + Path.GetFileName(newFullPath);
                    }
                    #endregion


                    if (model.Id == 0)
                    {
                        ctx.Categories.Add(model);
                        ctx.SaveChanges();
                        var guid = Guid.NewGuid();
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["CategoryImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        model.ImageUrl = ConfigurationManager.AppSettings["CategoryImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        //var existingCategory = ctx.Categories.FirstOrDefault(x => x.Id == model.Id);
                        if (httpRequest.Files.Count == 0)
                        {
                            // Check if image deleted
                            if (model.ImageDeletedOnEdit == false)
                            {
                                model.ImageUrl = existingCategory.ImageUrl;
                            }
                        }
                        else
                        {
                            Utility.DeleteFileIfExists(existingCategory.ImageUrl);
                            var guid = Guid.NewGuid();
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["CategoryImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                            postedFile.SaveAs(newFullPath);
                            model.ImageUrl = ConfigurationManager.AppSettings["CategoryImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        }

                        model.Sorting = existingCategory.Sorting;
                        ctx.Entry(existingCategory).CurrentValues.SetValues(model);
                        ctx.SaveChanges();
                    }


                    CustomResponse<Category> response = new CustomResponse<Category>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = model
                    };

                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        /// <summary>
        /// Add product with image. This is multipart request
        /// </summary>
        /// <returns></returns>
        [Route("AddProduct")]
        public async Task<IHttpActionResult> AddProductWithImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                Product model = new Product();
                Product existingProduct = new Product();

                if (httpRequest.Params["Id"] != null)
                {
                    model.Id = Convert.ToInt32(httpRequest.Params["Id"]);
                }

                if (httpRequest.Params["ImageDeletedOnEdit"] != null)
                {
                    model.ImageDeletedOnEdit = Convert.ToBoolean(httpRequest.Params["ImageDeletedOnEdit"]);
                }

                model.WeightUnit = Convert.ToInt32(httpRequest.Params["WeightUnit"]);

                if (model.WeightUnit == 1) // 1 for gm, 2 for kg
                    model.WeightInGrams = Convert.ToDouble(httpRequest.Params["WeightInGrams"]);
                else
                    model.WeightInKiloGrams = Convert.ToDouble(httpRequest.Params["WeightInKiloGrams"]);

                model.Name = httpRequest.Params["Name"];
                model.Price = Convert.ToDouble(httpRequest.Params["Price"]);
                model.Category_Id = Convert.ToInt32(httpRequest.Params["Category_Id"]);
                model.Description = httpRequest.Params["Description"];
                model.Store_Id = Convert.ToInt32(httpRequest.Params["Store_Id"]);
                model.DiscountPercentage = Convert.ToDouble(httpRequest.Params["DiscountPercentage"]);
                model.DiscountPrice = Convert.ToDouble(httpRequest.Params["DiscountPrice"]);
                model.CreatedDate = DateTime.UtcNow;


                Validate(model);

                #region Validations

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image" }
                    });
                }
                #endregion

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (model.Id == 0)
                    {
                        if (ctx.Products.Any(x => x.Store_Id == model.Store_Id && x.Name == model.Name && x.Store_Id == model.Store_Id && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Product already exist under same store and category" }
                            });
                        }
                    }
                    else
                    {
                        existingProduct = ctx.Products.FirstOrDefault(x => x.Id == model.Id);
                        if (existingProduct.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) == false || existingProduct.Store_Id != model.Store_Id || existingProduct.Store_Id != model.Store_Id)
                        {
                            if (ctx.Products.Any(x => x.IsDeleted == false && x.Store_Id == model.Store_Id && x.Store_Id == model.Store_Id && x.Name.Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Product with same name already exist under same store." }
                                });
                            }
                        }
                    }

                    HttpPostedFile postedFile = null;
                    string fileExtension = string.Empty;

                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
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
                                //int count = 1;
                                //fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                                //newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + postedFile.FileName);

                                //while (File.Exists(newFullPath))
                                //{
                                //    string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                //    newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + tempFileName + fileExtension);
                                //}
                                //postedFile.SaveAs(newFullPath);
                            }
                        }
                        //model.ImageUrl = ConfigurationManager.AppSettings["ProductImageFolderPath"] + Path.GetFileName(newFullPath);
                    }
                    #endregion
                    if (model.Id == 0)
                    {
                        if (!ctx.Admins.Any(x => x.Email == User.Identity.Name))
                        {
                            model.Category_Id = ctx.Stores.Where(x => x.MerchantEmail.Contains(User.Identity.Name)).FirstOrDefault().Category_Id;
                            model.Store_Id = ctx.Stores.Where(x => x.MerchantEmail.Contains(User.Identity.Name)).FirstOrDefault().Id;
                        }
                        //var stores = ctx.Stores.Where(x => x.MerchantEmail.Contains(User.Identity.Name)).FirstOrDefault().Category_Id;
                        model.CreatedDate = DateTime.UtcNow;
                        ctx.Products.Add(model);
                        ctx.SaveChanges();
                        var guid = Guid.NewGuid();
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        model.ImageUrl = ConfigurationManager.AppSettings["ProductImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        if (!ctx.Admins.Any(x => x.Email == User.Identity.Name))
                        {
                            Product _product = new Product();
                            _product = ctx.Products.Where(y => y.Id == model.Id).FirstOrDefault();
                            int prod_catid = -1;
                            int prod_store_id = -1;
                            prod_catid = Convert.ToInt32(_product.Category_Id);
                            prod_store_id = _product.Store_Id;
                            model.Store = ctx.Stores.Where(x => x.Id == prod_store_id).FirstOrDefault();
                            model.Store_Id = prod_store_id;
                            model.Category_Id = prod_catid;
                        }
                        //existingProduct = ctx.Products.FirstOrDefault(x => x.Id == model.Id);
                        if (httpRequest.Files.Count == 0)
                        {
                            // Check if image deleted
                            if (model.ImageDeletedOnEdit == false)
                            {
                                model.ImageUrl = existingProduct.ImageUrl;
                            }
                        }
                        else
                        {
                            Utility.DeleteFileIfExists(existingProduct.ImageUrl);
                            var guid = Guid.NewGuid();
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                            postedFile.SaveAs(newFullPath);
                            model.ImageUrl = ConfigurationManager.AppSettings["ProductImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        }

                        ctx.Entry(existingProduct).CurrentValues.SetValues(model);
                        ctx.SaveChanges();
                    }
                    CustomResponse<Product> response = new CustomResponse<Product>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = model
                    };

                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "Merchant")]
        /// <summary>
        /// Add store with image, multipart request
        /// </summary>
        /// <returns></returns>
        [Route("AddStore")]
        public async Task<IHttpActionResult> AddStoreWithImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                StoreBindingModel model = new StoreBindingModel();
                Store existingStore = new Store();

                if (httpRequest.Params["Id"] != null)
                {
                    model.Id = Convert.ToInt32(httpRequest.Params["Id"]);
                }

                if (httpRequest.Params["ImageDeletedOnEdit"] != null)
                {
                    model.ImageDeletedOnEdit = Convert.ToBoolean(httpRequest.Params["ImageDeletedOnEdit"]);
                }

                model.Name = httpRequest.Params["StoreName"];
                model.CategoryId = Convert.ToInt32(httpRequest.Params["CategoryId"]);
                model.BrandId = Convert.ToInt32(httpRequest.Params["BrandId"]);
                model.Latitude = Convert.ToDouble(httpRequest.Params["Lat"]);
                model.Longitude = Convert.ToDouble(httpRequest.Params["Long"]);
                model.Description = httpRequest.Params["Description"];
                model.Address = httpRequest.Params["Address"];
                
                TimeSpan openFrom, openTo;
                TimeSpan.TryParse(httpRequest.Params["Open_From"], out openFrom);
                TimeSpan.TryParse(httpRequest.Params["Open_To"], out openTo);

                if (openFrom != null)
                    model.Open_From = openFrom;

                if (openTo != null)
                    model.Open_To = openTo;

                if (httpRequest.Params["StoreDeliveryHours"] != null)
                {
                    var storeDeliveryHours = JsonConvert.DeserializeObject<StoreDeliveryHours>(httpRequest.Params["StoreDeliveryHours"]);

                    if (model.Id > 0)
                    {
                    }
                    else
                    {
                    }
                    model.StoreDeliveryHours = storeDeliveryHours;
                }


                Validate(model);

                #region Validations
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image" }
                    });
                }
                #endregion

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (model.Id == 0)
                    {
                        if (ctx.Stores.Any(x => x.Name == model.Name && x.Longitude == model.Longitude && x.Latitude == model.Latitude && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Store with same name and location already exists." }
                            });
                        }
                    }
                    else
                    {
                        existingStore = ctx.Stores.Include(x => x.StoreDeliveryHours).FirstOrDefault(x => x.Id == model.Id);
                        if (existingStore.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            if (ctx.Stores.Any(x => x.IsDeleted == false && x.Id == model.Id && x.Name.Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Store with same name already exist" }
                                });
                            }
                        }
                    }

                    HttpPostedFile postedFile = null;
                    string fileExtension = string.Empty;

                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            int MaxContentLength = 1024 * 1024 * 10; //Size = 10 MB  

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
                            else if (postedFile.ContentLength > MaxContentLength)
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload a file upto 1 mb" }
                                });
                            }
                            else
                            {
                                //    int count = 1;
                                //    fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                                //    newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["StoreImageFolderPath"] + postedFile.FileName);

                                //    while (File.Exists(newFullPath))
                                //    {
                                //        string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                //        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["StoreImageFolderPath"] + tempFileName + fileExtension);
                                //    }
                                //    postedFile.SaveAs(newFullPath);
                            }
                        }
                        //model.ImageUrl = ConfigurationManager.AppSettings["StoreImageFolderPath"] + Path.GetFileName(newFullPath);
                    }
                    #endregion


                    Store storeModel = new Store();
                    storeModel.Id = model.Id;
                    storeModel.Name = model.Name;
                    storeModel.Open_From = model.Open_From;
                    storeModel.Open_To = model.Open_To;
                    storeModel.Description = model.Description;
                    storeModel.Latitude = model.Latitude;
                    storeModel.Longitude = model.Longitude;
                    storeModel.ImageUrl = model.ImageUrl;
                    storeModel.StoreDeliveryHours = model.StoreDeliveryHours;
                    storeModel.ImageDeletedOnEdit = model.ImageDeletedOnEdit;
                    storeModel.Location = Utility.CreatePoint(model.Latitude, model.Longitude);
                    storeModel.StoreDeliveryHours.Id = storeModel.Id;
                    storeModel.Address = model.Address;
                    storeModel.Category_Id = model.CategoryId;
                    storeModel.Box_Id = model.BrandId;
                    if (!ctx.Admins.Any(x => x.Email == User.Identity.Name))
                    { var user = ctx.Users.Where(x => x.Email.Contains(User.Identity.Name)).FirstOrDefault();
                        if (user != null) 
                        {
                           var _box = ctx.Boxes.Where(x => x.Name==user.FullName).FirstOrDefault();
                            storeModel.MerchantEmail = User.Identity.Name;
                            storeModel.Category_Id = user.CategoryId;
                            storeModel.Box_Id = _box.Id;
                        }
                      
                    }
                    var brand = await ctx.Boxes.FirstOrDefaultAsync(x => x.Id == model.BrandId);

                    if (brand != null)
                    {
                        var merchant = await ctx.Users.FirstOrDefaultAsync(x => x.Id == brand.MerchantId);
                        if (merchant != null)
                        {
                            storeModel.MerchantEmail = merchant.Email;
                            storeModel.MerchantPhone = merchant.Phone;
                            storeModel.MerchantPin = merchant.MerchantPin;
                            storeModel.Category_Id = merchant.CategoryId;
                           
                        }
                    }



                    if (storeModel.Id == 0)
                    {
                        ctx.Stores.Add(storeModel);
                        ctx.SaveChanges();
                        var guid = Guid.NewGuid();
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["StoreImageFolderPath"] + storeModel.Id + "_" + guid + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        storeModel.ImageUrl = ConfigurationManager.AppSettings["StoreImageFolderPath"] + storeModel.Id + "_" + guid + fileExtension;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        if (httpRequest.Files.Count == 0)
                        {
                            // Check if image deleted
                            if (storeModel.ImageDeletedOnEdit == false)
                            {
                                storeModel.ImageUrl = existingStore.ImageUrl;
                            }
                        }
                        else
                        {
                            Utility.DeleteFileIfExists(existingStore.ImageUrl);
                        var guid = Guid.NewGuid();
                           /*     newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["StoreImageFolderPath"] + storeModel.Id + "_" + guid + fileExtension);
                            postedFile.SaveAs(newFullPath);
                            */
                            //var guid = Guid.NewGuid();
                            //var temp = "~/" + ConfigurationManager.AppSettings["StoreImageFolderPath"] + storeModel.Id + "_" + guid + fileExtension;
                            //newFullPath = HttpContext.Current.Server.MapPath(temp);
                            //postedFile.SaveAs(newFullPath);
                            storeModel.ImageUrl = ConfigurationManager.AppSettings["StoreImageFolderPath"] + storeModel.Id + "_" + guid + fileExtension;
                        }

                        ctx.Entry(existingStore).CurrentValues.SetValues(storeModel);

                        if (existingStore.StoreDeliveryHours == null)
                            ctx.StoreDeliveryHours.Add(storeModel.StoreDeliveryHours);
                        else
                            ctx.Entry(existingStore.StoreDeliveryHours).CurrentValues.SetValues(storeModel.StoreDeliveryHours);
                        ctx.SaveChanges();
                    }


                    CustomResponse<Store> response = new CustomResponse<Store>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = storeModel
                    };
                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        /// <summary>
        /// Get Dashboard Stats
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAdminDashboardStats")]
        public async Task<IHttpActionResult> GetAdminDashboardStats()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    DateTime TodayDate = DateTime.Now.Date;

                    WebDashboardStatsViewModel model = new WebDashboardStatsViewModel
                    {
                        TotalProducts = ctx.Products.Count(x => x.IsDeleted == false),
                        TotalStores = ctx.Stores.Count(x => x.IsDeleted == false),
                        TotalUsers = ctx.Users.Count(),
                        TodayOrders = ctx.Orders.Count(x => DbFunctions.TruncateTime(x.OrderDateTime) == TodayDate.Date),
                        DeviceUsage = ctx.Database.SqlQuery<DeviceStats>("select Count(Platform) as Count, (Count(Platform) * 100)/(select COUNT(Id) from UserDevices) as Percentage from UserDevices group by Platform order by Platform").ToList()
                    };

                    CustomResponse<WebDashboardStatsViewModel> response = new CustomResponse<WebDashboardStatsViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = model
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("SearchAdmins")]
        public async Task<IHttpActionResult> SearchAdmins(string FirstName, string LastName, string Email, string Phone, int? StoreId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    string conditions = string.Empty;

                    if (!String.IsNullOrEmpty(FirstName))
                        conditions += " And Admins.FirstName Like '%" + FirstName.Trim() + "%'";

                    if (!String.IsNullOrEmpty(LastName))
                        conditions += " And Admins.LastName Like '%" + LastName.Trim() + "%'";

                    if (!String.IsNullOrEmpty(Email))
                        conditions += " And Admins.Email Like '%" + Email.Trim() + "%'";

                    if (!String.IsNullOrEmpty(Phone))
                        conditions += " And Admins.Phone Like '%" + Phone.Trim() + "%'";

                    if (StoreId.HasValue && StoreId.Value != 0)
                        conditions += " And Admins.Store_Id = " + StoreId;

                    #region query
                    var query = @"SELECT
  Admins.Id,
  Admins.FirstName,
  Admins.LastName,
  Admins.Email,
  Admins.Phone,
  Admins.Role,
  Admins.ImageUrl,
  Stores.Name AS StoreName
FROM Admins
LEFT OUTER JOIN Stores
  ON Stores.Id = Admins.Store_Id
WHERE Admins.IsDeleted = 0
AND Stores.IsDeleted = 0 " + conditions + @" UNION
SELECT
  Admins.Id,
  Admins.FirstName,
  Admins.LastName,
  Admins.Email,
  Admins.Phone,
  Admins.Role,
  Admins.ImageUrl,
  '' AS StoreName
FROM Admins
WHERE Admins.IsDeleted = 0
AND ISNULL(Admins.Store_Id, 0) = 0 " + conditions;

                    #endregion


                    var admins = ctx.Database.SqlQuery<SearchAdminViewModel>(query).ToList();

                    return Ok(new CustomResponse<SearchAdminListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new SearchAdminListViewModel { Admins = admins } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "User", "Guest")]
        [HttpGet]
        [Route("SearchProducts")]
        public async Task<IHttpActionResult> SearchProducts(string ProductName, float? ProductPrice, string CategoryName, int? StoreId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var query = "select Products.*, Stores.Name as StoreName, Categories.Name as CategoryName from Products join Categories on Products.Category_Id = Categories.Id join Stores on Products.Store_Id = Stores.Id where Products.IsDeleted = 0 and Categories.IsDeleted = 0 and Stores.IsDeleted = 0";
                    if (!String.IsNullOrEmpty(CategoryName))
                        query += " And Categories.Name Like '%" + CategoryName + "%'";

                    if (!String.IsNullOrEmpty(ProductName))
                        query += " And Products.Name Like '%" + ProductName + "%'";

                    if (ProductPrice.HasValue)
                        query += " And Price = " + ProductPrice.Value;

                    if (StoreId.HasValue && StoreId.Value != 0)
                        query += " And Products.Store_Id = " + StoreId;

                    var products = ctx.Database.SqlQuery<SearchProductViewModel>(query).ToList();

                    foreach (var product in products)
                    {
                        product.Weight = product.WeightUnit == (int)WeightUnits.gm ? Convert.ToString(product.WeightInGrams) + " gm" : Convert.ToString(product.WeightInKiloGrams + " kg");
                        var avgRating = ctx.Database.SqlQuery<double?>("select Avg(Cast(Rating as float))  from ProductRatings where Product_Id = " + product.Id).ToList().FirstOrDefault();
                        if (avgRating != null)
                        {
                            product.AverageRating = avgRating.Value;
                        }

                    }

                    return Ok(new CustomResponse<SearchProductListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new SearchProductListViewModel { Products = products } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "User")]
        [HttpGet]
        [Route("SearchCategories")]
        public async Task<IHttpActionResult> SearchCategories(string CategoryName, int? StoreId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var query = @"select
Categories.Id,
Categories.Name,
Categories.Description,
Categories.Status,
Categories.ImageUrl,
Categories.ParentCategoryId,
Categories.IsDeleted
from Categories

where Categories.IsDeleted = 0";

                    //if (!String.IsNullOrEmpty(CategoryName))
                    //    query += " And Categories.Name Like '%" + CategoryName + "%'";

                    //if (StoreId.HasValue && StoreId.Value != 0)
                    //    query += " And Categories.Store_Id = " + StoreId;

                    //query += "group by Categories.Id,Categories.Name, Categories.Description, Categories.Status, Categories.ImageUrl, Categories.ParentCategoryId, Categories.IsDeleted, Stores.Name";

                    var categories = ctx.Database.SqlQuery<SearchCategoryViewModel>(query).ToList();
                    return Ok(new CustomResponse<SearchCategoryListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new SearchCategoryListViewModel { Categories = categories } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "User")]
        [HttpGet]
        [Route("SearchOffers")]
        public async Task<IHttpActionResult> SearchOffers(string OfferName, int? StoreId = null)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var query = "select Offers.*, Stores.Name as StoreName from Offers join Stores on Offers.Store_Id = Stores.Id where Offers.IsDeleted = 0 and Stores.IsDeleted = 0";


                    if (!String.IsNullOrEmpty(OfferName))
                        query += " And Offers.Name Like '%" + OfferName + "%'";

                    if (StoreId.HasValue && StoreId.Value != 0)
                        query += " And Offers.Store_Id = " + StoreId;

                    var offers = ctx.Database.SqlQuery<SearchOfferViewModel>(query).ToList();

                    if (offers != null)
                    {
                        foreach (var offer in offers)
                        {
                            offer.Store = ctx.Stores.FirstOrDefault(x => x.Id == offer.Store_Id);
                        }
                    }


                    return Ok(new CustomResponse<SearchOfferListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new SearchOfferListViewModel { Offers = offers } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "User")]
        [HttpGet]
        [Route("SearchPackages")]
        public async Task<IHttpActionResult> SearchPackages(string PackageName, int? StoreId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var query = "select Packages.*, Stores.Name as StoreName from Packages join Stores on Packages.Store_Id = Stores.Id where Packages.IsDeleted = 0 and Stores.IsDeleted = 0";

                    if (!String.IsNullOrEmpty(PackageName))
                        query += " And Packages.Name Like '%" + PackageName + "%'";

                    if (StoreId.HasValue && StoreId.Value != 0)
                        query += " And Packages.Store_Id = " + StoreId;

                    var packages = ctx.Database.SqlQuery<SearchPackageViewModel>(query).ToList();
                    return Ok(new CustomResponse<SearchPackageListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new SearchPackageListViewModel { Packages = packages } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("DeleteEntity")]
        //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        public async Task<IHttpActionResult> DeleteEntity(int EntityType, int Id)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    switch (EntityType)
                    {
                        case (int)BasketEntityTypes.City:
                            ctx.Cities.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.Product:
                            ctx.Products.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.Category:
                            ctx.Categories.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            ctx.Database.ExecuteSqlCommand("update products set isdeleted = 1 where category_id = " + Id);
                            break;
                        case (int)BasketEntityTypes.Store:
                            ctx.Stores.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            ctx.Database.ExecuteSqlCommand("update products set isdeleted = 1 where store_id = " + Id + @"; 
                            update packages set isdeleted = 1 where store_id = " + Id + @";  
                            update offers set isdeleted = 1 where store_id = " + Id);
                            break;
                        case (int)BasketEntityTypes.Package:
                            ctx.Packages.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.Admin:
                            ctx.Admins.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.Offer:
                            ctx.Offers.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.Box:
                            var box = ctx.Boxes.FirstOrDefault(x => x.Id == Id);
                            box.IsDeleted = true;
                            ctx.Database.ExecuteSqlCommand("update BoxVideos set isdeleted = 1 where Box_Id = " + Id + @"; 
                            update UserSubscriptions set isdeleted = 1 where Box_Id = " + Id);

                            var user = ctx.Users.Find(box.MerchantId);
                            if (user != null)
                            {
                                user.IsDeleted = true;
                            }
                            break;
                        default:
                            break;
                    }
                    ctx.SaveChanges();
                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("GetReadyForDeliveryOrders")]
        public async Task<IHttpActionResult> GetReadyForDeliveryOrders()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    #region query
                    var query = @"
select 
Orders.Id,
Orders.OrderDateTime as CreatedOn,
Orders.Total as OrderTotal,
Orders.DeliveryMan_Id as DeliveryManId,
Case When Orders.PaymentMethod = 0 Then 'Pending' Else 'Paid' End As PaymentStatus,
Stores.Name as StoreName,
Stores.Id as StoreId,
Stores.Location as StoreLocation,
Users.FullName as CustomerName
from Orders
join Users on Users.ID = Orders.User_ID
join StoreOrders on StoreOrders.Order_Id = Orders.Id
join Stores on Stores.Id = StoreOrders.Store_Id
where 
Orders.IsDeleted = 0
and Orders.Status = " + (int)OrderStatuses.ReadyForDelivery;
                    #endregion

                    SearchOrdersListViewModel responseModel = new SearchOrdersListViewModel { Orders = ctx.Database.SqlQuery<SearchOrdersViewModel>(query).ToList() };

                    foreach (var order in responseModel.Orders)
                    {
                        var deliveryMen = ctx.DeliveryMen.Where(x => x.Location.Distance(order.StoreLocation) < Global.NearbyStoreRadius).ToList();

                        foreach (var deliverer in deliveryMen)
                        {
                            order.DeliveryMen.Add(new DelivererOptionsViewModel { Id = deliverer.Id, Name = deliverer.FullName });
                        }
                    }

                    //If a deliverer is in radius any of store in order. That deliverer will be selected.

                    var duplicateOrders = responseModel.Orders.GroupBy(x => x.Id).Where(g => g.Count() > 1).Select(y => y.Key);

                    var DuplicateDeliveryMenUnion = responseModel.Orders.Where(x => duplicateOrders.Contains(x.Id)).SelectMany(x1 => x1.DeliveryMen).Distinct(new DelivererOptionsViewModel.Comparer()).ToList();

                    foreach (var order in responseModel.Orders.Where(x => duplicateOrders.Contains(x.Id)))
                    {
                        order.DeliveryMen = DuplicateDeliveryMenUnion;
                    }

                    return Ok(new CustomResponse<SearchOrdersListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = responseModel });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("AssignOrdersToDeliverer")]
        public async Task<IHttpActionResult> AssignOrdersToDeliverer(SearchOrdersListViewModel model)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    foreach (var order in model.Orders)
                    {
                        var existingOrder = ctx.Orders.Include(x => x.StoreOrders).FirstOrDefault(x => x.Id == order.Id);
                        if (existingOrder != null)
                        {
                            foreach (var storeOrder in existingOrder.StoreOrders)
                            {
                                storeOrder.Status = (int)OrderStatuses.AssignedToDeliverer;
                            }
                            existingOrder.DeliveryMan_Id = order.DeliveryManId;
                            existingOrder.Status = (int)OrderStatuses.AssignedToDeliverer;
                        }
                    }
                    ctx.SaveChanges();
                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("SearchOrders")]
        public async Task<IHttpActionResult> SearchOrders(string StartDate, string EndDate, int? OrderStatusId, int? PaymentMethodId, int? PaymentStatusId, int? StoreId)
        {
            try
            {
                DateTime startDateTime;
                DateTime endDateTime;
                startDateTime = DateTime.ParseExact(StartDate, "d/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                endDateTime = DateTime.ParseExact(EndDate, "d/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                #region query
                var query = @"
select 
Orders.Id,
StoreOrders.Id as StoreOrder_Id,
Orders.OrderDateTime as CreatedOn,
Orders.Total as OrderTotal,
StoreOrders.Status as OrderStatus,
Orders.DeliveryMan_Id as DeliveryManId,
Case When Orders.PaymentMethod = 0 Then 'Pending' Else 'Paid' End As PaymentStatus,
Stores.Name as StoreName,
Stores.Id as StoreId,
Stores.Location as StoreLocation,
Users.FullName as CustomerName
from Orders
join Users on Users.ID = Orders.User_ID
join StoreOrders on StoreOrders.Order_Id = Orders.Id
join Stores on Stores.Id = StoreOrders.Store_Id
where 
Orders.IsDeleted = 0
and 
 CAST(orders.OrderDateTime AS DATE) >= '" + startDateTime.Date + "' and CAST(orders.OrderDateTime as DATE) <= '" + endDateTime.Date + "'";
                #endregion

                if (OrderStatusId.HasValue)
                    query += " and orders.Status = " + OrderStatusId.Value;

                if (PaymentMethodId.HasValue)
                    query += " and orders.PaymentMethod = " + PaymentMethodId.Value;

                if (PaymentStatusId.HasValue)
                    query += " and orders.PaymentStatus = " + PaymentStatusId.Value;

                if (StoreId.HasValue)
                    query += " and Stores.Id = " + StoreId.Value;

                SearchOrdersListViewModel returnModel = new SearchOrdersListViewModel();

                using (SkriblContext ctx = new SkriblContext())
                {
                    returnModel.Orders = ctx.Database.SqlQuery<SearchOrdersViewModel>(query).ToList();

                    foreach (var order in returnModel.Orders)
                    {
                        order.OrderStatusName = Utility.GetOrderStatusName(order.OrderStatus);
                    }
                    return Ok(new CustomResponse<SearchOrdersListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = returnModel });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("ChangeOrderStatus")]
        public async Task<IHttpActionResult> ChangeOrderStatus(ChangeOrderStatusListBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (SkriblContext ctx = new SkriblContext())
                {
                    //Mark Statuses for StoreOrders
                    foreach (var order in model.Orders)
                    {
                        var existingStoreOrder = ctx.StoreOrders.Include(x => x.Order.User.UserDevices).FirstOrDefault(x => x.Id == order.StoreOrder_Id);
                        if (existingStoreOrder != null)
                        {
                            if (order.Status > existingStoreOrder.Status)
                            {
                                PushNotificationType pushType = PushNotificationType.Announcement;
                                Notification Notification = new Notification();
                                if (order.Status == (int)OrderStatuses.Accepted)
                                {
                                    Notification.Title = "Order Accepted";
                                    Notification.Text = "Your order# " + existingStoreOrder.Order.Id + " has been accepted by store.";
                                    pushType = PushNotificationType.OrderAccepted;
                                    existingStoreOrder.Order.User.Notifications.Add(Notification);
                                }
                                else if (order.Status == (int)OrderStatuses.AssignedToDeliverer)
                                {
                                    Notification.Title = "Order Assigned To Delivery Boy";
                                    Notification.Text = "Your order#" + existingStoreOrder.Order.Id + " has been assigned to delivery boy.";
                                    pushType = PushNotificationType.OrderAssignedToDeliverer;
                                    existingStoreOrder.Order.User.Notifications.Add(Notification);
                                }
                                else if (order.Status == (int)OrderStatuses.Dispatched)
                                {
                                    Notification.Title = "Order Dispatched";
                                    Notification.Text = "Your order#" + existingStoreOrder.Order.Id + " has been dispatched.";
                                    pushType = PushNotificationType.OrderDispatched;
                                    existingStoreOrder.Order.User.Notifications.Add(Notification);
                                }
                                else if (order.Status == (int)OrderStatuses.Completed)
                                {
                                    Notification.Title = "Order Completed";
                                    Notification.Text = "Your order#" + existingStoreOrder.Order.Id + " has been marked as completed.";
                                    pushType = PushNotificationType.OrderCompleted;
                                    existingStoreOrder.Order.User.Notifications.Add(Notification);
                                }
                                else if (order.Status == (int)OrderStatuses.Rejected)
                                {
                                    Notification.Title = "Order Rejected";
                                    Notification.Text = "Your order#" + existingStoreOrder.Order.Id + " has been rejected by store, we are very sorry for inconvenience.";
                                    pushType = PushNotificationType.OrderRejected;
                                    existingStoreOrder.Order.User.Notifications.Add(Notification);
                                }
                                existingStoreOrder.Status = order.Status;
                                ctx.SaveChanges();
                                if (existingStoreOrder.Order.User.IsNotificationsOn)
                                {
                                    var usersToPushAndroid = existingStoreOrder.Order.User.UserDevices.Where(x => x.Platform == true).ToList();
                                    var usersToPushIOS = existingStoreOrder.Order.User.UserDevices.Where(x => x.Platform == false).ToList();
                                    Utility.SendPushNotifications(usersToPushAndroid, usersToPushIOS, Notification, (int)pushType);
                                }
                            }
                        }
                    }
                    //Mark Statuses for Orders
                    foreach (var order in model.Orders)
                    {
                        var existingOrder = ctx.Orders.Include(x => x.StoreOrders).FirstOrDefault(x => x.Id == order.OrderId);
                        existingOrder.Status = existingOrder.StoreOrders.Min(x => x.Status);
                    }

                    ctx.SaveChanges();
                }
                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [Route("AddPackage")]
        public async Task<IHttpActionResult> AddPackage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                Package model = new Package();
                Package existingPackage = new Package();

                if (httpRequest.Params["Id"] != null)
                {
                    model.Id = Convert.ToInt32(httpRequest.Params["Id"]);
                }

                if (httpRequest.Params["ImageDeletedOnEdit"] != null)
                {
                    model.ImageDeletedOnEdit = Convert.ToBoolean(httpRequest.Params["ImageDeletedOnEdit"]);
                }
                model.Name = httpRequest.Params["Name"];
                model.Price = Convert.ToDouble(httpRequest.Params["Price"]);
                model.Description = httpRequest.Params["Description"];
                model.Store_Id = Convert.ToInt32(httpRequest.Params["Store_Id"]);
                model.Status = 0;
                if (httpRequest.Params["package_products"] != null)
                {
                    var packageProducts = JsonConvert.DeserializeObject<List<Package_Products>>(httpRequest.Params["package_products"]);

                    if (model.Id > 0)
                    {
                        foreach (var item in packageProducts)
                        {
                            //item.Product_Id = item.Id;
                            item.Id = item.PackageProductId;

                        }
                    }
                    else
                    {
                        foreach (var item in packageProducts)
                            item.Product_Id = item.Id;
                    }
                    model.Package_Products = packageProducts;
                }

                Validate(model);

                #region Validations

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image" }
                    });
                }
                #endregion

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (model.Id == 0)
                    {
                        if (ctx.Packages.Any(x => x.Store_Id == model.Store_Id && x.Name == model.Name && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Package already exist under same store" }
                            });
                        }
                    }
                    else
                    {
                        existingPackage = ctx.Packages.Include(x => x.Package_Products).FirstOrDefault(x => x.Id == model.Id);
                        if (existingPackage.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) == false || existingPackage.Store_Id != model.Store_Id)
                        {
                            if (ctx.Packages.Any(x => x.IsDeleted == false && x.Store_Id == model.Store_Id && x.Name.Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Package with same name already exist under same store" }
                                });
                            }
                        }
                    }

                    HttpPostedFile postedFile = null;
                    string fileExtension = string.Empty;

                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
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
                                //int count = 1;
                                //fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                                //newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + postedFile.FileName);

                                //while (File.Exists(newFullPath))
                                //{
                                //    string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                //    newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + tempFileName + fileExtension);
                                //}
                                //postedFile.SaveAs(newFullPath);
                            }
                        }
                        //model.ImageUrl = ConfigurationManager.AppSettings["ProductImageFolderPath"] + Path.GetFileName(newFullPath);
                    }
                    #endregion

                    if (model.Id == 0)
                    {
                        ctx.Packages.Add(model);
                        ctx.SaveChanges();
                        var guid = Guid.NewGuid();
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["PackageImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        model.ImageUrl = ConfigurationManager.AppSettings["PackageImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        //existingProduct = ctx.Products.FirstOrDefault(x => x.Id == model.Id);
                        if (httpRequest.Files.Count == 0)
                        {
                            // Check if image deleted
                            if (model.ImageDeletedOnEdit == false)
                            {
                                model.ImageUrl = existingPackage.ImageUrl;
                            }
                        }
                        else
                        {
                            Utility.DeleteFileIfExists(existingPackage.ImageUrl);
                            var guid = Guid.NewGuid();
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["PackageImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                            postedFile.SaveAs(newFullPath);
                            model.ImageUrl = ConfigurationManager.AppSettings["PackageImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        }

                        ctx.Entry(existingPackage).CurrentValues.SetValues(model);

                        foreach (var oldPP in existingPackage.Package_Products.ToList())
                        {
                            ctx.Package_Products.Remove(oldPP);
                        }

                        foreach (var packageProduct in model.Package_Products)
                        {
                            packageProduct.Package_Id = existingPackage.Id;
                            existingPackage.Package_Products.Add(packageProduct);

                            #region commented

                            //var originalPackageProduct = existingPackage.Package_Products.Where(c => c.Id == packageProduct.Id).SingleOrDefault();

                            //if (originalPackageProduct != null)
                            //{
                            //    // Yes -> Update scalar properties of child item
                            //    packageProduct.Package_Id = originalPackageProduct.Package_Id;
                            //    ctx.Entry(originalPackageProduct).CurrentValues.SetValues(packageProduct);
                            //}
                            //else
                            //{
                            //    // No -> It's a new child item -> Insert
                            //    packageProduct.Package_Id = existingPackage.Id;
                            //    existingPackage.Package_Products.Add(packageProduct);
                            //} 
                            #endregion
                        }
                        ctx.SaveChanges();
                    }

                    CustomResponse<Package> response = new CustomResponse<Package>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = model
                    };

                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin","Merchant")]
        [Route("AddOffer")]
        public async Task<IHttpActionResult> AddOffer()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                Offer model = new Offer();
                Offer existingOffer = new Offer();

                if (httpRequest.Params["Id"] != null)
                {
                    model.Id = Convert.ToInt32(httpRequest.Params["Id"]);
                }

                if (httpRequest.Params["ImageDeletedOnEdit"] != null)
                {
                    model.ImageDeletedOnEdit = Convert.ToBoolean(httpRequest.Params["ImageDeletedOnEdit"]);
                }
                model.Name = httpRequest.Params["Name"];
                model.ValidFrom = DateTime.ParseExact(httpRequest.Params["ValidFrom"], "dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                model.ValidUpto = DateTime.ParseExact(httpRequest.Params["ValidTo"], "dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                //model.Price = Convert.ToDouble(httpRequest.Params["Price"]);
                model.Description = httpRequest.Params["Description"];
                model.Store_Id = Convert.ToInt32(httpRequest.Params["Store_Id"]);
                model.Status = 0;
                if (httpRequest.Params["offer_products"] != null)
                {
                    var offerProducts = JsonConvert.DeserializeObject<List<Offer_Products>>(httpRequest.Params["offer_products"]);

                    if (model.Id > 0)
                    {
                        foreach (var item in offerProducts)
                        {
                            //item.Product_Id = item.Id;
                            item.Id = item.OfferProductId;

                        }
                    }
                    else
                    {
                        foreach (var item in offerProducts)
                            item.Product_Id = item.Id;
                    }
                    model.Offer_Products = offerProducts;
                }

                if (httpRequest.Params["offer_packages"] != null)
                {
                    var offerPackages = JsonConvert.DeserializeObject<List<Offer_Packages>>(httpRequest.Params["offer_packages"]);

                    if (model.Id > 0)
                    {
                        foreach (var item in offerPackages)
                        {
                            //item.Product_Id = item.Id;
                            item.Id = item.OfferPackageId;

                        }
                    }
                    else
                    {
                        foreach (var item in offerPackages)
                            item.Package_Id = item.Id;
                    }
                    model.Offer_Packages = offerPackages;
                }
                Validate(model);

                #region Validations

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image" }
                    });
                }
                #endregion

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (model.Id == 0)
                    {
                        if (ctx.Offers.Any(x => x.Store_Id == model.Store_Id && x.Name == model.Name && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Offer with same name already exist under same store" }
                            });
                        }
                    }
                    else
                    {
                        existingOffer = ctx.Offers.Include(x => x.Offer_Products).Include(x => x.Offer_Packages).FirstOrDefault(x => x.Id == model.Id);
                        if (existingOffer.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) == false || existingOffer.Store_Id != model.Store_Id)
                        {
                            if (ctx.Offers.Any(x => x.IsDeleted == false && x.Store_Id == model.Store_Id && x.Name.Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Offer with same name already exist under same store" }
                                });
                            }
                        }
                    }

                    HttpPostedFile postedFile = null;
                    string fileExtension = string.Empty;

                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
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
                                //int count = 1;
                                //fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                                //newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + postedFile.FileName);

                                //while (File.Exists(newFullPath))
                                //{
                                //    string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                //    newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + tempFileName + fileExtension);
                                //}
                                //postedFile.SaveAs(newFullPath);
                            }
                            //model.ImageUrl = ConfigurationManager.AppSettings["ProductImageFolderPath"] + Path.GetFileName(newFullPath);
                        }
                    }
                    #endregion

                    if (model.Id == 0)
                    {
                        ctx.Offers.Add(model);
                        ctx.SaveChanges();
                        var guid = Guid.NewGuid();
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["OfferImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        model.ImageUrl = ConfigurationManager.AppSettings["OfferImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        ctx.SaveChanges();
                    }
                    else
                    {
                        //existingProduct = ctx.Products.FirstOrDefault(x => x.Id == model.Id);
                        if (httpRequest.Files.Count == 0)
                        {
                            // Check if image deleted
                            if (model.ImageDeletedOnEdit == false)
                            {
                                model.ImageUrl = existingOffer.ImageUrl;
                            }
                        }
                        else
                        {
                            Utility.DeleteFileIfExists(existingOffer.ImageUrl);
                            var guid = Guid.NewGuid();
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["OfferImageFolderPath"] + model.Id + "_" + guid + fileExtension);
                            postedFile.SaveAs(newFullPath);
                            model.ImageUrl = ConfigurationManager.AppSettings["OfferImageFolderPath"] + model.Id + "_" + guid + fileExtension;
                        }

                        ctx.Entry(existingOffer).CurrentValues.SetValues(model);

                        //Delete and Insert OfferProducts

                        foreach (var oldOP in existingOffer.Offer_Products)
                        {
                            oldOP.IsDeleted = true;
                        }

                        foreach (var newOP in model.Offer_Products)
                        {
                            var oldOP = ctx.Offer_Products.FirstOrDefault(x => x.Id == newOP.Id);
                            if (oldOP == null)
                            {
                                newOP.Offer_Id = existingOffer.Id;
                                existingOffer.Offer_Products.Add(newOP);
                            }
                            else
                            {
                                newOP.Offer_Id = existingOffer.Id;
                                ctx.Entry(ctx.Offer_Products.FirstOrDefault(x => x.Id == oldOP.Id)).CurrentValues.SetValues(newOP);
                            }
                        }

                        //Delete and Insert OfferPackages
                        foreach (var oldOP in existingOffer.Offer_Packages)
                        {
                            oldOP.IsDeleted = true;
                        }

                        foreach (var newOP in model.Offer_Packages)
                        {
                            var oldOP = ctx.Offer_Packages.FirstOrDefault(x => x.Id == newOP.Id);
                            if (oldOP == null)
                            {
                                newOP.Offer_Id = existingOffer.Id;
                                existingOffer.Offer_Packages.Add(newOP);
                            }
                            else
                            {
                                newOP.Offer_Id = existingOffer.Id;
                                ctx.Entry(ctx.Offer_Packages.FirstOrDefault(x => x.Id == oldOP.Id)).CurrentValues.SetValues(newOP);
                            }
                        }
                        ctx.SaveChanges();
                    }



                    CustomResponse<Offer> response = new CustomResponse<Offer>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = model
                    };

                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin", "User", "Guest")]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(AdminSetPasswordBindingModel model)
        {
            try
            {
                var userEmail = User.Identity.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    throw new Exception("User Email is empty in user.identity.name.");
                }
                else if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (ctx.Admins.Any(x => x.Email == User.Identity.Name))
                    {


                        var user = ctx.Admins.FirstOrDefault(x => x.Email == userEmail && x.Password == model.OldPassword);
                        if (user != null)
                        {
                            user.Password = model.NewPassword;
                            ctx.SaveChanges();
                            return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                        }
                        else
                            return Ok(new CustomResponse<Error> { Message = "Forbidden", StatusCode = (int)HttpStatusCode.Forbidden, Result = new Error { ErrorMessage = "Invalid old password." } });
                    }
                    else
                    {
                        var user = ctx.Users.FirstOrDefault(x => x.Email == userEmail && x.Password == model.OldPassword);
                        if (user != null)
                        {
                            user.Password = model.NewPassword;
                            ctx.SaveChanges();
                            return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                        }
                        else
                            return Ok(new CustomResponse<Error> { Message = "Forbidden", StatusCode = (int)HttpStatusCode.Forbidden, Result = new Error { ErrorMessage = "Invalid old password." } });

                    }

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [Route("AddBox")]
        public async Task<IHttpActionResult> AddBox(AddBoxBindingModel model)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    if (model.Id > 0)
                    {
                        var existingBox = ctx.Boxes.FirstOrDefault(x => x.Id == model.Id);

                        ctx.Entry(existingBox).CurrentValues.SetValues(model);
                        await ctx.SaveChangesAsync();
                    }
                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [Route("AddMerchant")]
        public async Task<IHttpActionResult> AddMerchantWithImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                RegisterMerchantBindingModel model = new RegisterMerchantBindingModel();

                var title = httpRequest.Params["Title"];
                var description = httpRequest.Params["Description"];

                model.FirstName = httpRequest.Params["FirstName"];
                model.LastName = httpRequest.Params["LastName"];
                model.Email = httpRequest.Params["Email"];
                model.ConfirmPassword = httpRequest.Params["ConfirmPassword"];
                model.Password = httpRequest.Params["Password"];

                var tmpId = Convert.ToInt32(httpRequest.Params["CategoryId"]);
                if (tmpId == 0)
                {
                    model.CategoryId = null;
                }
                else
                {
                    model.CategoryId = Convert.ToInt32(httpRequest.Params["CategoryId"]);
                }

                model.Phone = httpRequest.Params["Phone"];
                model.Country = httpRequest.Params["Country"];
                model.City = httpRequest.Params["City"];
                model.Area = httpRequest.Params["Area"];
                var password = Membership.GeneratePassword(8, 1);
                model.Password = password;
                model.ConfirmPassword = password;

                Validate(model);

                #region Validations
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request." }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image." }
                    });
                }
                #endregion

                using (SkriblContext ctx = new SkriblContext())
                {
                    if (ctx.Users.Any(x => x.Email == model.Email))
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "Conflict",
                            StatusCode = (int)HttpStatusCode.Conflict,
                            Result = new Error { ErrorMessage = "User with email already exists." }
                        });
                    }


                    HttpPostedFile postedFile = null;
                    string fileExtension = string.Empty;

                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            //int MaxContentLength = 1024 * 1024 * 10; //Size = 1 MB  

                            IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                            //var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                            var ext = Path.GetExtension(postedFile.FileName);
                            fileExtension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(fileExtension))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload image of type .jpg,.gif,.png." }
                                });
                            }
                            else if (postedFile.ContentLength > Global.MaximumImageSize)
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload a file upto " + Global.ImageSize + "." }
                                });
                            }
                            else
                            {
                            }
                        }
                    }

                    #endregion

                    var Pin = Utility.GenerateMerchantPin();

                    var IsPinUsed = true;

                    while (IsPinUsed)
                    {
                        var findPin = ctx.Users.FirstOrDefault(x => x.MerchantPin == Pin);
                        if (findPin == null)
                        {
                            IsPinUsed = false;
                        }
                        else
                        {
                            Pin = Utility.GenerateMerchantPin();
                        }
                    }

                    User userModel;

                    userModel = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        FullName = model.FirstName + " " + model.LastName,
                        UserName = model.FirstName + " " + model.LastName,
                        Email = model.Email,
                        Password = model.Password,
                        CategoryId = (int)model.CategoryId,
                        Phone = model.Phone,
                        Country = model.Country,
                        City = model.City,
                        Area = model.Area,
                        Status = (int)Global.StatusCode.NotVerified,
                        AccountType = "5",
                        SignInType = 5,
                        IsNotificationsOn = true,
                        IsDeleted = false,
                        MerchantPin = Pin
                    };


                    ctx.Users.Add(userModel);
                    ctx.SaveChanges();

                    var box = new Box
                    {
                        Category_Id = (int)model.CategoryId,
                        Status = 1,
                        Name = title,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                        Description = description,
                        MerchantId = userModel.Id
                    };

                    ctx.Boxes.Add(box);
                    ctx.SaveChanges();

                    if (httpRequest.Files.Count > 0)
                    {
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["UserImageFolderPath"] + userModel.Id + fileExtension);
                        postedFile.SaveAs(newFullPath);
                        userModel.ProfilePictureUrl = ConfigurationManager.AppSettings["UserImageFolderPath"] + userModel.Id + fileExtension;
                        ctx.SaveChanges();
                    }

                    await userModel.GenerateToken(Request);
                    CustomResponse<User> response = new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = userModel };
                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        //[Route("AddBox")]
        //public async Task<IHttpActionResult> AddBox(AddBoxBindingModel model)
        //{
        //    try
        //    {
        //        using (SkriblContext ctx = new SkriblContext())
        //        {
        //            if (model.Id > 0)
        //            {
        //                var existingBox = ctx.Boxes.FirstOrDefault(x => x.Id == model.Id);

        //                ctx.Entry(existingBox).CurrentValues.SetValues(model);
        //                ctx.SaveChanges();
        //                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
        //            }
        //            else
        //            {
        //                if (ctx.Categories.Any(x => x.Name == model.Name && !x.IsDeleted))
        //                    return Ok(new CustomResponse<Error> { Message = "Forbidden", StatusCode = (int)HttpStatusCode.Forbidden, Result = new Error { ErrorMessage = "Category with this name already exists." } });
        //                else
        //                {
        //                    ctx.Boxes.Add(new Box
        //                    {
        //                        Category_Id = model.Category_Id,
        //                        Status = 1,
        //                        Name = model.Name,
        //                        IsDeleted = false,
        //                        CreatedDate = DateTime.Now,
        //                        Description = model.Description,
        //                    });
        //                    ctx.SaveChanges();

        //                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [Route("GetAllBoxes")]
        public async Task<IHttpActionResult> GetAllBoxes()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<SearchBoxesViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new SearchBoxesViewModel { Boxes = ctx.Boxes.Include(x => x.Category).Where(x => x.IsDeleted == false).ToList() }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [Route("AddNotification")]
        public async Task<IHttpActionResult> AddNotification(NotificationBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (SkriblContext ctx = new SkriblContext())
                {
                    AdminNotifications adminNotification = new AdminNotifications { CreatedDate = DateTime.Now, Title = model.Title, TargetAudienceType = model.TargetAudience, Description = model.Description };

                    ctx.AdminNotifications.Add(adminNotification);
                    ctx.SaveChanges();
                    if (model.TargetAudience == (int)NotificationTargetAudienceTypes.User || model.TargetAudience == (int)NotificationTargetAudienceTypes.UserAndDeliverer)
                    {
                        var users = ctx.Users.Where(x => x.IsDeleted == false).Include(x => x.UserDevices).Where(x => x.IsDeleted == false);

                        await users.ForEachAsync(a => a.Notifications.Add(new Notification { Title = model.Title, Text = model.Description, Status = 0, AdminNotification_Id = adminNotification.Id }));

                        await ctx.SaveChangesAsync();

                        var usersToPushAndroid = users.Where(x => x.IsNotificationsOn).SelectMany(x => x.UserDevices.Where(x1 => x1.Platform == true)).ToList();
                        var usersToPushIOS = users.Where(x => x.IsNotificationsOn).SelectMany(x => x.UserDevices.Where(x1 => x1.Platform == false)).ToList();

                        Utility.LogErrorString(usersToPushIOS.Count.ToString());

                        HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                        {
                            Global.objPushNotifications.SendAndroidPushNotification(usersToPushAndroid, adminNotification);
                            Global.objPushNotifications.SendIOSPushNotification(usersToPushIOS, adminNotification);

                        });

                    }

                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("SearchNotifications")]
        public async Task<IHttpActionResult> SearchNotifications()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<SearchAdminNotificationsViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new SearchAdminNotificationsViewModel
                        {
                            Notifications = ctx.AdminNotifications.ToList()
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("ChangeAdminStatuses")]
        public async Task<IHttpActionResult> ChangeAdminStatuses(ChangeUserStatusListBindingModel model)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    foreach (var user in model.Users)
                    {
                        var tmpUser = ctx.Admins.FirstOrDefault(x => x.Id == user.UserId && x.Role == 2);
                        tmpUser.IsDeleted = user.Status;
                        ctx.Entry(tmpUser).State = EntityState.Modified;
                    }

                    ctx.SaveChanges();
                }

                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("ChangeUserStatuses")]
        public async Task<IHttpActionResult> ChangeUserStatuses(ChangeUserStatusListBindingModel model)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    foreach (var user in model.Users)
                    {
                        var tmpUser = ctx.Users.FirstOrDefault(x => x.Id == user.UserId);
                        tmpUser.IsDeleted = user.Status;
                        ctx.Entry(tmpUser).State = EntityState.Modified;
                    }

                    ctx.SaveChanges();
                }

                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("ChangeBoxStatuses")]
        public async Task<IHttpActionResult> ChangeBoxStatuses(ChangeBoxStatusListBindingModel model)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    foreach (var box in model.Boxes)
                        ctx.Boxes.FirstOrDefault(x => x.Id == box.BoxId).Status = box.Status;
                    ctx.SaveChanges();
                }

                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("GetUsers")]
        public async Task<IHttpActionResult> GetUsers()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<SearchUsersViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new SearchUsersViewModel
                        {
                            Users = ctx.Users.ToList()
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        // [BasketApi.Authorize("SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("GetAgents")]
        public async Task<IHttpActionResult> GetAgents()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<AdminViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new AdminViewModel
                        {
                            Agents = ctx.Admins.Where(x => x.Role == 2).Select(x => new AgentViewModel
                            {
                                Id = x.Id,
                                FirstName = x.FirstName,
                                LastName = x.LastName,
                                Email = x.Email,
                                Phone = x.Phone,
                                Role = x.Role,
                                ImageUrl = x.ImageUrl,
                                IsDeleted = x.IsDeleted
                            }).ToList()
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("GetMerchants")]
        public async Task<IHttpActionResult> GetMerchants()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<SearchUsersViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new SearchUsersViewModel
                        {
                            Users = ctx.Users.Where(x => x.AccountType == "5").ToList()
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]

        [HttpGet]
        [Route("GetUser")]
        public async Task<IHttpActionResult> GetUser(int UserId, int SignInType)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    BasketSettings.LoadSettings();

                    if (SignInType == (int)RoleTypes.User)
                    {
                        var user = ctx.Users.Include(x => x.Orders).Include(x => x.UserSubscriptions.Select(y => y.Box)).Include(x => x.Favourites.Select(y => y.Product)).Include(x => x.Feedback.Select(z => z.Store)).Include(x => x.UserAddresses).Include(x => x.PaymentCards).FirstOrDefault(x => x.Id == UserId);
                        return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = user });
                    }
                    else
                    {
                        var Deliverer = ctx.DeliveryMen.Include(x => x.DelivererAddresses).FirstOrDefault(x => x.Id == UserId);
                        Deliverer.SignInType = (int)RoleTypes.Deliverer;
                        return Ok(new CustomResponse<DeliveryMan> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = Deliverer });
                    }

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetEntityById")]
        public async Task<IHttpActionResult> GetEntityById(int EntityType, int Id)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    switch (EntityType)
                    {
                        case (int)BasketEntityTypes.Product:
                            var product = ctx.Products.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false);
                            product.Store = ctx.Stores.FirstOrDefault(x => x.Id == product.Store_Id && x.IsDeleted == false);
                            return Ok(new CustomResponse<Product> {
                                Message = Global.ResponseMessages.Success,
                                StatusCode = (int)HttpStatusCode.OK,
                                Result = product
                            });

                        case (int)BasketEntityTypes.Category:
                            return Ok(new CustomResponse<Category> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Categories.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });

                        case (int)BasketEntityTypes.Store:
                            return Ok(new CustomResponse<Store> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Stores.Include(x => x.StoreDeliveryHours).FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });

                        case (int)BasketEntityTypes.Package:
                            return Ok(new CustomResponse<Package> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Packages.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });

                        case (int)BasketEntityTypes.Admin:
                            return Ok(new CustomResponse<DAL.Admin> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Admins.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });

                        case (int)BasketEntityTypes.Offer:
                            return Ok(new CustomResponse<DAL.Offer> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Offers.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });
                        case (int)BasketEntityTypes.Box:
                            return Ok(new CustomResponse<DAL.Box> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Boxes.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });
                        case (int)BasketEntityTypes.City:
                            return Ok(new CustomResponse<DAL.Cities> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Cities.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });

                        default:
                            return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "Invalid entity type" } });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
        [HttpGet]
        [Route("GetStoreByCategoryIdForAdmin")]
        public async Task<IHttpActionResult> GetStoreByCategoryIdForAdmin(int Category_Id)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {

                    var Stores = ctx.Stores.Where(x => x.Category_Id == Category_Id && !x.IsDeleted).ToList();

                    return Ok(new CustomResponse<List<Store>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = Stores });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
       

        [HttpGet]
        [Route("GetStoreByMerchantIdForAdmin")]
        public async Task<IHttpActionResult> GetStoreByMerchantIdForAdmin(int Box_Id)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {

                    var Stores = ctx.Stores.Where(x => x.Box_Id == Box_Id && !x.IsDeleted).ToList();

                    return Ok(new CustomResponse<List<Store>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = Stores });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
        [HttpGet]
        [Route("GetMerchantByCategoryIdForAdmin")]
        public async Task<IHttpActionResult> GetMerchantByCategoryIdForAdmin(int Category_Id)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {

                    var Boxes = ctx.Boxes.Where(x => x.Category_Id == Category_Id && !x.IsDeleted).ToList();

                    return Ok(new CustomResponse<List<Box>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = Boxes });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
        [HttpGet]
        [Route("GetAllBrands")]
        public async Task<IHttpActionResult> GetAllBrands()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    CustomResponse<BoxViewModel> response = new CustomResponse<BoxViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new BoxViewModel
                        {
                            Boxes = ctx.Boxes.Where(x => !x.IsDeleted).ToList(),
                            TotalRecords = ctx.Boxes.Count(x => !x.IsDeleted)
                        }
                    };
                    return Ok(response);
                }

            }
            /* try
            {
               using (SkriblContext ctx = new SkriblContext())
                {
                    var stores = new List<Box>();

                    stores =  ctx.Boxes.Where(x => x.IsDeleted == false).ToList().Select(x => new Box
                    {
                        Id = x.Id,
                        Name = x.Name

                    }).ToList();
                   
                    CustomResponse<IEnumerable<Box>> response = new CustomResponse<IEnumerable<Box>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = stores
                    };
                    return Ok(response);
                }
            }*/
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
       
        [HttpGet]
        [Route("GetBrandsByCategoryIdForAdmin")]
        public async Task<IHttpActionResult> GetBrandsByCategoryIdForAdmin(int Category_Id)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {

                    var Boxes = await ctx.Boxes.Where(x => x.Category_Id == Category_Id && !x.IsDeleted).ToListAsync();

                    return Ok(new CustomResponse<List<Box>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = Boxes });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
        [BasketApi.Authorize("Agent", "SuperAdmin", "ApplicationAdmin")]
        [Route("AddCity")]
        public async Task<IHttpActionResult> AddCity(AddCityBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (SkriblContext ctx = new SkriblContext())
                {
                    Cities city = new Cities();

                    if (model.Id == 0)
                    {
                        if (ctx.Cities.Where(x => x.CityName == model.CityName && x.IsDeleted == false && Convert.ToInt32(x.Latitude) == Convert.ToInt32(model.Latitude) && Convert.ToInt32(x.Longitude) == Convert.ToInt32(model.Longitude)).ToList().Count == 0) 
                        {
                            city = ctx.Cities.Add(new Cities
                            {
                                CityName = model.CityName,
                                Latitude = model.Latitude,
                                Longitude = model.Longitude,
                                IsDeleted = false
                            });
                            ctx.SaveChanges();
                        }
                    }
                    else
                    {
                        city = ctx.Cities.FirstOrDefault(x => x.Id == model.Id);
                        city.CityName = model.CityName;
                        city.Latitude = model.Latitude;
                        city.Longitude = model.Longitude;
                        ctx.SaveChanges();
                    }

                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });


                }

            }

            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
    }
}
