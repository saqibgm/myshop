// RTL Support provided by Credo inc (www.credo.co.il  ||   info@credo.co.il)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Customers;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfService : IPdfService
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IGenericAttributeService _genericAttributeService;


        #endregion

        #region Ctor

        public PdfService(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAddressAttributeFormatter addressAttributeFormatter,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            INopFileProvider fileProvider,
            ICustomerService customerService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            IGenericAttributeService genericAttributeService)
        {
            _addressSettings = addressSettings;
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _addressAttributeFormatter = addressAttributeFormatter;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _measureService = measureService;
            _fileProvider = fileProvider;
            _customerService = customerService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _vendorService = vendorService;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get font
        /// </summary>
        /// <returns>Font</returns>
        protected virtual Font GetFont()
        {
            //nopCommerce supports Unicode characters
            //nopCommerce uses Free Serif font by default (~/App_Data/Pdf/FreeSerif.ttf file)
            //It was downloaded from http://savannah.gnu.org/projects/freefont
            return GetFont(_pdfSettings.FontFileName);
        }

        /// <summary>
        /// Get font
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <returns>Font</returns>
        protected virtual Font GetFont(string fontFileName)
        {
            if (fontFileName == null)
                throw new ArgumentNullException(nameof(fontFileName));

            var fontPath = _fileProvider.Combine(_fileProvider.MapPath("~/App_Data/Pdf/"), fontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }

        /// <summary>
        /// Get font direction
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>Font direction</returns>
        protected virtual int GetDirection(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }

        /// <summary>
        /// Get element alignment
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="isOpposite">Is opposite?</param>
        /// <returns>Element alignment</returns>
        protected virtual int GetAlignment(Language lang, bool isOpposite = false)
        {
            //if we need the element to be opposite, like logo etc`.
            if (!isOpposite)
                return lang.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

            return lang.Rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;
        }

        /// <summary>
        /// Get PDF cell
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <returns>PDF cell</returns>
        protected virtual PdfPCell GetPdfCell(string resourceKey, Language lang, Font font)
        {
            return new PdfPCell(new Phrase(_localizationService.GetResource(resourceKey, lang.Id), font));
        }

        /// <summary>
        /// Get PDF cell
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="font">Font</param>
        /// <returns>PDF cell</returns>
        protected virtual PdfPCell GetPdfCell(object text, Font font, bool noBorder = true)
        {
            var cell = new PdfPCell(new Phrase(text.ToString(), font));
            if (!noBorder) cell.Border = Rectangle.NO_BORDER;
            return cell;

        }

        /// <summary>
        /// Get paragraph
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <param name="args">Locale arguments</param>
        /// <returns>Paragraph</returns>
        protected virtual Paragraph GetParagraph(string resourceKey, Language lang, Font font, params object[] args)
        {
            return GetParagraph(resourceKey, string.Empty, lang, font, args);
        }

        /// <summary>
        /// Get paragraph
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="indent">Indent</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <param name="args">Locale arguments</param>
        /// <returns>Paragraph</returns>
        protected virtual Paragraph GetParagraph(string resourceKey, string indent, Language lang, Font font, params object[] args)
        {
            var formatText = _localizationService.GetResource(resourceKey, lang.Id);
            return new Paragraph(indent + (args.Any() ? string.Format(formatText, args) : formatText), font);
        }

        /// <summary>
        /// Print footer
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="pdfWriter">PDF writer</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        protected virtual void PrintFooter(PdfSettings pdfSettingsByStore, PdfWriter pdfWriter, Rectangle pageSize, Language lang, Font font)
        {
            var newFont = new Font(font.Family, font.Size - 2, font.Style, font.Color);

            if (string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) && string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2))
                return;

            var column1Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1)
                ? new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn1
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            var column2Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2)
                ? new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn2
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

            if (!column1Lines.Any() && !column2Lines.Any())
                return;

            var totalLines = Math.Max(column1Lines.Count, column2Lines.Count);
            const float margin = 23;

            //if you have really a lot of lines in the footer, then replace 9 with 10 or 11
            var footerHeight = totalLines * 9;
            var directContent = pdfWriter.DirectContent;
            directContent.MoveTo(pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight);
            directContent.LineTo(pageSize.GetRight(margin), pageSize.GetBottom(margin) + footerHeight);
            directContent.Stroke();

            var footerTable = new PdfPTable(2)
            {
                WidthPercentage = 100f,
                RunDirection = GetDirection(lang)
            };
            footerTable.SetTotalWidth(new float[] { 250, 250 });

            //column 1
            if (column1Lines.Any())
            {
                var column1 = new PdfPCell(new Phrase())
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                foreach (var footerLine in column1Lines)
                {
                    column1.Phrase.Add(new Phrase(footerLine, newFont));
                    column1.Phrase.Add(new Phrase(Environment.NewLine));
                }

                footerTable.AddCell(column1);
            }
            else
            {
                var column = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
                footerTable.AddCell(column);
            }

            //column 2
            if (column2Lines.Any())
            {
                var column2 = new PdfPCell(new Phrase())
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                foreach (var footerLine in column2Lines)
                {
                    column2.Phrase.Add(new Phrase(footerLine, newFont));
                    column2.Phrase.Add(new Phrase(Environment.NewLine));
                }

                footerTable.AddCell(column2);
            }
            else
            {
                var column = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
                footerTable.AddCell(column);
            }

            footerTable.WriteSelectedRows(0, totalLines, pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight, directContent);
        }

        /// <summary>
        /// Print order notes
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="order">Order</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="font">Font</param>
        protected virtual PdfPTable PrintOrderNotes(PdfSettings pdfSettingsByStore, Order order, Language lang, Document doc, Font font)
        {
            var newFont = new Font(font.Family, 9f, font.Style, font.Color);
            var newTFont = new Font(font.Family, 9f, font.Style, font.Color);
            newTFont.SetStyle(Font.BOLD);
            if (!pdfSettingsByStore.RenderOrderNotes)
                return null;

            var orderNotes = order.OrderNotes
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList();

            if (!orderNotes.Any())
                return null;

            var notesHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cellOrderNote = GetPdfCell("PDFInvoice.OrderNotes", lang, newTFont);
            cellOrderNote.Border = Rectangle.NO_BORDER;
            notesHeader.AddCell(cellOrderNote);


            //var notesTable = new PdfPTable(2)
            //{
            //    RunDirection = GetDirection(lang),
            //    WidthPercentage = 100f
            //};
            //notesTable.SetWidths(lang.Rtl ? new[] { 70, 30 } : new[] { 30, 70 });

            ////created on
            //cellOrderNote = GetPdfCell("PDFInvoice.OrderNotes.CreatedOn", lang, font);
            //cellOrderNote.BackgroundColor = BaseColor.LightGray;
            //cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            //notesTable.AddCell(cellOrderNote);

            ////note
            //cellOrderNote = GetPdfCell("PDFInvoice.OrderNotes.Note", lang, font);
            //cellOrderNote.BackgroundColor = BaseColor.LightGray;
            //cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            //notesTable.AddCell(cellOrderNote);

            foreach (var orderNote in orderNotes)
            {

                //cellOrderNote = GetPdfCell(_dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc), font);
                //cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                //notesTable.AddCell(cellOrderNote);

                //cellOrderNote = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(_orderService.FormatOrderNoteText(orderNote), true, true), font);
                //cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                //notesTable.AddCell(cellOrderNote);
                //should we display a link to downloadable files here?
                //I think, no. Anyway, PDFs are printable documents and links (files) are useful here
                cellOrderNote = GetPdfCell(orderNote.Note, lang, newFont);
                cellOrderNote.Border = Rectangle.NO_BORDER;
                notesHeader.AddCell(cellOrderNote);

            }
            return notesHeader;
            //doc.Add(notesHeader);
            //doc.Add(new Paragraph(" "));
            //doc.Add(notesTable);
        }

        /// <summary>
        /// Print totals
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">PDF document</param>
        protected virtual PdfPTable PrintTotals(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //vendors cannot see totals
            if (vendorId != 0)
                return null;

            //subtotal
            var totalsTable = new PdfPTable(3)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 50f,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            totalsTable.SetWidths(new[] { 55, 20, 25 });
            // Add Quantity Total 
            //int QTotal = order.OrderItems.Sum(x => x.Quantity);
            //if (QTotal > 0)
            //{
            //    var qt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Total-Quantity", lang.Id), font);
            //    qt.HorizontalAlignment = Element.ALIGN_LEFT;
            //    qt.VerticalAlignment = Element.ALIGN_CENTER;
            //    qt.Border = Rectangle.BOTTOM_BORDER;
            //    qt.BorderWidth = 0.3f;
            //    totalsTable.AddCell(qt);
            //    var q0 = GetPdfCell("", font);
            //    q0.HorizontalAlignment = Element.ALIGN_LEFT;
            //    q0.VerticalAlignment = Element.ALIGN_CENTER;
            //    q0.Border = Rectangle.BOTTOM_BORDER;
            //    q0.BorderWidth = 0.3f;
            //    totalsTable.AddCell(q0);
            //    var q = GetPdfCell(Convert.ToString(QTotal), font);
            //    q.HorizontalAlignment = Element.ALIGN_LEFT;
            //    q.VerticalAlignment = Element.ALIGN_CENTER;
            //    q.Border = Rectangle.BOTTOM_BORDER;
            //    q.BorderWidth = 0.3f;

            //    totalsTable.AddCell(q);
            //}
            var newFont = new Font(font.Family, 10f, font.Style, font.Color);
            var newFontB = new Font(font.Family, 10f, font.Style, font.Color);
            newFontB.SetStyle(Font.BOLD);
            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                var orderSubtotalInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                var orderSubtotalInclTaxStr = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, lang, true);

                var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), newFontB);
                pt.HorizontalAlignment = Element.ALIGN_LEFT;
                pt.VerticalAlignment = Element.ALIGN_CENTER;
                pt.Border = Rectangle.BOTTOM_BORDER;
                pt.BorderWidth = 0.3f;
                totalsTable.AddCell(pt);
                int QTotal = order.OrderItems.Sum(x => x.Quantity);
                var p0 = GetPdfCell(QTotal, newFontB);
                p0.HorizontalAlignment = Element.ALIGN_CENTER;
                p0.VerticalAlignment = Element.ALIGN_CENTER;
                p0.Border = Rectangle.BOTTOM_BORDER;
                p0.BorderWidth = 0.3f;
                totalsTable.AddCell(p0);
                var p = GetPdfCell(orderSubtotalInclTaxStr, newFontB);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.VerticalAlignment = Element.ALIGN_CENTER;
                p.Border = Rectangle.BOTTOM_BORDER;
                p.BorderWidth = 0.3f;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax

                var orderSubtotalExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                var orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, lang, false);

                var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), newFontB);
                pt.HorizontalAlignment = Element.ALIGN_LEFT;
                pt.VerticalAlignment = Element.ALIGN_CENTER;
                pt.Border = Rectangle.BOTTOM_BORDER;
                pt.BorderWidth = 0.3f;
                totalsTable.AddCell(pt);
                int QTotal = order.OrderItems.Sum(x => x.Quantity);
                var p0 = GetPdfCell(QTotal, newFontB);
                p0.HorizontalAlignment = Element.ALIGN_CENTER;
                p0.VerticalAlignment = Element.ALIGN_CENTER;
                p0.Border = Rectangle.BOTTOM_BORDER;
                p0.BorderWidth = 0.3f;
                totalsTable.AddCell(p0);
                var p = GetPdfCell(orderSubtotalExclTaxStr, newFontB);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.VerticalAlignment = Element.ALIGN_CENTER;
                p.Border = Rectangle.BOTTOM_BORDER;
                p.BorderWidth = 0.3f;
                totalsTable.AddCell(p);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                    !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    var orderSubTotalDiscountInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(
                        -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);
                    var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Discount", lang.Id), newFont);
                    pt.HorizontalAlignment = Element.ALIGN_LEFT;
                    pt.VerticalAlignment = Element.ALIGN_CENTER;
                    pt.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(pt);
                    var p0 = GetPdfCell("", newFont);
                    p0.HorizontalAlignment = Element.ALIGN_LEFT;
                    p0.VerticalAlignment = Element.ALIGN_CENTER;
                    p0.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p0);
                    var p = GetPdfCell(orderSubTotalDiscountInCustomerCurrencyStr, newFont);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.VerticalAlignment = Element.ALIGN_CENTER;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax

                    var orderSubTotalDiscountExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(
                        -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Discount", lang.Id), newFont);
                    pt.HorizontalAlignment = Element.ALIGN_LEFT;
                    pt.VerticalAlignment = Element.ALIGN_CENTER;
                    pt.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(pt);
                    var p0 = GetPdfCell("", newFont);
                    p0.HorizontalAlignment = Element.ALIGN_LEFT;
                    p0.VerticalAlignment = Element.ALIGN_CENTER;
                    p0.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p0);
                    var p = GetPdfCell(orderSubTotalDiscountInCustomerCurrencyStr, newFont);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.VerticalAlignment = Element.ALIGN_CENTER;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    var orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(
                        orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Shipping", lang.Id), newFont);
                    pt.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pt.VerticalAlignment = Element.ALIGN_CENTER;
                    pt.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(pt);
                    var p0 = GetPdfCell("", newFont);
                    p0.HorizontalAlignment = Element.ALIGN_LEFT;
                    p0.VerticalAlignment = Element.ALIGN_CENTER;
                    p0.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p0);
                    var p = GetPdfCell(orderShippingInclTaxStr, newFont);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.VerticalAlignment = Element.ALIGN_CENTER;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    var orderShippinInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    var orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(
                        orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);
                    var orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(
                        orderShippinInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Shipping", lang.Id), newFont);
                    pt.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pt.VerticalAlignment = Element.ALIGN_CENTER;
                    pt.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(pt);
                    var p0 = GetPdfCell("", newFont);
                    p0.HorizontalAlignment = Element.ALIGN_LEFT;
                    p0.VerticalAlignment = Element.ALIGN_CENTER;
                    p0.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p0); var p = GetPdfCell(orderShippingInclTaxStr, newFont);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.VerticalAlignment = Element.ALIGN_CENTER;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), newFont);
                    pt.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pt.VerticalAlignment = Element.ALIGN_CENTER;
                    pt.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(pt);
                    var p0 = GetPdfCell("", newFont);
                    p0.HorizontalAlignment = Element.ALIGN_LEFT;
                    p0.VerticalAlignment = Element.ALIGN_CENTER;
                    p0.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p0);
                    var p = GetPdfCell(paymentMethodAdditionalFeeInclTaxStr, newFont);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.VerticalAlignment = Element.ALIGN_CENTER;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), newFont);
                    pt.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pt.VerticalAlignment = Element.ALIGN_CENTER;
                    pt.Border = Rectangle.NO_BORDER;
                    pt.BorderWidth = 0.3f;
                    totalsTable.AddCell(pt);
                    var p0 = GetPdfCell("", newFont);
                    p0.HorizontalAlignment = Element.ALIGN_LEFT;
                    p0.VerticalAlignment = Element.ALIGN_CENTER;
                    p0.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p0);
                    var p = GetPdfCell(paymentMethodAdditionalFeeExclTaxStr, newFont);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.VerticalAlignment = Element.ALIGN_CENTER;
                    p.Border = Rectangle.NO_BORDER;
                    p.BorderWidth = 0.3f;
                    totalsTable.AddCell(p);
                }
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, lang);
                }
            }

            if (displayTax)
            {
                var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Tax", lang.Id), newFont);
                pt.HorizontalAlignment = Element.ALIGN_RIGHT;
                pt.VerticalAlignment = Element.ALIGN_CENTER;
                pt.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(pt);
                var taxInfo = order.TaxRates.Split(":");
                var taxRate = taxInfo.Length == 2 ? taxInfo[0] + "%" : "0";
                var p0 = GetPdfCell(taxRate, newFont);
                p0.HorizontalAlignment = Element.ALIGN_CENTER;
                p0.VerticalAlignment = Element.ALIGN_CENTER;
                p0.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p0);
                var p = GetPdfCell(taxStr, newFont);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.VerticalAlignment = Element.ALIGN_CENTER;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(_localizationService.GetResource("PDFInvoice.TaxRate", lang.Id),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = _priceFormatter.FormatPrice(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, lang);

                    var p0 = GetPdfCell("", newFont);
                    p0.HorizontalAlignment = Element.ALIGN_LEFT;
                    p0.VerticalAlignment = Element.ALIGN_CENTER;
                    p0.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p0);
                    var pt = GetPdfCell(taxRate, newFont);
                    pt.HorizontalAlignment = Element.ALIGN_LEFT;
                    pt.VerticalAlignment = Element.ALIGN_CENTER;
                    pt.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(pt);

                    var p = GetPdfCell(taxValue, newFont);
                    p.HorizontalAlignment = Element.ALIGN_LEFT;
                    p.VerticalAlignment = Element.ALIGN_CENTER;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                var orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, lang);

                var p0 = GetPdfCell("", newFont);
                p0.HorizontalAlignment = Element.ALIGN_LEFT;
                p0.VerticalAlignment = Element.ALIGN_CENTER;
                p0.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p0);
                var pt = GetPdfCell(_localizationService.GetResource("PDFInvoice.Discount", lang.Id), newFont);
                pt.HorizontalAlignment = Element.ALIGN_RIGHT;
                pt.VerticalAlignment = Element.ALIGN_CENTER;
                pt.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(pt);

                var p = GetPdfCell(orderDiscountInCustomerCurrencyStr, newFont);
                p.HorizontalAlignment = Element.ALIGN_LEFT;
                p.VerticalAlignment = Element.ALIGN_CENTER;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            ////gift cards
            //foreach (var gcuh in order.GiftCardUsageHistory)
            //{
            //    var gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", lang.Id),
            //        gcuh.GiftCard.GiftCardCouponCode);
            //    var gcAmountStr = _priceFormatter.FormatPrice(
            //        -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
            //        order.CustomerCurrencyCode, false, lang);

            //    var p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
            //    p.HorizontalAlignment = Element.ALIGN_LEFT;
            //    p.VerticalAlignment = Element.ALIGN_CENTER;
            //    p.Border = Rectangle.NO_BORDER;
            //    totalsTable.AddCell(p);

            //}

            ////reward points
            //if (order.RedeemedRewardPointsEntry != null)
            //{
            //    var rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", lang.Id),
            //        -order.RedeemedRewardPointsEntry.Points);
            //    var rpAmount = _priceFormatter.FormatPrice(
            //        -_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
            //        true, order.CustomerCurrencyCode, false, lang);

            //    var p = GetPdfCell($"{rpTitle} {rpAmount}", font);
            //    p.HorizontalAlignment = Element.ALIGN_LEFT;
            //    p.VerticalAlignment = Element.ALIGN_CENTER;
            //    p.Border = Rectangle.NO_BORDER;
            //    totalsTable.AddCell(p);
            //}

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);

            var ptTotal = GetPdfCell(_localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id), newFontB);
            ptTotal.HorizontalAlignment = Element.ALIGN_LEFT;
            ptTotal.VerticalAlignment = Element.ALIGN_CENTER;
            ptTotal.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            ptTotal.BorderWidth = 0.3f;
            totalsTable.AddCell(ptTotal);
            var pTotal0 = GetPdfCell("", newFontB);
            pTotal0.HorizontalAlignment = Element.ALIGN_LEFT;
            pTotal0.VerticalAlignment = Element.ALIGN_CENTER;
            pTotal0.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            pTotal0.BorderWidth = 0.3f;
            totalsTable.AddCell(pTotal0);
            var pTotal = GetPdfCell(orderTotalStr, newFontB);
            pTotal.HorizontalAlignment = Element.ALIGN_LEFT;
            pTotal.VerticalAlignment = Element.ALIGN_CENTER;
            pTotal.Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            pTotal.BorderWidth = 0.3f;
            totalsTable.AddCell(pTotal);

            var lastCell = GetPdfCell("", newFontB);
            lastCell.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(lastCell);
            totalsTable.AddCell(lastCell);
            totalsTable.AddCell(lastCell);

            return totalsTable;
            //doc.Add(totalsTable);
        }

        protected virtual PdfPTable PrintComments(int vendorId, PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //subtotal
            var commentsTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            commentsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            //checkout attributes
            commentsTable.AddCell(PrintCheckoutAttributes(vendorId, order, doc, lang, font));
            //paymetn info
            commentsTable.AddCell(PrintPayment(vendorId, lang, order, font, titleFont, doc));
            commentsTable.AddCell(new Paragraph(""));
            //order notes
            commentsTable.AddCell(PrintOrderNotes(pdfSettingsByStore, order, lang, doc, font));
            commentsTable.AddCell(new Paragraph(""));


            return commentsTable;
            //doc.Add(totalsTable);
        }

        protected virtual PdfPTable PrintMessages(Language lang, Order order, Font font, Font titleFont, Document doc, PdfWriter pdfWriter)
        {
            //subtotal
            var messageTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                TotalWidth = 550
            };
            var yPos = 120;
            var newFontB = new Font(font.Family, 9f, font.Style, font.Color);
            //newFontB.SetStyle(Font.BOLD);

            if (string.IsNullOrEmpty(order.TaxRates) || (order.TaxRates.StartsWith("0") && order.OrderTax == 0))
            {
                var messageVat = GetPdfCell(_localizationService.GetResource("PDFInvoice.Message.NoVat", lang.Id), newFontB);
                messageVat.HorizontalAlignment = Element.ALIGN_LEFT;
                messageVat.VerticalAlignment = Element.ALIGN_CENTER;
                messageVat.Border = Rectangle.NO_BORDER;
                messageTable.AddCell(messageVat);
                yPos = 135;
            }
            var messageDate = GetPdfCell(_localizationService.GetResource("PDFInvoice.Message.InvoiceDate", lang.Id), newFontB);
            messageDate.HorizontalAlignment = Element.ALIGN_LEFT;
            messageDate.VerticalAlignment = Element.ALIGN_CENTER;
            messageDate.Border = Rectangle.NO_BORDER;
            messageTable.AddCell(messageDate);
            var messageTerms= GetPdfCell(_localizationService.GetResource("PDFInvoice.Message.InvoiceTerms", lang.Id), newFontB);
            messageTerms.HorizontalAlignment = Element.ALIGN_LEFT;
            messageTerms.VerticalAlignment = Element.ALIGN_CENTER;
            messageTerms.Border = Rectangle.NO_BORDER;
            messageTable.AddCell(messageTerms);
            //messageTable.WriteSelectedRows(0, -1, 20, yPos, pdfWriter.DirectContent);
            return messageTable;
            //doc.Add(totalsTable);
        }

        /// <summary>
        /// Print checkout attributes
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="order">Order</param>
        /// <param name="doc">Document</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        protected virtual PdfPTable PrintCheckoutAttributes(int vendorId, Order order, Document doc, Language lang, Font font)
        {
            var newTFont = new Font(font.Family, 9f, font.Style, font.Color);
            //newTFont.SetStyle(Font.BOLD);
            //vendors cannot see checkout attributes
            if (vendorId != 0 || string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                return null;
            var attribTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cCheckoutAttributes = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true, true), newTFont);
            cCheckoutAttributes.Border = Rectangle.NO_BORDER;
            cCheckoutAttributes.HorizontalAlignment = Element.ALIGN_LEFT;
            attribTable.AddCell(cCheckoutAttributes);
            return attribTable;
            //doc.Add(attribTable);
        }

        /// <summary>
        /// Print products
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font</param>
        protected virtual void PrintProducts(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            var cellProducts = GetPdfCell("PDFInvoice.Product(s)", lang, titleFont);
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));

            var orderItems = order.OrderItems;

            var count = 4 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { 4, new[] { 50, 20, 10, 20 } },
                { 5, new[] { 45, 15, 15, 10, 15 } },
                { 6, new[] { 40, 13, 13, 12, 10, 12 } }
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);
            productsTable.DefaultCell.BorderWidth = Rectangle.NO_BORDER;
            productsTable.DefaultCell.BorderColor = BaseColor.White;
            //product name
            var cellProductItem = GetPdfCell("PDFInvoice.ProductName", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = GetPdfCell("PDFInvoice.SKU", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = GetPdfCell("PDFInvoice.VendorName", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //price
            cellProductItem = GetPdfCell("PDFInvoice.ProductPrice", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //qty
            cellProductItem = GetPdfCell("PDFInvoice.ProductQuantity", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //total
            cellProductItem = GetPdfCell("PDFInvoice.ProductTotal", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorsByIds(orderItems.Select(item => item.Product.VendorId).ToArray()) : new List<Vendor>();

            foreach (var orderItem in orderItems)
            {
                var p = orderItem.Product;

                //a vendor should have access only to his products
                if (vendorId > 0 && p.VendorId != vendorId)
                    continue;

                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
                pAttribTable.DefaultCell.BorderWidth = Rectangle.NO_BORDER;
                pAttribTable.DefaultCell.BorderColor = BaseColor.White;
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                //product name
                var name = _localizationService.GetLocalized(p, x => x.Name, lang.Id);
                pAttribTable.AddCell(new Paragraph(name, font));
                cellProductItem.AddElement(new Paragraph(name, font));
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }

                //rental info
                if (orderItem.Product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalStartDateUtc.Value)
                        : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalEndDateUtc.Value)
                        : string.Empty;
                    var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);

                    var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                    pAttribTable.AddCell(rentalInfoParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = _productService.FormatSku(p, orderItem.AttributesXml);
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == p.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, false);
                }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //total
                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang, false);
                }

                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }

            doc.Add(productsTable);
        }


        /// <summary>
        /// Print products
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font with sr#</param>
        protected virtual void CustomPrintProducts(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var orderItems = order.OrderItems;

            var count = 7 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { 7, new[] {6, 9, 6,37, 16, 9, 17 } },
                { 8, new[] {6, 9, 6, 33, 14, 11, 7, 14 } },
                { 9, new[] {6, 9, 6, 29, 11, 10, 9, 7, 13 } }
            };
            var newFont = new Font(font.Family, 10f, font.Style, font.Color);
            newFont.SetStyle(Font.BOLD);

            productsTable.DefaultCell.BorderWidth = Rectangle.NO_BORDER;
            productsTable.DefaultCell.BorderColor = BaseColor.White;
            productsTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
            productsTable.DefaultCell.BorderWidth = 1;
            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);
            // Serial Number
            //product name
            var cellSerialItem = GetPdfCell("PDFInvoice.SerialNumber", lang, newFont);
            cellSerialItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
            cellSerialItem.BorderWidth = 0.5f;
            cellSerialItem.PaddingTop = 10;
            cellSerialItem.PaddingBottom = 10;
            cellSerialItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellSerialItem);
            var cellActicleNo = GetPdfCell("PDFInvoice.ArticleNo", lang, newFont);
            cellActicleNo.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
            cellActicleNo.BorderWidth = 0.5f;
            cellActicleNo.PaddingTop = 10;
            cellActicleNo.PaddingBottom = 10;
            cellActicleNo.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellActicleNo);
            var cellPositionItem = GetPdfCell("PDFInvoice.WarehousePosition", lang, newFont);
            cellPositionItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
            cellPositionItem.BorderWidth = 0.5f;
            cellPositionItem.PaddingTop = 10;
            cellPositionItem.PaddingBottom = 10;
            cellPositionItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellPositionItem);
            //product name
            var cellProductItem = GetPdfCell("PDFInvoice.ProductName", lang, newFont);
            cellProductItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
            cellProductItem.BorderWidth = 0.5f;
            cellProductItem.PaddingTop = 10;
            cellProductItem.PaddingBottom = 10;
            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
            productsTable.AddCell(cellProductItem);

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = GetPdfCell("PDFInvoice.SKU", lang, newFont);
                cellProductItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
                cellProductItem.BorderWidth = 0.5f;
                cellProductItem.PaddingTop = 10;
                cellProductItem.PaddingBottom = 10;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = GetPdfCell("PDFInvoice.VendorName", lang, newFont);
                cellProductItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
                cellProductItem.BorderWidth = 0.5f;
                cellProductItem.PaddingTop = 10;
                cellProductItem.PaddingBottom = 10;
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }

            //qty
            cellProductItem = GetPdfCell("PDFInvoice.ProductQuantity", lang, newFont);
            cellProductItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
            cellProductItem.BorderWidth = 0.5f;
            cellProductItem.PaddingTop = 10;
            cellProductItem.PaddingBottom = 10;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //price
            cellProductItem = GetPdfCell("PDFInvoice.ProductPrice", lang, newFont);
            cellProductItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
            cellProductItem.BorderWidth = 0.5f;
            cellProductItem.PaddingTop = 10;
            cellProductItem.PaddingBottom = 10;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //total
            cellProductItem = GetPdfCell("PDFInvoice.ProductTotal", lang, newFont);
            cellProductItem.Border = Rectangle.TOP_BORDER + Rectangle.BOTTOM_BORDER;
            cellProductItem.BorderWidth = 0.5f;
            cellProductItem.PaddingTop = 10;
            cellProductItem.PaddingBottom = 10;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);
            Int16 SerialNo = 1;
            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorsByIds(orderItems.Select(item => item.Product.VendorId).ToArray()) : new List<Vendor>();

            foreach (var orderItem in orderItems)
            {
                var p = orderItem.Product;

                //a vendor should have access only to his products
                if (vendorId > 0 && p.VendorId != vendorId)
                    continue;

                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
                pAttribTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER;
                pAttribTable.DefaultCell.BorderColor = BaseColor.White;
                pAttribTable.DefaultCell.BorderWidth = 1;
                cellProductItem.BorderWidth = 0.5f;


                //Serial Number
                cellProductItem = GetPdfCell((SerialNo++).ToString(), font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                productsTable.AddCell(cellProductItem);

                //Serial Number
                cellProductItem = GetPdfCell((orderItem.ProductId + 10000).ToString(), font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                productsTable.AddCell(cellProductItem);

                //Warehouse Position
                cellProductItem = GetPdfCell((orderItem.Product?.WarehousePosition ?? "").ToString(), font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                productsTable.AddCell(cellProductItem);

                var name = _localizationService.GetLocalized(p, x => x.Name, lang.Id);
                // pAttribTable.AddCell(new Paragraph(name, font));
                var cellProductIName = GetPdfCell(name, font);
                cellProductIName.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                pAttribTable.AddCell(cellProductIName);
                cellProductItem.AddElement(new Paragraph(name, font));
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }

                //rental info
                if (orderItem.Product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalStartDateUtc.Value)
                        : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalEndDateUtc.Value)
                        : string.Empty;
                    var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);

                    var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                    pAttribTable.AddCell(rentalInfoParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = _productService.FormatSku(p, orderItem.AttributesXml);
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.Border = Rectangle.NO_BORDER;
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellProductItem.PaddingTop = 5;
                    cellProductItem.PaddingBottom = 5;
                    productsTable.AddCell(cellProductItem);
                }

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == p.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.Border = Rectangle.NO_BORDER;
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellProductItem.PaddingBottom = 10;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, false);
                }

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //unit price
                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //total
                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang, false);
                }

                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            doc.Add(productsTable);
        }
        /// <summary>
        /// Print addresses
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="doc">Document</param>
        protected virtual void PrintAddresses(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            var addressTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;
            //addressTable.SetWidths(new[] { 50, 50 });

            //billing info
            PrintBillingInfo(vendorId, lang, titleFont, order, font, doc);
            ////shipping info           

            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }

        /// <summary>
        /// Print shipping info
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">PDF table for address</param>
        protected virtual void PrintShippingInfo(Language lang, Order order, Font titleFont, Font font, Document doc)
        {
            var shippingAddress = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
                HorizontalAlignment = Rectangle.ALIGN_LEFT

            };
            shippingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
            shippingAddress.SetWidths(new[] { 20, 80 });
            var newFont = new Font(font.Family, 12f, font.Style, font.Color);

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                //cell = new PdfPCell();
                //cell.Border = Rectangle.NO_BORDER;
                const string indent = "";

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddress == null)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    shippingAddress.AddCell(GetParagraph("PDFInvoice.ShippingInformation", lang, titleFont));
                    var shippingInformation = "";
                    if (!string.IsNullOrEmpty(order.ShippingAddress.Company))
                    {
                        //shippingAddress.AddCell(GetParagraph("PDFInvoice.Company", indent, lang, font, order.ShippingAddress.Company));
                        shippingInformation += order.ShippingAddress.Company + ", ";
                    }
                    //shippingAddress.AddCell(GetParagraph("PDFInvoice.Name", indent, lang, font, order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName));
                    shippingInformation += order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName + ", ";

                    if (_addressSettings.StreetAddressEnabled)
                    {
                        //shippingAddress.AddCell(GetParagraph("PDFInvoice.Address", indent, lang, font, order.ShippingAddress.Address1));
                        shippingInformation += order.ShippingAddress.Address1 + ", ";
                    }
                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.ShippingAddress.Address2))
                    {
                        //shippingAddress.AddCell(GetParagraph("PDFInvoice.Address2", indent, lang, font, order.ShippingAddress.Address2));
                        shippingInformation += order.ShippingAddress.Address2 + ", ";
                    }
                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{order.ShippingAddress.ZipPostalCode}, " +
                             $"{indent}{order.ShippingAddress.City}" +
                            $"{(!string.IsNullOrEmpty(order.ShippingAddress.County) ? (", " + $"{order.ShippingAddress.County}") : string.Empty)}" +
                            $"{(order.ShippingAddress.StateProvince != null ? (", " + _localizationService.GetLocalized(order.ShippingAddress.StateProvince, x => x.Name, lang.Id)) : string.Empty)}";
                        //shippingAddress.AddCell(new Paragraph(addressLine, font));
                        shippingInformation += addressLine + ", ";

                    }

                    if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                    {
                        //shippingAddress.AddCell(new Paragraph(indent + _localizationService.GetLocalized(order.ShippingAddress.Country, x => x.Name, lang.Id), font));
                        shippingInformation += _localizationService.GetLocalized(order.ShippingAddress.Country, x => x.Name, lang.Id);
                    }
                    //if (_addressSettings.PhoneEnabled)
                    //{
                    //    //shippingAddress.AddCell(GetParagraph("PDFInvoice.Phone", indent, lang, font, order.ShippingAddress.PhoneNumber)); 
                    //}
                    //if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(order.ShippingAddress.FaxNumber))
                    //{
                    //    //shippingAddress.AddCell(GetParagraph("PDFInvoice.Fax", indent, lang, font, order.ShippingAddress.FaxNumber));
                    //}
                    //custom attributes
                    var customShippingAddressAttributes =
                        _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        //TODO: we should add padding to each line (in case if we have several custom address attributes)
                        shippingAddress.AddCell(new Paragraph(
                            indent + HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                    shippingAddress.AddCell(new Paragraph(shippingInformation, font));

                    //shippingAddress.AddCell(new Paragraph(" "));
                }
                else if (order.PickupAddress != null)
                {
                    shippingAddress.AddCell(GetParagraph("PDFInvoice.Pickup", lang, titleFont));
                    if (!string.IsNullOrEmpty(order.PickupAddress.Address1))
                        shippingAddress.AddCell(new Paragraph(
                            $"{indent}{string.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.PickupAddress.Address1)}",
                            font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.City))
                        shippingAddress.AddCell(new Paragraph($"{indent}{order.PickupAddress.City}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.County))
                        shippingAddress.AddCell(new Paragraph($"{indent}{order.PickupAddress.County}", font));
                    if (order.PickupAddress.Country != null)
                        shippingAddress.AddCell(
                            new Paragraph($"{indent}{_localizationService.GetLocalized(order.PickupAddress.Country, x => x.Name, lang.Id)}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.ZipPostalCode))
                        shippingAddress.AddCell(new Paragraph($"{indent}{order.PickupAddress.ZipPostalCode}", font));
                    shippingAddress.AddCell(new Paragraph(" "));
                }

                //shippingAddress.AddCell(GetParagraph("PDFInvoice.ShippingMethod", indent, lang, font, order.ShippingMethod));
                //shippingAddress.AddCell(new Paragraph());
                doc.Add(shippingAddress);
                // addressTable.AddCell(shippingAddress);
            }
            else
            {
                shippingAddress.AddCell(new Paragraph(" "));
                doc.Add(shippingAddress);
            }
        }

        /// <summary>
        /// Print billing info
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">Address PDF table</param>
        protected virtual void PrintBillingInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            var addressTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;

            const string indent = "   ";
            var billingAddress = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            billingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
            var newFont = new Font(font.Family, 12f, font.Style, font.Color);
            //billingAddress.AddCell(GetParagraph("PDFInvoice.BillingInformation", lang, titleFont));

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(order.BillingAddress.Company))
                billingAddress.AddCell(GetPdfCell(order.BillingAddress.Company, newFont, false));
            //billingAddress.AddCell(GetParagraph("PDFInvoice.Company", indent, lang, newFont, order.BillingAddress.Company));

            //billingAddress.AddCell(GetParagraph("PDFInvoice.Name", indent, lang, newFont, order.BillingAddress.FirstName + " " + order.BillingAddress.LastName));
            if (_addressSettings.StreetAddressEnabled)
                billingAddress.AddCell(GetPdfCell(order.BillingAddress.Address1, newFont, false));
            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.BillingAddress.Address2))
                billingAddress.AddCell(GetPdfCell(order.BillingAddress.Address2, newFont, false));
            //if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(order.BillingAddress.FaxNumber))
            //    billingAddress.AddCell(GetParagraph("PDFInvoice.Fax", indent, lang, newFont, order.BillingAddress.FaxNumber));
            if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
            {
                var addressLine = $"{indent}{order.BillingAddress.City}, " +
                    $"{(!string.IsNullOrEmpty(order.BillingAddress.County) ? $"{order.BillingAddress.County}, " : string.Empty)}" +
                    $"{(order.BillingAddress.StateProvince != null ? _localizationService.GetLocalized(order.BillingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                    $"{order.BillingAddress.ZipPostalCode}";
                billingAddress.AddCell(GetPdfCell(addressLine.Trim(), newFont, false));
            }

            if (_addressSettings.CountryEnabled && order.BillingAddress.Country != null)
                billingAddress.AddCell(GetPdfCell(order.BillingAddress.Country.Name, newFont, false));
            //if (_addressSettings.PhoneEnabled)
            //    billingAddress.AddCell(GetParagraph("PDFInvoice.Phone", string.Empty, lang, newFont, order.BillingAddress.PhoneNumber));
            billingAddress.AddCell("");
            billingAddress.AddCell("");
            //VAT number
            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddress.AddCell(GetPdfCell(string.Format(_localizationService.GetResource("PDFInvoice.VATNumber", lang.Id), order.VatNumber), newFont, false));

            ////custom attributes
            //var customBillingAddressAttributes =
            //    _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            //if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            //{
            //    //TODO: we should add padding to each line (in case if we have several custom address attributes)
            //    billingAddress.AddCell(
            //        new Paragraph(indent + HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true), font));
            //}

            ////vendors payment details
            //if (vendorId == 0)
            //{
            //    //payment method
            //    var paymentMethod = _paymentPluginManager.LoadPluginBySystemName(order.PaymentMethodSystemName);
            //    var paymentMethodStr = paymentMethod != null
            //        ? _localizationService.GetLocalizedFriendlyName(paymentMethod, lang.Id)
            //        : order.PaymentMethodSystemName;
            //    if (!string.IsNullOrEmpty(paymentMethodStr))
            //    {
            //        billingAddress.AddCell(new Paragraph(" "));
            //        billingAddress.AddCell(GetParagraph("PDFInvoice.PaymentMethod", indent, lang, newFont, paymentMethodStr));
            //        billingAddress.AddCell(new Paragraph());
            //    }

            //    //custom values
            //    var customValues = _paymentService.DeserializeCustomValues(order);
            //    if (customValues != null)
            //    {
            //        foreach (var item in customValues)
            //        {
            //            billingAddress.AddCell(new Paragraph(" "));
            //            billingAddress.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, newFont));
            //            billingAddress.AddCell(new Paragraph());
            //        }
            //    }
            //}

            addressTable.AddCell(billingAddress);
            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }
        /// <summary>
        /// Print billing info`
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">PrintInvoiceInfo</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">PrintInvoiceInfo PDF table</param>
        protected virtual void PrintInvoiceInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            const string indent = "   ";
            var InvoiceInfo = new PdfPTable(3) { RunDirection = GetDirection(lang) };
            InvoiceInfo.DefaultCell.Border = Rectangle.TOP_BORDER;
            InvoiceInfo.DefaultCell.Padding = 10;
            InvoiceInfo.DefaultCell.BorderWidth = 1;
            InvoiceInfo.HorizontalAlignment = Rectangle.ALIGN_CENTER;
            InvoiceInfo.WidthPercentage = 96f;
            InvoiceInfo.SetWidths(new[] { 40, 30, 30 });
            // Get Saleman BioData
            Core.Domain.Customers.Customer customer = _customerService.GetAllCustomers().Where(x => x.Id == order.CustomerId).FirstOrDefault();
            string customerName = customer.CompanyName;
            if (customer != null)
            {
                var firstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                var lastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute);
                if (firstName != null && lastName != null)
                    customerName = firstName + " " + lastName;
            }
            string salemanName = string.Empty;
            if (customer != null && customer.TeamMemberOfSaleman > 0)
            {
                Core.Domain.Customers.Customer salesman = _customerService.GetAllCustomers().Where(x => x.Id == customer.TeamMemberOfSaleman).FirstOrDefault();
                if (salesman != null)
                {
                    var firstName = _genericAttributeService.GetAttribute<string>(salesman, NopCustomerDefaults.FirstNameAttribute);
                    var lastName = _genericAttributeService.GetAttribute<string>(salesman, NopCustomerDefaults.LastNameAttribute);
                    if (firstName != null && lastName != null)
                        salemanName = firstName + " " + lastName;
                }

            }

            if (order.OrderStatusId == (int)OrderStatus.Complete)
            {
                var newFont = new Font(font.Family, 16f, font.Style, font.Color);
                newFont.SetStyle(Font.BOLD);
                // Invoice Info
                //var cellInvoiceInfo = GetPdfCell(string.Format(_localizationService.GetResource("PDFInvoice.InvoiceInfo", lang.Id)), newFont);
                //cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
                //cellInvoiceInfo.VerticalAlignment = Element.ALIGN_CENTER;
                //cellInvoiceInfo.BackgroundColor = BaseColor.LightGray;
                //cellInvoiceInfo.Border = Rectangle.TOP_BORDER;
                //InvoiceInfo.AddCell(cellInvoiceInfo);

                // Invoice No
                var cellOrderNo = GetPdfCell(string.Format(_localizationService.GetResource("PDFInvoice.Order#", lang.Id), ": " + order.CustomOrderNumber), newFont);
                cellOrderNo.HorizontalAlignment = Element.ALIGN_LEFT;
                cellOrderNo.VerticalAlignment = Element.ALIGN_CENTER;
                cellOrderNo.PaddingLeft = 2;
                cellOrderNo.PaddingTop = 15;
                cellOrderNo.PaddingBottom = 15;
                cellOrderNo.Border = Rectangle.TOP_BORDER;
                cellOrderNo.BorderWidthTop = 1;
                InvoiceInfo.AddCell(cellOrderNo);
                // Invoice
            }
            else
            {
                var newFont = new Font(font.Family, 16f, font.Style, font.Color);
                newFont.SetStyle(Font.BOLD);
                // Proforma Info
                //var cellInvoiceInfo = GetPdfCell(string.Format(_localizationService.GetResource("PDFInvoice.ProformaInfo", lang.Id), ""), titleFont);
                //cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
                //cellInvoiceInfo.VerticalAlignment = Element.ALIGN_CENTER;
                //cellInvoiceInfo.BackgroundColor = BaseColor.LightGray;
                //cellInvoiceInfo.Border = Rectangle.NO_BORDER;
                //InvoiceInfo.AddCell(cellInvoiceInfo);
                // Proforma no
                var cellOrderNo = GetPdfCell(string.Format(_localizationService.GetResource("PDFInvoice.Proforma", lang.Id), ": " + order.CustomOrderNumber), newFont);
                cellOrderNo.HorizontalAlignment = Element.ALIGN_LEFT;
                cellOrderNo.VerticalAlignment = Element.ALIGN_CENTER;
                cellOrderNo.PaddingLeft = 2;
                cellOrderNo.PaddingTop = 15;
                cellOrderNo.PaddingBottom = 15;
                cellOrderNo.Border = Rectangle.TOP_BORDER;
                cellOrderNo.BorderWidthTop = 1;
                InvoiceInfo.AddCell(cellOrderNo);
            }
            var tFont = new Font(font.Family, 11f, font.Style, font.Color);
            // ReferenceNo
            var cellRefNo = GetPdfCell(string.Format(_localizationService.GetResource("PDFInvoice.ReferenceNo", lang.Id), ": " + order.ReferneceNo), tFont);
            cellRefNo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellRefNo.VerticalAlignment = Element.ALIGN_CENTER;
            cellRefNo.PaddingLeft = 2;
            cellRefNo.Border = Rectangle.TOP_BORDER;
            cellRefNo.PaddingTop = 15;
            cellRefNo.PaddingBottom = 15;
            cellRefNo.BorderWidthTop = 1;
            InvoiceInfo.AddCell(cellRefNo);
            // Order Date
            var CellOrderDate = GetPdfCell(_localizationService.GetResource("Account.CustomerOrders.OrderDate", lang.Id) + ": " + _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("d", new CultureInfo(lang.LanguageCulture)), tFont);
            // GetPdfCell(GetParagraph("PDFInvoice.OrderDate", lang, font, _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D", new CultureInfo(lang.LanguageCulture)), font);
            CellOrderDate.HorizontalAlignment = Element.ALIGN_LEFT;
            CellOrderDate.VerticalAlignment = Element.ALIGN_CENTER;
            CellOrderDate.Border = Rectangle.TOP_BORDER;
            CellOrderDate.PaddingTop = 15;
            CellOrderDate.PaddingBottom = 15;
            CellOrderDate.BorderWidthTop = 1;
            CellOrderDate.PaddingLeft = 2;

            InvoiceInfo.AddCell(CellOrderDate);

            // Customer
            var CellCustomer = GetPdfCell(_localizationService.GetResource("PDFInvoice.CustomerNo", lang.Id) + ": " + order.CustomerId, tFont);
            CellCustomer.HorizontalAlignment = Element.ALIGN_LEFT;
            CellCustomer.VerticalAlignment = Element.ALIGN_CENTER;
            CellCustomer.Border = Rectangle.TOP_BORDER;
            CellCustomer.PaddingTop = 10;
            CellCustomer.PaddingBottom = 10;
            CellCustomer.BorderWidthTop = 0.5f;
            CellCustomer.PaddingLeft = 2;
            InvoiceInfo.AddCell(CellCustomer);
            // Customer
            var CellCustomerName = GetPdfCell("" + customerName, tFont);
            CellCustomerName.HorizontalAlignment = Element.ALIGN_LEFT;
            CellCustomerName.VerticalAlignment = Element.ALIGN_CENTER;
            CellCustomerName.Border = Rectangle.TOP_BORDER;
            CellCustomerName.PaddingTop = 10;
            CellCustomerName.PaddingBottom = 10;
            CellCustomerName.BorderWidthTop = 0.5f;
            CellCustomerName.PaddingLeft = 2;
            InvoiceInfo.AddCell(CellCustomerName);
            // Saleman
            var CellSaleman = GetPdfCell(_localizationService.GetResource("Admin.Orders.List.Saleman", lang.Id) + ": " + salemanName, tFont);
            CellSaleman.HorizontalAlignment = Element.ALIGN_LEFT;
            CellSaleman.VerticalAlignment = Element.ALIGN_CENTER;
            CellSaleman.Border = Rectangle.TOP_BORDER;
            CellSaleman.PaddingTop = 10;
            CellSaleman.PaddingBottom = 10;
            CellSaleman.BorderWidthTop = 0.5f;
            CellSaleman.PaddingLeft = 2;
            InvoiceInfo.AddCell(CellSaleman);
            // Order Status
            var CellStatus = GetPdfCell(_localizationService.GetResource("Account.CustomerOrders.OrderStatus", lang.Id) + ": " + order.OrderStatus.ToString(), tFont);
            CellStatus.HorizontalAlignment = Element.ALIGN_LEFT;
            CellStatus.VerticalAlignment = Element.ALIGN_CENTER;
            CellStatus.Border = Rectangle.NO_BORDER;
            CellStatus.PaddingTop = 10;
            CellStatus.PaddingBottom = 10;
            CellStatus.PaddingLeft = 2;
            InvoiceInfo.AddCell(CellStatus);
            // Reduce Space
            var CellEmpty = GetPdfCell("    ", titleFont);
            CellEmpty.HorizontalAlignment = Element.ALIGN_LEFT;
            CellEmpty.VerticalAlignment = Element.ALIGN_CENTER;
            CellEmpty.Border = Rectangle.NO_BORDER;
            CellEmpty.PaddingLeft = 2;
            InvoiceInfo.AddCell(CellEmpty);
            // Genral Statement
            // InvoiceInfo.AddCell(getPDFgetpdf GetParagraph("PDFInvoice.BillingInformation", lang, titleFont));
            //var cellGeneralStatement = GetPdfCell(_localizationService.GetResource("PDFInvoice.GeneralStateInvoiceStatement", lang.Id), tFont);
            //cellGeneralStatement.HorizontalAlignment = Element.ALIGN_LEFT;
            //cellGeneralStatement.VerticalAlignment = Element.ALIGN_CENTER;
            //cellGeneralStatement.Border = Rectangle.NO_BORDER;
            //cellGeneralStatement.BackgroundColor = BaseColor.LightGray;
            //InvoiceInfo.AddCell(cellGeneralStatement);
            // add to address table
            doc.Add(InvoiceInfo);
            doc.Add(new Paragraph(" "));
        }
        /// <summary>
        /// Print products
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">CustomPrintCmpanySlogan</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font with sr#</param>
        protected virtual void CustomPrintCmpanySlogan(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var companySlogander = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            var cellProducts = GetPdfCell("PDFInvoice.CompanySlogan", lang, font);
            cellProducts.Border = Rectangle.NO_BORDER;
            companySlogander.AddCell(cellProducts);
            doc.Add(companySlogander);
            doc.Add(new Paragraph(" "));
        }
        /// <summary>
        /// Print products
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">CustomPrintCmpanySlogan</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font with sr#</param>
        protected virtual void CustomPrintShippingHeading(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var companyShippingAddressHeading = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            var cellProducts = GetPdfCell("PDFInvoice.ShippingInfoHeading", lang, font);
            cellProducts.Border = Rectangle.NO_BORDER;
            companyShippingAddressHeading.AddCell(cellProducts);
            doc.Add(new Paragraph(" "));
            doc.Add(companyShippingAddressHeading);
            doc.Add(new Paragraph(" "));
        }
        /// <summary>
        /// Print header
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        protected virtual void PrintHeader(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //logo
            int padding = 4;
            var logoPicture = _pictureService.GetPictureById(pdfSettingsByStore.LogoPictureId);
            var logoExists = logoPicture != null;

            //header
            var headerTable = new PdfPTable(logoExists ? 2 : 1)
            {
                RunDirection = GetDirection(lang)
            };

            //  Logo  headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.8f, 0.2f });

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.7f, 0.3f } : new[] { 0.7f, 0.3f });
            headerTable.WidthPercentage = 100f;

            headerTable.AddCell(new PdfPCell(new PdfPCell { Border = Rectangle.NO_BORDER }));
            //logo               
            if (logoExists)
            {
                var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                var logo = Image.GetInstance(logoFilePath);
                logo.Alignment = GetAlignment(lang, false);
                logo.ScaleToFit(95f, 80f);

                var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
                cellLogo.HorizontalAlignment = Element.ALIGN_LEFT;
                cellLogo.VerticalAlignment = Element.ALIGN_BOTTOM;
                cellLogo.AddElement(logo);
                cellLogo.PaddingBottom = padding;
                cellLogo.PaddingLeft = padding;
                headerTable.AddCell(cellLogo);
            }
            //store info
            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;

            var addressCell = GetPdfCell(store.CompanyAddress, font);
            addressCell.VerticalAlignment = Element.ALIGN_BOTTOM;
            addressCell.Border = Rectangle.NO_BORDER;
            headerTable.AddCell(addressCell);

            // Set Header Info
            //var cellHeader = GetPdfCell(store.CompanyName, font);
            var cellHeader = GetPdfCell(_localizationService.GetResource("PDFInvoice.Contact", lang.Id), font);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(store.CompanyAddress));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(_localizationService.GetResource("PDFInvoice.VAT", lang.Id) + ": " + store.CompanyVat ));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(_localizationService.GetResource("Account.Fields.Phone", lang.Id) + ": " + pdfSettingsByStore.HeaderPhone));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            //cellHeader.Phrase.Add(new Phrase(_localizationService.GetResource("Account.Fields.Fax", lang.Id) + ": " + pdfSettingsByStore.HeaderFax));
            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(_localizationService.GetResource("Homepage", lang.Id) + ": " + pdfSettingsByStore.HeaderHomePage));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(_localizationService.GetResource("Account.Fields.Email", lang.Id) + ": " + pdfSettingsByStore.HeaderEmail));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            //var myVAT = _settingService.GetSettingByKey<string>("Store.CreatelVAT");
            //if (myVAT != null)
            //{
            //    cellHeader.Phrase.Add(new Phrase(string.Format(_localizationService.GetResource("PDFInvoice.VATNumber", lang.Id), myVAT.ToString())));
            //}
            cellHeader.PaddingBottom = padding;
            cellHeader.PaddingRight = padding;
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.VerticalAlignment = Element.ALIGN_BOTTOM;
            cellHeader.Border = Rectangle.NO_BORDER;
            headerTable.AddCell(cellHeader);
            // Add table to document
            doc.Add(headerTable);

        }

        protected virtual PdfPTable PrintPayment(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            const string indent = "";
            var patymentInfo = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            patymentInfo.DefaultCell.Border = Rectangle.NO_BORDER;
            //var newFont = new Font(font.Family, 12f, font.Style, font.Color);
            var newTFont = new Font(font.Family, 9f, font.Style, font.Color);
            //newTFont.SetStyle(Font.BOLD);
            //billingAddress.AddCell(GetParagraph("PDFInvoice.BillingInformation", lang, titleFont));

            //vendors payment details
            if (vendorId == 0)
            {
                //payment method
                var paymentMethod = _paymentPluginManager.LoadPluginBySystemName(order.PaymentMethodSystemName);
                var paymentMethodStr = paymentMethod != null
                    ? _localizationService.GetLocalizedFriendlyName(paymentMethod, lang.Id)
                    : order.PaymentMethodSystemName;
                if (!string.IsNullOrEmpty(paymentMethodStr))
                {
                    patymentInfo.AddCell(GetParagraph("PDFInvoice.PaymentMethod", indent, lang, newTFont, paymentMethodStr));
                }

                //custom values
                var customValues = _paymentService.DeserializeCustomValues(order);
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        patymentInfo.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, newTFont));
                    }
                }
            }

            // Paymetn Due Date
            var paymentDueOn = "";
            if (order.PaymentTermId == 1)
            { paymentDueOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc.AddDays(7), DateTimeKind.Utc).ToString("d", new CultureInfo(lang.LanguageCulture)); }
            else if (order.PaymentTermId == 2)
            { paymentDueOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc.AddDays(14), DateTimeKind.Utc).ToString("d", new CultureInfo(lang.LanguageCulture)); }
            else if (order.PaymentTermId == 3)
            { paymentDueOn = _localizationService.GetResource("Admin.Orders.PaymentTerm.COD", lang.Id); }
            else if (order.PaymentTermId == 4)
            { paymentDueOn = _localizationService.GetResource("Admin.Orders.PaymentTerm.OSA", lang.Id); }
            if (!string.IsNullOrEmpty(paymentDueOn))
            {
                patymentInfo.AddCell(GetPdfCell(_localizationService.GetResource("Admin.Orders.PaymentDueOn", lang.Id) + ": " + paymentDueOn, newTFont, false));
            }
            return patymentInfo;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
        /// <returns>A path of generated file</returns>
        public virtual string PrintOrderToPdf(Order order, int languageId = 0, int vendorId = 0)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var fileName = $"order_{order.OrderGuid}_{CommonHelper.GenerateRandomDigitCode(4)}.pdf";
            var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var orders = new List<Order> { order };
                PrintOrdersToPdf(fileStream, orders, languageId, vendorId);
            }

            return filePath;
        }

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
        public virtual void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            doc.SetMargins(20, 20, 20, 70);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var ordCount = orders.Count;
            var ordNum = 0;

            foreach (var order in orders)
            {
                if (order != null)
                {
                    //by default _pdfSettings contains settings for the current active store
                    //and we need PdfSettings for the store which was used to place an order
                    //so let's load it based on a store of the current order
                    var pdfSettingsByStore = _settingService.LoadSetting<PdfSettings>(order.StoreId);

                    var lang = _workContext.WorkingLanguage;
                    if (lang == null || !lang.Published)
                        lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);

                    //header
                    PrintHeader(pdfSettingsByStore, lang, order, font, titleFont, doc);

                    // Company Slogan
                    //CustomPrintCmpanySlogan(vendorId, lang, titleFont, doc, order, font, attributesFont);
                    //addresses
                    //PrintAddresses(vendorId, lang, titleFont, order, font, doc);

                    PrintBillingInfo(vendorId, lang, titleFont, order, font, doc);

                    PrintInvoiceInfo(vendorId, lang, titleFont, order, font, doc);

                    //products
                    // PrintProducts(vendorId, lang, titleFont, doc, order, font, attributesFont);
                    CustomPrintProducts(vendorId, lang, titleFont, doc, order, font, attributesFont);
                    //doc.Add(Chunk.Newline);
                    Paragraph lineSeparator = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 1)));
                    // Set gap between line paragraphs.
                    lineSeparator.SetLeading(0.5F, 0.5F);
                    doc.Add(lineSeparator);
                    //doc.Add(Chunk.Newline);
                    //totals
                    var totalsTable = new PdfPTable(2)
                    {
                        RunDirection = GetDirection(lang),
                        WidthPercentage = 100f,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };
                    totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    totalsTable.SetWidths(new[] { 50, 50 });
                    totalsTable.AddCell(PrintComments(vendorId, pdfSettingsByStore, lang, order, font, titleFont, doc));
                    totalsTable.AddCell(PrintTotals(vendorId, lang, order, font, titleFont, doc));
                    doc.Add(totalsTable);
                    PrintShippingInfo(lang, order, titleFont, font, doc);
                    doc.Add(Chunk.Newline);
                    doc.Add(PrintMessages(lang, order, font, titleFont, doc, pdfWriter));
                    doc.Add(Chunk.Newline);

                    // Shipping Address Para
                    //CustomPrintShippingHeading(vendorId, lang, titleFont, doc, order, font, attributesFont);
                    //Shipping Address
                    //PrintMessages(lang, order, font, titleFont, doc, pdfWriter);
                    //footer
                    PrintFooter(pdfSettingsByStore, pdfWriter, pageSize, lang, font);

                    ordNum++;
                    if (ordNum < ordCount)
                    {
                        doc.NewPage();
                    }
                }
            }

            doc.Close();
        }

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        public virtual void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipments == null)
                throw new ArgumentNullException(nameof(shipments));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var shipmentCount = shipments.Count;
            var shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                var order = shipment.Order;

                var lang = _workContext.WorkingLanguage;
                if (lang == null || !lang.Published)
                    lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);

                var addressTable = new PdfPTable(1);
                if (lang.Rtl)
                    addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;

                addressTable.AddCell(GetParagraph("PDFPackagingSlip.Shipment", lang, titleFont, shipment.Id));
                addressTable.AddCell(GetParagraph("PDFPackagingSlip.Order", lang, titleFont, order.CustomOrderNumber));

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddress == null)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(order.ShippingAddress.Company))
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Company", lang, font, order.ShippingAddress.Company));

                    addressTable.AddCell(GetParagraph("PDFPackagingSlip.Name", lang, font, order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Phone", lang, font, order.ShippingAddress.PhoneNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Address", lang, font, order.ShippingAddress.Address1));

                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.ShippingAddress.Address2))
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Address2", lang, font, order.ShippingAddress.Address2));

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{order.ShippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(order.ShippingAddress.County) ? $"{order.ShippingAddress.County}, " : string.Empty)}" +
                            $"{(order.ShippingAddress.StateProvince != null ? _localizationService.GetLocalized(order.ShippingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{order.ShippingAddress.ZipPostalCode}";
                        addressTable.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                        addressTable.AddCell(new Paragraph(_localizationService.GetLocalized(order.ShippingAddress.Country, x => x.Name, lang.Id), font));

                    //custom attributes
                    var customShippingAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        addressTable.AddCell(new Paragraph(HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                }
                else
                    if (order.PickupAddress != null)
                {
                    addressTable.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Pickup", lang.Id), titleFont));
                    if (!string.IsNullOrEmpty(order.PickupAddress.Address1))
                        addressTable.AddCell(new Paragraph($"   {string.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.PickupAddress.Address1)}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.City))
                        addressTable.AddCell(new Paragraph($"   {order.PickupAddress.City}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.County))
                        addressTable.AddCell(new Paragraph($"   {order.PickupAddress.County}", font));
                    if (order.PickupAddress.Country != null)
                        addressTable.AddCell(new Paragraph($"   {_localizationService.GetLocalized(order.PickupAddress.Country, x => x.Name, lang.Id)}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.ZipPostalCode))
                        addressTable.AddCell(new Paragraph($"   {order.PickupAddress.ZipPostalCode}", font));
                    addressTable.AddCell(new Paragraph(" "));
                }

                addressTable.AddCell(new Paragraph(" "));

                addressTable.AddCell(GetParagraph("PDFPackagingSlip.ShippingMethod", lang, font, order.ShippingMethod));
                addressTable.AddCell(new Paragraph(" "));
                doc.Add(addressTable);

                var productsTable = new PdfPTable(3) { WidthPercentage = 100f };
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] { 20, 20, 60 });
                }
                else
                {
                    productsTable.SetWidths(new[] { 60, 20, 20 });
                }

                //product name
                var cell = GetPdfCell("PDFPackagingSlip.ProductName", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //SKU
                cell = GetPdfCell("PDFPackagingSlip.SKU", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = GetPdfCell("PDFPackagingSlip.QTY", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                foreach (var si in shipment.ShipmentItems)
                {
                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    //product name
                    var orderItem = _orderService.GetOrderItemById(si.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var p = orderItem.Product;
                    var name = _localizationService.GetLocalized(p, x => x.Name, lang.Id);
                    productAttribTable.AddCell(new Paragraph(name, font));
                    //attributes
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }

                    //rental info
                    if (orderItem.Product.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                            ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                            ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                        var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);

                        var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                        productAttribTable.AddCell(rentalInfoParagraph);
                    }

                    productsTable.AddCell(productAttribTable);

                    //SKU
                    var sku = _productService.FormatSku(p, orderItem.AttributesXml);
                    cell = GetPdfCell(sku ?? string.Empty, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = GetPdfCell(si.Quantity, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                doc.Add(productsTable);

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        /// <summary>
        /// Print products to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        public virtual void PrintProductsToPdf(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var lang = _workContext.WorkingLanguage;

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();

            var productNumber = 1;
            var prodCount = products.Count;

            foreach (var product in products)
            {
                var productName = _localizationService.GetLocalized(product, x => x.Name, lang.Id);
                var productDescription = _localizationService.GetLocalized(product, x => x.FullDescription, lang.Id);

                var productTable = new PdfPTable(1) { WidthPercentage = 100f };
                productTable.DefaultCell.Border = Rectangle.NO_BORDER;
                if (lang.Rtl)
                {
                    productTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                }

                productTable.AddCell(new Paragraph($"{productNumber}. {productName}", titleFont));
                productTable.AddCell(new Paragraph(" "));
                productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(productDescription, decode: true)), font));
                productTable.AddCell(new Paragraph(" "));

                if (product.ProductType == ProductType.SimpleProduct)
                {
                    //simple product
                    //render its properties such as price, weight, etc
                    var priceStr = $"{product.Price:0.00} {_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode}";
                    if (product.IsRental)
                        priceStr = _priceFormatter.FormatRentalProductPeriod(product, priceStr);
                    productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Price", lang.Id)}: {priceStr}", font));
                    productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.SKU", lang.Id)}: {product.Sku}", font));

                    if (product.IsShipEnabled && product.Weight > decimal.Zero)
                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Weight", lang.Id)}: {product.Weight:0.00} {_measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name}", font));

                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.StockQuantity", lang.Id)}: {_productService.GetTotalStockQuantity(product)}", font));

                    productTable.AddCell(new Paragraph(" "));
                }

                var pictures = _pictureService.GetPicturesByProductId(product.Id);
                if (pictures.Any())
                {
                    var table = new PdfPTable(2) { WidthPercentage = 100f };
                    if (lang.Rtl)
                    {
                        table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    }

                    foreach (var pic in pictures)
                    {
                        var picBinary = _pictureService.LoadPictureBinary(pic);
                        if (picBinary == null || picBinary.Length <= 0)
                            continue;

                        var pictureLocalPath = _pictureService.GetThumbLocalPath(pic, 200, false);
                        var cell = new PdfPCell(Image.GetInstance(pictureLocalPath))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    if (pictures.Count % 2 > 0)
                    {
                        var cell = new PdfPCell(new Phrase(" "))
                        {
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    productTable.AddCell(table);
                    productTable.AddCell(new Paragraph(" "));
                }

                if (product.ProductType == ProductType.GroupedProduct)
                {
                    //grouped product. render its associated products
                    var pvNum = 1;
                    foreach (var associatedProduct in _productService.GetAssociatedProducts(product.Id, showHidden: true))
                    {
                        productTable.AddCell(new Paragraph($"{productNumber}-{pvNum}. {_localizationService.GetLocalized(associatedProduct, x => x.Name, lang.Id)}", font));
                        productTable.AddCell(new Paragraph(" "));

                        //uncomment to render associated product description
                        //string apDescription = associated_localizationService.GetLocalized(product, x => x.ShortDescription, lang.Id);
                        //if (!string.IsNullOrEmpty(apDescription))
                        //{
                        //    productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(apDescription)), font));
                        //    productTable.AddCell(new Paragraph(" "));
                        //}

                        //uncomment to render associated product picture
                        //var apPicture = _pictureService.GetPicturesByProductId(associatedProduct.Id).FirstOrDefault();
                        //if (apPicture != null)
                        //{
                        //    var picBinary = _pictureService.LoadPictureBinary(apPicture);
                        //    if (picBinary != null && picBinary.Length > 0)
                        //    {
                        //        var pictureLocalPath = _pictureService.GetThumbLocalPath(apPicture, 200, false);
                        //        productTable.AddCell(Image.GetInstance(pictureLocalPath));
                        //    }
                        //}

                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Price", lang.Id)}: {associatedProduct.Price:0.00} {_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode}", font));
                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.SKU", lang.Id)}: {associatedProduct.Sku}", font));

                        if (associatedProduct.IsShipEnabled && associatedProduct.Weight > decimal.Zero)
                            productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Weight", lang.Id)}: {associatedProduct.Weight:0.00} {_measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name}", font));

                        if (associatedProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                            productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.StockQuantity", lang.Id)}: {_productService.GetTotalStockQuantity(associatedProduct)}", font));

                        productTable.AddCell(new Paragraph(" "));

                        pvNum++;
                    }
                }

                doc.Add(productTable);

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #endregion
    }
}