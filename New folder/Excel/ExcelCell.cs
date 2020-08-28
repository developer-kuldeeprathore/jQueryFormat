// <copyright file="ExcelCell.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Framework.Excel
{
    /// <summary>
    /// CellType
    /// </summary>
    public enum CellType
    {
        /// <summary>
        /// The empty
        /// </summary>
        Empty,

        /// <summary>
        /// The invalid
        /// </summary>
        Invalid,

        /// <summary>
        /// The exist
        /// </summary>
        Exist,

        /// <summary>
        /// The duplicate
        /// </summary>
        Duplicate,

        /// <summary>
        /// The not matched
        /// </summary>
        NotMatched
    }

    /// <summary>
    /// ExcelCell
    /// </summary>
    public class ExcelCell
    {
        /// <summary>
        /// Gets or sets a value indicating whether [cell valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cell valid]; otherwise, <c>false</c>.
        /// </value>
        public bool CellValid { get; set; }

        /// <summary>
        /// Gets or sets the color code.
        /// </summary>
        /// <value>
        /// The color code.
        /// </value>
        public string ColorCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
    }
}