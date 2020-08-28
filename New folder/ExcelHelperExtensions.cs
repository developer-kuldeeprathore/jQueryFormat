// <copyright file="ExcelHelperExtensions.cs" company="Tetraskelion Softwares Pvt. Ltd.">
// Copyright (c) Tetraskelion Softwares Pvt. Ltd. All rights reserved.
// </copyright>

namespace TravelMint.UI
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using OfficeOpenXml;
    using TravelMint.Service.CrossCutting;
    using TravelMint.UI.Framework.Excel;

    /// <summary>
    /// ExcelHelperExtensions
    /// </summary>
    public static class ExcelHelperExtensions
    {
        /// <summary>
        /// The property dictionary
        /// </summary>
        private static readonly Dictionary<Type, List<PropertyInfo>> PropertyDictionary = new Dictionary<Type, List<PropertyInfo>>();

        /// <summary>
        /// The cell attributes dictionary
        /// </summary>
        private static readonly Dictionary<PropertyInfo, List<Attribute>> CellAttributesDictionary = new Dictionary<PropertyInfo, List<Attribute>>();

        /// <summary>
        /// The cell empty attributre dictionary
        /// </summary>
        private static readonly Dictionary<PropertyInfo, List<Attribute>> CellEmptyAttributreDictionary = new Dictionary<PropertyInfo, List<Attribute>>();

        /// <summary>
        /// The filter by dictionary
        /// </summary>
        private static readonly Dictionary<string, IEnumerable<string>> FilterByDictionary = new Dictionary<string, IEnumerable<string>>();

        /// <summary>
        /// To the cell key.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>ToCellKey</returns>
        public static string ToCellKey(this PropertyInfo propertyInfo)
        {
            return GetPropertyInfo<CellAttribute>(propertyInfo, "HeaderKey");
        }

        /// <summary>
        /// Rows the empty.
        /// </summary>
        /// <param name="excelRow">The excel row.</param>
        /// <param name="ws">The ws.</param>
        /// <returns>RowEmpty</returns>
        public static bool RowEmpty(this ExcelRow excelRow, ExcelWorksheet ws)
        {
            bool empty = true;
            if (excelRow != null || ws != null)
            {
                for (int i = 1; i < ws.Dimension.Columns; i++)
                {
                    if (!string.IsNullOrEmpty(ws.Cells[1, i].Value as string))
                    {
                        empty = false;
                        break;
                    }
                }
            }

            return empty;
        }

        /// <summary>
        /// To the cell validate.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        public static void ToCellValidate<TEntity>(this TEntity[] entities)
            where TEntity : class
        {
            foreach (var entity in entities)
            {
                PropertyInfo[] properties = entity.GetType().GetProperties(true);
                List<ExcelCell> cells = new List<ExcelCell>(properties.Length);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    var cellEmptyIf = (CellEmptyIfAttribute)propertyInfo.GetCellEmptyAttibute().FirstOrDefault();

                    foreach (Attribute item in propertyInfo.GetCellAttributes())
                    {
                        var value = propertyInfo.GetValue(entity) == null ? string.Empty : (string)propertyInfo.GetValue(entity);
                        var attribute = item as CellAttribute;
                        if (attribute != null)
                        {
                            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(attribute.DefaultValue))
                            {
                                value = attribute.DefaultValue;
                                propertyInfo.SetValue(entity, value);
                            }

                            if ((attribute.Numbers || attribute.Decimal) && !attribute.AllowEmpty && string.IsNullOrEmpty(value))
                            {
                                value = "0";
                                if (!string.IsNullOrEmpty(attribute.DefaultValue))
                                {
                                    value = attribute.DefaultValue;
                                }

                                propertyInfo.SetValue(entity, value);
                            }

                            if (cellEmptyIf != null)
                            {
                                CellEmptyIf(cellEmptyIf, properties, ref attribute, entity);
                            }

                            ExpressionFilter filterExpression = new ExpressionFilter { PropertyName = propertyInfo.Name, Value = value };

                            if (!attribute.AllowEmpty && string.IsNullOrEmpty(value))
                            {
                                cells.AddCellInfo(false, propertyInfo.Name, Constants.CellEmptyColor, "Cell should not be empty");
                            }
                            else if (attribute.Date && !string.IsNullOrEmpty(value))
                            {
                                DateTime dt;
                                DateTime.TryParse(value, out dt);
                                if (dt == DateTime.MinValue)
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "Date is not valid");
                                }
                            }
                            else if ((attribute.Numbers || attribute.Decimal) && !string.IsNullOrEmpty(value))
                            {
                                int convertToInteger;
                                decimal convertToDecimal;

                                decimal minValue = Convert.ToDecimal(attribute.MinValue);
                                decimal maxValue = Convert.ToDecimal(attribute.MaxValue);

                                if (attribute.Numbers && !int.TryParse(value, out convertToInteger))
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "value is not valid");
                                }

                                if (attribute.Decimal && !decimal.TryParse(value, out convertToDecimal))
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "value is not valid");
                                }

                                if (attribute.Decimal && decimal.TryParse(value, out convertToDecimal) && (convertToDecimal < minValue || convertToDecimal > maxValue))
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "value is not valid");
                                }

                                if (attribute.Numbers && int.TryParse(value, out convertToInteger) && (convertToInteger < minValue || convertToInteger > maxValue))
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "value is not valid");
                                }
                            }
                            else if (attribute.Email && !string.IsNullOrEmpty(value) && !Regex.IsMatch(value, Constants.EmailRegex))
                            {
                                cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "value is not valid email address");
                            }
                            else if (attribute.PIN && !string.IsNullOrEmpty(value) && (value.Length > 6 || value.Length < 6))
                            {
                                cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "PIN Number should be 6 Digit");
                            }
                            else if (attribute.Unique && !string.IsNullOrEmpty(value))
                            {
                                filterExpression.Operation = ExpressionType.Equal;
                                if (entities.Count(GetExpression<TEntity>(new List<ExpressionFilter> { filterExpression }).Compile()) > 1)
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellDuplicateColor, "Duplicate Value");
                                }
                            }
                            else if (attribute.SameValue && !string.IsNullOrEmpty(value))
                            {
                                filterExpression.Operation = ExpressionType.Equal;
                                if (entities.Count(GetExpression<TEntity>(new List<ExpressionFilter> { filterExpression }).Compile()) == 0 && entities.Length > 0)
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "Value Should Be Same in Column");
                                }
                            }
                            else if (!string.IsNullOrEmpty(attribute.Contains) && !string.IsNullOrEmpty(value))
                            {
                                string spliter = !string.IsNullOrEmpty(attribute.Spliter) ? attribute.Spliter : ",";

                                var splitedValues = attribute.Contains.Split(new[] { spliter }, StringSplitOptions.RemoveEmptyEntries);
                                if (splitedValues.Count(x => x.ToLower().Contains(value.ToLower())) == 0)
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellNotMatchedColor, "value is not valid");
                                }
                            }

                            if (attribute.MinLength > 0 || attribute.MaxLength > 0)
                            {
                                if (!string.IsNullOrEmpty(value) && value.Length < attribute.MinLength)
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, $"Value should be minimum {attribute.MinLength} Digit");
                                }

                                if (!string.IsNullOrEmpty(value) && value.Length > attribute.MaxLength)
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, $"Value should not be max {attribute.MinLength} Digit");
                                }
                            }

                            IEnumerable<string> filterIn;
                            if (FilterByDictionary.TryGetValue(propertyInfo.Name, out filterIn))
                            {
                                if (filterIn.Where(x => !string.IsNullOrEmpty(x)).Count(x => x.ToLower().Contains(value.ToLower())) == 0)
                                {
                                    cells.AddCellInfo(false, propertyInfo.Name, Constants.CellInvalidColor, "value is not valid");
                                }
                            }
                        }
                    }
                }

                if (cells.Count > 0)
                {
                    var cellInfo = properties.FirstOrDefault(p => p.PropertyType == typeof(List<ExcelCell>));
                    cellInfo?.SetValue(entity, cells);
                }
            }
        }

        /// <summary>
        /// Adds the cell error.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="property">The property.</param>
        /// <param name="message">The message.</param>
        /// <param name="cellType">Type of the cell.</param>
        public static void AddCellError<TEntity>(this IEnumerable<TEntity> entities, bool condition, string property, string message, CellType cellType = CellType.Empty)
            where TEntity : ExcelCell
        {
            if (entities == null)
            {
                entities = new List<TEntity>();
            }

            if (condition)
            {
                var cell = new ExcelCell { PropertyName = property, Message = message };
                switch (cellType)
                {
                    case CellType.Empty: cell.ColorCode = Constants.CellEmptyColor; break;
                    case CellType.Invalid: cell.ColorCode = Constants.CellInvalidColor; break;
                    case CellType.Exist: cell.ColorCode = Constants.CellCodeExists; break;
                    case CellType.Duplicate: cell.ColorCode = Constants.CellDuplicateColor; break;
                    case CellType.NotMatched: cell.ColorCode = Constants.CellNotMatchedColor; break;
                }

                ((List<ExcelCell>)entities).Add(cell);
            }
        }

        /// <summary>
        /// To the HTML table.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="isErrors">if set to <c>true</c> [is errors].</param>
        /// <returns>ToHtmlTable</returns>
        public static string ToHtmlTable<TEntity>(this IEnumerable<TEntity> entities, bool isErrors)
        {
            StringBuilder tblBody = new StringBuilder();
            StringBuilder tblHead = new StringBuilder();
            bool isHead = false;
            int td = 0;
            if (isErrors)
            {
                foreach (TEntity entity in entities)
                {
                    List<ExcelCell> infoCells = null;
                    var cellinfoType = entity.GetType().GetProperties(true).FirstOrDefault(c => c.PropertyType == typeof(List<ExcelCell>));
                    if (cellinfoType != null)
                    {
                        infoCells = (List<ExcelCell>)cellinfoType.GetValue(entity) ?? new List<ExcelCell>();
                    }

                    if (!isHead)
                    {
                        tblHead.Append("<tr>");
                        foreach (PropertyInfo propertyInfo in entity.GetType().GetProperties(true))
                        {
                            var key = propertyInfo.ToCellKey();
                            if (!string.IsNullOrEmpty(key))
                            {
                                tblHead.Append($"<th>{propertyInfo.ToCellKey()}</th>");
                            }
                        }

                        tblHead.Append("</tr>");
                    }

                    tblBody.Append("<tr>");
                    foreach (var propertyInfo in entity.GetType().GetProperties(true))
                    {
                        if (propertyInfo.PropertyType != typeof(List<ExcelCell>) && propertyInfo.GetCellAttributes().Length > 0)
                        {
                            var infoCell = infoCells.FirstOrDefault(c => c.PropertyName == propertyInfo.Name);
                            var colorCode = infoCell != null ? infoCell.ColorCode : "#FFFFFF";
                            var message = infoCell != null ? infoCell.Message : string.Empty;

                            var key = propertyInfo.ToCellKey();
                            if (!string.IsNullOrEmpty(key))
                            {
                                var value = propertyInfo.GetValue(entity) == null ? string.Empty : (string)propertyInfo.GetValue(entity);
                                tblBody.Append($"<td data-toggle='tooltip' title='{message}' style='background:{colorCode}'>{value}</th>");
                                td++;
                            }
                        }
                    }

                    tblBody.Append($"</tr>");
                    isHead = true;
                }
            }

            if (td == 0)
            {
                return string.Empty;
            }

            return $"<table id='{Guid.NewGuid()}' class='table table-standard table-bordered table-striped table-striped-min-width dataTable no-footer'><thead>{tblHead}</thead><tbody>{tblBody}</tbody></table>";
        }

        /// <summary>
        /// Batches the specified skip.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns>Batch</returns>
        public static IEnumerable<TEntity> Batch<TEntity>(this IEnumerable<TEntity> entities, int skip, int take)
        {
            return entities.Skip(skip).Take(take);
        }

        /// <summary>
        /// Adds the batch item.
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        /// <returns>AddBatchItem</returns>
        public static object AddBatchItem(this Dictionary<string, object> dic, string key, object data)
        {
            object existData;
            if (dic.TryGetValue(key, out existData))
            {
                dic[key] = data;
            }
            else
            {
                dic.Add(key, data);
            }

            return dic[key];
        }

        /// <summary>
        /// Gets the batch item.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dic">The dic.</param>
        /// <param name="key">The key.</param>
        /// <returns>GetBatchItem</returns>
        public static object GetBatchItem<TEntity>(this Dictionary<string, object> dic, string key)
            where TEntity : class
        {
            object existData;
            if (dic.TryGetValue(key, out existData))
            {
                return dic[key];
            }

            dic.Add(key, (TEntity)Activator.CreateInstance(typeof(TEntity)));

            return dic[key];
        }

        /// <summary>
        /// To the excel sheet.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>ToExcelSheet</returns>
        public static TEntity[] ToExcelSheet<TEntity>(this ExcelWorksheet worksheet, int startRow)
            where TEntity : class
        {
            return WorkSheetToArray<TEntity>(worksheet, startRow);
        }

        /// <summary>
        /// To the excel sheet.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="filterKeyValues">The filter key values.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>ToExcelSheet</returns>
        public static TEntity[] ToExcelSheet<TEntity>(this ExcelWorksheet worksheet, Dictionary<string, IEnumerable<string>> filterKeyValues, int startRow = 2)
            where TEntity : class
        {
            foreach (var item in filterKeyValues)
            {
                IEnumerable<string> values;
                if (FilterByDictionary.TryGetValue(item.Key, out values) == false)
                {
                    FilterByDictionary.Add(item.Key, item.Value);
                }
            }

            return WorkSheetToArray<TEntity>(worksheet, startRow);
        }

        /// <summary>
        /// To the entity list.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="entityList">The entity list.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>ToEntityList</returns>
        public static ExcelWorksheet ToEntityList<TEntity>(this ExcelWorksheet worksheet, List<TEntity> entityList, int startRow)
            where TEntity : class
        {
            return ListToWorkSheet<TEntity>(worksheet, entityList, startRow);
        }

        /// <summary>
        /// To the header keys.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// ToHeaderKeys
        /// </returns>
        public static ExportGridHeaders[] ToExcelColumns<TEntity>(this TEntity entity)
              where TEntity : class
        {
            var columns = new List<ExportGridHeaders>();
            PropertyInfo[] propertyInfo = entity.GetType().GetProperties(true);
            Properties[] properties = propertyInfo.GetProperties();
            foreach (Properties property in properties)
            {
                if (!string.IsNullOrEmpty(property.CellKey))
                {
                    columns.Add(new ExportGridHeaders
                    {
                        Key = property.Name,
                        Title = $"{property.CellKey}{(!property.AllowEmpty ? "*" : string.Empty)}",
                        PropertyName = property.Name
                    });
                }
            }

            return columns.ToArray();
        }

        /// <summary>
        /// To the data table.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The data.</param>
        /// <returns>List To DataTable</returns>
        public static DataTable ToExcelDataTable<TEntity>(this List<TEntity> entities)
        {
            DataTable dataTable = new DataTable();

            if (entities != null)
            {
                PropertyInfo[] propertyInfo = null;
                Properties[] properties = null;

                if (dataTable.Columns.Count == 0)
                {
                    TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                    propertyInfo = entity.GetType().GetProperties(true).Where(x => x.PropertyType != typeof(List<ExcelCell>)).ToArray();
                    properties = propertyInfo.GetProperties();
                    foreach (Properties property in properties)
                    {
                        if (property.PropertyValueType != null)
                        {
                            dataTable.Columns.Add(property.CellKey, Nullable.GetUnderlyingType(property.PropertyValueType) ?? property.PropertyValueType);
                        }
                        else
                        {
                            dataTable.Columns.Add(property.CellKey, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
                        }
                    }
                }

                foreach (var entity in entities)
                {
                    object[] cell = new object[properties.Length];
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var cellvalue = entity.GetType().GetProperty(properties[i].Name).GetValue(entity, null);
                        cell[i] = cellvalue ?? string.Empty;
                    }

                    dataTable.Rows.Add(cell);
                }
            }

            if (dataTable.Rows.Count == 0)
            {
                dataTable.Rows.Add(dataTable.NewRow());
            }

            return dataTable;
        }

        /// <summary>
        /// Exports the excel.
        /// </summary>
        /// <param name="dataTables">The data tables.</param>
        /// <param name="worksheets">Worksheet Name</param>
        /// <param name="excelTemplate">The excel template.</param>
        /// <param name="rowindex">The rowindex.</param>
        /// <returns>
        /// ExportExcel
        /// </returns>
        /// <exception cref="FileNotFoundException">Excel Template File Not Found</exception>
        /// <exception cref="ArgumentNullException">Workbook sheet not found in excel</exception>
        public static byte[] ExportToExcel(this DataTable[] dataTables, string[] worksheets, string excelTemplate = "", int rowindex = 2)
        {
            byte[] result = null;

            if (dataTables != null && dataTables.Count() > 0)
            {
                MemoryStream output = new MemoryStream();
                if (!string.IsNullOrEmpty(excelTemplate))
                {
                    if (!File.Exists(excelTemplate))
                    {
                        throw new FileNotFoundException("Excel Template File Not Found.");
                    }

                    using (FileStream templateDocumentStream = System.IO.File.OpenRead(excelTemplate))
                    {
                        using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                        {
                            for (int index = 0; index < worksheets.Length; index++)
                            {
                                ExcelWorksheet workSheet = package.Workbook.Worksheets[worksheets[index]];
                                if (worksheets == null)
                                {
                                    throw new ArgumentNullException($"workbook {worksheets[index]} not found in excel");
                                }

                                workSheet.Cells[$"A{rowindex}"].LoadFromDataTable(dataTables[index], false);
                                for (int colIndex = 1; colIndex <= dataTables[index].Columns.Count; colIndex++)
                                {
                                    workSheet.Column(colIndex).AutoFit();
                                }
                            }

                            result = package.GetAsByteArray();
                        }
                    }
                }
                else
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        for (int index = 0; index < worksheets.Length; index++)
                        {
                            ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(worksheets[index]);
                            if (worksheets == null)
                            {
                                throw new ArgumentNullException($"workbook {worksheets[index]} not found in excel");
                            }

                            rowindex = 1;
                            workSheet.Cells[$"A{rowindex}"].LoadFromDataTable(dataTables[index], true);
                            for (int colIndex = 1; colIndex <= dataTables[index].Columns.Count; colIndex++)
                            {
                                workSheet.Column(colIndex).AutoFit();
                            }

                            using (ExcelRange r = workSheet.Cells[rowindex, 1, rowindex, dataTables[index].Columns.Count])
                            {
                                r.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5b9bd5"));
                            }
                        }

                        result = package.GetAsByteArray();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Exports the excel.
        /// </summary>
        /// <typeparam name="TEntity">class</typeparam>
        /// <param name="data">The data.</param>
        /// <param name="sheetname">The sheetname.</param>
        /// <param name="template">The template.</param>
        /// <param name="rowindex">The rowindex.</param>
        /// <returns>
        /// ExportExcel
        /// </returns>
        public static byte[] ToExcel<TEntity>(this List<TEntity> data, string sheetname, string template = "", int rowindex = 2)
        {
            var dataTables = new DataTable[] { data.ToExcelDataTable() };
            var sheetnames = new string[] { sheetname };

            return ExportToExcel(dataTables, sheetnames, template, rowindex);
        }

        /// <summary>
        /// To the excel.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dataTables">The data tables.</param>
        /// <param name="sheetnames">The sheetnames.</param>
        /// <param name="template">The template.</param>
        /// <param name="rowindex">The rowindex.</param>
        /// <returns>Get Excel Sheet from Multiple Sheets with template or without templates</returns>
        /// <exception cref="ArgumentNullException">Sources or sheet should not be null</exception>
        /// <exception cref="IndexOutOfRangeException">workbook sheets count and argument data count should be equal</exception>
        public static byte[] ToExcel<TEntity>(this IEnumerable<DataTable> dataTables, IEnumerable<string> sheetnames, string template = "", int rowindex = 2)
        {
            if (dataTables == null || sheetnames == null)
            {
                throw new ArgumentNullException("Sources or Sheets should not be null");
            }

            if (dataTables.Count() != sheetnames.Count())
            {
                throw new IndexOutOfRangeException("workbook sheets count and argument data count should be equal");
            }

            return ExportToExcel(dataTables.ToArray(), sheetnames.ToArray(), template, rowindex);
        }

        /// <summary>
        /// Cells the empty if.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="cellEmptyIfAttribute">The cell empty if attribute.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="cellAttribute">The cell attribute.</param>
        /// <param name="entity">The entity.</param>
        private static void CellEmptyIf<TAttribute, TEntity>(TAttribute cellEmptyIfAttribute, PropertyInfo[] properties, ref CellAttribute cellAttribute, TEntity entity)
            where TAttribute : CellEmptyIfAttribute
            where TEntity : class
        {
            if (cellEmptyIfAttribute != null && !string.IsNullOrEmpty(cellEmptyIfAttribute.Property))
            {
                var validateTo = properties.FirstOrDefault(x => x.Name == cellEmptyIfAttribute.Property);
                if (validateTo != null)
                {
                    var validateToValue = validateTo.GetValue(entity) == null ? string.Empty : (string)validateTo.GetValue(entity);
                    switch (cellEmptyIfAttribute.ExpressionType)
                    {
                        case ExpressionType.NotEqual:
                            cellAttribute.AllowEmpty = validateToValue.ToLower() != cellEmptyIfAttribute.Value.ToLower();
                            break;

                        case ExpressionType.Equal:
                            cellAttribute.AllowEmpty = validateToValue.ToLower() == cellEmptyIfAttribute.Value.ToLower();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Works the sheet to array.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>WorkSheetToArray</returns>
        private static TEntity[] WorkSheetToArray<TEntity>(ExcelWorksheet worksheet, int startRow)
                    where TEntity : class
        {
            TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
            PropertyInfo[] propertyInfo = entity.GetType().GetProperties(true);

            Properties[] properties = propertyInfo.GetProperties();
            Dictionary<string, int> columns = new Dictionary<string, int>();
            var jArray = new JArray();
            try
            {
                long maxColumns = worksheet.Dimension.Columns;
                long maxRows = worksheet.Dimension.Rows;
                foreach (Properties property in properties)
                {
                    for (int index = 1; index <= maxColumns; index++)
                    {
                        var cellKey = property.CellKey;
                        var noOfMerge = 0;

                        validateCell:

                        var rowindex = (startRow - 1 > 0 ? startRow - 1 : 1) - noOfMerge;
                        ExcelRange cell = worksheet.Cells[rowindex == 0 ? 1 : rowindex, index];
                        if (cell.Merge && noOfMerge == 0)
                        {
                            noOfMerge = 1;
                            goto validateCell;
                        }

                        var cellValue = cell.Value as string;
                        if ((!string.IsNullOrEmpty(cellValue) ? cellValue : string.Empty).TrimEnd('*').Trim().ToLower() == cellKey.ToLower() && !string.IsNullOrEmpty(cellKey.TrimEnd('*').Trim()))
                        {
                            columns.Add(cellKey, index);
                        }
                    }
                }

                for (int row = startRow; row <= maxRows; row++)
                {
                    int counter = 0;
                    int blank = 1;
                    JObject jObject = new JObject();
                    foreach (var property in properties)
                    {
                        var dicKey = property.Name;
                        var dicValue = string.Empty;
                        if (property.Type != typeof(List<ExcelCell>))
                        {
                            for (int col = 1; col <= columns.Count; col++)
                            {
                                var column = columns.FirstOrDefault(c => c.Key == property.CellKey);
                                if (column.Key == property.CellKey && column.Value > 0)
                                {
                                    var excelCell = worksheet.Cells[row, column.Value];
                                    var cellAttribute = property.Attribute as CellAttribute;
                                    if (cellAttribute != null && cellAttribute.Date)
                                    {
                                        if (excelCell.Style.Numberformat.Format.ToLower().Contains("yy"))
                                        {
                                            excelCell.Style.Numberformat.Format = "dd/MM/yyyy";
                                            dicValue = Convert.ToString(excelCell.Text);
                                            break;
                                        }

                                        try
                                        {
                                            dicValue = DateTimeExtensions.FromOADate(Convert.ToDouble(worksheet.Cells[row, column.Value].Value)).ToString("dd/MM/yyyy");
                                            break;
                                        }
                                        catch (FormatException ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                        }
                                    }

                                    if (cellAttribute != null && cellAttribute.Decimal)
                                    {
                                        ////excelCell.Style.Numberformat.Format = "0.00";
                                        dicValue = Convert.ToString(excelCell.Text).Replace("%", string.Empty);
                                        break;
                                    }

                                    try
                                    {
                                        dicValue = Convert.ToString(worksheet.Cells[row, column.Value].Text);
                                        break;
                                    }
                                    catch (NullReferenceException)
                                    {
                                        dicValue = Convert.ToString(worksheet.Cells[row, column.Value].Value);
                                        break;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(dicValue.Trim()))
                            {
                                blank++;
                            }

                            jObject.Add(dicKey, dicValue.Trim());
                        }
                        else
                        {
                            jObject.Add(dicKey, new JArray());
                        }

                        if (string.IsNullOrEmpty(dicValue))
                        {
                            counter++;
                        }
                    }

                    if (counter > 0 && counter == properties.Length)
                    {
                        //// skip statement
                    }
                    else
                    {
                        if (blank > 1 && blank > (columns.Count - 2))
                        {
                            Console.WriteLine("s");
                        }
                        else
                        {
                            jArray.Add(jObject);
                        }
                    }
                }

                TEntity[] entities = Newtonsoft.Json.JsonConvert.DeserializeObject<TEntity[]>(jArray.ToString());
                entities.ToCellValidate();
                return entities;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Lists to work sheet.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="entityList">The entity list.</param>
        /// <param name="startRow">The start row.</param>
        /// <returns>ListToWorkSheet</returns>
        private static ExcelWorksheet ListToWorkSheet<TEntity>(ExcelWorksheet worksheet, List<TEntity> entityList, int startRow)
                   where TEntity : class
        {
            Type entity = typeof(TEntity);
            PropertyInfo[] propertyInfo = entity.GetProperties();

            Properties[] properties = propertyInfo.GetProperties();
            Dictionary<string, int> columns = new Dictionary<string, int>();
            int rowNo = startRow;

            long maxColumns = worksheet.Dimension.Columns;
            foreach (Properties property in properties)
            {
                for (int index = 1; index <= maxColumns; index++)
                {
                    var cellKey = property.CellKey;
                    var noOfMerge = 0;

                    validateCell:

                    var rowindex = (startRow - 1 > 0 ? startRow - 1 : 1) - noOfMerge;
                    ExcelRange cell = worksheet.Cells[rowindex == 0 ? 1 : rowindex, index];
                    if (cell.Merge && noOfMerge == 0)
                    {
                        noOfMerge = 1;
                        goto validateCell;
                    }

                    var cellValue = cell.Value as string;
                    if ((!string.IsNullOrEmpty(cellValue) ? cellValue : string.Empty).TrimEnd('*').Trim().ToLower() == cellKey.ToLower() && !string.IsNullOrEmpty(cellKey.TrimEnd('*').Trim()))
                    {
                        columns.Add(property.Name, index);
                    }
                }
            }

            foreach (TEntity entityItem in entityList)
            {
                try
                {
                    foreach (var item in columns)
                    {
                        var prop = propertyInfo.FirstOrDefault(x => x.Name.ToLower() == item.Key.ToLower());
                        string value = prop.GetValue(entityItem) as string;
                        worksheet.Cells[rowNo, item.Value].Value = value;
                        worksheet.Cells[rowNo, item.Value].Style.Numberformat.Format = GetValueFromEntity(prop, value);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                rowNo++;
            }

            return worksheet;
        }

        /// <summary>
        /// Gets the value from entity.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="value">The value.</param>
        /// <returns>GetValueFromEntity</returns>
        private static string GetValueFromEntity(PropertyInfo propertyInfo, string value)
        {
            string valueType = string.Empty;

            var custom = propertyInfo.GetCustomAttribute<CellAttribute>();
            if (custom.Date)
            {
                valueType = "dd/MM/yyyy";
            }
            else if (custom.Decimal)
            {
                if (value.Contains("%"))
                {
                    valueType = "0%";
                }
                else
                {
                    valueType = "0.00";
                }
            }
            else if (custom.Numbers)
            {
                valueType = "0";
            }
            else
            {
                valueType = "0.00";
            }

            return valueType;
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="cached">if set to <c>true</c> [cached].</param>
        /// <returns>GetProperties</returns>
        private static PropertyInfo[] GetProperties(this Type type, bool cached)
        {
            List<PropertyInfo> properties;
            if (PropertyDictionary.TryGetValue(type, out properties) == false)
            {
                if (type != null)
                {
                    properties = type.GetProperties().Where(x => x.PropertyType == typeof(string) || x.PropertyType == typeof(List<ExcelCell>)).ToList();
                    if (cached)
                    {
                        PropertyDictionary.Add(type, properties);
                    }
                }
            }

            return properties?.ToArray();
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>GetProperties</returns>
        private static Properties[] GetProperties(this PropertyInfo[] propertyInfo)
        {
            if (propertyInfo.Length == 0)
            {
                return new List<Properties>().ToArray();
            }

            Properties[] properties = new Properties[propertyInfo.Length];

            for (int index = 0; index < propertyInfo.Length; index++)
            {
                Type propertyValueType = null;
                CellAttribute cellAttribute = (CellAttribute)propertyInfo[index].GetCellAttributes().FirstOrDefault();
                if (cellAttribute != null)
                {
                    if (cellAttribute.Numbers)
                    {
                        propertyValueType = typeof(int);
                    }

                    if (cellAttribute.Decimal)
                    {
                        propertyValueType = typeof(decimal);
                    }
                }

                properties[index] = new Properties
                {
                    Name = propertyInfo[index].Name,
                    CellKey = propertyInfo[index].ToCellKey(),
                    Type = propertyInfo[index].PropertyType,
                    Attribute = cellAttribute,
                    AllowEmpty = propertyInfo[index].CellAllowEmpty(),
                    PropertyType = propertyInfo[index].PropertyType,
                    PropertyValueType = propertyValueType
                };
            }

            return properties;
        }

        /// <summary>
        /// Gets the property information.
        /// </summary>
        /// <typeparam name="TAttributre">The type of the attributre.</typeparam>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <returns>GetPropertyInfo</returns>
        private static string GetPropertyInfo<TAttributre>(PropertyInfo propertyInfo, string keyName)
            where TAttributre : Attribute
        {
            string value = string.Empty;
            foreach (Attribute attribute in propertyInfo.GetCustomAttributes<Attribute>(true).Where(x => x.GetType() == typeof(TAttributre)))
            {
                var property = attribute.GetType().GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, keyName, StringComparison.OrdinalIgnoreCase));
                if (property != null)
                {
                    value = property.GetValue(attribute) as string;
                }
            }

            return value;
        }

        /// <summary>
        /// Adds the cell information.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entities">The entities.</param>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        /// <param name="propertyname">The propertyname.</param>
        /// <param name="colorCode">The color code.</param>
        /// <param name="message">The message.</param>
        private static void AddCellInfo<TEntity>(this List<TEntity> entities, bool isValid, string propertyname, string colorCode, string message)
        {
            if (typeof(TEntity) == typeof(ExcelCell))
            {
                bool isvalid = false;

                TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));

                foreach (PropertyInfo propertyInfo in entity.GetType().GetProperties(true))
                {
                    switch (propertyInfo.Name)
                    {
                        case "CellValid":
                            propertyInfo.SetValue(entity, isValid); isvalid = true;
                            break;

                        case "ColorCode":
                            propertyInfo.SetValue(entity, colorCode); isvalid = true;
                            break;

                        case "PropertyName":
                            propertyInfo.SetValue(entity, propertyname); isvalid = true;
                            break;

                        case "Message":
                            propertyInfo.SetValue(entity, message); isvalid = true;
                            break;
                    }
                }

                if (isvalid)
                {
                    entities.Add(entity);
                }
            }
        }

        /// <summary>
        /// Gets the cell attributes.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>GetCellAttributes</returns>
        private static Attribute[] GetCellAttributes(this PropertyInfo propertyInfo)
        {
            var properties = new List<Attribute>();
            if (CellAttributesDictionary.TryGetValue(propertyInfo, out properties) == false)
            {
                properties = propertyInfo.GetCustomAttributes<Attribute>(true).Where(x => x.GetType() == typeof(CellAttribute)).ToList();
                CellAttributesDictionary.Add(propertyInfo, properties);
            }

            if (properties == null)
            {
                properties = new List<Attribute>();
            }

            return properties.ToArray();
        }

        private static bool CellAllowEmpty(this PropertyInfo propertyInfo)
        {
            var isRequired = false;
            var properties = propertyInfo.GetCustomAttributes<CellAttribute>(true).Where(x => x.GetType() == typeof(CellAttribute)).ToList();
            if (properties != null && properties.Count > 0)
            {
                isRequired = properties.FirstOrDefault().AllowEmpty;
            }

            return isRequired;
        }

        /// <summary>
        /// Gets the cell empty attibute.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>GetCellEmptyAttibute</returns>
        private static Attribute[] GetCellEmptyAttibute(this PropertyInfo propertyInfo)
        {
            var properties = new List<Attribute>();
            if (CellEmptyAttributreDictionary.TryGetValue(propertyInfo, out properties) == false)
            {
                properties = propertyInfo.GetCustomAttributes<Attribute>(true).Where(x => x.GetType() == typeof(CellEmptyIfAttribute)).ToList();
                CellEmptyAttributreDictionary.Add(propertyInfo, properties);
            }

            if (properties == null)
            {
                properties = new List<Attribute>();
            }

            return properties.ToArray();
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="filters">The filters.</param>
        /// <returns>GetExpression</returns>
        private static Expression<Func<TEntity, bool>> GetExpression<TEntity>(IList<ExpressionFilter> filters)
        {
            if (filters.Count == 0)
            {
                return null;
            }

            ParameterExpression param = Expression.Parameter(typeof(TEntity), "t");
            Expression exp = null;

            if (filters.Count == 1)
            {
                exp = GetExpression<TEntity>(param, filters[0]);
            }
            else if (filters.Count == 2)
            {
                exp = GetExpression<TEntity>(param, filters[0], filters[1]);
            }
            else
            {
                while (filters.Count > 0)
                {
                    var f1 = filters[0];
                    var f2 = filters[1];

                    if (exp == null)
                    {
                        exp = GetExpression<TEntity>(param, filters[0], filters[1]);
                    }
                    else
                    {
                        exp = Expression.AndAlso(exp, GetExpression<TEntity>(param, filters[0], filters[1]));
                    }

                    filters.Remove(f1);
                    filters.Remove(f2);

                    if (filters.Count == 1)
                    {
                        exp = Expression.AndAlso(exp, GetExpression<TEntity>(param, filters[0]));
                        filters.RemoveAt(0);
                    }
                }
            }

            return Expression.Lambda<Func<TEntity, bool>>(exp, param);
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="param">The parameter.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>GetExpression</returns>
        private static Expression GetExpression<TEntity>(ParameterExpression param, ExpressionFilter filter)
        {
            MemberExpression member = Expression.Property(param, filter.PropertyName);
            ConstantExpression constant = Expression.Constant(filter.Value);

            switch (filter.Operation)
            {
                case ExpressionType.Equal:
                    return Expression.Equal(member, constant);

                case ExpressionType.GreaterThan:
                    return Expression.GreaterThan(member, constant);

                case ExpressionType.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, constant);

                case ExpressionType.LessThan:
                    return Expression.LessThan(member, constant);

                case ExpressionType.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, constant);
            }

            return null;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="param">The parameter.</param>
        /// <param name="filter1">The filter1.</param>
        /// <param name="filter2">The filter2.</param>
        /// <returns>GetExpression</returns>
        private static BinaryExpression GetExpression<TEntity>(ParameterExpression param, ExpressionFilter filter1, ExpressionFilter filter2)
        {
            Expression bin1 = GetExpression<TEntity>(param, filter1);
            Expression bin2 = GetExpression<TEntity>(param, filter2);

            return Expression.AndAlso(bin1, bin2);
        }
    }
}