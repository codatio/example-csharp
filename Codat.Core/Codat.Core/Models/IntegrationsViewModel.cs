using System.Collections.Generic;
using Codat.Public.Client;

namespace Codat.Core.Models
{
    public class IntegrationsViewModel
    {
        public IntegrationsViewModel(List<Integration> integrations, List<Company> companies)
        {
            Integrations = integrations;
            Companies = companies;
        }

        public List<Integration> Integrations { get; }
        public List<Company> Companies { get; }
    }
}
