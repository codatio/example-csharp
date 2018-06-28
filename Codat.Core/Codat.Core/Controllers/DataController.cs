using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codat.Public.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Codat.Core.Controllers
{
    [Route("[controller]")]
    public class DataController : Controller
    {
        private readonly CodatClient _codatClient;

        public DataController(CodatClient codatClient)
        {
            _codatClient = codatClient;
        }

        [HttpGet("{companyId}")]
        public async Task<IActionResult> Index(Guid companyId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var company = await _codatClient.Companies.GetAsync(companyId, cancellationToken);
            if (company == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var datasets = await _codatClient.Data.HistoryAllAsync(companyId, cancellationToken);

            return View(new DataViewModel(company, datasets.ToList()));
        }

        [HttpGet("{companyId}/pull/{dataType}")]
        public async Task<IActionResult> Pull(Guid companyId, string dataType, CancellationToken cancellationToken = default(CancellationToken))
        {
            var company = await _codatClient.Companies.GetAsync(companyId, cancellationToken);
            if (company == null)
            {
                return RedirectToAction("Index", "Home");
            }

            switch (dataType)
            {
                case "invoices":
                {
                    // Retrieve data from the API via any one of the specific data clients.

                    var invoices = await _codatClient.Invoices.ListPagedAsync(companyId, 1, null, null, null, cancellationToken);

                    return View("PullInvoices", invoices.Results.ToList());
                }
                default:
                {
                    return RedirectToAction("Index", new {companyId = companyId});
                }
            }
        }

        [HttpGet("{companyId}/push/{dataType}")]
        public async Task<IActionResult> Push(Guid companyId, string dataType, CancellationToken cancellationToken = default(CancellationToken))
        {
            var company = await _codatClient.Companies.GetAsync(companyId, cancellationToken);
            if (company == null)
            {
                return RedirectToAction("Index", "Home");
            }

            switch (dataType)
            {
                case "bankStatements":
                {
                    return View("PushBankStatments", new PushDataPostModel());
                }
                default:
                {
                    return RedirectToAction("Index", new { companyId = companyId });
                }
            }
        }

        [HttpPost("{companyId}/push/{dataType}")]
        public async Task<IActionResult> PushPost(Guid companyId, string dataType, PushDataPostModel postModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            var company = await _codatClient.Companies.GetAsync(companyId, cancellationToken);
            if (company == null)
            {
                return RedirectToAction("Index", "Home");
            }


            /*
             * ---- DATA CONNECTIONS
             *
             * The data connection is the placeholder for the link to the given platform.
             * It is possible for a company to have multiple connections, but generally they 
             * should only have one.
             */

            var companyDataConnection = company.DataConnections.SingleOrDefault();
            if (companyDataConnection == null)
            {
                return RedirectToAction("Index", "Home");
            }

            switch (dataType)
            {
                case "bankStatements":
                {
                    /*
                     * ---- BANK ACCOUNT
                     *
                     * Most entities do not require parent data to exist when pushing, e.g. suppliers or customers.
                     * However when pushing bank transactions there must be a parent bank account to push them to.
                     */

                    BankStatementAccount account;

                    if (string.IsNullOrWhiteSpace(postModel.AccountId))
                    {

                        // 1. If no valid account given, then create one.

                        var response = await _codatClient.BankAccounts.CreateAccountAsync(companyId, companyDataConnection.Id, new BankStatementAccount
                        {
                            // A name for the acount is required.
                            AccountName = "New Account",

                            // A currency for the account is required.
                            Currency = "GBP"
                        }, cancellationToken);

                        account = response.Data;
                    }
                    else
                    {

                        // 2. If account id is given then retrieve it.

                        account = await _codatClient.BankAccounts.SingleAsync(companyId, postModel.AccountId, null, cancellationToken);
                        if (account == null)
                        {
                            return RedirectToAction("Push", new { companyId = companyId, dataType = dataType });
                        }
                    }
                    
                    // 3. Push the new bank transaction.

                    var pushResponse = await _codatClient.BankAccounts.CreateTransactionsAsync(companyId,
                        companyDataConnection.Id, account.Id, new BankTransactions
                        {
                            AccountId = account.Id,
                            Transactions = new ObservableCollection<BankStatementLine>
                            {
                                new BankStatementLine
                                {
                                    Amount = postModel.Amount,
                                    Description = postModel.Description,
                                    Date = DateTime.UtcNow,
                                    TransactionType = postModel.Amount < 0
                                        ? BankStatementLineTransactionType.Credit
                                        : BankStatementLineTransactionType.Debit
                                }
                            }
                        }, cancellationToken);

                    return View("PushBankStatments", new PushDataPostModel
                    {
                        AccountId = account.Id,
                        Amount = 0,
                        Description = string.Empty
                    });
                }
                default:
                {
                    return RedirectToAction("Index", new { companyId = companyId });
                }
            }
        }
    }
}
