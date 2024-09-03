using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelReader.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.Data;

namespace ExcelReader.Services
{
    /**
     * ClosedXML (OpenXML specs) based implementation
     */
    public static class ExcelService2
    {
        public static DataTable? ReadExcelFile(string path)
        {
            try
            {
                using (var wb = new XLWorkbook(path))
                {
                    var dataTable = new DataTable();

                    var ws = wb.Worksheet(1);
                    var firstRow = ws.FirstRowUsed();
                    //columns and names
                    foreach (var cell in firstRow.Cells())
                    {
                        dataTable.Columns.Add(cell.GetValue<string>());
                    }

                    foreach (var row in ws.RowsUsed().Skip(1))
                    {
                        var dtrow = dataTable.NewRow();
                        for (int i = 0; i < row.Cells().Count(); i++)
                        {
                            dtrow[i] = row.Cell(i + 1).GetValue<string>();
                        }
                        dataTable.Rows.Add(dtrow);

                    }

                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
                //throw;
            }
        }

        public static XLWorkbook? WriteExcelFile(IList<string> columns, IEnumerable<Role> rows, string? filePath = null)
        {
            try
            {
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("List of users");

                    //add the headers

                    for (var i = 1; i <= columns.Count(); i++)
                    {
                        ws.Cell(1, i).Value = columns[i - 1].ToString();
                    }
                    //add the rows
                    int rowNumber = 2;
                    foreach (var row in rows)
                    {
                       

                        for (var i = 1; i <= columns.Count(); i++)
                        {
                            ws.Cell(rowNumber, i).Value = typeof(Role).GetProperty(columns[i - 1]).GetValue(row, null).ToString();
                        }

                        rowNumber++;
                    }



                    if (!filePath.IsNullOrEmpty())
                    {
                        wb.SaveAs(filePath);
                    }
                    return wb;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
                //throw;
            }

        }
    }
}
