using System;
using Newtonsoft.Json;
using OmniBean.PowerCrypt4.Utilities;

namespace FlamingLeaf.Api
{
    /// <summary>
    ///     FlamingApiRequest
    /// </summary>
    public class FlamingApiRequest
    {
        public string Action { get; set; }

        public string[] Arguments { get; set; }

        public string Metadata { get; set; }

        public int Status { get; set; }

        public static string CraftApiRequest(FlamingApiRequest apiRequest)
        {
            var rJson = JsonConvert.SerializeObject(apiRequest);
            var j64 = Convert.ToBase64String(rJson.GetBytes());
            return j64;
        }
    }
}