// <copyright file="ExcelBatchData.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Framework.Excel
{
    /// <summary>
    /// ExcelBatchData
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class ExcelBatchData<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelBatchData{TEntity}"/> class.
        /// </summary>
        public ExcelBatchData()
        {
            this.RawData = (TEntity)System.Activator.CreateInstance(typeof(TEntity));
        }

        /// <summary>
        /// Gets or sets the raw data.
        /// </summary>
        /// <value>
        /// The raw data.
        /// </value>
        public TEntity RawData { get; set; }
    }
}