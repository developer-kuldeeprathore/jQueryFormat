// <copyright file="UploadController.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Controllers
{
    using System.Globalization;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using TravelMint.Service.CrossCutting;

    /// <summary>
    /// Upload Controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class UploadController : Controller
    {
        private IHostingEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadController"/> class.
        /// </summary>
        /// <param name="environment">The evn.</param>
        public UploadController(IHostingEnvironment environment)
        {
            this.environment = environment;
        }

        /// <summary>
        /// Files this instance.
        /// </summary>
        /// <returns>Json Result</returns>
        [HttpPost]
        public JsonResult File()
        {
            string filePath = string.Empty;
            foreach (var file in this.Request.Form.Files)
            {
                var fileDataContent = file;
                if (fileDataContent != null && fileDataContent.Length > 0)
                {
                    var stream = fileDataContent.OpenReadStream();

                    var fileName = fileDataContent.FileName;
                    var storagePath = Path.Combine(this.environment.ContentRootPath, "wwwroot", Constants.Storage.TrimStart('/'));
                    filePath = Path.Combine(storagePath, fileName);
                    if (!Directory.Exists(storagePath))
                    {
                        Directory.CreateDirectory(storagePath);
                    }

                    try
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        using (var fileStream = System.IO.File.Create(filePath))
                        {
                            stream.CopyTo(fileStream);
                        }

                        Utility utility = new Utility();
                        utility.MergeFile(filePath);
                    }
                    catch (IOException ex)
                    {
                        return this.Json(new { success = false, data = ex.Message }, new Newtonsoft.Json.JsonSerializerSettings { Culture = CultureInfo.CurrentCulture });
                    }
                }
            }

            return this.Json(new { success = true, data = filePath }, new Newtonsoft.Json.JsonSerializerSettings { Culture = CultureInfo.CurrentCulture });
        }
    }
}