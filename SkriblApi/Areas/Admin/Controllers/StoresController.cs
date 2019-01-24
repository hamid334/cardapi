using BasketApi;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using WebApplication1.Areas.Admin.ViewModels;
using System.Data.Entity;
using BasketApi.ViewModels;

namespace BasketApi.Areas.Admin.Controllers
{
    //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin", "User")]
    [RoutePrefix("api/Store")]
    public class StoresController : ApiController
    {
        /// <summary>
        /// Get All Stores
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllStores")]
        public async Task<IHttpActionResult> GetAllStores()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var stores = ctx.Stores.Where(x => x.IsDeleted == false).ToList();

                    foreach (var store in stores)
                    {
                        store.CalculateAverageRating();
                    }
                    CustomResponse<IEnumerable<Store>> response = new CustomResponse<IEnumerable<Store>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = stores
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
        [Route("GetStoreByCategoryId")]
        public async Task<IHttpActionResult> GetStoreByCategoryId(int Category_Id, double? longitude = 0, double? latitude = 0, int? Page = 0, int? Items = 10)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var stores = ctx.Stores.Where(x => x.Category_Id == Category_Id && x.IsDeleted == false).ToList();

                    if (latitude != 0 && longitude != 0)
                    {
                        if (stores != null)
                        {
                            var sourceLoc = Utility.CreatePoint(latitude.Value, longitude.Value);
                            foreach (var store in stores)
                            {
                                store.Distance = Convert.ToDouble(String.Format("{0:0.00}",store.Location.Distance(sourceLoc).Value));
                            }
                        } 
                    }

                    CustomResponse<StoresViewModel> response = new CustomResponse<StoresViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new StoresViewModel {
                            Stores=stores,
                            TotalRecords=ctx.Stores.Count(x=>x.Category_Id==Category_Id && !x.IsDeleted)
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

        [HttpGet]
        [Route("SearchStoreInCategory")]
        public async Task<IHttpActionResult> SearchStoreInCategory(int Category_Id, double? longitude = 0, double? latitude = 0, string SearchString="",int? Page=0,int? Items=8)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var stores = ctx.Stores.Where(x =>x.Name.Contains(SearchString) && x.Category_Id==Category_Id && x.IsDeleted == false).ToList();

                    if (latitude != 0 && longitude != 0)
                    {
                        if (stores != null)
                        {
                            foreach (var store in stores)
                            {
                                var sourceLoc = Utility.CreatePoint(latitude.Value, longitude.Value);
                                store.Distance = Convert.ToDouble(String.Format("{0:0.00}", store.Location.Distance(sourceLoc).Value));
                            }
                        }
                    }

                    CustomResponse<StoresViewModel> response = new CustomResponse<StoresViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new StoresViewModel
                        {
                            Stores = stores.OrderBy(x=>x.Id).Skip(Page.Value*Items.Value).Take(Items.Value).ToList(),
                            TotalRecords = stores.Count
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

        ///// <summary>
        ///// Get Stores by Franchisor Id
        ///// </summary>
        ///// <param name="FranchisorId"></param>
        ///// <returns></returns>
        [HttpGet]
        [Route("GetAllStoresByFranchisorId")]
        public async Task<IHttpActionResult> GetAllStoresByFranchisorId(int FranchisorId)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    CustomResponse<IEnumerable<Store>> response = new CustomResponse<IEnumerable<Store>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = ctx.Stores.ToList()
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
        [Route("GetNearByStores")]
        public async Task<IHttpActionResult> GetNearByStores(double Latitude, double Longitude,int? Page=0,int? Items=6)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {

                    var point = Utility.CreatePoint(Latitude, Longitude);

                    var Store = ctx.Stores.Where(x => x.Location.Distance(point) < Global.NearbyStoreRadius && x.IsDeleted == false).OrderBy(x=>x.Id).Skip(Page.Value*Items.Value).Take(Items.Value).ToList();

                    if(Store != null)
                    {
                        foreach (var item in Store)
                        {
                            item.Distance = Convert.ToDouble(String.Format("{0:0.00}", item.Location.Distance(point).Value));
                        }
                    }

                   CustomResponse<StoresViewModel> response = new CustomResponse<StoresViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new StoresViewModel {
                            Stores=Store,
                            TotalRecords=ctx.Stores.Count(x=>!x.IsDeleted)
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

        
        /// <summary>
        /// Get Stores Count
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetStoresCount")]
        //public async Task<IHttpActionResult> GetStoresCount()
        //{
        //    try
        //    {
        //        using (SkriblContext ctx = new SkriblContext())
        //        {
        //            StoreCountViewModel model = new StoreCountViewModel { TotalStores = ctx.Stores.Count() };
        //            CustomResponse<StoreCountViewModel> response = new CustomResponse<StoreCountViewModel>
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

        /// <summary>
        /// Get Nearby Stores, Default radius is 50 miles. To get all stores, give long and lat = 0
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        //[Route("GetNearbyStores")]
        //public async Task<IHttpActionResult> GetNearbyStores(double longitude, double latitude, int PageSize, int PageNo, string filterTypes)
        //{
        //    try
        //    {
        //        var point = Utility.CreatePoint(latitude, longitude);

        //        using (SkriblContext ctx = new SkriblContext())
        //        {
        //            StoresViewModel storesViewModel = new StoresViewModel();
        //            if (longitude == 0 && latitude == 0)
        //            {
        //                storesViewModel.Count = ctx.Stores.Count(x => x.IsDeleted == false);
        //                storesViewModel.Stores = ctx.Stores.Include(x => x.StoreDeliveryHours).Include(x => x.StoreRatings).Where(x => x.IsDeleted == false).OrderByDescending(x => x.Id).Page(PageSize, PageNo).ToList();
        //            }
        //            else
        //            {
        //                storesViewModel.Count = ctx.Stores.Where(x => x.Location.Distance(point) < Global.NearbyStoreRadius && x.IsDeleted == false).Count();
        //                storesViewModel.Stores = ctx.Stores.Include(x => x.StoreDeliveryHours).Include(x => x.StoreRatings).Where(x => x.Location.Distance(point) < Global.NearbyStoreRadius && x.IsDeleted == false).OrderByDescending(x => x.Id).Page(PageSize, PageNo).ToList();
        //            }

        //            foreach (var store in storesViewModel.Stores)
        //            {
        //                store.CalculateAverageRating();
        //            }

        //            CustomResponse<StoresViewModel> reponse = new CustomResponse<StoresViewModel>
        //            {
        //                Message = Global.ResponseMessages.Success,
        //                StatusCode = (int)HttpStatusCode.OK,
        //                Result = storesViewModel
        //            };

        //            return Ok(reponse);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}
    }
}
