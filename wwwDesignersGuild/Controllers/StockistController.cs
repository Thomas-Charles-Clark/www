using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Threading;
using System.Globalization;
using System.Web;
using System.Data;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Stockists;
using System.Data.SqlClient;
using System.Configuration;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
    public class StockistController : ApiController
    {
        #region Controller routines


        [HttpPost]
        public async Task<StockistSearchSelect> stockistSearch(PassStockistSearch objStockistSearch)
        {
            if (objStockistSearch == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
            }

            //  Has any future date been passed
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            //  Get the database user id from the session
            var session = HttpContext.Current.Session;

            Int64 visitorDbId = -1;

            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            var objCountry = new cmsCountry();

            var objCountryData = await objCountry.countrySelectList(null, null);

            var objCountryItem = objCountryData.countries.Where(ci => ci.Code == objStockistSearch.countryCode).SingleOrDefault();

            //  Perform the search
            var objStockist = new Stockist();

            var objStockistResult = await objStockist.stockistSearch(visitorDbId, objCountryItem.CountryID, useDate, objStockistSearch);
           
            objStockistResult.featuredStockists = objStockistResult.featuredStockists.OrderBy(p => p.SortOrder).ToList<FeaturedStockist>();
            objStockistResult.stockistResults = objStockistResult.stockistResults.OrderBy(p => p.SortOrder).ToList<StockistResult>();

            return objStockistResult;
        }


        [HttpPost]
        public async Task<StockistDetail> stockistDetail(PassStockistDetail stockistDetail)
        {
            //  Has any future date been passed
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            //  Perform the search
            var objStockist = new Stockist();
            var objStockistDetail = await objStockist.stockistDetail(stockistDetail.stockistId, useDate);

            if (objStockistDetail.Info != null)
            {
                objStockistDetail.Info = objStockistDetail.Info.Replace("\n", "<br />");
            }
            
            return objStockistDetail;
        }


        #endregion

        

        [HttpPost]
        public IHttpActionResult GetPieceLevelStockInfo([FromBody]PassPieceFetch objProduct)
        {
            //  Has any future date been passed
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            var result = FetchStockInfo(objProduct, (int?)HttpContext.Current.Session["memberTradeAccountID"], useDate);

            return Json(result);
        }


        public ProductStockInfoReturn FetchStockInfo(PassPieceFetch objProduct, int? memberTradeAccountId, DateTime? useDate)
        {
            var result = new ProductStockInfoReturn();

            var preresult = new List<ProductStockFetchResult>();

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@TradeAccountID", memberTradeAccountId),
                new SqlParameter("@ProductID",  objProduct.ProductID),
                new SqlParameter("@CountryID", objProduct.CountryID),
                new SqlParameter("@LanguageID", objProduct.LanguageID),
                new SqlParameter("@Date", useDate)
            };

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = "[pub].[Stock_Select]";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddRange(parameters.ToArray());

                try
                {
                    conn.Open();

                    var reader = cmd.ExecuteReader();

                    while(reader.Read())
                    {
                        preresult.Add(new ProductStockFetchResult()
                        {
                            StockLocationID = reader.GetInt16(0),
                            StockLocationTypeID = reader.GetByte(1),
                            ProductCode = reader.GetString(2),
                            ProductID = reader.GetInt32(3),
                            StockLocationCode = reader.GetString(4),
                            StockLocationName = reader.GetString(5),
                            StockLocationSortOrder = reader.GetInt64(6),
                            Batch = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Piece = reader.IsDBNull(8) ? null : reader.GetString(8),
                            SortOrder = reader.GetInt64(9),
                            AvailableNowQuantity = reader.GetDecimal(10),
                            BulkQuantity = reader.IsDBNull(11) ? (decimal?)null : reader.GetDecimal(11),
                            WaitingToBePutAwayQuantity = reader.GetDecimal(12),
                            OtherCustomerRingFencedQuantity = reader.GetDecimal(13),
                            OnReserveQuantity = reader.GetDecimal(14),
                            DueInQuantity = reader.GetDecimal(15),
                            DueInDate = reader.IsDBNull(16) ? null : (DateTime?)reader.GetDateTime(16),
                            DueInMessage = reader.IsDBNull(17) ? null : reader.GetString(17),
                            POConfirmed = reader.IsDBNull(18) ? (bool?)null : (bool?)reader.GetBoolean(18)
                        });
                    }

                    // pub.Stock_Message piggybacks on portal.Stock_Message which actually outputs what we're after but doesn't
                    // bother translating it - and won't ever bother translating it. Skip this result set:
                    reader.NextResult();

                    // This final result set can be translated. It isn't, at the time of writing, but at least it can be.
                    reader.NextResult();

                    while (reader.Read())
                    {
                        result.SupplierAvailabilityMessage = reader.IsDBNull(0) ? null : reader.GetString(0);
                    }

                    reader.Close();
                }
                finally
                {
                    conn.Close();
                }
            }

            if (preresult.Any())
            {
                result.Code = preresult.First().ProductCode;
                result.TotalStock = preresult.Select(p => p.AvailableNowQuantity).Sum()?.ToString("N2");
                result.TotalPurchaseOrderLength = preresult.Select(p => p.DueInQuantity).Sum().ToString("N2");

                var batchGrouping = preresult.Where(p => p.StockLocationTypeID == 1 && p.AvailableNowQuantity > 0).GroupBy(p => p.Batch).ToList();

                var supplierBatchGrouping = preresult.Where(p => p.StockLocationTypeID == 2 && p.AvailableNowQuantity > 0).GroupBy(p => p.Batch).ToList();

                result.Batches = batchGrouping.Select(p => new Batch()
                {
                    BatchNumber = p.Key,
                    ItemCount = p.Count().ToString("N2"),
                    Length = p.Select(q => q.AvailableNowQuantity).Sum()?.ToString("N2"),
                    Pieces = p.Select(q => new BatchPiece() { Length = q.AvailableNowQuantity?.ToString("N2"), Quantity = "1" }).ToList()
                }).ToList();

                result.SupplierStock = supplierBatchGrouping.Select(p => new Batch()
                {
                    BatchNumber = p.Key,
                    ItemCount = p.Count().ToString("N2"),
                    Length = p.Select(q => q.AvailableNowQuantity).Sum()?.ToString("N2"),
                    Pieces = p.Select(q => new BatchPiece() { Length = q.AvailableNowQuantity?.ToString("N2"), Quantity = "1" }).ToList()
                }).ToList();

                result.PurchaseOrders = preresult.Where(p => p.DueInQuantity > 0).OrderBy(p => p.DueInDate).Select(p => new PurchaseOrder() {
                    PurchaseOrderNumber = null,
                    DueDate = p.DueInDate?.ToString("dd/MM/yyyy"),
                    Length = p.DueInQuantity.ToString("N2"),
                    POConfirmed = p.POConfirmed
                }).ToList();
            }
                 
            return result;
        }

        public class PassProductID
        {
            public int ProductID { get; set; }
        }

        public class PassPieceFetch
        {
            public int ProductID { get; set; }
            public short CountryID { get; set; }
            public byte LanguageID { get; set; }
        }

        public class BasketItemAllocationStockFetchResult
        {
            public short StockLocationID { get; set; }
            public byte StockLocationTypeID { get; set; }
            public string StockLocationCode { get; set; }
            public string StockLocationName { get; set; }
            public long StockLocationSortOrder { get; set; }
            public string Batch { get; set; }
            public string Piece { get; set; }
            public long SortOrder { get; set; }
            public decimal? AvailableNowQuantity { get; set; }
            public decimal? WaitingToBePutAwayQuantity { get; set; }
            public decimal? BulkQuantity { get; set; }
            public decimal? ThisCustomerRingFencedQuantity { get; set; }
            public decimal? OtherCustomerRingFencedQuantity { get; set; }
            public decimal OnReserveQuantity { get; set; }
            public decimal DueInQuantity { get; set; }
            public DateTime? DueInDate { get; set; }
            public string DueInMessage { get; set; }
        }

        public class ProductStockFetchResult : BasketItemAllocationStockFetchResult
        {
            public int ProductID { get; set; }
            public string ProductCode { get; set; }
            public bool? POConfirmed { get; set; }
        }

        public class ProductStockInfoReturn
        {
            public ProductStockInfoReturn() { }

            public string Code { get; set; }
            public string TotalStock { get; set; }
            public List<Batch> Batches { get; set; }
            public List<Batch> SupplierStock { get; set; }
            public List<PurchaseOrder> PurchaseOrders { get; set; }
            public string TotalPurchaseOrderLength { get; set; }

            public string SupplierAvailabilityMessage { get; set; }
        }

        public class Batch
        {
            public Batch() { }

            public string BatchNumber { get; set; }
            public string ItemCount { get; set; }
            public string Length { get; set; }
            public List<BatchPiece> Pieces { get; set; }
        }

        public class BatchPiece
        {
            public BatchPiece() { }
            
            public string Quantity { get; set; }
            public string Length { get; set; }
        }

        public class PurchaseOrder
        {
            public PurchaseOrder() { }

            public string PurchaseOrderNumber { get; set; }
            public string DueDate { get; set; }
            public string Length { get; set; }
            public bool? POConfirmed { get; set; }
        }
    }
}
