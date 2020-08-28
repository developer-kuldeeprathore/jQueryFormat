// <copyright file="Properties.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Framework.Excel
{
    using System;

    /// <summary>
    /// Properties
    /// </summary>
    public class Properties
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the cell key.
        /// </summary>
        /// <value>
        /// The cell key.
        /// </value>
        public string CellKey { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public Attribute Attribute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow empty].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow empty]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the type of the property value.
        /// </summary>
        /// <value>
        /// The type of the property value.
        /// </value>
        public Type PropertyValueType { get; set; }
    }
}