// <copyright file="ExcelSheets.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.ViewModels.Excel
{
    using TravelMint.UI.Framework.Excel;

    /// <summary>
    /// ExcelSheet
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityDetails">The type of the entity details.</typeparam>
    public class ExcelSheets<TEntity, TEntityDetails>
        where TEntity : class
        where TEntityDetails : class
    {
        /// <summary>
        /// Gets or sets the t entity.
        /// </summary>
        /// <value>
        /// The t entity.
        /// </value>
        public ExcelSheet<TEntity> WorkSheet1 { get; set; }

        /// <summary>
        /// Gets or sets the t entity details.
        /// </summary>
        /// <value>
        /// The t entity details.
        /// </value>
        public ExcelSheet<TEntityDetails> WorkSheet2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is single sheet.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is single sheet; otherwise, <c>false</c>.
        /// </value>
        public bool IsSingleSheet { get; set; }
    }
}