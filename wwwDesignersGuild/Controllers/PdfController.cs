using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Visitor;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading;
using wwwDesignersGuild.Models.Checkout;
using wwwDesignersGuild.Models;

namespace wwwDesignersGuild.Controllers
{
    public class PdfController : Controller
    {

        public async Task<FileStreamResult> GenerateReturn(string countryCode, string returnIdentifier)
        {
            //  Must be logged in
            // Not anymore! Tom 06/12/2023 = Returns from Order Confirmations! Brilliant.
            //if (dgFramework.isUserLoggedIn(Session["loginStatus"]) == false)
            //{
            //    return null;
            //}

            //  Get the basket country details
            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

            if (dgFramework.doesCountryExistFromCode(countryCode, objCountryData)) { }
            else
            {
                return null;
            }

            var objCountryItem = dgFramework.getCountryFromCode(countryCode, objCountryData);

            //  Set the resource file language
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objCountryItem.DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;


            //  Get the visitor id
            long visitorDbId;
            if (Session["visitorIdFromDatabase"] == null)
            {
                return null;
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }


            //  Request the paperwork
            var visitorFactory = new VisitorFactory();
            var returnPaperworkData = await visitorFactory.getReturnPaperwork(Convert.ToInt32(dgCrypto.Decrypt(returnIdentifier)), objCountryItem.LanguageID, visitorDbId);

            if (returnPaperworkData.memberReturnItems.Count() == 0)
            {
                return null;
            }

            //  Generate the "Return" PDF
            BuildPDF(countryCode, objCountryItem, returnPaperworkData, Request.MapPath("~"), Response);

            return null;
        }

        public async Task<FileStreamResult> GenerateReceipt(string countryCode, string transactionIdentifier)
        {

            //  Must be logged in
            if (dgFramework.isUserLoggedIn(Session["loginStatus"]) == false)
            {
                return null;
            }

            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

            if (dgFramework.doesCountryExistFromCode(countryCode, objCountryData)) { }
            else
            {
                return null;
            }

            var objCountryItem = dgFramework.getCountryFromCode(countryCode, objCountryData);

            //  Set the resource file language
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objCountryItem.DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            //  Get the visitor id
            long visitorDbId;
            if (Session["visitorIdFromDatabase"] == null)
            {
                return null;
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }


            //  Get the return data
            var visitorFactory = new VisitorFactory();
            var receiptData = await visitorFactory.getReceiptData(visitorDbId, Convert.ToInt32(dgCrypto.Decrypt(transactionIdentifier)));

            if (receiptData.receiptLines.Count() == 0)
            {
                return null;
            }

            //  Generate the "Receipt" PDF
            BuildReceiptPDF(countryCode, objCountryItem, receiptData, Request.MapPath("~"), Response);

            return null;
        }

        public async Task<FileStreamResult> GenerateGiftVoucher(string culture, string id)
        {
            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);

            if (dgFramework.doesCountryExistFromLanguageCulture(culture, objCountryData)) { }
            else
            {
                return null;
            }

            var objCountryItem = dgFramework.getCountryFromLanguageCulture(culture, objCountryData);

            //  Set the resource file language
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objCountryItem.DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            var db = new checkoutDb();

            var giftVoucherCode = dgCrypto.Decrypt(HttpUtility.UrlDecode(id));

            var result = await db.getGiftVoucherForPDF(giftVoucherCode);

            BuildGiftVoucherPDF(result, objCountryItem, Request.MapPath("~"), Response);

            return null;
        }

        #region Build Returns PDF

        public static void BuildPDF(String basketCountry, countryItem objCountryItem, memberReturnPaperworkGenerate returnPaperworkData, String rootPath, HttpResponseBase response)
        {
            try
            {
                MemoryStream fs = new MemoryStream();
                Document doc;
                
                if (basketCountry.ToUpper() == "UK")
                {
                    doc = new Document(PageSize.A4, 0, 0, 20, 20);
                }
                else
                {
                    doc = new Document(PageSize.LETTER, 0, 0, 20, 20);
                }
                    
                PdfWriter pdfWriter = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                BaseFont gillSansMT = BaseFont.CreateFont(rootPath + "css\\dist\\fonts\\src\\gill-sans-mt.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                Font font28 = new Font(gillSansMT, 28, Font.NORMAL);
                Font font18 = new Font(gillSansMT, 16, Font.NORMAL);
                Font font20BoldUnderline = new Font(gillSansMT, 20, Font.BOLD | Font.UNDERLINE);
                Font font20Bold = new Font(gillSansMT, 20, Font.BOLD);
                Font font20 = new Font(gillSansMT, 20, Font.NORMAL);

                PdfPTable page1 = new PdfPTable(1);
                PdfPTable page2 = new PdfPTable(1);
                PdfPTable page2a = new PdfPTable(1);

                Image logo = Image.GetInstance(rootPath + "Content\\images\\logo\\DESIGNERS_GUILD_BLACK_BIG.tif");
                logo.ScalePercent(10);
                PdfPCell header = new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingTop = 25,
                    PaddingBottom = 30
                };

                Chunk lineChunk = new Chunk("----------------------------", font28);
                Phrase linePhrase = new Phrase(lineChunk);
                Paragraph lineParagraph = new Paragraph(linePhrase);
                PdfPCell line = new PdfPCell(lineParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER
                };

                Chunk returnsChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryPDFReturnsLabel", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font28);
                Phrase returnsPhrase = new Phrase(returnsChunk);
                Paragraph returnsParagraph = new Paragraph(returnsPhrase);
                PdfPCell returnsLabel = new PdfPCell(returnsParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER
                };

                Chunk pleaseAttachChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryPDFAttachText", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font18);
                Phrase pleaseAttachPhrase = new Phrase(pleaseAttachChunk);
                Paragraph pleaseAttachParagraph = new Paragraph(pleaseAttachPhrase);
                PdfPCell pleaseAttach = new PdfPCell(pleaseAttachParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingTop = 45
                };
                PdfPCell pleaseAttachBottom = new PdfPCell(pleaseAttachParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER
                };

                Chunk lineLongChunk = new Chunk("----------------------------------------------------", font28);
                Phrase lineLongPhrase = new Phrase(lineLongChunk);
                Paragraph lineLongParagraph = new Paragraph(lineLongPhrase);
                PdfPCell lineLong = new PdfPCell(lineLongParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingLeft = 0
                };

                String returnAddress = returnPaperworkData.memberReturnHeaders[0].ReturnsAddress.Replace(",,", ",");
                returnAddress = returnAddress.Replace(", ,", ",");
                Chunk addressChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryPDFAddressPrefix", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + System.Environment.NewLine + returnAddress, font28);
                Phrase addressPhrase = new Phrase(addressChunk);
                Paragraph addressParagraph = new Paragraph(addressPhrase);
                addressParagraph.SetLeading(100f, 100f);
                PdfPCell address = new PdfPCell(addressParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER
                };
                address.PaddingTop = address.PaddingBottom = 60;
                address.SetLeading(1.25f, 1.25f);

                Chunk returnsSlipChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryPDFReturnsSlip", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font28);
                Phrase returnsSlipPhrase = new Phrase(returnsSlipChunk);
                Paragraph returnsSlipParagraph = new Paragraph(returnsSlipPhrase);
                PdfPCell returnsSlip = new PdfPCell(returnsSlipParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER
                };

                Chunk importantChunk = new Chunk(HtmlHelperExtra.Resource("Important", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font20BoldUnderline);
                Phrase importantPhrase = new Phrase(importantChunk);
                Paragraph importantParagraph = new Paragraph(importantPhrase);
                PdfPCell important = new PdfPCell(importantParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingTop = 35
                };

                Chunk pleaseIncludeChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryPDFIncludeText", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font18);
                Phrase pleaseIncludePhrase = new Phrase(pleaseIncludeChunk);
                Paragraph pleaseIncludeParagraph = new Paragraph(pleaseIncludePhrase);
                PdfPCell pleaseInclude = new PdfPCell(pleaseIncludeParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingTop = 15
                };

                Chunk orderNumberChunk = new Chunk(HtmlHelperExtra.Resource("OrderNumber", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + " " + returnPaperworkData.memberReturnHeaders[0].OrderHeadID, font20Bold);
                Phrase orderNumberPhrase = new Phrase(orderNumberChunk);
                Paragraph orderNumberParagraph = new Paragraph(orderNumberPhrase);
                PdfPCell orderNumber = new PdfPCell(orderNumberParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingTop = 15,
                    PaddingBottom = 15
                };

                Chunk pleaseIndicateChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryPleaseIndicate", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font20Bold);
                Phrase pleaseIndicatePhrase = new Phrase(pleaseIndicateChunk);
                Paragraph pleaseIndicateParagraph = new Paragraph(pleaseIndicatePhrase);
                PdfPCell pleaseIndicate = new PdfPCell(pleaseIndicateParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingTop = 15
                };


                Chunk customerContactDetailsChunk;
                Phrase customerContactDetailsPhrase;
                Chunk customerDetailsChunk;
                if (returnPaperworkData.memberReturnDetails.Count > 0)
                {
                    customerContactDetailsChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryCustomerContactDetails", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + "\n", font20);
                    customerContactDetailsPhrase = new Phrase(customerContactDetailsChunk);
                    customerDetailsChunk = new Chunk(String.Format("{0}\n{1}\n{2}", returnPaperworkData.memberReturnDetails[0].FullName, returnPaperworkData.memberReturnDetails[0].Email, returnPaperworkData.memberReturnDetails[0].PhoneNumber), font18);
                }
                else
                {
                    customerContactDetailsPhrase = new Phrase();
                    customerDetailsChunk = new Chunk();
                }
                Phrase customerDetailsPhrase = new Phrase(customerDetailsChunk);
                Paragraph customerContactParagraph = new Paragraph();
                customerContactParagraph.Add(customerContactDetailsPhrase);
                customerContactParagraph.Add(customerDetailsPhrase);
                PdfPCell customerContact = new PdfPCell(customerContactParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_RIGHT,
                    PaddingTop = 45
                };
                customerContact.SetLeading(1.25f, 1.25f);

                page1.AddCell(header);
                page1.AddCell(line);
                page1.AddCell(returnsLabel);
                page1.AddCell(line);

                page1.AddCell(pleaseAttach);
                page1.AddCell(lineLong);

                page1.AddCell(address);
                page1.AddCell(lineLong);
                page1.AddCell(pleaseAttachBottom);

                page2.AddCell(header);
                page2.AddCell(line);
                page2.AddCell(returnsSlip);
                page2.AddCell(line);

                page2.AddCell(important);
                page2.AddCell(pleaseInclude);
                page2.AddCell(orderNumber);

                PdfPTable table = createTable(returnPaperworkData.memberReturnItems, rootPath, objCountryItem);

                page2a.AddCell(pleaseIndicate);
                page2a.AddCell(customerContact);

                doc.Add(page1);
                doc.NewPage();
                doc.Add(page2);
                doc.Add(table);
                doc.Add(page2a);
                doc.Close();
                pdfWriter.Close();

                response.ContentType = $"application/pdf";
                response.Buffer = true;
                response.Clear();
                response.OutputStream.Write(fs.GetBuffer(), 0, fs.GetBuffer().Length);
                response.OutputStream.Flush();
                response.End();
                fs.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static PdfPTable createTable(List<memberReturnItem> items, String rootPath, countryItem objCountryItem)
        {
            BaseFont gillSansMT = BaseFont.CreateFont(rootPath + "css\\dist\\fonts\\src\\gill-sans-mt.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
            Font font12 = new Font(gillSansMT, 12);
            Font font12Bold = new Font(gillSansMT, 12, Font.BOLD);

            PdfPTable table = new PdfPTable(5)
            {
                HeaderRows = 1,
                SplitLate = true,
                SplitRows = true
            };
            table.DefaultCell.PaddingBottom = 5f;

            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryPDFCode", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryPDFName", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryPDFQuantity", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryPDFReturnReason", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryPDFInThisParcel", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));

            PdfPCell YN = new PdfPCell
            {
                BackgroundColor = new BaseColor(242, 242, 242)
            };

            foreach (memberReturnItem item in items)
            {
                table.AddCell(new Phrase(item.Code, font12));
                table.AddCell(new Phrase(item.Name, font12));
                table.AddCell(new Phrase(item.ReturnedQuantity.ToString(), font12));
                table.AddCell(new Phrase(item.ReturnReason, font12));
                table.AddCell(YN);
            }

            return table;
        }


        #endregion

        #region Build Receipt PDF

        public static void BuildReceiptPDF(String basketCountry, countryItem objCountryItem, memberReceiptGenerate receiptData, String rootPath, HttpResponseBase response)
        {
            try
            {
                MemoryStream fs = new MemoryStream();
                Document doc;
                
                if (basketCountry.ToUpper() == "UK")
                {
                    doc = new Document(PageSize.A4, 0, 0, 20, 20);
                }
                else
                {
                    doc = new Document(PageSize.LETTER, 0, 0, 20, 20);
                }
                PdfWriter pdfWriter = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                BaseFont gillSansMT = BaseFont.CreateFont(rootPath + "css\\dist\\fonts\\src\\gill-sans-mt.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                Font font28 = new Font(gillSansMT, 28, Font.NORMAL);
                Font font18 = new Font(gillSansMT, 16, Font.NORMAL);
                Font font14Bold = new Font(gillSansMT, 14, Font.BOLD);
                Font font20BoldUnderline = new Font(gillSansMT, 20, Font.BOLD | Font.UNDERLINE);
                Font font20Bold = new Font(gillSansMT, 20, Font.BOLD);
                Font font20 = new Font(gillSansMT, 20, Font.NORMAL);

                PdfPTable page1 = new PdfPTable(1);

                Image logo = Image.GetInstance(rootPath + "\\Content\\images\\logo\\DESIGNERS_GUILD_BLACK_BIG.tif");
                logo.ScalePercent(10);
                PdfPCell header = new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_CENTER,
                    PaddingTop = 25,
                    PaddingBottom = 30
                };

                Chunk invoiceChunk = new Chunk(HtmlHelperExtra.Resource("Receipt", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font28);
                Phrase invoicePhrase = new Phrase(invoiceChunk);
                Paragraph invoiceParagraph = new Paragraph(invoicePhrase);
                PdfPCell invoice = new PdfPCell(invoiceParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_RIGHT
                };

                Chunk orderSummaryChunk = new Chunk("\n\n\n" + HtmlHelperExtra.Resource("OrderSummary", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + "\n\n", font14Bold);
                Phrase orderSummaryPhrase = new Phrase(orderSummaryChunk);
                Paragraph orderSummaryParagraph = new Paragraph(orderSummaryPhrase);
                PdfPCell orderSummary = new PdfPCell(orderSummaryParagraph)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Rectangle.ALIGN_LEFT
                };

                page1.AddCell(header);
                page1.AddCell(invoice);
                page1.AddCell(orderSummary);

                PdfPTable summaryTable = createReceiptSummaryTable(receiptData.receiptSummarys[0], rootPath, objCountryItem);

                doc.Add(page1);
                doc.Add(summaryTable);

                foreach(receiptHeader order in receiptData.receiptHeaders)
                {
                    PdfPTable orderInformation = createReceiptOrderInformationBlock(order, rootPath, objCountryItem);
                    var orderItems = from d in receiptData.receiptLines
                                                   where (order.OrderHeadID == d.OrderHeadID)
                                                   select d;

                    PdfPTable table = createReceiptItemTable(orderItems, rootPath, objCountryItem);
                    doc.Add(orderInformation);
                    doc.Add(table);
                }

                doc.Close();

                pdfWriter.Close();

                response.ContentType = "application/pdf";
                response.Buffer = true;
                response.Clear();
                response.OutputStream.Write(fs.GetBuffer(), 0, fs.GetBuffer().Length);
                response.OutputStream.Flush();
                response.End();
                fs.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static PdfPTable createReceiptSummaryTable(receiptSummary item, String rootPath, countryItem objCountryItem)
        {
            BaseFont gillSansMT = BaseFont.CreateFont(rootPath + "css\\dist\\fonts\\src\\gill-sans-mt.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
            Font font12 = new Font(gillSansMT, 12);
            Font font12Bold = new Font(gillSansMT, 12, Font.BOLD);

            PdfPTable table = new PdfPTable(2)
            {
                HeaderRows = 1,
                SplitLate = true,
                SplitRows = true
            };
            table.SetWidths(new int[] { 100, 50 });
            table.WidthPercentage = 40f;

            table.DefaultCell.PaddingBottom = 5f;
            String totalItemsVal = String.IsNullOrEmpty(item.TotalItems.ToString()) ? "0.00" : ((decimal)item.TotalItems).ToString("###,##0.00");
            String totalGiftBoxVal = String.IsNullOrEmpty(item.TotalGiftBox.ToString()) ? "0.00" : ((decimal)item.TotalGiftBox).ToString("###,##0.00");
            String totalPandPVal = String.IsNullOrEmpty(item.TotalPandP.ToString()) ? "0.00" : ((decimal)item.TotalPandP).ToString("###,##0.00");
            String totalPromotionDiscountVal = String.IsNullOrEmpty(item.TotalPromotionDiscount.ToString()) ? "0.00" : ((decimal)item.TotalPromotionDiscount).ToString("###,##0.00");
            String totalUSSalesTaxVal = String.IsNullOrEmpty(item.TotalUSSalesTax.ToString()) ? "0.00" : ((decimal)item.TotalUSSalesTax).ToString("###,##0.00");
            String totalVATExemptionVal = String.IsNullOrEmpty(item.TotalVATExemption.ToString()) ? "0.00" : ((decimal)item.TotalVATExemption).ToString("###,##0.00");
            String totalPayableVal = String.IsNullOrEmpty(item.TotalPayable.ToString()) ? "0.00" : ((decimal)item.TotalPayable).ToString("###,##0.00");

            string currencySymbol = HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();

            String totalItems = String.Format("{0}{1}", currencySymbol, totalItemsVal);
            String totalGiftBox = String.Format("{0}{1}", currencySymbol, totalGiftBoxVal);
            String totalPandP = String.Format("{0}{1}", currencySymbol, totalPandPVal);
            String totalPromotionDiscount = String.Format("{0}{1}", currencySymbol, totalPromotionDiscountVal);
            String totalUSSalesTax = String.Format("{0}{1}", currencySymbol, totalUSSalesTaxVal);
            String totalVATExemption = String.Format("{0}{1}", currencySymbol, totalVATExemptionVal);
            String totalPayable = String.Format("{0}{1}", currencySymbol, totalPayableVal);

            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptPDFItems", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
            table.AddCell(new Phrase(totalItems, font12));

            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptPDFGiftBox", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
            table.AddCell(new Phrase(totalGiftBox, font12));

            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptPDFPostagePackaging", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
            table.AddCell(new Phrase(totalPandP, font12));

            if (totalPromotionDiscountVal != "0.00")
            {
                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
                table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptPDFPromotionDiscount", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
                table.AddCell(new Phrase(totalPromotionDiscount, font12));
            }

            if (totalUSSalesTaxVal != "0.00")
            {
                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
                table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptUSSaleTax", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
                table.AddCell(new Phrase(totalUSSalesTax, font12));
            }

            if (totalVATExemptionVal != "0.00")
            {
                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
                table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptVATExemption", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
                table.AddCell(new Phrase(totalVATExemption, font12));
            }

            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptPDFTotalPaid", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
            table.AddCell(new Phrase(totalPayable, font12));

            PdfPTable tableContainer = new PdfPTable(2)
            {
                HeaderRows = 1,
                SplitLate = true,
                SplitRows = true
            };
            tableContainer.DefaultCell.Border = 0;
            table.SetWidths(new int[] { 100, 50 });


            tableContainer.AddCell("");
            tableContainer.AddCell("");

            tableContainer.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
            tableContainer.AddCell(table);
            tableContainer.AddCell("");

            return tableContainer;
        }

        public static PdfPTable createReceiptOrderInformationBlock(receiptHeader order, String rootPath, countryItem objCountryItem)
        {
            BaseFont gillSansMT = BaseFont.CreateFont(rootPath + "css\\dist\\fonts\\src\\gill-sans-mt.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);

            PdfPTable orderSection = new PdfPTable(1);
            Font font14Bold = new Font(gillSansMT, 14, Font.BOLD);
            Font font14 = new Font(gillSansMT, 14, Font.NORMAL);
            Font font12 = new Font(gillSansMT, 12, Font.NORMAL);
            _ = new Font(gillSansMT, 12, Font.BOLD);

            Chunk orderNumberChunk = new Chunk("\n\n\n" + HtmlHelperExtra.Resource("OrderNumber", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + " " + order.OrderHeadID, font14Bold);
            Phrase orderNumberPhrase = new Phrase(orderNumberChunk);
            Paragraph orderNumberParagraph = new Paragraph(orderNumberPhrase);
            PdfPCell orderNumber = new PdfPCell(orderNumberParagraph)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Rectangle.ALIGN_LEFT
            };

            Chunk orderDateChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryReceiptPDFOrderDate", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + " " + order.CreatedDT.ToString("dddd dd MMMM yyyy"), font12);
            Phrase orderDatePhrase = new Phrase(orderDateChunk);
            Paragraph orderDateParagraph = new Paragraph(orderDatePhrase);
            PdfPCell orderDate = new PdfPCell(orderDateParagraph)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Rectangle.ALIGN_LEFT
            };


            String name = String.IsNullOrEmpty(order.FullName?.ToString()) ? "" : order.FullName + ", ";
            String addressLine1 = String.IsNullOrEmpty(order.AddressLine1?.ToString()) ? "" : order.AddressLine1 + ", ";
            String addressLine2 = String.IsNullOrEmpty(order.AddressLine2?.ToString()) ? "" : order.AddressLine2 + ", ";
            String town = String.IsNullOrEmpty(order.Town?.ToString()) ? "" : order.Town + ", ";
            String county = String.IsNullOrEmpty(order.County?.ToString()) ? "" : order.County + ", ";
            String postcode = String.IsNullOrEmpty(order.PostCode?.ToString()) ? "" : order.PostCode + ", ";
            String country = String.IsNullOrEmpty(order.Country?.ToString()) ? "" : order.Country;

            String deliveredAddress = String.Format("{0}{1}{2}{3}{4}{5}{6}", name, addressLine1, addressLine2, town, county, postcode, country);
            Chunk deliveredToChunk = new Chunk(HtmlHelperExtra.Resource("orderHistoryReceiptPDFDeliveredTo", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + " " + deliveredAddress, font14);
            Phrase deliveredToPhrase = new Phrase(deliveredToChunk);
            Paragraph deliveredToParagraph = new Paragraph(deliveredToPhrase);
            PdfPCell deliveredTo = new PdfPCell(deliveredToParagraph)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Rectangle.ALIGN_LEFT,
                PaddingBottom = 20
            };

            orderSection.AddCell(orderNumber);
            orderSection.AddCell(orderDate);
            orderSection.AddCell(deliveredTo);

            return orderSection;

        }

        public static PdfPTable createReceiptItemTable(IEnumerable<receiptLine> items, String rootPath, countryItem objCountryItem)
        {
            BaseFont gillSansMT = BaseFont.CreateFont(rootPath + "css\\dist\\fonts\\src\\gill-sans-mt.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
            Font font12 = new Font(gillSansMT, 12);
            Font font12Bold = new Font(gillSansMT, 12, Font.BOLD);

            PdfPTable table = new PdfPTable(5)
            {
                HeaderRows = 1,
                SplitLate = true,
                SplitRows = true
            };
            table.DefaultCell.PaddingBottom = 5f;

            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptProductCode", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryPDFName", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptQuantity", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptPrice", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));
            table.AddCell(new Phrase(HtmlHelperExtra.Resource("orderHistoryReceiptSubTotal", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString(), font12Bold));

            foreach (receiptLine item in items)
            {
                var currencySymbol = HtmlHelperExtra.Resource("currencySymbol", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString();
                String price = String.Format("{0}{1}", currencySymbol, ((decimal)item.FinalPriceInc).ToString("###,##0.00"));
                String subtotal = String.Format("{0}{1}", currencySymbol, ((decimal)item.SubTotal).ToString("###,##0.00"));

                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_LEFT;
                table.AddCell(new Phrase(item.Code, font12));
                table.AddCell(new Phrase(item.Name, font12));
                table.DefaultCell.HorizontalAlignment = Rectangle.ALIGN_RIGHT;
                table.AddCell(new Phrase(item.Quantity.ToString(), font12));
                table.AddCell(new Phrase(price, font12));
                table.AddCell(new Phrase(subtotal, font12));
            }

            return table;
        }

        #endregion

        #region Build Gift Voucher PDF



        public static void BuildGiftVoucherPDF(
            GiftVoucherPDF objGiftVoucher,
            countryItem objCountryItem,
            string rootPath,
            HttpResponseBase response)
        {
            
            MemoryStream fs = new MemoryStream();
            Document doc = new Document(PageSize.A6.Rotate(), 0, 0, 20, 0);

            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, fs);
            doc.Open();

            iTextSharp.text.Image background = iTextSharp.text.Image.GetInstance(rootPath + "Content\\images\\general\\gift-voucher-background.png");

            //Resize image depend upon your need
            //For give the size to image
            background.ScaleToFit(420, 300);

            //If you want to choose image as background then,

            background.Alignment = iTextSharp.text.Image.UNDERLYING;

            //If you want to give absolute/specified fix position to image.
            background.SetAbsolutePosition(0, 0);

            doc.Add(background);

            var fontColour = new BaseColor(255, 255, 255);

            BaseFont gillSansMT = BaseFont.CreateFont(rootPath + "css\\dist\\fonts\\src\\gill-sans-mt-custom.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
            var font64 = new Font(gillSansMT, 60, Font.NORMAL, fontColour);
            var font32 = new Font(gillSansMT, 32, Font.NORMAL, fontColour);
            var font28 = new Font(gillSansMT, 28, Font.NORMAL, fontColour);
            var font18 = new Font(gillSansMT, 16, Font.NORMAL, fontColour);
            var font14= new Font(gillSansMT, 14, Font.NORMAL, fontColour);
            var font8= new Font(gillSansMT, 8, Font.NORMAL, fontColour);
            var font14Bold = new Font(gillSansMT, 14, Font.BOLD, fontColour);
            var font14Blue = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.BLUE);
            var font20BoldUnderline = new Font(gillSansMT, 20, Font.BOLD | Font.UNDERLINE, fontColour);
            var font20Bold = new Font(gillSansMT, 20, Font.BOLD, fontColour);
            var font20 = new Font(gillSansMT, 20, Font.NORMAL, fontColour);

            var headTable = new PdfPTable(1);
            var bodyTable = new PdfPTable(2);

            bodyTable.WidthPercentage = 90;

            var footTable = new PdfPTable(1);

            footTable.WidthPercentage = 98;

            headTable.SpacingBefore = 20.0f;

            headTable.DefaultCell.Border = Rectangle.NO_BORDER;

            bodyTable.DefaultCell.Border = Rectangle.NO_BORDER;

            footTable.DefaultCell.Border = Rectangle.NO_BORDER;

            bodyTable.SpacingBefore = 25.0f;

            bodyTable.SpacingAfter = 25.0f;

            bodyTable.SetWidths(new float[] { 40.0f, 60.0f });

            var logo = Image.GetInstance(rootPath + "\\Content\\images\\logo\\dg-logo-512-37.png");
            logo.ScalePercent(50);

            var headerCell = new PdfPCell(logo)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingTop = 20,
                PaddingBottom = 20
            };

            headTable.AddCell(headerCell);

            var anchor = new Anchor("www.designersguild.com", font14) 
            { 
                Leading = 12
            };

            anchor.Reference = "https://www.designersguild.com/";

            var anchorParagraph = new Paragraph(anchor)
            {
                Leading = 14,
                SpacingBefore = 20,
                SpacingAfter = 24,
                Alignment = Element.ALIGN_CENTER,
            };

            var tcsChunk = new Chunk($"{HtmlHelperExtra.Resource("GiftVoucherTCsApply", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now)}", font8);

            var tcsParagraph = new Paragraph(tcsChunk) 
            {
                Alignment = Element.ALIGN_CENTER
            };

            var footerCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                PaddingTop = 0
            };

            footerCell.AddElement(anchorParagraph);

            footerCell.AddElement(tcsParagraph);

            footTable.AddCell(footerCell);

            var giftVoucherValueChunk = new Chunk($"{objGiftVoucher.CurrencySymbol}{objGiftVoucher.ValueInc:N0}", font64);
            
            var giftVoucherChunk = new Chunk($"{HtmlHelperExtra.Resource("GiftVoucher", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now)}", font20);
            
            var expiryDateChunk = new Chunk($"{HtmlHelperExtra.Resource("CheckoutPayGiftExpiry", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now)} {objGiftVoucher.ExpiryDT.Day.convertIntToOrdinal()} {objGiftVoucher.ExpiryDT:MMMM} {objGiftVoucher.ExpiryDT:yyyy}", font14);

            var giftVoucherValuePhrase = new Phrase(64, giftVoucherValueChunk);

            var giftVoucherValueParagraph = new Paragraph(giftVoucherValuePhrase)
            {
                Leading = 42.0f
            };

            var giftVoucherParagraph = new Paragraph(giftVoucherChunk)
            {

            };

            var expiryDateParagraph = new Paragraph(expiryDateChunk) 
            {
                Leading = 16
            };

            var giftVoucherCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_TOP,
                HorizontalAlignment = Element.ALIGN_RIGHT,
            };

            giftVoucherCell.AddElement(giftVoucherValueParagraph);
            giftVoucherCell.AddElement(giftVoucherParagraph);

            var expiryCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                Colspan = 2
            };

            expiryCell.AddElement(expiryDateParagraph);

            //var giftVoucherParagraph2 = new Paragraph(giftVoucherChunk);

            //giftVoucherCell.AddElement(giftVoucherParagraph2);

            var pleaseEnterChunk = new Chunk(HtmlHelperExtra.Resource("PleaseEnter", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + System.Environment.NewLine, font20);

            var codeChunk = new Chunk(objGiftVoucher.Code + System.Environment.NewLine, font20);

            var atCheckoutChunk = new Chunk(HtmlHelperExtra.Resource("AtCheckout", objCountryItem.LanguageID, objCountryItem.CountryID, DateTime.Now).ToString() + System.Environment.NewLine, font20);

            var giftVoucherCodeCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_TOP,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                PaddingLeft = 4
            };

            var pleaseEnterParagraph = new Paragraph(pleaseEnterChunk)
            {
                Leading = 14
            };

            var codeParagraph = new Paragraph(codeChunk)
            {
                Leading = 20,
                SpacingBefore = 6,
                SpacingAfter = 4
            };

            var atCheckoutParagraph = new Paragraph(atCheckoutChunk)
            {
                Leading = 20
            };

            giftVoucherCodeCell.AddElement(pleaseEnterParagraph);
            giftVoucherCodeCell.AddElement(codeParagraph);
            giftVoucherCodeCell.AddElement(atCheckoutParagraph);

            //table.AddCell(header);
            bodyTable.AddCell(giftVoucherCell);
            bodyTable.AddCell(giftVoucherCodeCell);
            bodyTable.AddCell(expiryCell);

            doc.Add(headTable);

            doc.Add(bodyTable);

            doc.Add(footTable);

            doc.Close();

            pdfWriter.Close();

            response.ContentType = "application/pdf";
            response.Buffer = true;
            response.Clear();
            response.OutputStream.Write(fs.GetBuffer(), 0, fs.GetBuffer().Length);
            response.OutputStream.Flush();
            response.End();
            fs.Close();
        }

        #endregion
    }
}