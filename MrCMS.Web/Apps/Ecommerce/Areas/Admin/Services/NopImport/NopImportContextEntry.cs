using System;
using MrCMS.Entities;

namespace MrCMS.Web.Apps.Ecommerce.Areas.Admin.Services.NopImport
{
    public class NopImportContextEntry
    {
        public Type Type { get; set; }
        public int Id { get; set; }

        public SystemEntity NewEntity { get; set; }

    }
}