using System.Collections.Generic;


namespace SUARweb.Exporters
{
    internal interface IExportableEntity
    {
        Dictionary<string, dynamic> GetExportData();
    }
}
