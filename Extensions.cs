// <copyright file="Extensions.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Routing;
    using TravelMint.Service.CrossCutting;

    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// To the select list.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        /// <returns>ToPagged List for Combo box - for use of Select 2 Instance</returns>
        public static dynamic ToPaggedList<TEntity>(this IEnumerable<TEntity> entities)
            where TEntity : class
        {
            if (entities == null)
            {
                entities = new List<TEntity>();
            }

            var enumerable = entities as IList<TEntity> ?? entities.ToList();
            return new
            {
                results = enumerable.ToList(),
                pagination = new { more = enumerable.Count >= Constants.ComboPaginationSize }
            };
        }

        /// <summary>
        /// To the select list.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        /// <returns>ToPagged List for Combo box - for use of Select 2 Instance</returns>
        public static List<SelectListItem> ToSelectList<TEntity>(this List<TEntity> entities)
            where TEntity : Dropdown
        {
            return entities?.Select(x => new SelectListItem() { Value = x.Id, Text = x.Name }).ToList() ?? new List<SelectListItem>();
        }

        /////// <summary>
        /////// To the amount.
        /////// </summary>
        /////// <param name="amount">The amount.</param>
        /////// <returns>ToAmount</returns>
        ////public static string ToAmount(this decimal amount)
        ////{
        ////    return amount.ToString("0.00").Replace(".00", string.Empty);
        ////}

        /// <summary>
        /// To the amount.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="upperRoundUp">if set to <c>true</c> [upper round up].</param>
        /// <returns>
        /// Convert Value to Formatted Value
        /// </returns>
        public static string ToAmount(this object amount, bool upperRoundUp = false)
        {
            decimal value = 0;

            decimal.TryParse(Convert.ToString(amount), out value);

            if (upperRoundUp)
            {
                value = Math.Ceiling(value);
            }

            return value.ToString("0.00").Replace(".00", string.Empty);
        }

        /// <summary>
        /// To the empty.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Zero Replace by string .emtpy</returns>
        public static string ToEmpty(this object value)
        {
            decimal.TryParse(Convert.ToString(value), out decimal input);

            return input == 0 ? string.Empty : input.ToString().Replace(".00", string.Empty);
        }

        /// <summary>
        /// Validates the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>ValidateUrl</returns>
        public static string ToUrl(this string url)
        {
            return (url ?? string.Empty).Replace("//", "/").Replace("~", string.Empty);
        }

        /// <summary>
        /// Gets the admin URL.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// GetAdminUrl
        /// </returns>
        public static string GetCompanyRouteUrl(this IUrlHelper urlHelper, string controllerName, string actionName, string id = "")
        {
            return urlHelper.RouteUrl(Constants.RouteAreaDefault, new { controller = controllerName, action = actionName, id });
        }

        /// <summary>
        /// Gets the system admin route URL.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>GetSystemAdminRouteUrl</returns>
        public static string GetAdministratorRouteUrl(this IUrlHelper urlHelper, string controllerName, string actionName, string id = "")
        {
            return urlHelper.RouteUrl(Constants.RouteAreaDefault, new { area = Constants.RouteAreaAdministrator, controller = controllerName, action = actionName, id });
        }

        /// <summary>
        /// Gets the route by parent.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="parentid">The parentid.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The action.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="next">if set to <c>true</c> [next].</param>
        /// <returns>GetRouteByParent</returns>
        public static string GetRouteByParent(this IUrlHelper urlHelper, Guid parentid, string controller, string action, string id = "", bool next = false)
        {
            return next
                ? urlHelper.RouteUrl(Constants.RouteByParentIdWithSteps, new { controller = controller, action = action, parentid = parentid, id, next = Convert.ToInt16(next) })
                : urlHelper.RouteUrl(Constants.RouteAreaDefaultWithParentId, new { controller = controller, action = action, parentid = parentid, id });
        }

        /// <summary>
        /// Gets the default route by parent.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="parentid">The parentid.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The action.</param>
        /// <returns>GetDefaultRouteByParent</returns>
        public static string GetDefaultRouteByParent(this IUrlHelper urlHelper, Guid parentid, string controller, string action)
        {
            return urlHelper.RouteUrl(Constants.RouteByParentId, new { controller = controller, action = action, parentid = parentid });
        }

        /// <summary>
        /// Gets the sales route URL.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>GetSalesRouteUrl</returns>
        public static string GetSalesRouteUrl(this IUrlHelper urlHelper, string controllerName, string actionName, string id = "")
        {
            return urlHelper.RouteUrl(Constants.RouteAreaDefault, new { controller = controllerName.ToLower(), action = actionName, id });
        }

        /////// <summary>
        /////// Gets the admin URL.
        /////// </summary>
        /////// <param name="urlHelper">The URL helper.</param>
        /////// <param name="subscriberid">The subscriberid.</param>
        /////// <param name="controller">The controller.</param>
        /////// <param name="action">The action.</param>
        /////// <returns>
        /////// GetAdminUrl
        /////// </returns>
        ////public static string GetCompanyUrl(this IUrlHelper urlHelper, string subscriberid, string controller, string action)
        ////{
        ////    return urlHelper.RouteUrl(Constants.RouteArea, new { subscriberid = subscriberid, controller = controller, action = action, area = Constants.AreaAdmin });
        ////}

        /////// <summary>
        /////// Gets the select list URL.
        /////// </summary>
        /////// <param name="urlHelper">The URL helper.</param>
        /////// <param name="apiController">The API controller.</param>
        /////// <param name="apiAction">The API action.</param>
        /////// <returns>Get Url For Selectlist from API</returns>
        ////public static string GetSelectListUrl(this IUrlHelper urlHelper, string apiController, string apiAction)
        ////{
        ////    return urlHelper.RouteUrl(Constants.RouteArea, new { controller = "shared", action = "getdropdownlist", area = Constants.AreaAdmin, apicontroller = apiController, apiAction = apiAction });
        ////}

        /// <summary>
        /// Gets the API URL.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="url">The URL.</param>
        /// <returns>GetApiUrl</returns>
        public static string GetApiUrl(this IUrlHelper urlHelper, string url)
        {
            return $"{DomainSetting.ApplicationName}";
        }

        /// <summary>
        /// Enums the select list.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="htmlUrlHelper">The HTML URL helper.</param>
        /// <param name="isTextDisplayName">Show Display Attribute instance of Text</param>
        /// <param name="isValueInteger">if set to <c>true</c> [is value integer].</param>
        /// <returns>
        /// EnumSelectList
        /// </returns>
        public static List<SelectListItem> EnumSelectList<TEnum>(this IHtmlHelper htmlUrlHelper, bool isTextDisplayName = false, bool isValueInteger = true)
        {
            var type = typeof(TEnum);
            List<SelectListItem> enumList = new List<SelectListItem>();
            foreach (TEnum data in Enum.GetValues(typeof(TEnum)))
            {
                var items = Enum.GetValues(typeof(TEnum)).Cast<Enum>().Where(x => x.ToString() == data.ToString());
                foreach (var item in items)
                {
                    enumList.Add(new SelectListItem()
                    {
                        Text = isTextDisplayName ? item.GetDisplayName() : item.ToString(),
                        Value = (isValueInteger ? ((int)Enum.Parse(typeof(TEnum), item.ToString())).ToString() : item.ToString()).Replace("_", string.Empty)
                    });
                }
            }

            return enumList;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Get Display Name Attribute Value</returns>
        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            var members = type.GetMember(value.ToString());
            if (members.Length == 0)
            {
                return string.Empty;
            }

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault();
            return attributes == null ? string.Empty : ((DisplayNameAttribute)attributes).DisplayName;
        }

        /// <summary>
        /// Determines whether [is next step].
        /// </summary>
        /// <param name="routeData">The route data.</param>
        /// <returns>
        ///   <c>true</c> if [is next step] [the specified route data]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNextStep(this RouteData routeData)
        {
            bool steps = false;
            if (routeData != null && routeData.Values["next"] != null)
            {
                int.TryParse(routeData.Values["next"].ToString(), out int nextCount);
                return nextCount == 1;
            }

            return steps;
        }

        /// <summary>
        /// Overlappingses the specified meetings.
        /// </summary>
        /// <param name="meetings">The meetings.</param>
        /// <returns>Overlappings</returns>
        public static IEnumerable<DateRange[]> Overlappings(this IEnumerable<DateRange> meetings)
        {
            var first = (DateRange)null;
            var orderByDates = meetings.OrderBy(m => m.Start);
            var checkedDates = new List<DateRange>();

            foreach (var range in orderByDates)
            {
                if (first != null)
                {
                    checkedDates.Add(first);

                    foreach (var uncheckedDate in orderByDates.Where(x => (x.Start >= first.Start && !(x == first)) && !checkedDates.Any(m => m == x)))
                    {
                        if (first.OverlapsWith(uncheckedDate))
                        {
                            yield return new[] { first, uncheckedDate };
                        }
                    }
                }

                first = range;
            }
        }
    }
}