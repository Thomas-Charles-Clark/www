using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System;

using ClosedXML.Excel;

namespace wwwDesignersGuild.Util
{
    public struct ColumnDefinition 
    {
        public string Heading;
        public string SQLColumn;
        public XLDataType DataType;
    }

    public static class SpreadsheetHelper
    {
        public static void WriteClearanceSpreadsheet(string filePath, List<XElement> productResults, List<XElement> stockResults)
        {
            // index into [i].ChildNodes get values to columns

            _ = new System.Globalization.CultureInfo("en-GB", true).TextInfo;

            var productColumnHeadings = new List<ColumnDefinition>() 
            {
                new ColumnDefinition() 
                { 
                    Heading = "Product Type", 
                    SQLColumn = "ProductType", 
                    DataType = XLDataType.Text 
                },
                new ColumnDefinition() 
                { 
                    Heading = "Brand", 
                    SQLColumn = "Brand", 
                    DataType = XLDataType.Text 
                },
                new ColumnDefinition() 
                { 
                    Heading = "Collection", 
                    SQLColumn = "Collection", 
                    DataType = XLDataType.Text 
                },
                new ColumnDefinition() 
                { 
                    Heading = "Product Code", 
                    SQLColumn = "Code", 
                    DataType = XLDataType.Text
                },
                new ColumnDefinition() 
                { 
                    Heading = "Product Name", 
                    SQLColumn = "Name", 
                    DataType = XLDataType.Text
                },
                new ColumnDefinition() 
                { 
                    Heading = "Product URL", 
                    SQLColumn = "URL", 
                    DataType = XLDataType.Text 
                },
                new ColumnDefinition() 
                { 
                    Heading = "Image URL", 
                    SQLColumn = "ImageURL", 
                    DataType = XLDataType.Text 
                },
                new ColumnDefinition() 
                { 
                    Heading = "Trade Price Ex. VAT", 
                    SQLColumn = "ListPrice", 
                    DataType = XLDataType.Text 
                },
                new ColumnDefinition() 
                { 
                    Heading = "Trade Clearance Price Ex. VAT", 
                    SQLColumn = "ClearancePrice", 
                    DataType = XLDataType.Number 
                },
                new ColumnDefinition() 
                { 
                    Heading = "Clearance Discount", 
                    SQLColumn = "ClearanceDiscountPercentage", 
                    DataType = XLDataType.Text
                },
                new ColumnDefinition() 
                {
                    Heading = "Stock", 
                    SQLColumn = "Stock", 
                    DataType = XLDataType.Number
                },
                new ColumnDefinition() 
                {
                    Heading = "Stock Checked Date", 
                    SQLColumn = "StockLastChecked", 
                    DataType = XLDataType.DateTime 
                }
            };

            //var data = new {
            //    SheetName = "Products",
            //    ColumnHeadings = columnHeadings,
            //    Data = productResults,
            //    NumberColumns = columnHeadings.Length,
            //    NumberRows = productResults.Count
            //};

            var wb = new XLWorkbook();

            var worksheet = wb.AddWorksheet("Products");

            for (var i = 0; i < productColumnHeadings.Count; i++)
            {
                var rowNumber = 1;
                var columnNumber = i + 1;
                worksheet.Cell(rowNumber, columnNumber).Value = productColumnHeadings[i].Heading;
            }

            //Formatting headers
            var headers = worksheet.Range(1, 1, 1, productColumnHeadings.Count);
            headers.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            headers.Style.Font.Bold = true;
            headers.Style.Fill.BackgroundColor = XLColor.Gainsboro;
            headers.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            headers.Style.Border.BottomBorderColor = XLColor.DarkGray;

            for (var i = 0; i < productResults.Count; i++)
            {
                var rowNumber = i + 2;

                for (var j = 0; j < productColumnHeadings.Count; j++)
                {
                    var columnNumber = j + 1;

                    var heading = productColumnHeadings[j];

                    var cell = worksheet.Cell(rowNumber, columnNumber);

                    if (heading.Heading.ToLowerInvariant().Contains("url") && 
                        !String.IsNullOrEmpty(productResults[i].Element(heading.SQLColumn).Value.ToString()))
                    {
                        cell.Hyperlink = new XLHyperlink(productResults[i].Element(heading.SQLColumn).Value.ToString());
                        cell.SetValue(productResults[i].Element(heading.SQLColumn).Value)
                            .SetDataType(heading.DataType);
                    }
                    else
                    {
                        cell.SetValue(productResults[i].Element(heading.SQLColumn).Value)
                            .SetDataType(heading.DataType);
                    }
                }
            }

            worksheet.Columns(1, productColumnHeadings.Count).AdjustToContents();

            if (stockResults.Any())
            {
                var stockColumnHeadings = new List<ColumnDefinition>()
                {
                    new ColumnDefinition()
                    {
                        Heading = "Product Type",
                        SQLColumn = "ProductType",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Brand",
                        SQLColumn = "Brand",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Collection",
                        SQLColumn = "Collection",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Product Code",
                        SQLColumn = "Code",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Product Name",
                        SQLColumn = "Name",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Product URL",
                        SQLColumn = "URL",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Image URL",
                        SQLColumn = "ImageURL",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Trade Price Ex. VAT",
                        SQLColumn = "ListPrice",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Trade Clearance Price Ex. VAT",
                        SQLColumn = "ClearancePrice",
                        DataType = XLDataType.Number
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Clearance Discount",
                        SQLColumn = "ClearanceDiscountPercentage",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Batch",
                        SQLColumn = "Batch",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Piece",
                        SQLColumn = "Piece",
                        DataType = XLDataType.Text
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Quantity",
                        SQLColumn = "Quantity",
                        DataType = XLDataType.Number
                    },
                    new ColumnDefinition()
                    {
                        Heading = "Stock Checked Date",
                        SQLColumn = "StockLastChecked",
                        DataType = XLDataType.DateTime
                    }
                };
                var stockWorksheet = wb.AddWorksheet("Stock");

                for (var i = 0; i < stockColumnHeadings.Count; i++)
                {
                    var rowNumber = 1;
                    var columnNumber = i + 1;
                    stockWorksheet.Cell(rowNumber, columnNumber).Value = stockColumnHeadings[i].Heading;
                }

                //Formatting headers
                var stockHeaders = stockWorksheet.Range(1, 1, 1, stockColumnHeadings.Count);
                stockHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                stockHeaders.Style.Font.Bold = true;
                stockHeaders.Style.Fill.BackgroundColor = XLColor.Gainsboro;
                stockHeaders.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                stockHeaders.Style.Border.BottomBorderColor = XLColor.DarkGray;

                for (var i = 0; i < stockResults.Count; i++)
                {
                    var rowNumber = i + 2;

                    for (var j = 0; j < stockColumnHeadings.Count; j++)
                    {
                        var columnNumber = j + 1;

                        var heading = stockColumnHeadings[j];

                        var cell = stockWorksheet.Cell(rowNumber, columnNumber);

                        if (heading.Heading.ToLowerInvariant().Contains("url") &&
                            !String.IsNullOrEmpty(stockResults[i].Element(heading.SQLColumn).Value.ToString()))
                        {
                            cell.Hyperlink = new XLHyperlink(stockResults[i].Element(heading.SQLColumn).Value.ToString());
                            cell.SetValue(stockResults[i].Element(heading.SQLColumn).Value)
                                .SetDataType(heading.DataType);
                        }
                        else
                        {
                            cell.SetValue(stockResults[i].Element(heading.SQLColumn).Value)
                                .SetDataType(heading.DataType);
                        }
                    }
                }

                stockWorksheet.Columns(1, stockColumnHeadings.Count).AdjustToContents();
            }

            wb.SaveAs(filePath);

        }
    }
}