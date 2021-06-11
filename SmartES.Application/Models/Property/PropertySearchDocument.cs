using Nest;
using System;
using System.Linq;

namespace SmartES.Application.Models.Property
{
    public class PropertySearchDocument
    {
        public PropertySearchDocument()
        {

        }
        public PropertySearchDocument(PropertyDetailsModel model)
        {
            PropertyId = model.PropertyId;
            SearchTexts = new[] { model.Name, model.FormerName, model.City, model.State, model.Market, model.StreetAddress}
                .Union(model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Union(model.FormerName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Union(model.City.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Union(model.State.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Union(model.Market.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Union(model.StreetAddress.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            StreetAddress = model.StreetAddress ?? "";
            City = model.City ?? "";
            Market = model.Market ?? "";
            State = model.State ?? "";
            Location = new float[] { model.Lat, model.Lng };
            Data = model;
        }

        [PropertyName("id")]
        public int PropertyId { get; set; }
        [Text(Analyzer = "i_analyzer", SearchAnalyzer = "s_analyzer")]
        public string[] SearchTexts { get; set; }
        [Keyword]
        public string StreetAddress { get; set; }
        [Keyword]
        public string City { get; set; }
        [Keyword]
        public string Market { get; set; }
        [Keyword]
        public string State { get; set; }
        [GeoPoint]
        public float[] Location { get; set; }
        [Object(Enabled = false)]
        public PropertyDetailsModel Data { get; set; }
    }
}
