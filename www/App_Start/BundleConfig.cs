using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild
{
    public class BundleConfig
    {
        private static readonly IItemTransform[] cssTransforms = { new CustomCssRewriteUrlTransform(), new SourceMappingURLTransform() };
        private static readonly IItemTransform[] sourceMapTransform = { new SourceMappingURLTransform() };

        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

#if DEBUG
            var cssPrefix = "~/css/dist";
            var cssSuffix = ".css";

            var jsPrefix = "~/js/src";
            var jsSuffix = ".js";
#else
            var cssPrefix = "~/css/dist";
            var cssSuffix = ".min.css";

            var jsPrefix = "~/js/dist";
            var jsSuffix = ".min.js";
#endif

            //
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //
            // Styles
            //
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //

            bundles.Add(new StyleBundle("~/bundles/css/fonts")
                .Include($"{cssPrefix}/fonts/fonts{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/font-awesome")
                .Include($"{cssPrefix}/fonts/font-awesome{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap-icons")
                .Include($"{cssPrefix}/fonts/bootstrap-icons{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap")
                .Include($"{cssPrefix}/lib/bootstrap/scss/bootstrap{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap-mobile")
                .Include($"{cssPrefix}/lib/bootstrap/scss/bootstrap-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/common")
                .Include($"{cssPrefix}/common/common{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/landing-page")
                .Include($"{cssPrefix}/common/landing-page{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/collection")
                .Include($"{cssPrefix}/common/collection{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/desktop")
                .Include($"{cssPrefix}/common/desktop{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/mobile")
                .Include($"{cssPrefix}/common/mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/forms")
                .Include($"{cssPrefix}/common/forms{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/article")
                .Include($"{cssPrefix}/models/article{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/article-mobile")
                .Include($"{cssPrefix}/models/article-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/furniture-modular")
                .Include($"{cssPrefix}/models/furniture-modular{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/furniture-modular-mobile")
                .Include($"{cssPrefix}/models/furniture-modular-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/furniture-modular-new")
                .Include($"{cssPrefix}/models/furniture-modular-new{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/furniture-modular-mobile-new")
                .Include($"{cssPrefix}/models/furniture-modular-mobile-new{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/furniture-wholesale")
                .Include($"{cssPrefix}/models/furniture-wholesale{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/furniture-wholesale-mobile")
                .Include($"{cssPrefix}/models/furniture-wholesale-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/basket-page")
                .Include($"{cssPrefix}/models/basket-page{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/basket-page-mobile")
                .Include($"{cssPrefix}/models/basket-page-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/login-page")
                .Include($"{cssPrefix}/models/login-page{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/login-page-mobile")
                .Include($"{cssPrefix}/models/login-page-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/checkout")
                .Include($"{cssPrefix}/lib/intl-tel-input/dist/intl-tel-input{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/lib/jquery-ui/minified/jquery-ui{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/models/checkout-page{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/checkout-mobile")
                .Include($"{cssPrefix}/models/checkout-page-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/kendo")
                .Include($"{cssPrefix}/lib/kendo/dist/2018.3.1017/kendo.common{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/lib/kendo/dist/2018.3.1017/kendo.default{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/kendo-datepicker")
                .Include($"{cssPrefix}/lib/kendo/dist-datepicker/datepicker{cssSuffix}", cssTransforms)
                );

            bundles.Add(new StyleBundle("~/bundles/css/material")
                .Include($"{cssPrefix}/lib/material/dist/material-components-web{cssSuffix}", cssTransforms)
            );

            bundles.Add(new StyleBundle("~/bundles/css/change-country")
                .Include($"{cssPrefix}/models/change-country{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/change-country-mobile")
                .Include($"{cssPrefix}/models/change-country-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/stockists")
                .Include($"{cssPrefix}/models/stockists{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/stockists-mobile")
                .Include($"{cssPrefix}/models/stockists-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/careers")
                .Include($"{cssPrefix}/models/careers{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/careers-mobile")
                .Include($"{cssPrefix}/models/careers-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/trade-common")
                .Include($"{cssPrefix}/common/trade-common{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/trade")
                .Include($"{cssPrefix}/models/basket-page{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/common/trade{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/trade-mobile")
                .Include($"{cssPrefix}/models/basket-page-mobile{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/common/trade-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/address-components")
                .Include($"{cssPrefix}/lib/material/dist/material-components-web{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/lib/intl-tel-input/dist/intl-tel-input{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/lib/jquery-ui/jquery-ui{cssSuffix}", cssTransforms)
                .Include($"{cssPrefix}/lib/jquery-ui/jquery.ui.autocomplete{cssSuffix}", cssTransforms)
            );

            bundles.Add(new StyleBundle("~/bundles/css/member-address-management")
                .Include($"{cssPrefix}/models/member-address-management{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/member-address-management-mobile")
                .Include($"{cssPrefix}/models/member-address-management-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/member-order-history")
                .Include($"{cssPrefix}/models/member-order-history{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/member-order-history-mobile")
                .Include($"{cssPrefix}/models/member-order-history-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/member-login-details")
                .Include($"{cssPrefix}/models/member-login-details{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/member-login-details-mobile")
                .Include($"{cssPrefix}/models/member-login-details-mobile{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/trade-document-wrapper")
                .Include($"{cssPrefix}/models/trade-document-wrapper{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/moodboards")
                .Include($"{cssPrefix}/models/member-moodboards{cssSuffix}", cssTransforms));

            bundles.Add(new StyleBundle("~/bundles/css/moodboards-mobile")
                .Include($"{cssPrefix}/models/member-moodboards-mobile{cssSuffix}", cssTransforms));

            //  
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //
            // Scripts
            //
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //

            bundles.Add(new ScriptBundle("~/bundles/js/common")
                .Include($"{jsPrefix}/models/common/common-functions{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/polyfills{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/util/extensions{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/common-models{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/stock-info{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/menu-scripts")
                .Include($"{jsPrefix}/models/common/basket{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/search{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/ecommerce-popup")
                .Include($"{jsPrefix}/models/common/ecommerce-popup{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/jquery")
                .Include($"{jsPrefix}/lib/jquery/dist/jquery{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap")
              .Include($"{jsPrefix}/lib/bootstrap/dist/bootstrap.bundle{jsSuffix}", sourceMapTransform)
              .Include($"{jsPrefix}/lib/bootstrap/custom/bootstrap.onload{jsSuffix}", sourceMapTransform)
            );

            bundles.Add(new ScriptBundle("~/bundles/js/landing-page")
                .Include($"{jsPrefix}/models/landing-page/article-grid{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/article-pod{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/attribute{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/attribute-group{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/attribute-group-group{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/product-type{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/page-section{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/pager{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/product-page-pod{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/product-page-grid{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/collection-grid{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/collection-pod{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/banner-grid{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/banner-pod{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/page-grid{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/page-pod{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/landing-page{jsSuffix}", sourceMapTransform));
                
            bundles.Add(new ScriptBundle("~/bundles/js/article-page")
                .Include($"{jsPrefix}/models/article-page/article-page{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-clearance")
                .Include($"{jsPrefix}/models/landing-page/product-page-grid-trade-clearance{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/forms")
                .Include($"{jsPrefix}/models/common/address{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/landing-page/forms{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/product-page")
                .Include($"{jsPrefix}/models/product-page/coverage-calculator{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/product-page/product-page{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/collection-page")
                .Include($"{jsPrefix}/models/collection/collection-page{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/ko")
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-3.5.0{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-validation{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-pre-rendered{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/custom/knockout-extensions{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/custom/knockout-validation-custom-rules{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/kendo")
                .Include($"{jsPrefix}/lib/kendo/dist/kendo.2018.3.1017.all{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist/kendo.aspnetmvc{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist/kendo.culture.en-GB{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-kendo{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/jszip/dist/jszip{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/kendo-datepicker")
                .Include($"{jsPrefix}/lib/kendo/dist-datepicker/kendo.core{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist-datepicker/kendo.data{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist-datepicker/kendo.userevents{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist-datepicker/kendo.selectable{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist-datepicker/kendo.calendar{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist-datepicker/kendo.popup{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/kendo/dist-datepicker/kendo.datepicker{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-kendo{jsSuffix}", sourceMapTransform));
                
            bundles.Add(new ScriptBundle("~/bundles/js/ko-components")
                .Include($"{jsPrefix}/lib/knockout/custom/knockout-components{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/material")
                .Include($"{jsPrefix}/lib/material/dist/material-components-web{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/linq")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/common-modals")
                .Include($"{jsPrefix}/models/common/modals/info-modal{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/modals/confirm-modal{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/modals/error-modal{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/modals/back-in-stock-login-modal{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/modals/back-in-stock-success-modal{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/modals/contact-customer-services-modal{jsSuffix}", sourceMapTransform)
                );

            bundles.Add(new ScriptBundle("~/bundles/js/large-image-modal")
                .Include($"{jsPrefix}/models/common/modals/large-image-modal{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/basket-page")
                .Include($"{jsPrefix}/models/basket-page/basket-item{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/basket-page/basket-page{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/login-page")
                .Include($"{jsPrefix}/models/login-page/zxcvbn{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/login-page/login-page{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/checkout-single-page")
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-3.5.0{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-validation{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/dist/knockout-pre-rendered{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/knockout/custom/knockout-extensions{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/intl-tel-input/dist/intl-tel-input-jquery{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/intl-tel-input/dist/data{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/intl-tel-input/dist/utils{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/jquery-ui/dist/jquery-ui-1.12.1.reduced{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/jquery-ui/dist/jquery-ui-extensions{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/checkout/checkout-single-page{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/checkout-apple")
                .Include($"{jsPrefix}/models/checkout/checkout-apple{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/checkout-login-page")
                .Include($"{jsPrefix}/models/checkout/checkout-login{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/stockists")
                .Include($"{jsPrefix}/models/stockists/stockists{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/site-corrector")
                .Include($"{jsPrefix}/models/common/site-corrector{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-login-prompt")
                .Include($"{jsPrefix}/models/common/trade-login-prompt{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/cookie-notice")
                .Include($"{jsPrefix}/models/common/cookie-notice{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/address-management")
                .Include($"{jsPrefix}/models/common/address{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/member/member-address-management{jsSuffix}", sourceMapTransform));
            
            bundles.Add(new ScriptBundle("~/bundles/js/order-history")
                .Include($"{jsPrefix}/models/common/address{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/member/member-order-history{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/member-login-details")
                .Include($"{jsPrefix}/models/login-page/zxcvbn{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/member/member-login-details{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-quick-ordering")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-quick-ordering{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-address-management")
                .Include($"{jsPrefix}/models/common/address{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-address-management{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/address-components")
                .Include($"{jsPrefix}/lib/material/dist/material-components-web{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/jquery-ui/dist/jquery-ui-1.12.1.reduced{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/jquery-ui/dist/jquery-ui-extensions{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/intl-tel-input/dist/intl-tel-input{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/intl-tel-input/dist/utils{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-contact-details")
                .Include($"{jsPrefix}/models/trade/trade-contact-details{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-invoices")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-invoices{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-order-status")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-order-status{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-proforma-invoices")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-proforma-invoices{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-reserve-orders")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/address{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-address-management{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-reserve-orders{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-datetimepicker")
                .Include($"{jsPrefix}/lib/moment/dist/moment-with-locales{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/bootstrap/dist/bootstrap-datetimepicker{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-downloads")
                .Include($"{jsPrefix}/models/trade/trade-downloads{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-order-uploads")
                .Include($"{jsPrefix}/models/trade/trade-order-uploads{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-request-access")
                .Include($"{jsPrefix}/models/trade/trade-request-access{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-login-details")
                .Include($"{jsPrefix}/models/login-page/zxcvbn{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-login-details{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-user-management")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-user-management{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-price-display")
                .Include($"{jsPrefix}/models/trade/trade-price-display{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-account-request")
                .Include($"{jsPrefix}/lib/linq/dist/linq{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/address{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-account-request{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/trade-registration")
                .Include($"{jsPrefix}/models/login-page/zxcvbn{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/trade/trade-registration{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/moodboard-signalr-prerequisites")
                .Include($"{jsPrefix}/lib/jquery/dist/jquery.signalR-2.4.3{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/moodboards")
                .Include($"{jsPrefix}/lib/fabric/dist/fabric{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/jquery-ui/dist/jquery-ui{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/lib/font-face-observer/dist/font-face-observer{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/common/emailer{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/member/member-moodboards{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/member-password-reset")
                .Include($"{jsPrefix}/models/login-page/zxcvbn{jsSuffix}", sourceMapTransform)
                .Include($"{jsPrefix}/models/member/member-password-reset{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/member-unsubscribe")
                .Include($"{jsPrefix}/models/member/member-unsubscribe{jsSuffix}", sourceMapTransform));

            bundles.Add(new ScriptBundle("~/bundles/js/emailer")
                .Include($"{jsPrefix}/models/common/emailer{jsSuffix}", sourceMapTransform));

            foreach (var b in BundleTable.Bundles)
            {
                b.Transforms.Clear();
            }

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif

        }

        // adapted from: https://stackoverflow.com/questions/12577108/asp-net-mvc4-bundling-with-twitter-bootstrap#answer-17702773
        // with thanks
        public class SourceMappingURLTransform : IItemTransform
        {
            private static string ConvertUrlsToAbsolute(string baseUrl, string content)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return content;
                }

                var regex = new Regex("# sourceMappingURL\\=(?<path>.+\\.map)");

                return regex.Replace(content, (match) => 
                {
                    return $"/*# sourceMappingURL={HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}{RebaseUrlToAbsolute(baseUrl, match?.Groups["path"]?.Value)} */";
                });
            }

            public string Process(string includedVirtualPath, string input)
            {
                if (includedVirtualPath == null)
                {
                    throw new ArgumentNullException("includedVirtualPath");
                }
                var directory = VirtualPathUtility.GetDirectory(includedVirtualPath);
                return ConvertUrlsToAbsolute(directory, input);
            }

            private static string RebaseUrlToAbsolute(string baseUrl, string url)
            {
                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(baseUrl) || url.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }
                if (!baseUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrl = string.Concat(baseUrl, "/");
                }
                return VirtualPathUtility.ToAbsolute(string.Concat(baseUrl, url));
            }
        }

        public class CustomCssRewriteUrlTransform : IItemTransform
        {
            public CustomCssRewriteUrlTransform()
            {
            }

            internal static string RebaseUrlToAbsolute(string baseUrl, string url)
            {
                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(baseUrl) || url.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }

                if (!baseUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    baseUrl += "/";
                }

                return VirtualPathUtility.ToAbsolute(baseUrl + url);
            }

            internal static string ConvertUrlsToAbsolute(string baseUrl, string content)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return content;
                }

                Regex regex = new Regex("url\\(['\"]?(?<url>[^(?!.*\"?data)][^)]+?)['\"]?\\)");
                return regex.Replace(content, (Match match) => "url(" + RebaseUrlToAbsolute(baseUrl, match.Groups["url"].Value) + ")");
            }

            public string Process(string includedVirtualPath, string input)
            {
                if (includedVirtualPath == null)
                {
                    throw new ArgumentNullException("includedVirtualPath");
                }

                string directory = VirtualPathUtility.GetDirectory(includedVirtualPath.Substring(1));
                return ConvertUrlsToAbsolute(directory, input);
            }
        }
    }
}
