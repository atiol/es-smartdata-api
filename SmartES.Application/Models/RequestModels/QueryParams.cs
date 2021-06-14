using SmartES.Application.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartES.Application.Models.RequestModels
{
    public class RequestParamsModel
    {
        public string Query { get; set; }
        public string Market { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = ElasticsearchConstants.MaximumPageSize;
    }
}
