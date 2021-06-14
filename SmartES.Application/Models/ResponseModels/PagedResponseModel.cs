using System;
using System.Collections.Generic;
using System.Text;

namespace SmartES.Application.Models.ResponseModels
{
    public class PagedResponseModel<TModel>
    {
        public IEnumerable<TModel> Items { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}
