// <copyright file="PaxDetailExcelViewModel.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.ViewModels.ExcelExport
{
    using System.Collections.Generic;
    using TravelMint.UI.Framework.Excel;

    /// <summary>
    /// PaxDetailExcelViewModel
    /// </summary>
    public class PaxDetailExcelViewModel
    {
        /// <summary>
        /// Gets or sets the sequence no.
        /// </summary>
        /// <value>
        /// The sequence no.
        /// </value>
        [Cell(HeaderKey = "S.No", AllowEmpty = true, Numbers = true)]
        public string SequenceNo { get; set; }

        /// <summary>
        /// Gets or sets the type of the pax.
        /// </summary>
        /// <value>
        /// The type of the pax.
        /// </value>
        [Cell(HeaderKey = "Pax Type", Contains = "Adult,Child,Tour Leader")]
        public string PaxType { get; set; }

        /// <summary>
        /// Gets or sets the room sharing.
        /// </summary>
        /// <value>
        /// The room sharing.
        /// </value>
        [Cell(HeaderKey = "Room Sharing", Contains = "SGL,DBL,TPL,TWN,CWB,CNB")]
        public string RoomSharing { get; set; }

        /// <summary>
        /// Gets or sets the room no.
        /// </summary>
        /// <value>
        /// The room no.
        /// </value>
        [Cell(HeaderKey = "Room No", Numbers = true)]
        public string RoomNo { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Cell(HeaderKey = "Title", Contains = "Mr.,Mrs.,Miss.")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Cell(HeaderKey = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Cell(HeaderKey = "Last Name", AllowEmpty = true)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the dateof birth.
        /// </summary>
        /// <value>
        /// The dateof birth.
        /// </value>
        [Cell(HeaderKey = "Date of Birth", AllowEmpty = true, Date = true)]
        public string DateofBirth { get; set; }

        /// <summary>
        /// Gets or sets the dateof birth.
        /// </summary>
        /// <value>
        /// The dateof birth.
        /// </value>
        [Cell(HeaderKey = "Gender", Contains = "Male,Female,Other")]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the nationality.
        /// </summary>
        /// <value>
        /// The nationality.
        /// </value>
        [Cell(HeaderKey = "Nationality", AllowEmpty = true)]
        public string Nationality { get; set; }

        /// <summary>
        /// Gets or sets the passport no.
        /// </summary>
        /// <value>
        /// The passport no.
        /// </value>
        [Cell(HeaderKey = "Passport No", AllowEmpty = true)]
        public string PassportNo { get; set; }

        /// <summary>
        /// Gets or sets the placeof issue.
        /// </summary>
        /// <value>
        /// The placeof issue.
        /// </value>
        [Cell(HeaderKey = "Passport Place of Issue", AllowEmpty = true)]
        public string PassportPlaceofIssue { get; set; }

        /// <summary>
        /// Gets or sets the dateof issue.
        /// </summary>
        /// <value>
        /// The dateof issue.
        /// </value>
        [Cell(HeaderKey = "Passport Date of Issue", AllowEmpty = true, Date = true)]
        public string PassportDateofIssue { get; set; }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        /// <value>
        /// The expiry date.
        /// </value>
        [Cell(HeaderKey = "Passport Expiry Date", AllowEmpty = true, Date = true)]
        public string PassportExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the visa no.
        /// </summary>
        /// <value>
        /// The visa no.
        /// </value>
        [Cell(HeaderKey = "Visa No", AllowEmpty = true)]
        public string VisaNo { get; set; }

        /// <summary>
        /// Gets or sets the visa placeof issue.
        /// </summary>
        /// <value>
        /// The visa placeof issue.
        /// </value>
        [Cell(HeaderKey = "Visa Place of Issue", AllowEmpty = true)]
        public string VisaPlaceofIssue { get; set; }

        /// <summary>
        /// Gets or sets the visa dateof issue.
        /// </summary>
        /// <value>
        /// The visa dateof issue.
        /// </value>
        [Cell(HeaderKey = "Visa Date of Issue", AllowEmpty = true, Date = true)]
        public string VisaDateofIssue { get; set; }

        /// <summary>
        /// Gets or sets the visa expiry date.
        /// </summary>
        /// <value>
        /// The visa expiry date.
        /// </value>
        [Cell(HeaderKey = "Visa Expiry Date", AllowEmpty = true, Date = true)]
        public string VisaExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the cell information.
        /// </summary>
        /// <value>
        /// The cell information.
        /// </value>
        public List<ExcelCell> CellInfo { get; set; } = new List<ExcelCell>();
    }
}