// <copyright file="SharedController.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Newtonsoft.Json;
    using OfficeOpenXml;
    using TravelMint.Service;
    using TravelMint.Service.CrossCutting;
    using TravelMint.UI.Framework.Excel;
    using TravelMint.UI.ViewModels;
    using TravelMint.UI.ViewModels.Excel;

    /// <summary>
    /// SharedController
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Authorize]
    public class SharedController : Controller
    {
        private readonly IHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedController"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        public SharedController(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        /// <value>
        /// The name of the controller.
        /// </value>
        protected string ControllerName => this.ControllerContext.ActionDescriptor.ControllerName.ToLower();

        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        /// <value>
        /// The authentication token.
        /// </value>
        protected string AuthenticationToken => this.User.GetAuthenticationToken();

        /// <summary>
        /// Gets the login identifier.
        /// </summary>
        /// <value>
        /// The login identifier.
        /// </value>
        protected Guid RoleId => this.User.GetRoleId();

        /// <summary>
        /// Gets the login identifier.
        /// </summary>
        /// <value>
        /// The login identifier.
        /// </value>
        protected Guid LoginId => this.User.GetLoginId();

        /// <summary>
        /// Gets the login identifier.
        /// </summary>
        /// <value>
        /// The login identifier.
        /// </value>
        protected Guid SubscriberId => this.User.GetSubscriberId();

        /// <summary>
        /// Gets the company identifier.
        /// </summary>
        /// <value>
        /// The company identifier.
        /// </value>
        protected Guid CompanyId => this.User.GetCompanyId();

        /// <summary>
        /// Gets the json ignore nullable.
        /// </summary>
        /// <value>
        /// The json ignore nullable.
        /// </value>
        protected JsonSerializerSettings JsonIgnoreNullable => new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <value>
        /// The HTTP client.
        /// </value>
        protected IHttpClient HttpClient => this.httpClient;

        /// <summary>
        /// Gets the language by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Get Language By Id
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetLanguageById(int id)
        {
            var viewModel = new LanguageViewModel();

            var apiResponse = await this.HttpClient.GetAsync("master/language", id);
            if (apiResponse.Status)
            {
                viewModel = JsonConvert.DeserializeObject<LanguageViewModel>(apiResponse.Response);
            }

            return this.Json(viewModel ?? new LanguageViewModel());
        }

        /// <summary>
        /// Determines whether [is ajax request].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is ajax request]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAjaxRequest()
        {
            return this.HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest";
        }

        /// <summary>
        /// Gets the model state errors.
        /// </summary>
        /// <returns>Get Model States</returns>
        public List<ModelStateEntry> GetModelStateErrors()
        {
            return this.ModelState.Values.Where(x => x.Errors.Any()).ToList();
        }

        /// <summary>
        /// Views the location.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="areaName">Name of the area.</param>
        /// <param name="controller">The controller.</param>
        /// <returns>
        /// Get View In Current Folder or SubDirecotry  view
        /// </returns>
        public string ViewLocation(string viewName, string areaName = "", string controller = "")
        {
            if (string.IsNullOrEmpty(controller))
            {
                controller = this.ControllerName;
            }

            return (!string.IsNullOrEmpty(areaName)
                     ? $"/Areas/{areaName}/Views/{controller}/{viewName}.cshtml"
                    : $"/Views/{controller}/{viewName}.cshtml").Replace("//", "/");
        }

        /// <summary>
        /// Called after the action method is invoked.
        /// </summary>
        /// <param name="context">The action executed context.</param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (this.IsAjaxRequest())
            {
                this.ViewBag.Layout = false;
                var url = this.Request.Headers["Referer"].ToString().ToLower();
                var iscompany = false;
                if (url.IndexOf("/administrator/company") > 0)
                {
                    iscompany = true;
                }

                if (this.LoginId != Guid.Empty && this.CompanyId == Guid.Empty && (this.ControllerName != "company" && !iscompany))
                {
                    context.Result = new BadRequestObjectResult(error: JsonConvert.SerializeObject(new Dictionary<string, string> { { "url", $"/{Constants.RouteAreaAdministrator}/{Constants.ApiCompany}" }, { "msg", "First choose your company." }, { "statuscode", "405" } }));
                }
                else if (this.LoginId == Guid.Empty)
                {
                    context.Result = new BadRequestObjectResult(error: JsonConvert.SerializeObject(new Dictionary<string, string> { { "url", $"/{Constants.RouteAreaAdministrator}/{Constants.ApiCompany}" }, { "msg", "Your session time out." }, { "statuscode", "401" } }));
                }
            }

            base.OnActionExecuted(context);
        }

        /// <summary>
        /// Gets the dropdown list.
        /// </summary>
        /// <param name="apicontroller">The apicontroller.</param>
        /// <param name="apiAction">The action.</param>
        /// <returns>Get Dropdown List Async</returns>
        public async Task<IActionResult> GetDropdownList(string apicontroller, string apiAction)
        {
            var apiResponse = await this.HttpClient.GetAsync($"{apicontroller}/{apiAction}");
            dynamic result = JsonConvert.DeserializeObject<dynamic>(apiResponse.Response);
            return this.Json(result);
        }

        /// <summary>
        /// Gets the select2 list.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// json list
        /// </returns>
        public async Task<IActionResult> GetSelect2List(string url, DropdownSearchOption options)
        {
            var apiResponse = await this.HttpClient.GetAsync($"{url}?search={options.Search}&page={options.Page}&DependendControlId={options.DependendControlId}{options.DependendOtherControlId}");
            dynamic result = JsonConvert.DeserializeObject<dynamic>(apiResponse.Response);
            return this.Json(result);
        }

        /// <summary>
        /// Gets the data table.
        /// </summary>
        /// <param name="api">The API controller.</param>
        /// <returns>Get DataTable Grid</returns>
        [HttpPost]
        public async Task<IActionResult> GetDataTable(string api)
        {
            var model = this.MapParameterModel(this.HttpContext);
            var setting = new JsonSerializerSettings()
            {
                Culture = CultureInfo.CurrentCulture,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateFormatString = "dd/MM/yyyy"
            };

            var dataTableResult = new DataTableResult();

            var url = api.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Count() > 1 ? api : api + "/datatable";

            var apiResponse = await this.HttpClient.GetPaggedListAsync(url, JsonConvert.SerializeObject(model));
            if (apiResponse.Status)
            {
                dataTableResult = JsonConvert.DeserializeObject<DataTableResult>(apiResponse.Response);
            }

            return this.Json(dataTableResult, setting);
        }

        /// <summary>
        /// Exports to excel.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <returns>
        /// Export To ExcelSheet
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> ExportToExcel(string api)
        {
            var model = this.MapParameterModel(this.HttpContext);
            var setting = new JsonSerializerSettings()
            {
                Culture = CultureInfo.CurrentCulture,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateFormatString = "dd/MM/yyyy"
            };

            var dataTableResult = new DataTableResult();

            var url = api.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Count() > 1 ? api : api + "/datatable";

            var apiResponse = await this.HttpClient.GetPaggedListAsync(url, JsonConvert.SerializeObject(model));
            if (apiResponse.Status)
            {
                dataTableResult = JsonConvert.DeserializeObject<DataTableResult>(apiResponse.Response);
            }

            return this.Json(dataTableResult, setting);

            ////var requestedForm = this.HttpContext.Request.Form;
            ////string parameters = requestedForm["Parameters"][0];
            ////DataTableParameter dataTableParameters = JsonConvert.DeserializeObject<DataTableParameter>(
            ////    parameters, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
            ////dataTableParameters.Length = -1;

            ////var result = await this.DataTableFilterResponse(dataTableParameters);

            ////List<ExportGridHeaders> headeKey = new List<ExportGridHeaders>();
            ////foreach (var item in dataTableParameters.Columns.Where(x => x.Visible))
            ////{
            ////    if (!string.IsNullOrEmpty(item.Data))
            ////    {
            ////        headeKey.Add(new ExportGridHeaders { Title = item.Title, Key = item.Data });
            ////    }
            ////}

            ////Newtonsoft.Json.Linq.JArray response = result["data"];

            ////if (dataTableParameters.Route.ToLower().Contains("invoice/" + Constants.RetriveGrid) || dataTableParameters.Route.ToLower().Contains("order/" + Constants.RetriveGrid))
            ////{
            ////    foreach (JObject jObject in response)
            ////    {
            ////        if (jObject["IsLocal"] != null)
            ////        {
            ////            jObject["IsLocal"] = (bool)(jObject["IsLocal"] as JValue).Value ? Constants.ValueDomestic : Constants.ValueExport;
            ////        }
            ////    }
            ////}

            ////var excelResponse = ExportGrid.WriteHtmlTable(headeKey, response);
            ////var bytes = Encoding.UTF8.GetBytes(excelResponse);
            ////string fileName = dataTableParameters.ControllerName + "_" + Convert.ToString(DateTime.Now.Ticks) + ".xls";
            ////return this.File(bytes, "application/vnd.ms-excel", fileName);
        }

        /// <summary>
        /// Froms the excel.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="excelfile">The excelfile.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="cellExistin">The cell existin.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>FromExcel</returns>
        public dynamic FromExcel<TEntity>(System.IO.FileInfo excelfile, string sheetName, Dictionary<string, IEnumerable<string>> cellExistin = null, int startRow = 2)
          where TEntity : class
        {
            if (cellExistin == null)
            {
                cellExistin = new Dictionary<string, IEnumerable<string>>();
            }

            var excelSheet = new ExcelSheet<TEntity>();
            excelSheet.Name = sheetName;

            using (ExcelPackage package = new ExcelPackage(excelfile))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    excelSheet.Status = false;
                    excelSheet.Message = $"{sheetName} not Found in Excel File.";
                    return excelSheet;
                }

                ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name.ToLower().Trim() == sheetName.ToLower().Trim());
                if (workSheet != null)
                {
                    startRow = workSheet.Row(1).RowEmpty(workSheet) ? startRow - 1 : startRow;

                    int rowindex = startRow == 0 ? 1 : startRow;

                    excelSheet.Records = cellExistin.Count > 0 ? workSheet.ToExcelSheet<TEntity>(cellExistin, rowindex) : workSheet.ToExcelSheet<TEntity>(rowindex);
                    excelSheet.Name = workSheet.Name;
                    excelSheet.Status = excelSheet.Records.Length > 0;
                    excelSheet.Message = excelSheet.Records.Length > 0 ? string.Empty : $"No Data Found in {sheetName}.";
                    return excelSheet;
                }
                else
                {
                    excelSheet.Status = false;
                    excelSheet.Message = $"{sheetName} not Found in Excel File.";
                    return excelSheet;
                }
            }
        }

        /// <summary>
        /// Froms the excel.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityDetail">The type of the entity detail.</typeparam>
        /// <param name="excelfile">The excelfile.</param>
        /// <param name="sheetNames">The sheet names.</param>
        /// <param name="cellExistin">The cell existin.</param>
        /// <param name="startRows">The start rows.</param>
        /// <returns>FromExcel</returns>
        public dynamic FromExcel<TEntity, TEntityDetail>(System.IO.FileInfo excelfile, string[] sheetNames, Dictionary<string, IEnumerable<string>> cellExistin = null, int[] startRows = null)
             where TEntity : class
            where TEntityDetail : class
        {
            if (startRows == null)
            {
                startRows = new int[] { 3, 3 };
            }

            if (cellExistin == null)
            {
                cellExistin = new Dictionary<string, IEnumerable<string>>();
            }

            var excelSheets = new ExcelSheets<TEntity, TEntityDetail>();

            using (ExcelPackage package = new ExcelPackage(excelfile))
            {
                var excelSheet = new ExcelSheet<TEntity>();
                excelSheet.Name = sheetNames[0];

                if (package.Workbook.Worksheets.Count == 0)
                {
                    excelSheet.Status = false;
                    excelSheet.Message = $"{sheetNames[0]} not Found in Excel File.";
                    return excelSheets;
                }

                ExcelWorksheet workSheet1 = package.Workbook.Worksheets.FirstOrDefault(x => x.Name.ToLower().Trim() == sheetNames[0].ToLower().Trim());
                ExcelWorksheet workSheet2;
                if (package.Workbook.Worksheets.Where(x => x.Name.ToLower().Trim() == sheetNames[1].ToLower().Trim()).Count() > 0)
                {
                    excelSheets.IsSingleSheet = false;
                    workSheet2 = package.Workbook.Worksheets.FirstOrDefault(x => x.Name.ToLower().Trim() == sheetNames[1].ToLower().Trim());
                }
                else
                {
                    startRows[1] = startRows[0];
                    excelSheets.IsSingleSheet = true;
                    workSheet2 = workSheet1;
                }

                excelSheets.WorkSheet1 = this.FromExcelSheet<TEntity>(workSheet1, sheetNames[0], cellExistin, startRows[0]);
                excelSheets.WorkSheet2 = this.FromExcelSheet<TEntityDetail>(workSheet2, sheetNames[1], cellExistin, startRows[1]);
                return excelSheets;
            }
        }

        /// <summary>
        /// To the excel.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityDetail">The type of the entity detail.</typeparam>
        /// <param name="excelfile">The excelfile.</param>
        /// <param name="entityLists">The entity lists.</param>
        /// <param name="sheetNames">The sheet names.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>ToExcel </returns>
        public byte[] ToExcel<TEntity, TEntityDetail>(System.IO.FileInfo excelfile, dynamic[] entityLists, string[] sheetNames, int[] startRow)
            where TEntity : class
            where TEntityDetail : class
        {
            using (ExcelPackage package = new ExcelPackage(excelfile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name.ToLower().Trim() == sheetNames[0].ToLower().Trim());
                ExcelWorksheet worksheetDetails = package.Workbook.Worksheets.FirstOrDefault(x => x.Name.ToLower().Trim() == sheetNames[1].ToLower().Trim());
                worksheet = worksheet.ToEntityList<TEntity>(entityLists[0] as List<TEntity>, startRow[0]);
                worksheetDetails = worksheetDetails.ToEntityList<TEntityDetail>(entityLists[1] as List<TEntityDetail>, startRow[1]);
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// To the excel.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="excelfile">The excelfile.</param>
        /// <param name="entityLists">The entity lists.</param>
        /// <param name="sheetNames">The sheet names.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>ToExcel</returns>
        public byte[] ToExcel<TEntity>(System.IO.FileInfo excelfile, List<TEntity> entityLists, string sheetNames, int startRow)
           where TEntity : class
        {
            using (ExcelPackage package = new ExcelPackage(excelfile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name.ToLower().Trim() == sheetNames.ToLower().Trim());
                worksheet = worksheet.ToEntityList<TEntity>(entityLists as List<TEntity>, startRow);
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        /// <param name="roleid">The roleid.</param>
        /// <returns>Get Role Name By ROleId </returns>
        protected async Task<string> GetRoleName(Guid roleid)
        {
            var roleName = string.Empty;
            var roleApiResponse = await this.HttpClient.GetAsync($"{Constants.ApiRoleManagement}/rolename", roleid);
            if (roleApiResponse.Status)
            {
                roleName = roleApiResponse.Response;
            }

            return roleName;
        }

        /// <summary>
        /// Gets the select list.
        /// </summary>
        /// <param name="apiUrl">The API URL.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="disablePagging">The disable pagging.</param>
        /// <returns>
        /// Get Select List Item
        /// </returns>
        protected async Task<List<SelectListItem>> GetSelectList(string apiUrl, object id, bool disablePagging = false)
        {
            var apiResponse = await this.HttpClient.GetSelectListAsync($"{apiUrl}{(disablePagging ? "?pagging=false" : string.Empty)}", id);
            if (apiResponse.Status)
            {
                var item = JsonConvert.DeserializeObject<dynamic>(apiResponse.Response)["results"];
                var result = JsonConvert.DeserializeObject<List<SelectListItem>>(item.ToString());
                return result;
            }

            return new List<SelectListItem>();
        }

        /// <summary>
        /// Gets the dropdown lists.
        /// </summary>
        /// <param name="apiUrl">The API URL.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="disablePagging">if set to <c>true</c> [disable pagging].</param>
        /// <returns>GetDropdownLists</returns>
        protected async Task<List<Dropdown>> GetDropdownLists(string apiUrl, object id, bool disablePagging = false)
        {
            var apiResponse = await this.HttpClient.GetSelectListAsync($"{apiUrl}{(disablePagging ? "?pagging=false" : string.Empty)}", id);
            if (apiResponse.Status)
            {
                var item = JsonConvert.DeserializeObject<dynamic>(apiResponse.Response)["results"];
                var result = JsonConvert.DeserializeObject<List<Dropdown>>(item.ToString());
                return result;
            }

            return new List<Dropdown>();
        }

        /// <summary>
        /// Updates the claims.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <returns>Update Claims</returns>
        protected async Task UpdateClaims(Claim[] claims)
        {
            if (claims != null && claims.Count() > 0)
            {
                var update = false;

                var identity = this.User.Identity as ClaimsIdentity;
                if (identity.Claims.Count() > 0)
                {
                    foreach (var claim in claims)
                    {
                        var getClaim = identity.Claims.FirstOrDefault(x => x.Type == claim.Type);
                        if (getClaim != null)
                        {
                            identity.RemoveClaim(getClaim);
                            identity.AddClaim(claim);
                            update = update == false ? true : update;
                        }
                    }
                }

                if (update)
                {
                    await this.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));
                }
            }
        }

        /// <summary>
        /// Gets the select list for value.
        /// </summary>
        /// <param name="apiUrl">The API URL.</param>
        /// <param name="value">The value.</param>
        /// <param name="disablePagging">if set to <c>true</c> [disable pagging].</param>
        /// <returns>GetSelectListForValue</returns>
        protected async Task<List<TempCheckBox>> GetSelectListForValue(string apiUrl, object value, bool disablePagging = false)
        {
            var apiResponse = await this.HttpClient.GetSelectListAsync($"{apiUrl}{(disablePagging ? "?pagging=false" : string.Empty)}", value);
            if (apiResponse.Status)
            {
                var item = JsonConvert.DeserializeObject<dynamic>(apiResponse.Response)["results"];
                var result = JsonConvert.DeserializeObject<List<TempCheckBox>>(item.ToString());
                return result;
            }

            return new List<TempCheckBox>();
        }

        /// <summary>
        /// Gets the widgets.
        /// </summary>
        /// <returns>Get Logged in User Widgets</returns>
        protected async Task<List<ViewModels.Widgets.Widget>> GetWidgets()
        {
            var widgets = new List<ViewModels.Widgets.Widget>();

            if (!this.User.IsSystemAdmin())
            {
                var apiResponse = await this.HttpClient.GetAsync($"/{Constants.ApiWidget}/{Constants.ApiUserWidgets}?roleid={this.RoleId}");
                if (apiResponse.Status)
                {
                    widgets = JsonConvert.DeserializeObject<List<ViewModels.Widgets.Widget>>(apiResponse.Response);

                    if (widgets != null)
                    {
                        widgets.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x.Style) &&
                            x.Style.TrimStart().StartsWith('{') && x.Style.TrimEnd().EndsWith('}'))
                            {
                                x.WidgetStyle = JsonConvert.DeserializeObject<ViewModels.Widgets.Styles>(x.Style);
                            }
                        });
                    }
                }
            }

            return widgets ?? new List<ViewModels.Widgets.Widget>();
        }

        private DataTableParameter MapParameterModel(HttpContext httpContext)
        {
            var request = httpContext.Request.Form;

            int draw = Convert.ToInt32(request["draw"]);
            int start = Convert.ToInt32(request["start"]);
            int length = Convert.ToInt32(request["length"]);

            var search = new DataTableSearch
            {
                Value = request["search[value]"],
                Regex = Convert.ToBoolean(request["search[regex]"])
            };

            var o = 0;
            var order = new List<DataTableOrder>();

            while (request["order[" + o + "][column]"].Count > 0)
            {
                order.Add(new DataTableOrder()
                {
                    Column = Convert.ToInt32(request["order[0][column]"].ToList()[0]),
                    Dir = request["order[" + o + "][dir]"]
                });
                o++;
            }

            // Columns
            var c = 0;
            var columns = new List<DataTableColumn>();
            while (request["columns[" + c + "][name]"].Count > 0)
            {
                columns.Add(new DataTableColumn
                {
                    Data = request["columns[" + c + "][data]"][0],
                    Name = request["columns[" + c + "][name]"][0],
                    Orderable = Convert.ToBoolean(request["columns[" + c + "][orderable]"][0]),
                    Search = new DataTableSearch
                    {
                        Value = request["columns[" + c + "][search][value]"][0],
                        Regex = Convert.ToBoolean(request["columns[" + c + "][search][regex]"][0])
                    }
                });
                c++;
            }

            var mapData = new DataTableParameter
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = search,
                Order = order,
                Columns = columns,
            };

            return mapData;
        }

        private ExcelSheet<TEntity> FromExcelSheet<TEntity>(ExcelWorksheet workSheet, string sheetName, Dictionary<string, IEnumerable<string>> cellExistin, int startRow)
          where TEntity : class
        {
            var excelSheet = new ExcelSheet<TEntity>();
            excelSheet.Name = sheetName;
            if (workSheet != null)
            {
                startRow = workSheet.Row(1).RowEmpty(workSheet) ? startRow - 1 : startRow;
                int rowindex = startRow == 0 ? 1 : startRow;
                excelSheet.Records = cellExistin.Count > 0 ? workSheet.ToExcelSheet<TEntity>(cellExistin, rowindex) : workSheet.ToExcelSheet<TEntity>(rowindex);
                excelSheet.Status = excelSheet.Records.Length > 0;
                excelSheet.Message = excelSheet.Records.Length > 0 ? string.Empty : $"No Data Found in {sheetName}.";
                return excelSheet;
            }
            else
            {
                excelSheet.Status = false;
                excelSheet.Message = $"{sheetName} not Found in Excel File.";
                return excelSheet;
            }
        }
    }
}