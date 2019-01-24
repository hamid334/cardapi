using BasketApi;
using BasketApi.Areas.SubAdmin.Models;
using BasketApi.ViewModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApplication1.Areas.Admin.Controllers
{
    //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin", "User", "Guest")]
    [RoutePrefix("api/Category")]
    public class CategoryController : ApiController
    {
        [Route("GetAllCategories")]
        public async Task<IHttpActionResult> GetAllCategories(int? Page = 0, int? Items = 6)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    CustomResponse<CategoryViewModel> response = new CustomResponse<CategoryViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new CategoryViewModel
                        {
                            Categories = ctx.Categories.Where(x => !x.IsDeleted).OrderBy(x => x.Sorting).Skip(Page.Value * Items.Value).Take(Items.Value).ToList(),
                            TotalRecords = ctx.Categories.Count(x => !x.IsDeleted)
                        }
                    };
                    return Ok(response);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[Route("GetSubCategoriesByCatId")]
        //public async Task<IHttpActionResult> GetSubCategoriesByCatId(int CatId)
        //{
        //    try
        //    {
        //        using (SkriblContext ctx = new SkriblContext())
        //        {
        //            var categories = ctx.Categories.Where(x => x.ParentCategoryId == CatId && x.IsDeleted == false).OrderBy(x => x.Name).ToList();
        //            categories.Insert(0, new Category { Name = "All", Id = CatId });
        //            CustomResponse<CategoriesViewModel> response = new CustomResponse<CategoriesViewModel>
        //            {
        //                Message = Global.ResponseMessages.Success,
        //                StatusCode = (int)HttpStatusCode.OK,
        //                Result = new CategoriesViewModel { Categories = categories }
        //            };
        //            return Ok(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}
    }
}
