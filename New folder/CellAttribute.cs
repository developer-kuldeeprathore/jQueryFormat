// <copyright file="CellAttribute.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;

    /// <summary>
    /// CellAttribute
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class CellAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether [allow empty].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow empty]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// Gets or sets the header key.
        /// </summary>
        /// <value>
        /// The header key.
        /// </value>
        public string HeaderKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CellAttribute"/> is email.
        /// </summary>
        /// <value>
        ///   <c>true</c> if email; otherwise, <c>false</c>.
        /// </value>
        public bool Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CellAttribute"/> is date.
        /// </summary>
        /// <value>
        ///   <c>true</c> if date; otherwise, <c>false</c>.
        /// </value>
        public bool Date { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CellAttribute"/> is pin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if pin; otherwise, <c>false</c>.
        /// </value>
        public bool PIN { get; set; }

        /// <summary>
        /// Gets or sets the color code.
        /// </summary>
        /// <value>
        /// The color code.
        /// </value>
        public string ColorCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CellAttribute"/> is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if valid; otherwise, <c>false</c>.
        /// </value>
        public bool Valid { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CellAttribute"/> is unique.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unique; otherwise, <c>false</c>.
        /// </value>
        public bool Unique { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [same value].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [same value]; otherwise, <c>false</c>.
        /// </value>
        public bool SameValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CellAttribute"/> is numbers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if numbers; otherwise, <c>false</c>.
        /// </value>
        public bool Numbers { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CellAttribute"/> is decimal.
        /// </summary>
        /// <value>
        ///   <c>true</c> if decimal; otherwise, <c>false</c>.
        /// </value>
        public bool Decimal { get; set; }

        /// <summary>
        /// Gets or sets the contains.
        /// </summary>
        /// <value>
        /// The contains.
        /// </value>
        public string Contains { get; set; }

        /// <summary>
        /// Gets or sets the spliter.
        /// </summary>
        /// <value>
        /// The spliter.
        /// </value>
        public string Spliter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [HTML visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [HTML visible]; otherwise, <c>false</c>.
        /// </value>
        public bool HtmlVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public string MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public string MaxValue { get; set; } = "99999999999999.99";

        /// <summary>
        /// Gets or sets the minimum length.
        /// </summary>
        /// <value>
        /// The minimum length.
        /// </value>
        public int MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        /// <value>
        /// The maximum length.
        /// </value>
        public int MaxLength { get; set; }
    }
}