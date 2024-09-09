using ExcelDataReader;
using System.Data;
using System.Text;

namespace Services
{
    public static class ExcelService
    {
        public static DataTable? ReadExcelFile(string path)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                try
                {

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        if (result.Tables.Count > 0)
                        {
                            return result.Tables[0];
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;

        }
    }
}
