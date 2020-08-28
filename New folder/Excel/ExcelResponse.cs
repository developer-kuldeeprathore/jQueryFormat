// <copyright file="ExcelResponse.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Framework.Excel
{
    using System.Collections.Generic;

    /// <summary>
    /// ExcelResponse
    /// </summary>
    public class ExcelResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ExcelResponse"/> is status.
        /// </summary>
        /// <value>
        ///   <c>true</c> if status; otherwise, <c>false</c>.
        /// </value>
        public bool Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the HTML tables.
        /// </summary>
        /// <value>
        /// The HTML tables.
        /// </value>
        public Dictionary<string, string> HtmlTables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the excel sheet.
        /// </summary>
        /// <value>
        /// The excel sheet.
        /// </value>
        public dynamic ExcelSheet { get; set; }

        /// <summary>
        /// Gets or sets the navigate URL.
        /// </summary>
        /// <value>
        /// The navigate URL.
        /// </value>
        public string NavigateUrl { get; set; }
    }
}