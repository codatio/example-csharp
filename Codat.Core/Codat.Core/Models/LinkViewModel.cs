using Codat.Public.Client;

namespace Codat.Core.Models
{
    public class LinkViewModel
    {
        public Integration SelectedIntegration { get; }

        public LinkViewModel(Integration selectedIntegration)
        {
            SelectedIntegration = selectedIntegration;
        }
    }
}
