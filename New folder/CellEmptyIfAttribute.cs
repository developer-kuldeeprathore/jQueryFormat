// <copyright file="CellEmptyIfAttribute.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// CellEmptyIfAttribute
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class CellEmptyIfAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the type of the expression.
        /// </summary>
        /// <value>
        /// The type of the expression.
        /// </value>
        public ExpressionType ExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public Type DataType { get; set; }
    }
}