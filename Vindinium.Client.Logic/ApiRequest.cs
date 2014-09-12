using System;
using Vindinium.Common.Entities;

namespace Vindinium.Client.Logic
{
    internal class ApiRequest : IApiRequest
    {
        #region IApiRequest Members

        public Uri Uri { get; set; }
        public string Parameters { get; set; }

        #endregion
    }
}