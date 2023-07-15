using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;

namespace SUARweb.Exporters
{
    internal class ExcelExporter : IMemoryExporter
    {
        private readonly IList<IExportableEntity> _data;

        public ExcelExporter(IList<IExportableEntity> data)
        {
            _data = data != null ? data : new List<IExportableEntity>();
        }

        public MemoryStream ExportToMemoryStream()
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Лист 1");

                if(_data.Count > 0)
                {
                    WriteHeaders(worksheet);
                    WriteData(worksheet);
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);

                return stream;
            }
        }

        private void WriteHeaders(IXLWorksheet sheet)
        {
            int col = 1;

            foreach(var key in _data[0].GetExportData().Keys)
            {
                sheet.Cell(1, col).Value = key;
                col++;
            }
            sheet.Row(1).Style.Font.Bold = true;
        }

        private void WriteData(IXLWorksheet sheet)
        {
            int row = 2;
            int col = 1;

            foreach(var item in _data)
            {
                foreach(var value in item.GetExportData().Values)
                {
                    sheet.Cell(row, col).Value = value;
                    col++;
                }
                row++;
            }
        }
    }
}
