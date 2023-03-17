using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace wwwDesignersGuild.Models.ProductReview
{
    public class ProductReview
    {
        public int ProductID { get; set; }
        public string Code { get; set; }
        public decimal Rating { get; set; }
        public string Date { get; set; }
        public DateTime DateTime { get; set; }
        public string OrderFormName { get; set; }
        public string ReadMoreURL { get; set; }
        public string Review { get; set; }

        private int ReviewWholeStars
        {
            get
            {
                return (int)Math.Truncate(Rating);
            }
        }

        private bool ReviewHalfStar
        {
            get
            {
                return Rating - Math.Truncate(Rating) > 0;
            }
        }

        private int RemainingStars
        {
            get
            {
                return 5 - ReviewWholeStars + (ReviewHalfStar ? 1 : 0);
            }
        }

        public IHtmlString ReviewStarsDisplay()
        {
            var sb = new StringBuilder();

            sb.AppendLine(@"<a href=""#reviews-panel"" class=""product-page-rating"" onclick=""bootstrap.Collapse.getOrCreateInstance(document.getElementById('reviews-panel')).show();"">");

            for (var i = 0; i < ReviewWholeStars; i++)
            {
                sb.AppendLine(@"<i class=""bi bi-star-fill""></i>");
            }

            if (ReviewHalfStar)
            {
                sb.AppendLine(@"<i class=""bi bi-star-half""></i>");
            }

            for (var i = 0; i < RemainingStars; i++)
            {
                sb.AppendLine(@"<i class=""bi bi-star""></i>");
            }

            sb.AppendLine("</a>");

            return new HtmlString($"{sb}");
        }
    }
}