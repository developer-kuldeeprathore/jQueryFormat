// <copyright file="ExcelController.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using TravelMint.Service.CrossCutting;
    using TravelMint.UI.ViewModels;

    /// <summary>
    /// ExcelController
    /// </summary>
    /// <seealso cref="TravelMint.UI.SharedController" />
    public class ExcelController : SharedController
    {
        private IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelController" /> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        public ExcelController(IHttpClient httpClient, IHostingEnvironment hostingEnvironment)
            : base(httpClient)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Imports this instance.
        /// </summary>
        /// <returns>import view</returns>
        public PartialViewResult Import()
        {
            return this.PartialView("Import");
        }

        /// <summary>
        /// Downloads the template.
        /// </summary>
        /// <param name="excelPath">The excel path.</param>
        /// <returns>DownloadTemplate</returns>
        public IActionResult DownloadTemplate(string excelPath)
        {
            var filePath = Path.Combine(this.hostingEnvironment.WebRootPath + "/" + excelPath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                byte[] bytes = System.IO.File.ReadAllBytes(Path.Combine(filePath));
                return this.File(bytes, contentType, Path.GetFileName(filePath));
            }
            else
            {
                return this.Content("File doesn't exist.");
            }
        }
    }
}