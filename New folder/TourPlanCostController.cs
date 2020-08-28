// <copyright file="TourPlanCostController.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Areas.Sales.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using TravelMint.Service.CrossCutting;
    using TravelMint.UI.Framework.Excel;
    using TravelMint.UI.ViewModels;
    using TravelMint.UI.ViewModels.ExcelExport;

    /// <summary>
    /// RouteController
    /// </summary>
    public class TourPlanCostController : SalesAreaController
    {
        /// <summary>
        /// The environment
        /// </summary>
        private readonly IHostingEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="TourPlanCostController" /> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="environment">The environment.</param>
        public TourPlanCostController(IHttpClient httpClient, IHostingEnvironment environment)
            : base(httpClient)
        {
            this.environment = environment;
        }

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>Index</returns>
        public IActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Excels the import.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="routename">The name.</param>
        /// <param name="routeid">The routeid.</param>
        /// <returns>
        /// ExcelImport
        /// </returns>
        public async Task<JsonResult> ExcelImport(string fileName, string routename, Guid routeid)
        {
            await Task.FromResult(0);
            var viewResponse = new ViewResponse();

            if (string.IsNullOrEmpty(fileName))
            {
                return this.Json(new ExcelResponse { Message = "Please Upload Valid File!" });
            }

            var roomSharingList = await this.GetSelectList("/master/roomsharingtypelist", 0);

            var storagePath = Path.Combine(this.environment.ContentRootPath, "wwwroot", Constants.Storage.TrimStart('/'));
            FileInfo file = new FileInfo(Path.Combine(storagePath, fileName));
            if (!Constants.ExcelExtension.Contains(Path.GetExtension(fileName)))
            {
                return this.Json(new ExcelResponse { Message = "Please Upload Only Excel File." });
            }

            ExcelResponse excel = new ExcelResponse();
            Dictionary<string, IEnumerable<string>> cellExistin = new Dictionary<string, IEnumerable<string>>();

            ExcelSheet<PaxDetailExcelViewModel> itemExcelSheet = this.FromExcel<PaxDetailExcelViewModel>(file, Excel.PaxDetails, cellExistin);

            if (!itemExcelSheet.Status)
            {
                excel.Message = itemExcelSheet.Message;
                return this.Json(excel);
            }

            TourViewModel tourView = new TourViewModel();
            tourView.RouteId = routeid;
            tourView.Name = routename;
            var adults = itemExcelSheet.Records.Where(pax => pax.PaxType.ToLower() == Enums.PassengerType.Adult.ToString().ToLower());
            var childs = itemExcelSheet.Records.Where(pax => pax.PaxType.ToLower() == Enums.PassengerType.Child.ToString().ToLower());
            var foc = itemExcelSheet.Records.Where(pax => pax.PaxType.ToLower() == "tour leader");

            tourView.TotalAdult = (short)adults.Count();

            var paxSingle = adults.Where(pax => pax.RoomSharing.ToLower() == "sgl").Select(x => x.RoomSharing);
            var paxDouble = adults.Where(pax => pax.RoomSharing.ToLower() == "dbl").Select(x => x.RoomSharing);
            var paxTriple = adults.Where(pax => pax.RoomSharing.ToLower() == "tpl").Select(x => x.RoomSharing);
            var paxTwin = adults.Where(pax => pax.RoomSharing.ToLower() == "twn").Select(x => x.RoomSharing);

            tourView.PaxSingle = (short)paxSingle.Count();
            tourView.PaxDouble = (short)paxDouble.Count();
            tourView.PaxTriple = (short)paxTriple.Count();
            tourView.PaxTwin = (short)paxTwin.Count();

            tourView.TotalChild = (short)childs.Count();
            tourView.ChildCnb = (short)childs.Count(pax => pax.RoomSharing.ToLower() == "cwb");
            tourView.ChildCwb = (short)childs.Count(pax => pax.RoomSharing.ToLower() == "cnb");

            tourView.TotalFoc = (short)foc.Count();
            var focsingle = foc.Where(pax => pax.RoomSharing.ToLower() == "sgl").Select(x => x.RoomSharing);
            var focdouble = foc.Where(pax => pax.RoomSharing.ToLower() == "dbl").Select(x => x.RoomSharing);

            tourView.Focsingle = (short)focsingle.Count();
            tourView.Focdouble = (short)focdouble.Count();

            tourView.TotalPax = (short)(tourView.TotalAdult + tourView.TotalChild + tourView.TotalFoc);

            if (tourView.TotalAdult > 0 && tourView.TotalAdult != ((tourView.PaxSingle * 1) + (tourView.PaxDouble * 2) + (tourView.PaxTriple * 3) + (tourView.PaxTwin * 2)))
            {
                itemExcelSheet.Records.Where(pax => pax.PaxType.ToLower() == Enums.PassengerType.Adult.ToString().ToLower()).ToList().ForEach(x =>
                {
                    x.CellInfo.AddCellError(true, nameof(PaxDetailExcelViewModel.PaxType), "Mismatch Total Adult with Sharing Rooms", CellType.Invalid);
                    x.CellInfo.AddCellError(true, nameof(PaxDetailExcelViewModel.RoomSharing), "Mismatch Total Adult with Sharing Rooms", CellType.Invalid);
                });
            }

            if (tourView.TotalChild > 0 && tourView.TotalChild != (tourView.ChildCnb + tourView.ChildCwb))
            {
                itemExcelSheet.Records.Where(pax => pax.PaxType.ToLower() == Enums.PassengerType.Child.ToString().ToLower()).ToList().ForEach(x =>
                {
                    x.CellInfo.AddCellError(true, nameof(PaxDetailExcelViewModel.PaxType), "Mismatch Total Child with Sharing Rooms", CellType.Invalid);
                    x.CellInfo.AddCellError(true, nameof(PaxDetailExcelViewModel.RoomSharing), "Mismatch Total Child with Sharing Rooms", CellType.Invalid);
                });
            }

            if (tourView.TotalFoc > 0 && tourView.TotalFoc != ((tourView.Focsingle * 1) + (tourView.Focdouble * 2)))
            {
                itemExcelSheet.Records.Where(pax => pax.PaxType.ToLower() == "tour leader").ToList().ForEach(x =>
                {
                    x.CellInfo.AddCellError(true, nameof(PaxDetailExcelViewModel.PaxType), "Mismatch Total Tour Leader with Sharing Rooms", CellType.Invalid);
                    x.CellInfo.AddCellError(true, nameof(PaxDetailExcelViewModel.RoomSharing), "Mismatch Total Tour Leader with Sharing Rooms", CellType.Invalid);
                });
            }

            var inValidItemList = itemExcelSheet.Records.Where(x => x.CellInfo.Any());
            if (inValidItemList.Any())
            {
                excel.Status = false;
                excel.HtmlTables.Add(itemExcelSheet.Name, itemExcelSheet.Records.ToHtmlTable(inValidItemList.Any()));
                return this.Json(excel);
            }
            else
            {
                if (itemExcelSheet.Records.Any())
                {
                    tourView.PaxDetail = new List<PaxDetailViewModel>();

                    for (int i = 1; i <= itemExcelSheet.Records.Count(); i++)
                    {
                        PaxDetailExcelViewModel excelData = itemExcelSheet.Records[i - 1];
                        PaxDetailViewModel paxDetail = new PaxDetailViewModel();

                        paxDetail.SequenceNo = Convert.ToSByte(i);
                        paxDetail.PassengerType = (byte)(excelData.PaxType == "Tour Leader" ? "FOC" : excelData.PaxType).ToEnum<Enums.PassengerType>();
                        var roomSharing = roomSharingList.FirstOrDefault(x => x.Text == excelData.RoomSharing);
                        paxDetail.RoomSharingTypeId = roomSharing == null ? (byte)0 : Convert.ToByte(roomSharing.Value);
                        paxDetail.Title = (byte)excelData.Title.TrimEnd('.').ToEnum<Enums.Title>();
                        paxDetail.GenderTypeId = (byte)excelData.Gender.ToEnum<Enums.GenderType>();
                        paxDetail.RoomNo = Convert.ToSByte(excelData.RoomNo);
                        paxDetail.FirstName = excelData.FirstName;
                        paxDetail.LastName = excelData.LastName;
                        paxDetail.DateofBirth = (!string.IsNullOrEmpty(excelData.DateofBirth)) ? Convert.ToDateTime(excelData.DateofBirth) : paxDetail.DateofBirth;
                        paxDetail.Nationality = excelData.Nationality;
                        paxDetail.PassportNo = excelData.PassportNo;
                        paxDetail.PassportIssuePlace = excelData.PassportPlaceofIssue;

                        paxDetail.PassportIssueDate = (!string.IsNullOrEmpty(excelData.PassportDateofIssue)) ? Convert.ToDateTime(excelData.PassportDateofIssue) : paxDetail.PassportIssueDate;
                        paxDetail.PassportExpiryDate = (!string.IsNullOrEmpty(excelData.PassportExpiryDate)) ? Convert.ToDateTime(excelData.PassportExpiryDate) : paxDetail.PassportExpiryDate;
                        paxDetail.VisaNo = excelData.VisaNo;
                        paxDetail.VisaIssuePlace = excelData.VisaPlaceofIssue;

                        paxDetail.VisaIssueDate = (!string.IsNullOrEmpty(excelData.VisaDateofIssue)) ? Convert.ToDateTime(excelData.VisaDateofIssue) : paxDetail.VisaIssueDate;
                        paxDetail.VisaExpiryDate = (!string.IsNullOrEmpty(excelData.VisaExpiryDate)) ? Convert.ToDateTime(excelData.VisaExpiryDate) : paxDetail.VisaExpiryDate;
                        tourView.PaxDetail.Add(paxDetail);
                    }

                    var apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiTourPlanCost}/tourdetail", JsonConvert.SerializeObject(tourView));
                    if (apiResponse.Status)
                    {
                        return this.Json(new ExcelResponse { Message = "Successfully Imported", Status = true, NavigateUrl = this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage) });
                    }

                    return this.Json(new ExcelResponse { Message = apiResponse.Exception });
                }
                else
                {
                    return this.Json(new ExcelResponse { Message = "No Records Found" });
                }
            }
        }

        /// <summary>
        /// Excels the export.
        /// </summary>
        /// <param name="id">The tourid.</param>
        /// <returns>
        /// ExcelExport
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> ExcelExport(Guid id)
        {
            List<PaxDetailExcelViewModel> paxDetails = new List<PaxDetailExcelViewModel>();

            var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/gettourdetail", id);
            if (apiResponse.Status)
            {
                var viewModel = JsonConvert.DeserializeObject<TourViewModel>(apiResponse.Response);
                viewModel.PaxDetail = viewModel.PaxDetail ?? new List<PaxDetailViewModel>();
                ////short totalpax, totalchild, totalfoc;
                ////totalpax = viewModel.TotalPax ?? 0;
                ////totalchild = viewModel.TotalChild ?? 0;
                ////totalfoc = viewModel.TotalFoc ?? 0;
                ////viewModel.TotalPaxCount = (short?)(totalpax + totalchild + totalfoc);

                if (viewModel.PaxDetail != null)
                {
                    for (int i = 0; i < viewModel.PaxDetail.Count; i++)
                    {
                        PaxDetailViewModel detail = viewModel.PaxDetail.ToList()[i];
                        PaxDetailExcelViewModel excelData = new PaxDetailExcelViewModel();
                        excelData.SequenceNo = (i + 1).ToString();
                        excelData.PaxType = detail.PassengerType == (int)Enums.PassengerType.Foc
                            ? "Tour Leader" : ((Enums.PassengerType)detail.PassengerType).ToString();
                        excelData.Title = ((Enums.Title)detail.Title).ToString();
                        excelData.Gender = ((Enums.GenderType)detail.GenderTypeId).ToString();
                        excelData.RoomSharing = detail.RoomSharingTypeList.FirstOrDefault()?.Name;
                        excelData.RoomNo = Convert.ToInt16(detail.RoomNo ?? 0).ToString();
                        excelData.Gender = detail.FirstName;
                        excelData.LastName = detail.LastName;
                        excelData.DateofBirth = detail.DateofBirth.ToString("dd MMMM yyyy");
                        excelData.Nationality = detail.Nationality;
                        excelData.PassportNo = detail.PassportNo;
                        excelData.PassportPlaceofIssue = detail.PassportIssuePlace;
                        excelData.PassportDateofIssue = detail.PassportIssueDate.ToString("dd MMMM yyyy");
                        excelData.PassportExpiryDate = detail.PassportExpiryDate.ToString("dd MMMM yyyy");
                        excelData.VisaNo = detail.VisaNo;
                        excelData.VisaPlaceofIssue = detail.VisaIssuePlace;
                        excelData.VisaDateofIssue = detail.VisaIssueDate.ToString("dd MMMM yyyy");
                        excelData.VisaExpiryDate = detail.VisaExpiryDate.ToString("dd MMMM yyyy");
                        paxDetails.Add(excelData);
                    }

                    string templateDocument = this.environment.WebRootPath + "/Template/Excel/PaxDetail.xlsx";

                    byte[] filecontent = paxDetails.ToExcel("PaxDetail", templateDocument);
                    return this.File(filecontent, Constants.ExcelContentType, $"PaxDetails(Tour-{viewModel.Name}).xlsx");
                }
            }

            return this.Json("Record Not Exist");
        }

        /// <summary>
        /// Manages the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="view">The view.</param>
        /// <returns>
        /// Manage
        /// </returns>
        [HttpGet]
        public ActionResult Manage(Guid? id, string view)
        {
            if (this.IsAjaxRequest())
            {
                this.TempData["RouteId"] = id;
            }

            return this.View(view);
        }

        /// <summary>
        /// Costings this instance.
        /// </summary>
        /// <returns>Costing</returns>
        [HttpGet]
        public ActionResult Costing()
        {
            return this.View();
        }

        /// <summary>
        /// Manages the tour detail.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// ManageTourDetail
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> ManageTourDetail(Guid parentId, Guid id)
        {
            ////this.ViewBag.RouteId = parentId;
            ////this.ViewBag.TourId = id;
            var model = new RouteViewModel();
            if (parentId != null && parentId != Guid.Empty)
            {
                var apiResponse = await this.HttpClient.GetAsync(Constants.ApiTourPlanCost, parentId);
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<RouteViewModel>(apiResponse.Response);
                }
            }

            return this.View(this.ViewLocation("/TourDetail/ManageTourDetail", Constants.RouteAreaSales), model);
        }

        /// <summary>
        /// Paxes the detail.
        /// </summary>
        /// <param name="parentId">The identifier.</param>
        /// <param name="id">The tour identifier.</param>
        /// <returns>
        /// PaxDetail
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> PaxDetail(Guid parentId, Guid id)
        {
            if (this.IsAjaxRequest())
            {
                var viewModel = new TourViewModel();
                if (parentId != Guid.Empty && id == Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync(Constants.ApiTourPlanCost, parentId);
                    if (apiResponse.Status)
                    {
                        if (viewModel.CanEdit == false)
                        {
                            var routeViewModel = JsonConvert.DeserializeObject<RouteViewModel>(apiResponse.Response);
                            viewModel.CanEdit = true;
                            viewModel.Name = routeViewModel.Name;
                            viewModel.RouteId = routeViewModel.Id;
                            viewModel.FinancialYearId = routeViewModel.FinancialYearId;
                            viewModel.StartDate = routeViewModel.StartDate;
                            viewModel.EndDate = routeViewModel.EndDate;
                            viewModel.TotalPax = routeViewModel.TotalPax;
                        }
                    }
                }
                else
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/gettourdetail", id);
                    if (apiResponse.Status)
                    {
                        viewModel = JsonConvert.DeserializeObject<TourViewModel>(apiResponse.Response);
                        viewModel.PaxDetail = viewModel.PaxDetail ?? new List<PaxDetailViewModel>();
                        viewModel.TourLeaderName = viewModel.PaxDetail.Where(x => x.TourId == id && x.IsTourLeader == true).Select(x => x.Id.ToString()).ToArray();
                    }
                }

                return this.PartialView(this.ViewLocation("/TourDetail/_PaxDetail", Constants.RouteAreaSales), viewModel);
            }

            return this.View();
        }

        /// <summary>
        /// Paxes the detail.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="nextview">The nextview.</param>
        /// <returns>
        /// PaxDetail
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> PaxDetail(TourViewModel model, string nextview)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                model.TourLeaderName = model.TourLeaderName ?? new List<string>().ToArray();

                if ((short)model.TotalPax < (short)((model.TotalAdult ?? 0) + (model.TotalChild ?? 0) + (model.TotalFoc ?? 0)))
                {
                    viewResponse.Alert.Message = "Pax Count Cannot be greater than total pax.";
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                    return this.Json(viewResponse);
                }
                else if (model.TotalAdult < (short)((model.PaxSingle ?? 0) + ((model.PaxDouble ?? 0) * 2) + ((model.PaxTriple ?? 0) * 3) + ((model.PaxTwin ?? 0) * 2)))
                {
                    viewResponse.Alert.Message = "Adult pax cannot be greater than total adult.";
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                    return this.Json(viewResponse);
                }
                else if (model.TotalChild < (short)((model.ChildCnb ?? 0) + (model.ChildCwb ?? 0)))
                {
                    viewResponse.Alert.Message = "child pax cannot be greater than total child.";
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                    return this.Json(viewResponse);
                }
                else if (model.TotalFoc < (short)((model.Focsingle ?? 0) + ((model.Focdouble ?? 0) * 2)))
                {
                    viewResponse.Alert.Message = "foc cannot be greater than total foc.";
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                    return this.Json(viewResponse);
                }
                else if (model.PaxDetail != null && model.TotalPax < model.PaxDetail.Count)
                {
                    viewResponse.Alert.Message = "pax rows cannot greater than total pax count.";
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                    return this.Json(viewResponse);
                }

                if (model.Id == Guid.Empty)
                {
                    apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiTourPlanCost}/tourdetail", JsonConvert.SerializeObject(model));
                    if (apiResponse.Status)
                    {
                        var record = JsonConvert.DeserializeObject<TourViewModel>(apiResponse.Response);
                        if (record != null)
                        {
                            model.Id = record.Id;
                            viewResponse.Alert.Message = Messages.RecordInsert;
                        }
                    }
                }
                else
                {
                    apiResponse = await this.HttpClient.PutAsync($"{Constants.ApiTourPlanCost}/tourdetail", JsonConvert.SerializeObject(model));
                    if (apiResponse.Status)
                    {
                        viewResponse.Alert.Message = Messages.RecordUpdate;
                    }
                }

                viewResponse.Status = true;

                viewResponse.NavigateUrl = !string.IsNullOrEmpty(nextview)
                ? this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.ManageTourDetail, model.Id.ToString()) + nextview
                : this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage);

                ////var urlString = System.Net.WebUtility.UrlDecode(this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, $"ManageTourDetail"));

                return this.Json(viewResponse);
            }

            return this.PartialView(this.ViewLocation("/TourDetail/_PaxDetail", Constants.RouteAreaSales), model);
        }

        /// <summary>
        /// Arrivals the departure detail.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="nextView">if set to <c>true</c> [next view].</param>
        /// <returns>
        /// ArrivalDepartureDetail
        /// </returns>
        public async Task<IActionResult> ArrivalDepartureDetail(Guid id, bool nextView)
        {
            var viewModel = new TourClientDetailModel() { TourId = id };

            if (id != Guid.Empty)
            {
                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/arrdepdetailbyid", id);
                if (apiResponse.Status)
                {
                    var viewModels = JsonConvert.DeserializeObject<List<TourClientDetailViewModel>>(apiResponse.Response);
                    if (viewModels != null && viewModels.Count > 0)
                    {
                        viewModel.TourClientDetail = viewModels;
                    }
                }
            }

            if (viewModel.TourClientDetail == null || viewModel.TourClientDetail.Count == 0)
            {
                viewModel.TourClientDetail = new List<TourClientDetailViewModel>();
                viewModel.TourClientDetail.Add(new TourClientDetailViewModel { TourId = id });
            }

            return this.PartialView(this.ViewLocation("/TourDetail/_ArrivalDepartureDetail", Constants.RouteAreaSales), viewModel);
        }

        /// <summary>
        /// Arrivals the departure detail.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="nextview">The nextview.</param>
        /// <returns>ArrivalDepartureDetail</returns>
        [HttpPost]
        public async Task<IActionResult> ArrivalDepartureDetail(TourClientDetailModel model, string nextview)
        {
            var viewResponse = new ViewResponse();
            if (this.ModelState.IsValid)
            {
                var viewModel = new TourViewModel();
                model.TourClientDetail = model.TourClientDetail ?? new List<TourClientDetailViewModel>();
                foreach (var item in model.TourClientDetail)
                {
                    item.TourId = model.TourId;
                    item.CountryId = "IN";
                }

                var apiResponsedetail = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/gettourdetail", model.TourId);
                if (apiResponsedetail.Status)
                {
                    short? totalpax = 0;
                    viewModel = JsonConvert.DeserializeObject<TourViewModel>(apiResponsedetail.Response);
                    foreach (var item in model.TourClientDetail.Where(x => x.TransferType == (byte)Enums.TransferType.Arrival))
                    {
                        totalpax += item.TotalPax;

                        if (totalpax > viewModel.TotalPax)
                        {
                            viewResponse.Alert.Message = "Total arrival not exceed of total pax.";
                            viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                            return this.Json(viewResponse);
                        }
                    }

                    totalpax = 0;
                    foreach (var item in model.TourClientDetail.Where(x => x.TransferType == (byte)Enums.TransferType.Departure))
                    {
                        totalpax += item.TotalPax;

                        if (viewModel.TotalPax < totalpax)
                        {
                            viewResponse.Alert.Message = "Total departure not exceed of total pax.";
                            viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                            return this.Json(viewResponse);
                        }
                    }
                }

                var apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiTourPlanCost}/tourclientdetail", JsonConvert.SerializeObject(model, Formatting.Indented, this.JsonIgnoreNullable));

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                }

                viewResponse.Status = true;
                viewResponse.NavigateUrl = !string.IsNullOrEmpty(nextview)
               ? this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.BookingArrivalDepartureDetail, model.TourId.ToString()) + nextview
               : this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage);

                return this.Json(viewResponse);
            }

            return this.PartialView(this.ViewLocation("/TourDetail/_ArrivalDepartureDetail", Constants.RouteNameCompany), model);
        }

        /// <summary>
        /// Adds the new row.
        /// </summary>
        /// <returns>AddPaxRow</returns>
        public IActionResult AddPaxRow()
        {
            PaxDetailViewModel model = new PaxDetailViewModel();
            return this.PartialView(this.ViewLocation("/TourDetail/_PaxDetailGrid", Constants.RouteAreaSales), model);
        }

        /// <summary>
        /// Adds the arrival row.
        /// </summary>
        /// <returns>AddArrivalRow</returns>
        public IActionResult AddArrivalRow()
        {
            TourClientDetailViewModel model = new TourClientDetailViewModel();
            return this.PartialView(this.ViewLocation("/TourDetail/_ArrivalDepartureRows", Constants.RouteAreaSales), model);
        }

        /// <summary>
        /// Associates the pax.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="clientdetailid">The clientdetailid.</param>
        /// <param name="transfertype">The transfertype.</param>
        /// <param name="paxdetailId">The paxdetail identifier.</param>
        /// <returns>
        /// AssociatePax
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AssociatePax(Guid id, Guid clientdetailid, byte transfertype, string paxdetailId)
        {
            var viewResponse = new ViewResponse();
            var model = new TourClientDetailModel();

            if (this.ModelState.IsValid)
            {
                model.TourId = id;
                model.TourclientDetailId = clientdetailid;
                model.TransferType = transfertype;
                model.PaxDetailIds = paxdetailId ?? string.Empty;
                model.AssociatePaxDetails = model.PaxDetailIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x)).ToArray();
                model.AssociatePaxDetails = model.AssociatePaxDetails ?? new List<Guid>().ToArray();
                ApiDataResponse apiResponse;
                if (model.TourclientDetailId != Guid.Empty)
                {
                    apiResponse = await this.HttpClient.PutAsync($"{Constants.ApiTourPlanCost}/Associatepaxdetail", JsonConvert.SerializeObject(model, Formatting.Indented, this.JsonIgnoreNullable));
                    if (apiResponse.Status)
                    {
                        viewResponse.Alert.Message = Messages.RecordUpdate;
                    }
                }

                return this.Json(viewResponse);
            }

            return this.PartialView(this.ViewLocation("/TourDetail/_ArrivalDepartureDetail", Constants.RouteNameCompany), model);
        }

        /// <summary>
        /// Manages the general details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="nextView">if set to <c>true</c> [next view].</param>
        /// <returns>
        /// ManageGeneralDetails
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> GeneralDetails(Guid? id, bool nextView)
        {
            if (nextView)
            {
                var model = new RouteViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync(Constants.ApiTourPlanCost, id);
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<RouteViewModel>(apiResponse.Response);

                        model.StatusList = await this.GetDropdownLists("/master/routestatuslist", 0);
                    }
                }
                else
                {
                    if (this.RouteData.Values["parentid"] != null)
                    {
                        var parentId = Guid.Parse(this.RouteData.Values["parentid"].ToString());
                        var apiResponse = await this.HttpClient.GetAsync(Constants.ApiLead, parentId);

                        if (apiResponse.Status)
                        {
                            var lead = JsonConvert.DeserializeObject<LeadViewModel>(apiResponse.Response);

                            model.Name = lead.Name;
                            model.Remark = lead.Remark;
                            model.IsAgent = lead.IsLeadForAgent;
                            model.AgentList = lead.AgentList;
                            model.AgentId = lead.AgentId;
                        }

                        model.LeadId = parentId;
                    }

                    model.IsPaxRange = true;
                    model.IsTourByValidityDate = true;
                    model.IsAgent = true;
                }

                return this.PartialView("_GeneralDetails", model);
            }

            this.TempData["RouteId"] = id;
            return this.View(nameof(this.Manage), "tour-general-info");
        }

        /// <summary>
        /// Manages the specified route view model.
        /// </summary>
        /// <param name="model">The route view model.</param>
        /// <returns>Manage</returns>
        [HttpPost]
        public async Task<JsonResult> GeneralDetails(RouteViewModel model)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                if (model.IsAgent)
                {
                    model.ClientName = null;
                    model.ClientEmail = null;
                    model.ClientMobile = null;
                    model.ClientCountryId = null;
                }
                else
                {
                    model.AgentId = null;
                }

                ApiDataResponse apiResponse;

                apiResponse = model.Id == Guid.Empty ? await this.HttpClient.PostAsync(Constants.ApiTourPlanCost, JsonConvert.SerializeObject(model)) :
                   await this.HttpClient.PutAsync(Constants.ApiTourPlanCost, JsonConvert.SerializeObject(model));

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = model.Id == Guid.Empty ? Messages.RecordInsert : Messages.RecordUpdate;
                    viewResponse.Status = true;

                    if (model.ButtonActionType == Constants.SaveAndNext)
                    {
                        if (model.Id == Guid.Empty)
                        {
                            model = JsonConvert.DeserializeObject<RouteViewModel>(apiResponse.Response);
                        }

                        viewResponse.Id = model.Id.ToString();
                    }
                    else
                    {
                        viewResponse.NavigateUrl = this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, model.ButtonActionType == Constants.SaveAndNew ? "generaldetails" : Constants.IndexPage);
                    }

                    this.ViewBag.RouteOptionTab = null;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Routes the options.
        /// </summary>
        /// <param name="tp">The tp.</param>
        /// <returns>
        /// Collection of route options by tour plan
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> RouteOptionTabs(Guid tp)
        {
            ////var viewResponse = new ViewResponse();
            var routeOptions = new List<Dropdown>();
            var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/routeoptions/{tp}");
            if (apiResponse.Status)
            {
                routeOptions = JsonConvert.DeserializeObject<List<Dropdown>>(apiResponse.Response);
            }

            ////else
            ////{
            ////    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
            ////    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
            ////}

            return this.Json(routeOptions);
        }

        /// <summary>
        /// Tours the plan cost.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="nextView">if set to <c>true</c> [next view].</param>
        /// <returns>
        /// view
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> RouteOption(Guid parentId, Guid id, bool nextView)
        {
            if (nextView)
            {
                var model = new QuotationViewModel();

                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotation}/quotationadultview/{id}/{Guid.Empty}");
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                }

                this.ViewBag.IsShowFIT = model.IsFIT;
                this.ViewBag.IsShowGIT = model.IsGIT;
                if (model.QuotationView != null && model.QuotationView.Count > 0)
                {
                    this.ViewBag.routeId = model.QuotationView.FirstOrDefault().RouteId;
                }

                var pageRedirect = "_OptionDetail";
                return this.PartialView(pageRedirect, model);
            }

            var apiResponse1 = await this.HttpClient.GetAsync($"{Constants.ApiTourPlan}/routeoptions/{parentId}");
            if (apiResponse1.Status)
            {
                this.ViewBag.RouteOptionTab = JsonConvert.DeserializeObject<List<Dropdown>>(apiResponse1.Response);
            }

            this.TempData["RouteId"] = parentId;
            return this.View(nameof(this.Manage));
        }

        /// <summary>
        /// Creates the route option.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <returns>return partial view</returns>
        public ActionResult CreateRouteOption(Guid parentId)
        {
            var model = new RouteOptionViewModel { RouteId = parentId };

            return this.View("_Routeoption", model);
        }

        /// <summary>
        /// Createrouteoptions the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// json result
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> CreateRouteOption(RouteOptionViewModel model)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                var apiResponse = new ApiDataResponse();
                if (model.Id == Guid.Empty)
                {
                    apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiTourPlanCost}/routeoption", JsonConvert.SerializeObject(model));
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<RouteOptionViewModel>(apiResponse.Response);
                    }
                }

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordInsert;
                    viewResponse.Status = true;
                    viewResponse.NavigateUrl = "/sales/tourplancost/" + model.RouteId + "/routeoption/" + model.Id;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Manages the day details.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        /// ManageDayDetails
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> ManageDayDetail(Guid parentId, Guid id, short sequence)
        {
            if (this.IsAjaxRequest())
            {
                var model = new RouteDetailViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/routedetail", id);
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<RouteDetailViewModel>(apiResponse.Response);
                        model.Experiences = model.RouteDetailExperience.Select(x => x.ExperienceId.ToString()).ToArray();

                        ////model.CityName = model.CityList != null && model.CityList.Count > 0 ? model.CityList.FirstOrDefault().Name : string.Empty;
                        ////model.DepartCityName = model.DepartCityList != null && model.DepartCityList.Count > 0 ? model.DepartCityList.FirstOrDefault().Name : string.Empty;
                    }
                }
                else
                {
                    if (sequence - 1 > 0)
                    {
                        var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/generatenextdaydetail/{parentId}/{sequence - 1}");
                        if (apiResponse.Status)
                        {
                            model = JsonConvert.DeserializeObject<RouteDetailViewModel>(apiResponse.Response);

                            ////if (model.CityList != null && model.CityList.Count > 0)
                            ////{
                            ////    model.CityName = model.CityList != null && model.CityList.Count > 0 ? model.CityList.FirstOrDefault().Name : string.Empty;
                            ////}
                        }
                    }

                    model.RouteOptionId = parentId;
                    model.SequenceNo = sequence;

                    if (model.HotelId != null && model.HotelId != Guid.Empty)
                    {
                        model.RouteOvernightId = (byte)Enums.RouteOvernight.Hotel;
                    }
                    else
                    {
                        model.RouteOvernightId = (byte)Enums.RouteOvernight.None;
                    }

                    model.RouteDepartModeId = (byte)Enums.RouteDepartMode.None;
                }

                return this.PartialView("_DayPlanner", model);
            }

            return this.View();
        }

        /// <summary>
        /// Manages the day details.
        /// </summary>
        /// <param name="model">The route day details view model.</param>
        /// <returns>ManageDayDetails</returns>
        [HttpPost]
        public async Task<IActionResult> ManageDayDetail(RouteDetailViewModel model)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                ////var transferType = model.RouteDepartModeId == (byte)Enums.RouteDepartMode.Surface ? "S" : model.RouteDepartModeId == (byte)Enums.RouteDepartMode.Flight ? "F" : model.RouteDepartModeId == (byte)Enums.RouteDepartMode.Train ? "T" : string.Empty;

                ////model.SectorText = model.SequenceNo == 1 ? "Arrive " + model.CityName :
                ////    model.DepartCityId != null && model.DepartCityId != 0 && model.CityId != model.DepartCityId && model.RouteDepartModeId != (byte)Enums.RouteDepartMode.Enroute ? $"Transfer to {model.DepartCityName} ({transferType})" :
                ////    model.RouteDepartModeId == (byte)Enums.RouteDepartMode.Enroute ? $"Enroute {model.DepartCityName}" : model.RouteDepartModeId == (byte)Enums.RouteDepartMode.TourEnd ? "Tour Ends" : model.CityName;

                apiResponse = model.Id == Guid.Empty ? await this.HttpClient.PostAsync($"{Constants.ApiTourPlanCost}/routedetail", JsonConvert.SerializeObject(model)) :
                   await this.HttpClient.PutAsync($"{Constants.ApiTourPlanCost}/routedetail", JsonConvert.SerializeObject(model));

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = model.Id == Guid.Empty ? Messages.RecordInsert : Messages.RecordUpdate;
                    viewResponse.Status = true;
                    if (model.Id == Guid.Empty)
                    {
                        viewResponse.NavigateUrl = "New";
                    }
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Quotations the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>json response</returns>
        [HttpPost]
        public async Task<JsonResult> Quotation(QuotationViewModel model)
        {
            var viewResponse = new ViewResponse();

            var alert = new AlertMessage();
            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = model.Id == Guid.Empty ? await this.HttpClient.PostAsync(Constants.ApiQuotation, JsonConvert.SerializeObject(model)) :
                    await this.HttpClient.PutAsync(Constants.ApiQuotation, JsonConvert.SerializeObject(model));

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = model.Id == Guid.Empty ? Messages.RecordInsert : Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }

                ////viewResponse.NavigateUrl = this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage);
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Gets the quotation adult view.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// GetQuotationAdultView
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> GetAdultCostDetail(Guid id)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();

                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotation}/quotationadultview/{id}/{Guid.Empty}");
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                }

                if (model.QuotationView.Count == 0)
                {
                    var list = new List<QuotationViewViewModel> { new QuotationViewViewModel { SequenceNo = 0 } };
                    model.QuotationView = list;
                }

                this.ViewBag.IsShowFIT = model.IsFIT;
                this.ViewBag.IsShowGIT = model.IsGIT;
                if (model.QuotationView != null)
                {
                    this.ViewBag.routeId = model.QuotationView.FirstOrDefault().RouteId;
                }

                return this.PartialView("_QuotationAdultView", model.QuotationView);
            }

            return this.View();

            ////if (this.IsAjaxRequest())
            ////{
            ////    var model = new List<TourPlantCostDetailViewModel>();

            ////    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTourPlanCost}/adultcostdetail/" + id);
            ////    if (apiResponse.Status)
            ////    {
            ////        model = JsonConvert.DeserializeObject<List<TourPlantCostDetailViewModel>>(apiResponse.Response);
            ////    }

            ////    ////this.ViewBag.IsShowFIT = model.IsFIT;
            ////    ////this.ViewBag.IsShowGIT = model.IsGIT;
            ////    ////if (model.QuotationView != null)
            ////    ////{
            ////    ////    this.ViewBag.routeId = model.QuotationView.FirstOrDefault().RouteId;
            ////    ////}

            ////    return this.PartialView("_AdultCost", model);
            ////}

            ////return this.View();
        }

        /// <summary>
        /// Gets the quotation child view.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="optionId">The option identifier.</param>
        /// <returns>GetQuotationChildView</returns>
        [HttpGet]
        public async Task<ActionResult> GetQuotationChildView(Guid id, Guid? optionId)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (optionId == null)
                {
                    optionId = Guid.Empty;
                }

                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotation}/quotationadultview/{id}/{optionId}");
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                }

                this.ViewBag.IsShowFIT = model.IsFIT;
                this.ViewBag.IsShowGIT = model.IsGIT;
                return this.PartialView("_QuotationChildView", model);
            }

            return this.View();
        }

        /// <summary>
        /// Hotels the quotation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cityId">The city identifier.</param>
        /// <param name="optionId">The option identifier.</param>
        /// <param name="paxType">Type of the pax.</param>
        /// <returns>HotelQuotation</returns>
        [HttpGet]
        public async Task<ActionResult> HotelQuotation(Guid id, long? cityId, Guid? optionId, byte paxType)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    if (cityId == 0 || cityId > 0)
                    {
                        var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationHotel}/{id}");
                        if (apiResponse.Status)
                        {
                            model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                        }
                    }
                    else
                    {
                        var url = $"{Constants.ApiQuotationHotel}/quotationhotel/{id}/{cityId ?? 0}/{optionId ?? Guid.Empty}/{paxType}";
                        var apiResponse = await this.HttpClient.GetAsync(url);
                        if (apiResponse.Status)
                        {
                            model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                        }
                    }
                }

                this.ViewBag.PaxType = paxType;
                return this.PartialView("_QuotationHotel", model);
            }

            return this.View();
        }

        /// <summary>
        /// Hotels the quotation city.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cityId">The city identifier.</param>
        /// <param name="optionId">The option identifier.</param>
        /// <param name="paxType">Type of the pax.</param>
        /// <returns>HotelQuotationCity</returns>
        [HttpGet]
        public async Task<ActionResult> HotelQuotationCity(Guid? id, long? cityId, Guid? optionId, byte paxType)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    if (optionId == null)
                    {
                        optionId = Guid.Empty;
                    }

                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationHotel}/quotationhotel/{id}/{cityId}/{optionId}/{paxType}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    }
                }

                if (model.QuotationHotelOption.Count > 0)
                {
                    this.ViewBag.OptionId = model.QuotationHotelOption.FirstOrDefault().Id;
                }

                this.ViewBag.PaxType = paxType;
                return this.PartialView("_QuotationHotelList", model.QuotationHotel);
            }

            return this.View();
        }

        /// <summary>
        /// Gets the hotel quotation.
        /// </summary>
        /// <param name="quotation">The quotation.</param>
        /// <returns>HotelQuotation</returns>
        [HttpPost]
        public async Task<JsonResult> HotelQuotation(QuotationViewModel quotation)
        {
            var viewResponse = new ViewResponse();

            var alert = new AlertMessage();
            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationHotel}/quotationhotel", JsonConvert.SerializeObject(quotation));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Adds the new row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="optionId">The option identifier.</param>
        /// <returns>AddNewRow</returns>
        public IActionResult AddNewRow(Guid id, Guid optionId)
        {
            var model = new QuotationHotelViewModel
            {
                RouteServiceCategoryId = 1,
                QuotationId = id,
                QuotationHotelOptionId = optionId,
                PaxType = (byte)Enums.PaxType.Pax
            };
            return this.PartialView("_QuotationHotelRow", model);
        }

        /// <summary>
        /// Adds the supplement row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="optionId">The option identifier.</param>
        /// <returns>AddSupplementRow</returns>
        public IActionResult AddSupplementRow(Guid id, Guid optionId)
        {
            var model = new QuotationHotelViewModel
            {
                RouteServiceCategoryId = 2,
                QuotationId = id,
                QuotationHotelOptionId = optionId,
                PaxType = (byte)Enums.PaxType.Pax
            };
            return this.PartialView("_QuotationHotelRow", model);
        }

        /// <summary>
        /// Adds the upgrade row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="optionId">The option identifier.</param>
        /// <returns>AddUpgradeRow</returns>
        public IActionResult AddUpgradeRow(Guid id, Guid optionId)
        {
            var model = new QuotationHotelViewModel
            {
                RouteServiceCategoryId = 3,
                QuotationId = id,
                QuotationHotelOptionId = optionId,
                PaxType = (byte)Enums.PaxType.Pax
            };
            return this.PartialView("_QuotationHotelRow", model);
        }

        /// <summary>
        /// Hotels the price view.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>HotelPriceView</returns>
        public ActionResult HotelPriceView(HotelPriceFilter filter)
        {
            if (this.IsAjaxRequest())
            {
                ////var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationFlight}/flightcostviewdetail" + "/" + Guid.Parse(sourceCityId.ToString()) + "/" + Guid.Parse(destinationCityId.ToString()));
                ////if (apiResponse.Status)
                ////{
                ////    model = JsonConvert.DeserializeObject<List<FlightCostViewsModel>>(apiResponse.Response, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                ////}

                return this.PartialView("_HotelPriceList", filter);
            }

            return this.View();
        }

        /// <summary>
        /// Routes the hotel restaurent view.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="routedetailid">The routedetailid.</param>
        /// <param name="dayNo">The day no.</param>
        /// <param name="paxType">Type of the pax.</param>
        /// <returns>
        /// RouteHotelRestaurentView
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> RouteHotelRestaurentView(Guid? id, Guid? routedetailid, int dayNo, byte paxType)
        {
            if (this.IsAjaxRequest())
            {
                var model = new RouteHotelRestaurentViewModel();
                if (id != null && id != Guid.Empty && routedetailid != null && routedetailid != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationRestaurant}/routehotelrestaurentdetail/{id}/{routedetailid}/{dayNo}/{paxType}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<RouteHotelRestaurentViewModel>(apiResponse.Response);
                    }
                }

                if (model.ObjRouteHotelRestaurentViewRow == null)
                {
                    model.ObjRouteHotelRestaurentViewRow = new List<RouteHotelRestaurentViewRowModel>();
                }

                model.DayNo = dayNo;
                this.ViewBag.PaxType = paxType;
                return this.PartialView("_RouteHotelRestaurentView", model);
            }

            return this.View();
        }

        /// <summary>
        /// Routes the hotel restaurent view.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// viewResponse
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> RouteHotelRestaurentView(RouteHotelRestaurentViewModel model)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                var apiResponse = new ApiDataResponse();
                if (model.QuotationRestaurantId == Guid.Empty)
                {
                    apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationRestaurant}/routehotelrestaurentdetail", JsonConvert.SerializeObject(model));
                }
                else
                {
                    apiResponse = await this.HttpClient.PutAsync($"{Constants.ApiQuotationRestaurant}/routehotelrestaurentdetail", JsonConvert.SerializeObject(model));
                }

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordInsert;
                    model = JsonConvert.DeserializeObject<RouteHotelRestaurentViewModel>(apiResponse.Response);
                    viewResponse.Status = true;
                    var urlString = System.Net.WebUtility.UrlDecode(this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, "RouteHotelRestaurentView/" + model.RouteDetailId + "?dayNo=" + model.DayNo + "&paxType=" + model.PaxType));
                    viewResponse.NavigateUrl = urlString;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Adds the upgrade suppliment row.
        /// </summary>
        /// <param name="routeServiceCategoryId">The route service category identifier.</param>
        /// <param name="mealType">Type of the meal.</param>
        /// <param name="routeOptionId">The route identifier.</param>
        /// <param name="dayNo">The day no.</param>
        /// <param name="cityId">The city identifier.</param>
        /// <param name="routedetailid">The routedetailid.</param>
        /// <returns>
        /// RouteHotelRestaurentViewRowModel
        /// </returns>
        public async Task<ActionResult> AddRestaurantUpgradeRow(byte routeServiceCategoryId, byte mealType, Guid routeOptionId, short dayNo, long cityId, Guid routedetailid)
        {
            var model = new RouteHotelRestaurentViewRowModel
            {
                RouteServiceCategoryId = routeServiceCategoryId,
                MealType = mealType,
                CityId = cityId,
                RouteOptionId = routeOptionId,
                DayNo = dayNo,
                PaxType = (byte)Enums.PaxType.Pax,
                RouteDetailId = routedetailid
            };
            var apiResponse = new ApiDataResponse();
            apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationRestaurant}/citydropdownbyrouteanddaynolist?id=" + routeOptionId + "&dayNo=" + dayNo + "&routedetailid=" + routedetailid + "&byId=true ");
            model.DayCityList = JsonConvert.DeserializeObject<List<Dropdown>>(apiResponse.Response);

            return this.PartialView("_RouteHotelRestaurentViewRow", model);
        }

        /// <summary>
        /// Adds the restaurant new row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="routeDetailId">The route detail identifier.</param>
        /// <param name="routeOptionId">The route option identifier.</param>
        /// <param name="dayNo">The day no.</param>
        /// <returns>AddRestaurantNewRow</returns>
        public async Task<IActionResult> AddRestaurantNewRow(Guid id, Guid routeDetailId, Guid routeOptionId, short dayNo)
        {
            var cityList = await this.GetDropdownLists("quotationrestaurant/citydropdownbyrouteanddaynolist", routeOptionId + "&dayNo=" + dayNo + "&routedetailid=" + routeDetailId);

            var model = new RouteHotelRestaurentViewRowModel
            {
                RouteServiceCategoryId = 1,
                QuotationId = id,
                RouteDetailId = routeDetailId,
                PaxType = (byte)Enums.PaxType.Pax,
                RouteOptionId = routeOptionId,
                DayNo = dayNo,
                DayCityList = SetSmartCombo.Option(cityList[0].Id.ToString(), cityList[0].Name.ToString())
            };

            return this.PartialView("_RouteHotelRestaurentViewRow", model);
        }

        /// <summary>
        /// Quotations the monument.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dayno">The day no.</param>
        /// <param name="paxType">Type of the pax.</param>
        /// <returns>QuotationMonument</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationMonument(Guid id, int dayno, Guid paxType)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationMonument}/quotationmonument/{id}/{dayno}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    }
                }

                var places = await this.GetRouteDetailPlaces(model.RouteOption.RouteDetail.Where(x => x.SequenceNo == dayno).FirstOrDefault().CityId);
                places.DayNo = dayno;
                places.CityName = model.RouteOption.RouteDetail.Where(x => x.SequenceNo == dayno).FirstOrDefault().CityName;

                ////var list = await this.GetSightSeen(model.RouteOption.RouteDetail.Where(x => x.SequenceNo == dayno).FirstOrDefault().CityId, paxType);
                ////model.RouteDetailMonument = list;
                return this.PartialView("_QuotationMonument", places);
            }

            return this.View();
        }

        /// <summary>
        /// Quotations the monument.
        /// </summary>
        /// <param name="quotation">The quotation.</param>
        /// <returns>QuotationMonument</returns>
        [HttpPost]
        public async Task<JsonResult> QuotationMonument(QuotationViewModel quotation)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                var nextModel = quotation.CommandButton;
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationMonument}/quotationmonument", JsonConvert.SerializeObject(quotation));

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordInsert;
                    viewResponse.Status = true;
                    quotation = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    if (nextModel == "SaveandNext")
                    {
                        viewResponse.NavigateUrl = System.Net.WebUtility.UrlDecode(this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, "QuotationMonument/" + quotation.Id + "?dayNo=" + quotation.SequenceNo + "&paxType=" + quotation.RouteDetailId));
                    }
                    else
                    {
                        viewResponse.NavigateUrl = System.Net.WebUtility.UrlDecode(this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, "QuotationMonument/" + quotation.Id + "?dayNo=" + quotation.SequenceNo + "&paxType=" + quotation.RouteDetailId));
                    }
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }

                ////viewResponse.NavigateUrl = this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage);
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Sets the end date.
        /// </summary>
        /// <param name="stdate">The stdate.</param>
        /// <param name="days">The days.</param>
        /// <returns>End date</returns>
        public JsonResult SetEndDate(string stdate, int days)
        {
            return this.Json(Convert.ToDateTime(stdate).AddDays(days).ToShortDateString());
        }

        /// <summary>
        /// Adds the monument cost data.
        /// </summary>
        /// <param name="dayNo">The day no.</param>
        /// <param name="cityId">The city identifier.</param>
        /// <param name="lstRouteMonument">The LST route monument.</param>
        /// <returns>AddMonumentCostData</returns>
        public IActionResult AddMonumentCostData(int dayNo, long cityId, string lstRouteMonument)
        {
            var model = new List<QuotationMonumentViewModel>();
            ////var list = JsonConvert.DeserializeObject<List<RouteDetailMonumentViewModel>>(lstRouteMonument);
            ////if (list.Count > 0)
            ////{
            ////    list.Remove(list[0]);
            ////}

            ////foreach (var item in list)
            ////{
            ////    if (item.MonumentId != null)
            ////    {
            ////        var monument = new QuotationMonumentViewModel
            ////        {
            ////            Id = Guid.NewGuid(),
            ////            RouteDetailId = item.RouteDetailId,
            ////            IsDrivePast = item.IsDrivePast,
            ////            MonumentType = (byte)Enums.MonumentType.Monument,
            ////            DayTypeId = item.DayTypeId,
            ////            MonumentId = item.MonumentId,
            ////            MonumentPackageId = item.MonumentPackageId,
            ////            ExcursionId = item.ExcursionId,
            ////            MonumentName = item.Name
            ////        };
            ////        model.Add(monument);
            ////    }

            ////    if (item.MonumentPackageId != null)
            ////    {
            ////        var monument = new QuotationMonumentViewModel
            ////        {
            ////            Id = Guid.NewGuid(),
            ////            RouteDetailId = item.RouteDetailId,
            ////            IsDrivePast = item.IsDrivePast,
            ////            MonumentType = (byte)Enums.MonumentType.MonumentPackage,
            ////            DayTypeId = item.DayTypeId,
            ////            MonumentId = item.MonumentId,
            ////            MonumentPackageId = item.MonumentPackageId,
            ////            ExcursionId = item.ExcursionId,
            ////            MonumentName = item.Name
            ////        };
            ////        model.Add(monument);
            ////    }

            ////    if (item.ExcursionId != null)
            ////    {
            ////        var monument = new QuotationMonumentViewModel
            ////        {
            ////            Id = Guid.NewGuid(),
            ////            RouteDetailId = item.RouteDetailId,
            ////            IsDrivePast = item.IsDrivePast,
            ////            MonumentType = (byte)Enums.MonumentType.Excursion,
            ////            DayTypeId = item.DayTypeId,
            ////            MonumentId = item.MonumentId,
            ////            MonumentPackageId = item.MonumentPackageId,
            ////            ExcursionId = item.ExcursionId,
            ////            MonumentName = item.Name
            ////        };
            ////        model.Add(monument);
            ////    }
            ////}

            return this.PartialView("_QuotationMonumentList", model);
        }

        /// <summary>
        /// Gets the quotation experience.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>QuotationExperience</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationExperience(Guid id)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationExperience}/quotationexperience/{id}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    }
                }

                foreach (var rowData in model.QuotationExperience)
                {
                    rowData.IsSupplement = rowData.RouteServiceCategoryId == 2 ? true : false;
                }

                return this.PartialView("_QuotationExperience", model);
            }

            return this.View();
        }

        /// <summary>
        /// Gets the quotation experience.
        /// </summary>
        /// <param name="quotation">The quotation.</param>
        /// <returns>QuotationExperience</returns>
        [HttpPost]
        public async Task<JsonResult> QuotationExperience(QuotationViewModel quotation)
        {
            foreach (var rowData in quotation.QuotationExperience)
            {
                rowData.RouteServiceCategoryId = rowData.IsSupplement == true ? (byte)2 : (byte)1;
            }

            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationExperience}/quotationexperience", JsonConvert.SerializeObject(quotation));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;

                    viewResponse.Status = true;
                    ////viewResponse.NavigateUrl = this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage);
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Adds the experience row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="bIsEscortIncluded">if set to <c>true</c> [b is escort included].</param>
        /// <returns>AddExperienceRow</returns>
        public IActionResult AddExperienceRow(Guid id, bool bIsEscortIncluded)
        {
            var model = new QuotationExperienceViewModel
            {
                Id = Guid.NewGuid(),
                QuotationId = id,
                RouteServiceCategoryId = 1,
                QuotationExperienceCost = new List<QuotationExperienceCostViewModel>()
            };
            this.ViewBag.bIsEscortIncluded = bIsEscortIncluded;
            return this.PartialView("_QuotationExperienceRow", model);
        }

        /// <summary>
        /// Adds the experience cost row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>AddExperienceCostRow</returns>
        public IActionResult AddExperienceCostRow(Guid id)
        {
            var model = new QuotationExperienceCostViewModel
            {
                QuotationExperienceId = id
            };
            return this.PartialView("_QuotationExperienceCostRow", model);
        }

        /// <summary>
        /// Gets the experience cost data.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="lstExperienceCost">The LST experience cost.</param>
        /// <param name="quotationExperienceId">The quotation experience identifier.</param>
        /// <returns>GetExperienceCostData</returns>
        public async Task<IActionResult> GetExperienceCostData(Guid id, List<QuotationExperienceCostViewModel> lstExperienceCost, Guid quotationExperienceId)
        {
            if (lstExperienceCost != null && lstExperienceCost.Count > 0)
            {
                return this.PartialView("_QuotationExperienceCostList", lstExperienceCost);
            }
            else
            {
                var model = new List<ExperienceCostViewModel>();
                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiExperience}/experincecostdata/" + id);
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<List<ExperienceCostViewModel>>(apiResponse.Response);
                }

                ////foreach (var item in model)
                ////{
                ////    var experienceCost = new QuotationExperienceCostViewModel
                ////    {
                ////        QuotationExperienceId = quotationExperienceId,
                ////        MinPax = item.MinPax,
                ////        MaxPax = item.MaxPax,
                ////        UptoChild = item.UptoChild,
                ////        Amount = item.Amount,
                ////        ChildAmount = item.ChildAmount,
                ////        UnitOfMeasureId = item.UnitOfMeasureId,
                ////        UnitOfMeasureValue = item.UnitOfMeasureValue,
                ////        Duration = item.Duration,
                ////        Class = item.Class
                ////    };
                ////    lstExperienceCost.Add(experienceCost);
                ////}

                return this.PartialView("_QuotationExperienceCostList", lstExperienceCost);
            }
        }

        /// <summary>
        /// Guides the quotation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>GuideQuotation</returns>
        [HttpGet]
        public async Task<ActionResult> GuideQuotation(Guid? id)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationGuide}/{id}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                        if (model.QuotationGuideLanguage != null)
                        {
                            if (model.GuideLanguageList == null)
                            {
                                model.GuideLanguageList = new List<Dropdown>();
                            }

                            if (model.GuideUpgradeLanguageList == null)
                            {
                                model.GuideUpgradeLanguageList = new List<Dropdown>();
                            }

                            foreach (var item in model.QuotationGuideLanguage.Where(x => x.RouteServiceCategoryId == 1))
                            {
                                model.GuidePackageLanguage = item.LanguageId.ToString();
                                model.GuideLanguageList.Add(new Dropdown() { Name = item.LanguageName, Id = item.LanguageId.ToString() });
                            }

                            model.GuideUpgradeLanguage = model.QuotationGuideLanguage.Where(x => x.RouteServiceCategoryId == 3).Select(x => x.LanguageId.ToString()).ToArray();
                            foreach (var item in model.QuotationGuideLanguage.Where(x => x.RouteServiceCategoryId == 3))
                            {
                                model.GuideUpgradeLanguageList.Add(new Dropdown() { Name = item.LanguageName, Id = item.LanguageId.ToString() });
                            }
                        }
                    }
                }

                return this.PartialView("_QuotationGuide", model);
            }

            return this.View();
        }

        /// <summary>
        /// Guides the quotation.
        /// </summary>
        /// <param name="objQuotation">The object quotation escort.</param>
        /// <returns>GuideQuotation</returns>
        [HttpPost]
        public async Task<JsonResult> GuideQuotation(QuotationViewModel objQuotation)
        {
            var viewResponse = new ViewResponse();

            var alert = new AlertMessage();
            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationGuide}", JsonConvert.SerializeObject(objQuotation));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;

                    viewResponse.Status = true;
                    ////viewResponse.NavigateUrl = this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage);
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Adds the guide new row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>AddGuideNewRow</returns>
        public async Task<IActionResult> AddGuideNewRow(Guid id)
        {
            var guidePaxRange = new List<GuidePaxRangeViewModel>();
            var apiResponse = await this.HttpClient.GetAsync($"{Constants.GuidePaxRange}");
            if (apiResponse.Status)
            {
                guidePaxRange = JsonConvert.DeserializeObject<List<GuidePaxRangeViewModel>>(apiResponse.Response);
            }

            var model = new QuotationGuideViewModel
            {
                Id = Guid.NewGuid(),
                QuotationId = id,
                QuotationGuideCost = new List<QuotationGuideCostViewModel>()
            };

            foreach (var item in guidePaxRange)
            {
                var childModel = new QuotationGuideCostViewModel
                {
                    Id = Guid.NewGuid(),
                    QuotationGuideId = model.Id,
                    MinPax = item.MinPax,
                    MaxPax = item.MaxPax
                };
                model.QuotationGuideCost.Add(childModel);
            }

            return this.PartialView("_QuotationGuideRow", model);
        }

        /// <summary>
        /// Gets the escort quotation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>ActionResult</returns>
        [HttpGet]
        public async Task<ActionResult> EscortQuotation(Guid? id)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationEscort}/quotationEscort" + "/" + Guid.Parse(id.ToString()));
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    }
                }

                model.QuotationEscortService = model.QuotationEscortService ?? new QuotationEscortServiceViewModel();
                return this.PartialView("_QuotationEscort", model);
            }

            return this.View();
        }

        /// <summary>
        /// Escorts the quotation.
        /// </summary>
        /// <param name="objQuotationEscort">The quotation escort.</param>
        /// <returns>
        /// Status
        /// </returns>
        [HttpPost]
        public async Task<JsonResult> EscortQuotation(QuotationViewModel objQuotationEscort)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = objQuotationEscort.Id == Guid.Empty ? await this.HttpClient.PostAsync($"{Constants.ApiQuotationEscort}/quotationEscort", JsonConvert.SerializeObject(objQuotationEscort)) :
                   await this.HttpClient.PutAsync($"{Constants.ApiQuotationEscort}/quotationEscort", JsonConvert.SerializeObject(objQuotationEscort));

                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = objQuotationEscort.Id == Guid.Empty ? Messages.RecordInsert : Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Adds the new row escort view model.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// model
        /// </returns>
        public IActionResult AddNewRowEscortViewModel(Guid id)
        {
            var model = new QuotationEscortViewModel
            {
                QuotationId = id
            };
            return this.PartialView("_QuotationEscortRow", model);
        }

        /// <summary>
        /// Adds the miscellaneous row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="bEscortIncluded">if set to <c>true</c> [is escort included].</param>
        /// <returns>AddMiscellaneousRow</returns>
        public IActionResult AddMiscellaneousRow(Guid id, bool bEscortIncluded)
        {
            var model = new QuotationMiscellaneousViewModel
            {
                Id = Guid.NewGuid(),
                QuotationId = id
            };
            this.ViewBag.bIsEscortIncluded = bEscortIncluded;
            return this.PartialView("_QuotationMiscellaneousRow", model);
        }

        /// <summary>
        /// Gets the quotation miscellaneous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>QuotationMiscellaneous</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationMiscellaneous(Guid id)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationMiscellaneous}/quotationmiscellaneous/{id}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    }
                }

                return this.PartialView("_QuotationMiscellaneous", model);
            }

            return this.View();
        }

        /// <summary>
        /// Gets the quotation miscellaneous.
        /// </summary>
        /// <param name="quotation">The quotation.</param>
        /// <returns>QuotationMiscellaneous</returns>
        [HttpPost]
        public async Task<JsonResult> QuotationMiscellaneous(QuotationViewModel quotation)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationMiscellaneous}/quotationmiscellaneous", JsonConvert.SerializeObject(quotation));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                    viewResponse.Status = true;
                    ////viewResponse.NavigateUrl = this.Url.GetSalesRouteUrl(this.ControllerContext.ActionDescriptor.ControllerName, Constants.IndexPage);
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Gets the quotation assistance.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>QuotationAssistance</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationAssistance(Guid id)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationAssistance}/quotationassistance/{id}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    }
                }

                return this.PartialView("_QuotationAssistance", model);
            }

            return this.View();
        }

        /// <summary>
        /// Gets the quotation assistance.
        /// </summary>
        /// <param name="quotation">The quotation.</param>
        /// <returns>QuotationAssistance</returns>
        [HttpPost]
        public async Task<JsonResult> QuotationAssistance(QuotationViewModel quotation)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationAssistance}/quotationassistance", JsonConvert.SerializeObject(quotation));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Adds the assistance row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>AddAssistanceRow</returns>
        public IActionResult AddAssistanceRow(Guid id)
        {
            var model = new QuotationAssistanceViewModel
            {
                QuotationId = id
            };
            return this.PartialView("_QuotationAssistanceRow", model);
        }

        /// <summary>
        /// Quotations the transport.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>QuotationTransport</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationTransport(Guid id)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationTransport}/quotationtransport/{id}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                        if (model.QuotationTransport != null)
                        {
                            if (model.QuotationTransportCost == null)
                            {
                                model.QuotationTransportCost = new List<QuotationTransportCostViewModel>();
                            }

                            foreach (var item in model.QuotationTransport.Where(x => x.RouteServiceCategoryId == 1))
                            {
                                foreach (var itemCost in item.QuotationTransportCost.Where(x => x.RouteServiceCategoryId == 1))
                                {
                                    model.QuotationTransportCost.Add(itemCost);
                                }

                                break;
                            }

                            if (model.QuotationTransportSupplementCost == null)
                            {
                                model.QuotationTransportSupplementCost = new List<QuotationTransportCostViewModel>();
                            }

                            foreach (var item in model.QuotationTransport.Where(x => x.RouteServiceCategoryId == 2))
                            {
                                foreach (var itemCost in item.QuotationTransportCost.Where(x => x.RouteServiceCategoryId == 2))
                                {
                                    model.QuotationTransportSupplementCost.Add(itemCost);
                                }

                                break;
                            }

                            if (model.QuotationTransportCost != null)
                            {
                                foreach (var itemCost in model.QuotationTransportCost)
                                {
                                    model.TransportCategory = model.QuotationTransportCost.Select(x => x.TransportCategoryId.ToString()).ToArray();
                                }
                            }

                            if (model.QuotationTransportSupplementCost != null)
                            {
                                foreach (var itemCost in model.QuotationTransportSupplementCost)
                                {
                                    model.TransportCategorySupplement = model.QuotationTransportSupplementCost.Select(x => x.TransportCategoryId.ToString()).ToArray();
                                }
                            }

                            await this.BindSelectList(model);
                        }
                    }
                }

                return this.PartialView("_QuotationTransport", model);
            }

            return this.View();
        }

        /// <summary>
        /// Quotations the transport.
        /// </summary>
        /// <param name="quotation">The quotation.</param>
        /// <returns>QuotationTransport</returns>
        [HttpPost]
        public async Task<JsonResult> QuotationTransport(QuotationViewModel quotation)
        {
            var viewResponse = new ViewResponse();
            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationTransport}/quotationtransport", JsonConvert.SerializeObject(quotation));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Adds the transport cost row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="routeDetailTransport">The route detail transport.</param>
        /// <returns>
        /// AddTransportCostRow
        /// </returns>
        public IActionResult AddTransportCostRow(string id, List<RouteDetailTransportViewModel> routeDetailTransport)
        {
            if (routeDetailTransport.Count == 0)
            {
                return null;
            }

            var model = new QuotationTransportViewModel
            {
                RouteServiceCategoryId = 1,
                Id = Guid.NewGuid(),
                DayFrom = routeDetailTransport.Min(x => x.DayFrom),
                DayTo = routeDetailTransport.Max(x => x.DayTo),
                CityFromId = routeDetailTransport.FirstOrDefault().CityFromId,
                CityToId = routeDetailTransport.LastOrDefault().CityToId,
                TransportTypeId = routeDetailTransport.Count > 1 ? (byte)3 : routeDetailTransport.FirstOrDefault().TransportTypeId,
                CityFromList = SetSmartCombo.Option(routeDetailTransport.FirstOrDefault().CityFromId.ToString(), routeDetailTransport.FirstOrDefault().CityFromName.ToString()),
                CityToList = SetSmartCombo.Option(routeDetailTransport.LastOrDefault().CityToId.ToString(), routeDetailTransport.LastOrDefault().CityToName.ToString())
            };

            model.TransportTypeList = SetSmartCombo.Option(model.TransportTypeId.ToString(), routeDetailTransport.Count > 1 ? "Package" : routeDetailTransport.FirstOrDefault().TransportTypeName);

            if (model.QuotationTransportCost == null)
            {
                model.QuotationTransportCost = new List<QuotationTransportCostViewModel>();
            }

            if (id.Length > 0 && id != "null")
            {
                foreach (var item in id.Split(","))
                {
                    var modelCost = new QuotationTransportCostViewModel
                    {
                        QuotationTransportId = model.Id,
                        RouteServiceCategoryId = 1,
                        TransportCategoryId = new Guid(item.ToString())
                    };
                    model.QuotationTransportCost.Add(modelCost);
                }
            }

            return this.PartialView("_QuotationTransportRow", model);
        }

        /// <summary>
        /// Adds the transport supplement cost row.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="isSupplementGridDisplay">The is supplement grid display.</param>
        /// <param name="quotationId">The quotation identifier.</param>
        /// <returns>AddTransportSupplementCostRow</returns>
        public async Task<ActionResult> AddTransportSupplementCostRow(int id, string vehicleId, int isSupplementGridDisplay, Guid quotationId)
        {
            var transportCategory = new List<TransportCategoryViewModel>();

            var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTransportCategory}/transportcategorybyids/{vehicleId}");
            if (apiResponse.Status)
            {
                transportCategory = JsonConvert.DeserializeObject<List<TransportCategoryViewModel>>(apiResponse.Response);
            }

            var model = new QuotationTransportViewModel
            {
                RouteServiceCategoryId = (byte)id,
                Id = Guid.NewGuid(),
                QuotationId = quotationId
            };

            if (model.QuotationTransportCost == null)
            {
                model.QuotationTransportCost = new List<QuotationTransportCostViewModel>();
            }

            foreach (var item in transportCategory)
            {
                var modelCost = new QuotationTransportCostViewModel
                {
                    QuotationTransportId = model.Id,
                    RouteServiceCategoryId = (byte)id,
                    TransportCategoryId = item.Id,
                    TransportCategoryName = item.Name
                };
                model.QuotationTransportCost.Add(modelCost);
                if (id == 4)
                {
                    break;
                }
            }

            var modellst = new List<QuotationTransportViewModel>();
            modellst.Add(model);
            if (id == 2)
            {
                if (isSupplementGridDisplay == 0)
                {
                    return this.PartialView("_QuotationTransportSupplement", modellst);
                }

                if (isSupplementGridDisplay == 1)
                {
                    return this.PartialView("_QuotationTransportSupplementRow", model);
                }
            }
            else
            {
                return this.PartialView("_QuotationTransportRow", model);
            }

            return this.View();
        }

        /// <summary>
        /// Gets the name of the transport category.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="quotationTransport">The LST quotation transport.</param>
        /// <param name="routeServiceId">The route service identifier.</param>
        /// <returns>GetTransportCategoryName</returns>
        public async Task<ActionResult> GetTransportCategoryName(string id, List<QuotationTransportViewModel> quotationTransport, int routeServiceId)
        {
            if (this.IsAjaxRequest())
            {
                var model = new List<TransportCategoryViewModel>();

                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTransportCategory}/transportcategorybyids/{id}");
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<List<TransportCategoryViewModel>>(apiResponse.Response);
                }

                foreach (var transportCategory in model)
                {
                    foreach (var quotationTransportRow in quotationTransport)
                    {
                        if (quotationTransportRow.QuotationTransportCost == null)
                        {
                            quotationTransportRow.QuotationTransportCost = new List<QuotationTransportCostViewModel>();
                        }

                        if (quotationTransportRow.QuotationTransportCost.Where(x => x.TransportCategoryId == transportCategory.Id).Count() == 0)
                        {
                            var modelQuotationCost = new QuotationTransportCostViewModel
                            {
                                Id = Guid.NewGuid(),
                                QuotationTransportId = quotationTransportRow.Id,
                                RouteServiceCategoryId = (byte)routeServiceId,
                                TransportCategoryId = transportCategory.Id,
                                TransportCategoryName = routeServiceId == 3 ? transportCategory.Name : transportCategory.Name
                            };
                            quotationTransportRow.QuotationTransportCost.Add(modelQuotationCost);
                        }
                    }
                }

                foreach (var quotationTransportRow in quotationTransport)
                {
                    var deleteChild = quotationTransportRow.QuotationTransportCost.Where(x => !model.Any(m => m.Id == x.TransportCategoryId)).ToList();
                    foreach (var item in deleteChild)
                    {
                        if (item.RouteServiceCategoryId == routeServiceId)
                        {
                            quotationTransportRow.QuotationTransportCost.Remove(item);
                        }
                    }

                    await this.BindSelectList(quotationTransportRow);
                }

                if (routeServiceId == 1 || routeServiceId == 3)
                {
                    return this.PartialView("_QuotationTransportPackage", quotationTransport);
                }
                else if (routeServiceId == 2)
                {
                    return this.PartialView("_QuotationTransportSupplement", quotationTransport);
                }
            }

            return this.View();
        }

        /// <summary>
        /// Quotations the train routedetail.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dayno">The dayno.</param>
        /// <param name="paxType">Type of the pax.</param>
        /// <returns>QuotationTrainRoutedetail</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationTrainRoutedetail(Guid? id, Guid dayno, byte paxType)
        {
            Guid routeOptionId = dayno;
            if (this.IsAjaxRequest())
            {
                var model = new List<QuotationTrainViewModel>();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationTrain}/quotationtrainroutedetail/{id}/{routeOptionId}/{paxType}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<List<QuotationTrainViewModel>>(apiResponse.Response);
                    }
                }

                this.ViewBag.PaxType = paxType;
                return this.PartialView("_QuotationRouteTrainView", model);
            }

            return this.View();
        }

        /// <summary>
        /// Quotations the trainroutedetail.
        /// </summary>
        /// <param name="quotationTrain">The quotation train.</param>
        /// <returns>QuotationTrainroutedetail</returns>
        [HttpPost]
        public async Task<ActionResult> QuotationTrainroutedetail(List<QuotationTrainViewModel> quotationTrain)
        {
            var viewResponse = new ViewResponse();
            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse = await this.HttpClient.PutAsync($"{Constants.ApiQuotationTrain}/quotationtrainroutedetail", JsonConvert.SerializeObject(quotationTrain));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Trains the cost view.
        /// </summary>
        /// <param name="sourceCityId">The source city identifier.</param>
        /// <param name="destinationCityId">The destination city identifier.</param>
        /// <param name="clickType">Type of the click.</param>
        /// <returns>
        /// model
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> TrainCostView(long sourceCityId, long destinationCityId, string clickType)
        {
            if (this.IsAjaxRequest())
            {
                var model = new List<TrainCostViewsModel>();
                if (sourceCityId > 0)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationTrain}/traincostviewdetail/{sourceCityId}/{destinationCityId}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<List<TrainCostViewsModel>>(apiResponse.Response);
                    }
                }

                return this.PartialView("_QuotationTrainCostView", model);
            }

            return this.View();
        }

        /// <summary>
        /// Quotations the flight routedetail.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dayno">The route option identifier.</param>
        /// <param name="paxType">Type of the pax.</param>
        /// <returns>QuotationFlightRoutedetail</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationFlightRoutedetail(Guid? id, Guid dayno, byte paxType)
        {
            var routeOptionId = dayno;
            if (this.IsAjaxRequest())
            {
                var model = new List<QuotationFlightViewModel>();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationFlight}/quotationflightroutedetail/{id}/{routeOptionId}/{paxType}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<List<QuotationFlightViewModel>>(apiResponse.Response);
                    }
                }

                this.ViewBag.PaxType = paxType;
                return this.PartialView("_QuotationRouteFlightView", model);
            }

            return this.View();
        }

        /// <summary>
        /// Quotations the flightroutedetail.
        /// </summary>
        /// <param name="quotationFlight">The model.</param>
        /// <returns>QuotationFlightroutedetail</returns>
        [HttpPost]
        public async Task<ActionResult> QuotationFlightRoutedetail(List<QuotationFlightViewModel> quotationFlight)
        {
            var viewResponse = new ViewResponse();
            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse = await this.HttpClient.PutAsync($"{Constants.ApiQuotationFlight}/quotationflightroutedetail", JsonConvert.SerializeObject(quotationFlight));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Flights the cost view.
        /// </summary>
        /// <param name="sourceCityId">The source city identifier.</param>
        /// <param name="destinationCityId">The destination city identifier.</param>
        /// <returns>FlightCostView</returns>
        [HttpGet]
        public async Task<ActionResult> FlightCostView(long sourceCityId, long destinationCityId)
        {
            if (this.IsAjaxRequest())
            {
                var model = new List<FlightCostViewsModel>();
                if (sourceCityId > 0)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationFlight}/flightcostviewdetail/{sourceCityId}/{destinationCityId}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<List<FlightCostViewsModel>>(apiResponse.Response);
                    }
                }

                return this.PartialView("_QuotationFlightCostView", model);
            }

            return this.View();
        }

        /// <summary>
        /// Adds the train supplement row.
        /// </summary>
        /// <param name="routeTrainView">The object model.</param>
        /// <returns>model</returns>
        [HttpPost]
        public IActionResult AddTrainSupplementRow(QuotationTrainViewModel routeTrainView)
        {
            routeTrainView.RouteServiceCategoryId = 2;
            return this.PartialView("_QuotationRouteTrainViewRow", routeTrainView);
        }

        /// <summary>
        /// Adds the train upgrade row.
        /// </summary>
        /// <param name="routeTrainView">The route train view.</param>
        /// <returns>routeTrainView</returns>
        [HttpPost]
        public IActionResult AddTrainUpgradeRow(QuotationTrainViewModel routeTrainView)
        {
            routeTrainView.RouteServiceCategoryId = 3;
            return this.PartialView("_QuotationRouteTrainViewRow", routeTrainView);
        }

        /// <summary>
        /// Adds the flight supplement row.
        /// </summary>
        /// <param name="routeFlightView">The route flight view.</param>
        /// <returns>routeFlightView</returns>
        [HttpPost]
        public IActionResult AddFlightSupplementRow(QuotationFlightViewModel routeFlightView)
        {
            routeFlightView.RouteServiceCategoryId = 2;
            return this.PartialView("_QuotationRouteFlightViewRow", routeFlightView);
        }

        /// <summary>
        /// Adds the Flight upgrade row.
        /// </summary>
        /// <param name="routeFlightView">The route Flight view.</param>
        /// <returns>
        /// routeFlightView
        /// </returns>
        [HttpPost]
        public IActionResult AddFlightUpgradeRow(QuotationFlightViewModel routeFlightView)
        {
            routeFlightView.RouteServiceCategoryId = 3;
            return this.PartialView("_QuotationRouteFlightViewRow", routeFlightView);
        }

        /// <summary>
        /// Quotations the summary data.
        /// </summary>
        /// <param name="optionId">The option identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="validFrom">The valid from.</param>
        /// <param name="validTo">The valid to.</param>
        /// <returns>QuotationSummaryData</returns>
        [HttpGet]
        public async Task<ActionResult> QuotationSummaryData(Guid optionId, Guid id, string validFrom, string validTo)
        {
            if (this.IsAjaxRequest())
            {
                var fromdate = Convert.ToDateTime(validFrom);
                var todate = Convert.ToDateTime(validFrom);
                var model = new QuotationSummaryViewModel();
                if (optionId != null && optionId != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationSummary}/quotationsummary/{optionId}/{id}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationSummaryViewModel>(apiResponse.Response);
                        await this.BindSelectListforQuotationSummary(model);
                    }
                }

                this.ViewBag.ValidFrom = fromdate;
                this.ViewBag.ValidTo = todate;
                return this.PartialView("_QuotationSummary", model);
            }

            return this.View();
        }

        /// <summary>
        /// Gets the quotation summary detail data.
        /// </summary>
        /// <param name="quotationId">The quotation identifier.</param>
        /// <param name="quotationSummaryId">The quotation summary identifier.</param>
        /// <param name="quotationHotelOptionId">The quotation hotel option identifier.</param>
        /// <param name="paxCount">The pax count.</param>
        /// <returns>GetQuotationSummaryDetailData</returns>
        [HttpGet]
        public async Task<ActionResult> GetQuotationSummaryDetailData(Guid quotationId, Guid quotationSummaryId, Guid quotationHotelOptionId, int paxCount)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationSummaryDetailViewModel();
                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationSummary}/quotationsummarydetailrow/{quotationId}/{quotationSummaryId}/{quotationHotelOptionId}/{paxCount}");
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<QuotationSummaryDetailViewModel>(apiResponse.Response);
                }

                return this.PartialView("_QuotationSummaryDetail", model);
            }

            return this.View();
        }

        /// <summary>
        /// Quotations the summary data.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>QuotationSummaryData</returns>
        [HttpPost]
        public async Task<ActionResult> QuotationSummaryData(QuotationSummaryViewModel model)
        {
            var viewResponse = new ViewResponse();

            if (this.ModelState.IsValid)
            {
                ApiDataResponse apiResponse;

                apiResponse = await this.HttpClient.PostAsync($"{Constants.ApiQuotationSummary}/quotationsummary", JsonConvert.SerializeObject(model));
                if (apiResponse.Status)
                {
                    viewResponse.Alert.Message = Messages.RecordUpdate;
                    viewResponse.Status = true;
                }
                else
                {
                    viewResponse.Alert.Message = JsonConvert.DeserializeObject<ApiResponse>(apiResponse.Response).Errors.ToString();
                    viewResponse.Alert.Type = Enums.MessageType.Error.ToString().ToLower();
                }
            }

            return this.Json(viewResponse);
        }

        /// <summary>
        /// Gets the service tax data.
        /// </summary>
        /// <param name="taxId">The tax identifier.</param>
        /// <param name="validFrom">The valid from.</param>
        /// <param name="validTo">The valid to.</param>
        /// <returns>GetServiceTaxData</returns>
        [HttpGet]
        public async Task<IActionResult> GetServiceTaxData(long taxId, DateTime validFrom, DateTime validTo)
        {
            if (this.IsAjaxRequest())
            {
                var model = new TaxRateMasterViewModel();
                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTaxRateMaster}/getTaxRateByTaxId/{taxId}/{validFrom.ToString().Replace('/', '-')}/{validTo.ToString().Replace('/', '-')}");
                if (apiResponse.Status)
                {
                    model = JsonConvert.DeserializeObject<TaxRateMasterViewModel>(apiResponse.Response);
                }

                return this.Json(model.TaxRateMasterDetail.Sum(x => x.Rate));
            }

            return null;
        }

        /// <summary>
        /// Gets the quotation supplement view.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="routeOptionId">The route option identifier.</param>
        /// <returns>GetQuotationSupplementAndUpgradeView</returns>
        [HttpGet]
        public async Task<ActionResult> GetQuotationSupplementAndUpgradeView(Guid id, int routeOptionId)
        {
            if (this.IsAjaxRequest())
            {
                var model = new QuotationViewModel();
                if (id != null && id != Guid.Empty)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotation}/quotationrouteoption/{id}/{routeOptionId}");
                    if (apiResponse.Status)
                    {
                        model = JsonConvert.DeserializeObject<QuotationViewModel>(apiResponse.Response);
                    }
                }

                if (routeOptionId == 2)
                {
                    return this.PartialView("_QuotationSupplement", model);
                }

                if (routeOptionId == 3)
                {
                    return this.PartialView("_QuotationUpgrade", model);
                }
            }

            return this.View();
        }

        /// <summary>
        /// Des the activate user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="statusid">The statusid.</param>
        /// <returns>
        /// Chnage Active Sttus
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> RouteStatus(Guid id, byte statusid)
        {
            var viewResponse = new ViewResponse();
            var apiResponse = await this.HttpClient.PutAsync($"{Constants.ApiTourPlanCost}/routestatus/{id}/{statusid}", JsonConvert.SerializeObject(string.Empty));
            if (apiResponse.Status)
            {
                viewResponse.Alert.Message = Constants.StatusChanged;
                viewResponse.Status = true;
            }

            return this.Json(viewResponse);
        }

        private async Task<RouteDetailPlacesViewModel> GetRouteDetailPlaces(long cityid)
        {
            var places = new RouteDetailPlacesViewModel();

            var monumentResponse = await this.HttpClient.GetAsync($"{Constants.ApiMonument}/bycity/{cityid}");
            if (monumentResponse.Status)
            {
                places.Place = JsonConvert.DeserializeObject<List<TempCheckBox>>(monumentResponse.Response);
            }

            var monumentPackageResponse = await this.HttpClient.GetAsync($"{Constants.ApiMonumentPackage}/bycity/{cityid}");
            if (monumentPackageResponse.Status)
            {
                places.Place.AddRange(JsonConvert.DeserializeObject<List<TempCheckBox>>(monumentPackageResponse.Response));
            }

            var excursionResponse = await this.HttpClient.GetAsync($"{Constants.ApiExcursion}/bycity/{cityid}");
            if (excursionResponse.Status)
            {
                places.Excursion = JsonConvert.DeserializeObject<ICollection<TempCheckBox>>(excursionResponse.Response);
            }

            var activityResponse = await this.HttpClient.GetAsync($"{Constants.ApiDayActivity}/bycity/{cityid}");
            if (activityResponse.Status)
            {
                places.Activity = JsonConvert.DeserializeObject<ICollection<TempCheckBox>>(activityResponse.Response);
            }

            var experienceResponse = await this.HttpClient.GetAsync($"{Constants.ApiExperience}/bycity/{cityid}");
            if (experienceResponse.Status)
            {
                places.Experience = JsonConvert.DeserializeObject<ICollection<TempCheckBox>>(experienceResponse.Response);
            }

            return places;
        }

        ////private async Task<List<RouteDetailMonumentViewModel>> GetSightSeen(long cityid, Guid routeDetailId)
        ////{
        ////    var sightseenlist = new List<RouteDetailMonumentViewModel>();
        ////    var monumentResponse = await this.HttpClient.GetAsync($"{Constants.ApiMonument}/bycity/{cityid}");
        ////    if (monumentResponse.Status)
        ////    {
        ////        var monumentList = JsonConvert.DeserializeObject<ICollection<TempCheckBox>>(monumentResponse.Response);

        ////        foreach (var monument in monumentList)
        ////        {
        ////            foreach (var item in Enum.GetValues(typeof(Enums.DayType)).Cast<Enums.DayType>())
        ////            {
        ////                if (item != Enums.DayType.Excursion)
        ////                {
        ////                    sightseenlist.Add(new RouteDetailMonumentViewModel
        ////                    {
        ////                        RouteDetailId = routeDetailId,
        ////                        MonumentId = Guid.Parse(monument.Id),
        ////                        Name = monument.Text,
        ////                        MonumentType = Enums.MonumentType.Monument,
        ////                        DayTypeId = (byte)item
        ////                    });
        ////                }
        ////            }
        ////        }
        ////    }

        ////    var monumentPackageResponse = await this.HttpClient.GetAsync($"{Constants.ApiMonumentPackage}/bycity/{cityid}");
        ////    if (monumentPackageResponse.Status)
        ////    {
        ////        var monumentPacakgeList = JsonConvert.DeserializeObject<ICollection<TempCheckBox>>(monumentPackageResponse.Response);

        ////        foreach (var package in monumentPacakgeList)
        ////        {
        ////            foreach (var item in Enum.GetValues(typeof(Enums.DayType)).Cast<Enums.DayType>())
        ////            {
        ////                if (item != Enums.DayType.Excursion)
        ////                {
        ////                    sightseenlist.Add(new RouteDetailMonumentViewModel
        ////                    {
        ////                        RouteDetailId = routeDetailId,
        ////                        MonumentPackageId = Guid.Parse(package.Id),
        ////                        Name = package.Text,
        ////                        MonumentType = Enums.MonumentType.MonumentPackage,
        ////                        ////IncludedMonuments = package.Additional,
        ////                        DayTypeId = (byte)item
        ////                    });
        ////                }
        ////            }
        ////        }
        ////    }

        ////    var excursionResponse = await this.HttpClient.GetAsync($"{Constants.ApiExcursion}/bycity/{cityid}");
        ////    if (excursionResponse.Status)
        ////    {
        ////        var monumentPacakgeList = JsonConvert.DeserializeObject<ICollection<TempCheckBox>>(excursionResponse.Response);

        ////        foreach (var excursion in monumentPacakgeList)
        ////        {
        ////            sightseenlist.Add(new RouteDetailMonumentViewModel
        ////            {
        ////                RouteDetailId = routeDetailId,
        ////                ExcursionId = Guid.Parse(excursion.Id),
        ////                Name = excursion.Text,
        ////                MonumentType = Enums.MonumentType.Excursion,
        ////                DayTypeId = (byte)Enums.DayType.Excursion,
        ////            });
        ////        }
        ////    }

        ////    return sightseenlist;
        ////}

        private async Task BindSelectList(QuotationViewModel model)
        {
            if (model != null)
            {
                if (model.TransportCategory != null)
                {
                    var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiTransportCategory}/transportcategorybyids/" + string.Join(",", model.TransportCategory));
                    var modelTransportCategory = new List<TransportCategoryViewModel>();
                    if (apiResponse.Status)
                    {
                        modelTransportCategory = JsonConvert.DeserializeObject<List<TransportCategoryViewModel>>(apiResponse.Response);
                    }

                    if (model.TransportCategoryList == null)
                    {
                        model.TransportCategoryList = new List<Dropdown>();
                    }

                    foreach (var item in modelTransportCategory)
                    {
                        model.TransportCategoryList.Add(new Dropdown() { Name = item.Name, Id = item.Id.ToString() });
                    }
                }

                if (model.TransportCategorySupplement != null)
                {
                    var apiSuppResponse = await this.HttpClient.GetAsync($"{Constants.ApiTransportCategory}" + "/transportcategorybyids/" + string.Join(",", model.TransportCategorySupplement));
                    var modelTransportCategorySupplement = new List<TransportCategoryViewModel>();
                    if (apiSuppResponse.Status)
                    {
                        modelTransportCategorySupplement = JsonConvert.DeserializeObject<List<TransportCategoryViewModel>>(apiSuppResponse.Response);
                    }

                    if (model.TransportCategorySupplementList == null)
                    {
                        model.TransportCategorySupplementList = new List<Dropdown>();
                    }

                    foreach (var item in modelTransportCategorySupplement)
                    {
                        model.TransportCategorySupplementList.Add(new Dropdown() { Name = item.Name, Id = item.Id.ToString() });
                    }
                }
            }
        }

        private async Task BindSelectList(QuotationTransportViewModel model)
        {
            if (model != null)
            {
                var cityFromList = await this.GetSelectList("master/citieslist", model.CityFromId);
                var cityToList = await this.GetSelectList("master/citieslist", model.CityToId);
                var transportTypeList = await this.GetSelectList("master/transporttypelist", model.TransportTypeId);
                var supplierList = await this.GetSelectList("master/supplierlist", model.SupplierId);
                var transportNameList = await this.GetSelectList("master/transportcontractnamelist", model.TransportContractDetailId);

                if (cityFromList != null && cityFromList.Count() > 0)
                {
                    model.CityFromList = SetSmartCombo.Option(model.CityFromId.ToString(), cityFromList[0].Text.ToString());
                }

                if (cityToList != null && cityToList.Count() > 0)
                {
                    model.CityToList = SetSmartCombo.Option(model.CityToId.ToString(), cityToList[0].Text.ToString());
                }

                if (transportTypeList != null && transportTypeList.Count() > 0)
                {
                    model.TransportTypeList = SetSmartCombo.Option(model.TransportTypeId.ToString(), transportTypeList[0].Text.ToString());
                }

                if (supplierList != null && supplierList.Count() > 0)
                {
                    model.SupplierList = SetSmartCombo.Option(model.SupplierId.ToString(), supplierList[0].Text.ToString());
                }

                if (transportNameList != null && transportNameList.Count() > 0)
                {
                    model.TransportNameList = SetSmartCombo.Option(model.TransportContractDetailId.ToString(), transportNameList[0].Text.ToString());
                }
            }
        }

        private async Task BindSelectListforQuotationSummary(QuotationSummaryViewModel model)
        {
            if (model != null)
            {
                var apiResponse = await this.HttpClient.GetAsync($"{Constants.ApiQuotationTransport}/quotationtransportcategory?id=" + model.QuotationId);
                var modelTransportCategory = new List<TransportCategoryViewModel>();
                if (apiResponse.Status)
                {
                    modelTransportCategory = JsonConvert.DeserializeObject<List<TransportCategoryViewModel>>(apiResponse.Response);
                }

                foreach (var item in model.QuotationSummaryDetail)
                {
                    if (item.TransportCategoryList == null)
                    {
                        item.TransportCategoryList = new List<Dropdown>();
                    }

                    foreach (var vehicle in modelTransportCategory)
                    {
                        item.TransportCategoryList.Add(new Dropdown() { Name = vehicle.Name, Id = vehicle.Id.ToString() });
                    }
                }
            }
        }

        //// ------------------------**************************************** New Tour Plan Cost Start------------------------------//
    }
}