using System;

namespace Codat.Core.Controllers
{
    public class StartLinkingPostModel
    {
        public string PlatformKey { get; set; }
        public string CompanyName { get; set; }
        public Guid? CompanyId { get; set; }
    }
}