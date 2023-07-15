using System.IO;

namespace SUARweb.Exporters
{
    internal interface IMemoryExporter
    {
        MemoryStream ExportToMemoryStream();
    }
}
