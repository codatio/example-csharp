using System.Collections.Generic;
using Codat.Public.Client;

namespace Codat.Core.Controllers
{
    public class DataViewModel
    {
        public Company Company { get; }
        public List<DataSet> DataSets { get; }

        public DataViewModel(Company company, List<DataSet> datasets)
        {
            Company = company;
            DataSets = datasets;
        }
    }
}