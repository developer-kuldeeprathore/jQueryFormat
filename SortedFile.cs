// <copyright file="SortedFile.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Sorted File
    /// </summary>
    internal struct SortedFile
    {
        /// <summary>
        /// Gets or sets the file order.
        /// </summary>
        /// <value>
        /// The file order.
        /// </value>
        public int FileOrder { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }
    }
}