using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codat.Core.Models;
using Codat.Public.Client;
using Microsoft.AspNetCore.Mvc;

namespace Codat.Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly CodatClient _codatClient;

        public HomeController(CodatClient codatClient)
        {
            _codatClient = codatClient;
        }

        public IActionResult Index()
        {
            return RedirectToAction("New");
        }

        public async Task<IActionResult> New(CancellationToken cancellationToken = default(CancellationToken))
        {
            /*
             * ---- INTEGRATIONS
             *
             * The integrations endpoint returns a full list of the supported platforms and their
             * status in relation to your own settings in Codat. This includes whether they are
             * enabled or the ability to check credentials for the authorisation flow e.g. client ID
             * and Client Secret (masked in GET).
             */

            var integrations = await _codatClient.Integrations.AvailableIntegrationsAsync(cancellationToken);

            /*
             * ---- COMPANIES
             *
             * Companies are the core object in the Codat system, they are analogous in most scenarios
             * to your own clients or users. A company ID is generally needed when interacting with
             * the API to either schedule a sync of data or pull data from a complete sync.
             */

            var companies = await _codatClient.Companies.ListPagedAsync(1, null, null, cancellationToken: cancellationToken);

            return View(new IntegrationsViewModel(

                integrations
                    .Where(i => i.Enabled)
                    .ToList(), 

                companies
                    .Results
                    .Where(c => 
                        c.Status != CompanyStatus.Deauthorized && 
                        c.Status != CompanyStatus.Unlinked)
                    .ToList()));
        }
    }
}
