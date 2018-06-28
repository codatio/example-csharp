using System.Collections.Generic;
using Codat.Public.Client;

namespace Codat.Core.Models
{
    public class LinkedCallbackViewModel
    {
        public LinkedCallbackViewModel(string callBackValue, Company company, List<DataSet> dataSets)
        {
            CallBackValue = callBackValue;
            Company = company;
            DataSets = dataSets;
        }

        public string CallBackValue { get; }
        public Company Company { get; }
        public List<DataSet> DataSets { get; }
    }
}
