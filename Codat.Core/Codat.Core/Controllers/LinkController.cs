using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codat.Core.Models;
using Codat.Public.Client;
using Microsoft.AspNetCore.Mvc;

namespace Codat.Core.Controllers
{
    [Route("[controller]")]
    public class LinkController : Controller
    {
        private readonly CodatClient _codatClient;

        public LinkController(CodatClient codatClient)
        {
            _codatClient = codatClient;
        }

        [HttpGet("{platformKey}")]
        public async Task<IActionResult> Index(string platformKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var integration = await _codatClient.Integrations.AvailableIntegrationsAsync(cancellationToken);

            return View(new LinkViewModel(integration.FirstOrDefault(i => i.Key == platformKey)));
        }
        
        [HttpPost("{platformKey}")]
        public async Task<IActionResult> Linked(StartLinkingPostModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Company company;

            if (model.CompanyId.HasValue == false)
            {
                company = await _codatClient.Companies.AddAsync(new AddCompanyModel
                {
                    Name = model.CompanyName ?? "Test Company",
                    PlatformType = model.PlatformKey
                }, cancellationToken);
            }
            else
            {
                company = await _codatClient.Companies.GetAsync(model.CompanyId.Value, cancellationToken);
            }

            /*
             * ---- REDIRECT
             *
             * The redirect here will send the user to the relevant platform to complete the authorisation flow. This will  then return the
             * user to your 'Authorisation Complete Redirection Url' as setup in the codat portal, or via the profile client in the API.
             */

            var profile = await _codatClient.Profile.GetProfileAsync(cancellationToken);

            profile.RedirectUrl = "http://localhost:10775/link/callback/{companyId}?someValue={someValue}";

            // NOTE 1 - Uncomment this line to set the current redirect in the environment to this app!

            //var response = await _codatClient.Profile.PutProfileAsync(profile, cancellationToken);

            // NOTE 2 - The additional query parameter shown here will be included in the callback string  
            //          if marked in the redirect using 'handlebars' style templating, see above in note 1.

            return Redirect(company.Redirect + "?someValue=HelloWorld");
        }

        [HttpGet("callback/{companyId}")]
        public async Task<IActionResult> LinkedCallback(Guid companyId, string someValue, CancellationToken cancellationToken = default(CancellationToken))
        {
            var company = await _codatClient.Companies.GetAsync(companyId, cancellationToken);

            /*
             * ---- COMPANY STATUS = LINKED
             *
             * Codat will automatically start fetching data from the platform once the
             * authorisation flow is complete. You can monitor the status of the data sets
             * using the history endpoint, or the more fine grained API on the Sync client.
             */

            // 1. Full history

            var datasets = await _codatClient.Data.HistoryAllAsync(companyId, cancellationToken);


            // 2. Fine grained status check - This query retrieves the latest invoices dataset information

            var invoices = await _codatClient.Sync.GetAllSyncDetailsAsync(companyId, "invoices", null, 1, null, null, cancellationToken);
            

            return View(new LinkedCallbackViewModel(
                callBackValue: someValue, 
                company: company, 
                dataSets: datasets.ToList()));
        }
    }
}