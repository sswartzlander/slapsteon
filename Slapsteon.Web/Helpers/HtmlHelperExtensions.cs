using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Slapsteon.Web.Helpers
{
    public static class HtmlHelperExtensions
    {    
        public static void Repeater<TModelItem>(this HtmlHelper helper,
                                                IEnumerable<TModelItem> items,
                                                Action<TModelItem> renderItem)
        {
            if (items == null)
                return;

            foreach (var item in items)
                renderItem(item);
        }
    }
}