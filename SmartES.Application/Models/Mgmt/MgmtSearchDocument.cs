using Nest;
using System;
using System.Linq;

namespace SmartES.Application.Models.Mgmt
{
    public class MgmtSearchDocument
    {
        public MgmtSearchDocument()
        {

        }

        public MgmtSearchDocument(MgmtDetailsModel model)
        {
            MgmtId = model.MgmtId;
            SearchTexts = new[] { model.Name, model.Market, model.State }
                .Union(model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Union(model.Market.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Union(model.State.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            Market = model.Market;
            State = model.State;
            Data = model;
        }

        [PropertyName("id")]
        public int MgmtId { get; set; }
        [SearchAsYouType(Analyzer = "i_analyzer", SearchAnalyzer = "s_analyzer")]
        public string[] SearchTexts { get; set; }
        [Keyword()]
        public string Market { get; set; }
        [Keyword]
        public string State { get; set; }
        [Object(Enabled = false)]
        public MgmtDetailsModel Data { get; set; }
    }
}
