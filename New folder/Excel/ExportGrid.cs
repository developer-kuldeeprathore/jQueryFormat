// <copyright file="ExportGrid.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI.Framework.Excel
{
    using System;
    using System.Data;
    using System.IO;
    using Newtonsoft.Json.Linq;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    /// <summary>
    /// ExportGrid
    /// </summary>
    public class ExportGrid
    {
        /// <summary>
        /// Generates the excel.
        /// </summary>
        /// <param name="worksheetName">Name of the workbook.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="excelData">The excel data.</param>
        /// <returns>
        /// GenerateExcel
        /// </returns>
        public static ExcelPackage GenerateExcel(string worksheetName, ExportGridHeaders[] columns, JArray excelData)
        {
            ExcelPackage excelPackage = new ExcelPackage();
            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add(worksheetName);
            ////workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            if (columns != null)
            {
                for (int index = 1; index <= columns.Length; index++)
                {
                    var column = columns[index - 1];
                    workSheet.Cells[1, 1].Value = string.IsNullOrEmpty(column.Title) ? column.Key : column.Title;
                }
            }

            var rowindex = 2;
            if (excelData != null && excelData.Count > 0)
            {
                for (int index = 0; index < excelData.Count; index++)
                {
                    for (int colIndex = 0; colIndex < columns.Length; colIndex++)
                    {
                        var strValue = (JValue)excelData[index][columns[colIndex].Key];
                        string cellValue = strValue.Type == JTokenType.Date ? Convert.ToDateTime(Convert.ToString(strValue)).ToString("dd/MM/yyyy") : Convert.ToString(strValue);
                        workSheet.Cells[rowindex, colIndex + 1].Value = cellValue;
                    }

                    rowindex++;
                }
            }

            return excelPackage;
        }

        /////// <summary>
        /////// Writes the HTML table.
        /////// </summary>
        /////// <param name="headerKey">The header key.</param>
        /////// <param name="resultData">The result data.</param>
        /////// <returns>string type</returns>
        ////public static string WriteHtmlTable(List<ExportGridHeaders> headerKey, JArray resultData)
        ////{
        ////    StringBuilder stringBuilder = new StringBuilder();

        ////    stringBuilder.Append("<table>");

        ////    if (headerKey != null && headerKey.Count > 0)
        ////    {
        ////        stringBuilder.Append("<tr>");
        ////        foreach (var col in headerKey)
        ////        {
        ////            stringBuilder.AppendFormat("<td>{0}</td>", string.IsNullOrEmpty(col.Title) ? col.Key : col.Title);
        ////        }

        ////        stringBuilder.Append("</tr>");
        ////    }

        ////    if (resultData != null && resultData.Count > 0)
        ////    {
        ////        foreach (Newtonsoft.Json.Linq.JToken item in resultData)
        ////        {
        ////            stringBuilder.Append("<tr>");
        ////            if (headerKey != null && headerKey.Count > 0)
        ////            {
        ////                foreach (ExportGridHeaders col in headerKey)
        ////                {
        ////                    var strValue = (Newtonsoft.Json.Linq.JValue)item[col.Key];
        ////                    string cellValue = strValue.Type == JTokenType.Date ? Convert.ToDateTime(Convert.ToString(strValue)).ToString("dd/MM/yyyy") : Convert.ToString(strValue);
        ////                    stringBuilder.AppendFormat("<td>{0}</td>", cellValue);
        ////                }
        ////            }

        ////            stringBuilder.Append("</tr>");
        ////        }
        ////    }

        ////    stringBuilder.Append("</table>");
        ////    return stringBuilder.ToString();
        ////}
    }
}