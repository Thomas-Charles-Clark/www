using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using wwwDesignersGuild.Models.Resource;
using HtmlAgilityPack;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels;
using System.Text.RegularExpressions;
using wwwDesignersGuild.Models.ImgSize;
using DocumentFormat.OpenXml.EMMA;
using DevExpress.Web.Rendering;
using wwwDesignersGuild.Models.Hotspot;
using System.Threading.Tasks;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;
using System.IO;
using System.Web.Hosting;
using System.Web.SessionState;
using System.Reflection;

namespace wwwDesignersGuild.Models.Helper
{
    public class HtmlHelperExtra
    {
        public static bool Debug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsNonProductionVersion
        {
            get
            {
                return (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == false);
            }
        }

        public static bool IsProductionVersion
        {
            get
            {
                return !IsNonProductionVersion;
            }
        }

        public static IHtmlString ImageSourceSetSizes(long imageId, ImageSize imageSize, int? maxWidth = null)
        {
            if (dgFramework.IsMobileDevice())
            {
                switch (imageSize)
                {
                    case ImageSize.One:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/90/{imageId} 90w, /image{Framework.WebPFallback}/156/{imageId} 156w""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Two:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/156/{imageId} 156w, /image{Framework.WebPFallback}/239/{imageId} 239w, /image{Framework.WebPFallback}/322/{imageId} 322w""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Three:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/239/{imageId} 239w, /image{Framework.WebPFallback}/322/{imageId} 322w, /image{Framework.WebPFallback}/380/{imageId} 380w""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Four:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/322/{imageId} 322w, /image{Framework.WebPFallback}/428/{imageId} 428w, /image{Framework.WebPFallback}/600/{imageId} 600w""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Five:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/428/{imageId} 428w, /image{Framework.WebPFallback}/600/{imageId} 600w, /image{Framework.WebPFallback}/654/{imageId} 654w""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Six:
                    case ImageSize.Seven:
                    case ImageSize.Eight:
                    case ImageSize.Nine:
                    case ImageSize.Ten:
                    case ImageSize.Eleven:
                    case ImageSize.Twelve:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/488/{imageId} 488w, /image{Framework.WebPFallback}/654/{imageId} 654w, /image{Framework.WebPFallback}/986/{imageId} 968w""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    //case ImageSize.Seven:
                    //    return new HtmlString(
                    //        $@"srcset=""/image{Framework.WebPFallback}/600/{imageId} 600w, /image{Framework.WebPFallback}/986/{imageId} 986w""" +
                    //        $@"data-image-size=""{(int)imageSize}""");
                    //case ImageSize.Eight:
                    //    return new HtmlString(
                    //        $@"srcset=""/image{Framework.WebPFallback}/654/{imageId} 654w, /image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w""" +
                    //        $@"data-image-size=""{(int)imageSize}""");
                    //case ImageSize.Nine:
                    //    return new HtmlString(
                    //        $@"srcset=""/image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w""" +
                    //        $@"data-image-size=""{(int)imageSize}""");
                    //case ImageSize.Ten:
                    //case ImageSize.Eleven:
                    //case ImageSize.Twelve:
                    //    switch (maxWidth)
                    //    {
                    //        case var _ when maxWidth >= 1546:
                    //            return new HtmlString(
                    //                $@"srcset=""/image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w, /image{Framework.WebPFallback}/1546/{imageId} 1546w""" +
                    //        $@"data-image-size=""{(int)imageSize}""");
                    //        default:
                    //            return new HtmlString(
                    //                $@"srcset=""/image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w""" +
                    //        $@"data-image-size=""{(int)imageSize}""");
                    //    }
                    default:
                        return null;
                }
            }
            else
            {
                switch (imageSize)
                {
                    case ImageSize.One:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/90/{imageId} 90w, /image{Framework.WebPFallback}/156/{imageId} 156w""" +
                            $@"sizes=""(max-width: 1599px) 90px, 156px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Two:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/156/{imageId} 156w, /image{Framework.WebPFallback}/239/{imageId} 239w, /image{Framework.WebPFallback}/322/{imageId} 322w""" +
                            $@"sizes=""(max-width: 1319px) 156px, (max-width: 1599px) 239px, 322px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Three:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/239/{imageId} 239w, /image{Framework.WebPFallback}/322/{imageId} 322w, /image{Framework.WebPFallback}/380/{imageId} 380w""" +
                            $@"sizes=""(max-width: 1319px) 239px, (max-width: 1599px) 322px, 380px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Four:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/322/{imageId} 322w, /image{Framework.WebPFallback}/428/{imageId} 428w, /image{Framework.WebPFallback}/600/{imageId} 600w""" +
                            $@"sizes=""(max-width: 1319px) 322px, (max-width: 1599px) 428px, 600px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Five:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/428/{imageId} 428w, /image{Framework.WebPFallback}/600/{imageId} 600w, /image{Framework.WebPFallback}/654/{imageId} 654w""" +
                            $@"sizes=""(max-width: 1319px) 428px, (max-width: 1599px) 600px, 654px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Six:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/488/{imageId} 488w, /image{Framework.WebPFallback}/654/{imageId} 654w, /image{Framework.WebPFallback}/986/{imageId} 968w""" +
                            $@"sizes=""(max-width: 1319px) 488px, (max-width: 1599px) 654px, 986px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Seven:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/600/{imageId} 600w, /image{Framework.WebPFallback}/986/{imageId} 986w""" +
                            $@"sizes=""(max-width: 1319px) 600px, 986px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Eight:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/654/{imageId} 654w, /image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w""" +
                            $@"sizes=""(max-width: 1319px) 654px, (max-width: 1599px) 986px, 1024px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Nine:
                        return new HtmlString(
                            $@"srcset=""/image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w""" +
                            $@"sizes=""(max-width: 1319px) 986px, 1024px""" +
                            $@"data-image-size=""{(int)imageSize}""");
                    case ImageSize.Ten:
                    case ImageSize.Eleven:
                    case ImageSize.Twelve:
                        if (maxWidth >= 1546)
                            return new HtmlString(
                                $@"srcset=""/image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w, /image{Framework.WebPFallback}/1546/{imageId} 1546w""" +
                                $@"sizes=""(max-width: 1319px) 654px, (max-width: 1599px) 1024px, 1546px""" +
                                $@"data-image-size=""{(int)imageSize}""");
                        else
                            return new HtmlString(
                                $@"srcset=""/image{Framework.WebPFallback}/986/{imageId} 986w, /image{Framework.WebPFallback}/1024/{imageId} 1024w""" +
                                $@"sizes=""(max-width: 1319px) 986px, 1024px""" +
                                $@"data-image-size=""{(int)imageSize}""");
                    default:
                        return null;
                }
            }
        }

        public static IHtmlString ImageSourceSetSizes(long imageId, List<BootstrapBreakpointImageSize> imageSizes, int? maxWidth)
        {
            /*
             * how many different breakpoints do we have? six:
             * 
             
            $grid-breakpoints: (
                xs: 0,
                sm: 576px,
                md: 768px,
                lg: 992px,
                xl: 1320px,
                xxl: 1600px,
                xxxl: 1920px
            ) !default;

            how can we straighten this problem out? you might have provided just one indication, or anything up to and including 6 different.


            need to figure out what size the image will be at each breakpoint, given the potential absence of breakpoints.

            if we're only provided with say col-xs-12 then we know that it's going to be a 12 wide all the way through.

            if we're only provided with say col-xs-6 then we know it'll be 6 wide all the way through, etc.

            what if we aren't provided with any?

            what if we're provided with g-col-12 g-col-4 g-col-6 ?

            so: sort by their size, if we have no start assume full-width on mobile and work up from there.

            if any is non-null, it becomes the assumed size for the next breakpoint up.

             */

            var xs = imageSizes.SingleOrDefault(i => i.Breakpoint == BootstrapBreakpoint.xs);
            var sm = imageSizes.SingleOrDefault(i => i.Breakpoint == BootstrapBreakpoint.sm);
            var md = imageSizes.SingleOrDefault(i => i.Breakpoint == BootstrapBreakpoint.md);
            var lg = imageSizes.SingleOrDefault(i => i.Breakpoint == BootstrapBreakpoint.lg);
            var xl = imageSizes.SingleOrDefault(i => i.Breakpoint == BootstrapBreakpoint.xl);
            var xxl = imageSizes.SingleOrDefault(i => i.Breakpoint == BootstrapBreakpoint.xxl);
            var xxxl = imageSizes.SingleOrDefault(i => i.Breakpoint == BootstrapBreakpoint.xxxl);

            if (xs == null)
            {
                xs = new BootstrapBreakpointImageSize(BootstrapBreakpoint.xs, ImageSize.Twelve);

                imageSizes.Add(xs);
            }

            if (sm == null)
            {
                sm = new BootstrapBreakpointImageSize(BootstrapBreakpoint.sm, xs.ImageSize);

                imageSizes.Add(sm);
            }

            if (md == null)
            {
                md = new BootstrapBreakpointImageSize(BootstrapBreakpoint.md, sm.ImageSize);

                imageSizes.Add(md);
            }

            if (lg == null)
            {
                lg = new BootstrapBreakpointImageSize(BootstrapBreakpoint.lg, md.ImageSize);

                imageSizes.Add(lg);
            }

            if (xl == null)
            {
                xl = new BootstrapBreakpointImageSize(BootstrapBreakpoint.xl, lg.ImageSize);

                imageSizes.Add(xl);
            }

            if (xxl == null)
            {
                xxl = new BootstrapBreakpointImageSize(BootstrapBreakpoint.xxl, xl.ImageSize);

                imageSizes.Add(xxl);
            }

            if (xxxl == null)
            {
                xxxl = new BootstrapBreakpointImageSize(BootstrapBreakpoint.xxxl, xxl.ImageSize);

                imageSizes.Add(xxxl);
            }

            imageSizes = imageSizes.OrderBy(i => i.Breakpoint).ToList();

            var sources = new List<string>() { };

            var srcset = new List<string>() { };

            var sizes = new List<string>() { };

            if (dgFramework.IsMobileDevice())
            {
                imageSizes = imageSizes.Skip(0).Take(3).ToList();
            }

            foreach (var i in imageSizes)
            {
                sizes.Add(DetermineSizesForBreakpointImageSize(i, maxWidth));
            }

            if (sizes.Count == 1 && sizes[0].Contains(" "))
            {
                sizes.Add(sizes[0].Split(' ')[2]);
            }

            foreach (var s in sizes)
            {
                var split = s.Split(' ');

                if (split.Count() == 3)
                {
                    sources.Add(split[2].Replace("px", ""));
                }
                else
                {
                    sources.Add(split[0].Replace("px", ""));
                }
            }

            var sourceSetAttribute = $@"srcset=""{String.Join(", ", sources.Distinct().Select(s => $"/image{Framework.WebPFallback}/{s}/{imageId} {s}w"))}""";

            var sizesAttribute = $@"sizes=""{String.Join(", ", sizes)}""";

            //if (dgFramework.IsMobileDevice())
            //{
            //    sizesAttribute = "";
            //}

            return new HtmlString($"{sourceSetAttribute}" +
                $"{sizesAttribute}");
        }

        // Don't go messing with this. 
        // Also don't go changing the Bootstrap breakpoints or column counts. Or I'll cry.
        public static string DetermineSizesForBreakpointImageSize(BootstrapBreakpointImageSize i, int? maxWidth)
        {
            var breakpointMaxWidth = BreakpointMaxWidthAttributeExtensions.GetMaxWidth(i.Breakpoint).MaxWidth;

            var imageSlotSize = 0;

            #region Lengthy switch statements 

            if (dgFramework.IsMobileDevice())
            {
                switch (i.Breakpoint)
                {
                    case BootstrapBreakpoint.xs:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 32;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 90;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 156;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 195;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 239;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Seven:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Eight:
                                    imageSlotSize = 380;
                                    break;
                                case ImageSize.Nine:
                                    imageSlotSize = 428;
                                    break;
                                case ImageSize.Ten:
                                    imageSlotSize = 488;
                                    break;
                                case ImageSize.Eleven:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Twelve:
                                    imageSlotSize = 600;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case BootstrapBreakpoint.sm:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 32;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 90;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 156;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 195;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 239;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Seven:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Eight:
                                    imageSlotSize = 380;
                                    break;
                                case ImageSize.Nine:
                                    imageSlotSize = 428;
                                    break;
                                case ImageSize.Ten:
                                    imageSlotSize = 488;
                                    break;
                                case ImageSize.Eleven:
                                    imageSlotSize = 488;
                                    break;
                                case ImageSize.Twelve:
                                    imageSlotSize = 600;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case BootstrapBreakpoint.md:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 90;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 156;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 195;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 239;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 380;
                                    break;
                                case ImageSize.Seven:
                                    imageSlotSize = 428;
                                    break;
                                case ImageSize.Eight:
                                    imageSlotSize = 488;
                                    break;
                                case ImageSize.Nine:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Ten:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Eleven:
                                    imageSlotSize = 654;
                                    break;
                                case ImageSize.Twelve:
                                    imageSlotSize = 986;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case BootstrapBreakpoint.lg:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 90;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 156;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 239;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 428;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 488;
                                    break;
                                case ImageSize.Seven:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Eight:
                                    imageSlotSize = 654;
                                    break;
                                case ImageSize.Nine:
                                case ImageSize.Ten:
                                case ImageSize.Eleven:
                                case ImageSize.Twelve:
                                    imageSlotSize = 986;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case BootstrapBreakpoint.xl:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 90;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 239;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 428;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 654;
                                    break;
                                case ImageSize.Seven:
                                    imageSlotSize = 986;
                                    break;
                                case ImageSize.Eight:
                                case ImageSize.Ten:
                                case ImageSize.Eleven:
                                case ImageSize.Twelve:
                                    imageSlotSize = 986;
                                    break;
                                //case ImageSize.Nine:
                                //    imageSlotSize = 986;
                                //    break;
                                //case ImageSize.Ten:
                                //case ImageSize.Eleven:
                                //case ImageSize.Twelve:
                                //    imageSlotSize = 1024;
                                //    if (maxWidth >= 1546)
                                //    {
                                //        imageSlotSize = 1546;
                                //    }
                                //    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case BootstrapBreakpoint.xxl:
                    case BootstrapBreakpoint.xxxl:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 156;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 380;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 654;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 986;
                                    break;
                                case ImageSize.Seven:
                                case ImageSize.Eight:
                                case ImageSize.Nine:
                                case ImageSize.Ten:
                                case ImageSize.Eleven:
                                case ImageSize.Twelve:
                                    imageSlotSize = 986;
                                    break;
                                //case ImageSize.Eight:
                                //case ImageSize.Nine:
                                //case ImageSize.Ten:
                                //case ImageSize.Eleven:
                                //case ImageSize.Twelve:
                                //    imageSlotSize = 1024;
                                //    if (maxWidth >= 1546)
                                //    {
                                //        imageSlotSize = 1546;
                                //    }
                                //    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (i.Breakpoint)
                {
                    case BootstrapBreakpoint.xs:
                    case BootstrapBreakpoint.sm:
                    case BootstrapBreakpoint.md:
                    case BootstrapBreakpoint.lg:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 90;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 195;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 239;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 428;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 488;
                                    break;
                                case ImageSize.Seven:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Eight:
                                    imageSlotSize = 654;
                                    break;
                                case ImageSize.Nine:
                                case ImageSize.Ten:
                                case ImageSize.Eleven:
                                case ImageSize.Twelve:
                                    imageSlotSize = 986;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case BootstrapBreakpoint.xl:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 90;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 239;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 428;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Six:
                                    imageSlotSize = 654;
                                    break;
                                case ImageSize.Seven:
                                case ImageSize.Eight:
                                case ImageSize.Nine:
                                    imageSlotSize = 986;
                                    break;
                                case ImageSize.Ten:
                                case ImageSize.Eleven:
                                case ImageSize.Twelve:
                                    imageSlotSize = 1024;
                                    if (maxWidth >= 1546)
                                    {
                                        imageSlotSize = 1546;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case BootstrapBreakpoint.xxl:
                    case BootstrapBreakpoint.xxxl:
                        {
                            switch (i.ImageSize)
                            {
                                case ImageSize.One:
                                    imageSlotSize = 156;
                                    break;
                                case ImageSize.Two:
                                    imageSlotSize = 322;
                                    break;
                                case ImageSize.Three:
                                    imageSlotSize = 380;
                                    break;
                                case ImageSize.Four:
                                    imageSlotSize = 600;
                                    break;
                                case ImageSize.Five:
                                    imageSlotSize = 654;
                                    break;
                                case ImageSize.Six:
                                case ImageSize.Seven:
                                    imageSlotSize = 986;
                                    break;
                                case ImageSize.Eight:
                                case ImageSize.Nine:
                                case ImageSize.Ten:
                                case ImageSize.Eleven:
                                case ImageSize.Twelve:
                                    imageSlotSize = 1024;
                                    if (maxWidth >= 1546)
                                    {
                                        imageSlotSize = 1546;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            #endregion

            if (!String.IsNullOrEmpty(breakpointMaxWidth))
            {
                return $"(max-width: {breakpointMaxWidth}) {imageSlotSize}px";
            }
            else 
            {
                return $"{imageSlotSize}px";
            }
        }

        public static IHtmlString InitialImageFolderSize(ImageSize imageSize)
        {
            switch (imageSize)
            {
                case ImageSize.One:
                    return new HtmlString("90");
                case ImageSize.Two:
                    return new HtmlString("156");
                case ImageSize.Three:
                    return new HtmlString("239");
                case ImageSize.Four:
                    return new HtmlString("322");
                case ImageSize.Five:
                    return new HtmlString("428");
                case ImageSize.Six:
                    return new HtmlString("488");
                case ImageSize.Seven:
                    return new HtmlString("600");
                case ImageSize.Eight:
                    return new HtmlString("654");
                case ImageSize.Nine:
                case ImageSize.Ten:
                case ImageSize.Eleven:
                case ImageSize.Twelve:
                    return new HtmlString("986");
                default:
                    return null;
            }
        }

        public static IHtmlString BannerImageSourceSetSizes(long imageId, int version, ImageSize imageSize)
        {
            switch (imageSize)
            {
                case ImageSize.One:
                case ImageSize.Two:
                    return new HtmlString($@"srcset=""/imagebanner{Framework.WebPFallback}/156/{imageId}?v={version} 156w""" +
                        $@"sizes=""(max-width: 156px) 156px,156px""");
                case ImageSize.Three:
                    return new HtmlString($@"srcset=""/imagebanner{Framework.WebPFallback}/156/{imageId}?v={version} 156w,/imagebanner{Framework.WebPFallback}/256/{imageId}?v={version} 256w""" +
                        $@"sizes=""(max-width: 156px) 156px,256px""");
                case ImageSize.Four:
                    return new HtmlString($@"srcset=""/imagebanner{Framework.WebPFallback}/156/{imageId}?v={version} 156w,/imagebanner{Framework.WebPFallback}/256/{imageId}?v={version} 256w,/imagebanner{Framework.WebPFallback}/342/{imageId}?v={version}""" +
                        $@"sizes=""(max-width: 156px) 156px,(max-width: 256px) 256px,342px""");
                case ImageSize.Five:
                    return new HtmlString($@"srcset=""/imagebanner{Framework.WebPFallback}/156/{imageId}?v={version} 156w,/imagebanner{Framework.WebPFallback}/256/{imageId}?v={version} 256w,/imagebanner{Framework.WebPFallback}/342/{imageId}?v={version} 342w,/imagebanner{Framework.WebPFallback}/512/{imageId}?v={version}""" + $@"sizes=""(max-width: 156px) 156px,(max-width: 256px) 256px,(max-width: 342px) 342px,512px""");
                case ImageSize.Six:
                case ImageSize.Seven:
                case ImageSize.Eight:
                case ImageSize.Nine:
                case ImageSize.Ten:
                case ImageSize.Eleven:
                case ImageSize.Twelve:
                default:
                    return new HtmlString($@"srcset=""/imagebanner{Framework.WebPFallback}/156/{imageId}?v={version} 156w,/imagebanner{Framework.WebPFallback}/256/{imageId}?v={version} 256w,/imagebanner{Framework.WebPFallback}/342/{imageId}?v={version},/imagebanner{Framework.WebPFallback}/512/{imageId}?v={version} 512w,/imagebanner{Framework.WebPFallback}/1024/{imageId}?v={version}""" + $@"sizes=""(max-width: 156px) 156px,(max-width: 256px) 256px,(max-width: 342px) 342px,(max-width: 512px) 512px,1024px""");
            }
        }

        public static IHtmlString EscapeForJS(string value)
        {
            return new HtmlString(HttpUtility.JavaScriptStringEncode(value));
        }

        public static IHtmlString EscapeForJS(object value)
        {
            return new HtmlString(value == null ? null : HttpUtility.JavaScriptStringEncode(value.ToString()));
        }

        public static IHtmlString Resource(string name, byte languageId, short countryId, DateTime useDate)
        {
            var resourceDefinition = new cmsResource().loadResourceFromCms(languageId, countryId, useDate);

            var value = resourceDefinition.Result.resourceSelects.Where(p => p.Name == name).Select(p => p.Value).SingleOrDefault();

            return new HtmlString(value);
        }

        // Thank you very much to logicnp http://stackoverflow.com/questions/6520006/find-common-items-across-multiple-lists-in-c-sharp
        public static List<T> FindCommon<T>(IEnumerable<List<T>> lists)
        {
            Dictionary<T, int> map = new Dictionary<T, int>();
            int listCount = 0; // number of lists

            foreach (IEnumerable<T> list in lists)
            {
                listCount++;
                foreach (T item in list)
                {
                    // Item encountered, increment count
                    if (!map.TryGetValue(item, out int currCount))
                        currCount = 0;

                    currCount++;
                    map[item] = currCount;
                }
            }

            List<T> result = new List<T>();
            foreach (KeyValuePair<T, int> kvp in map)
            {
                // Items whose occurrence count is equal to the number of lists are common to all the lists
                if (kvp.Value == listCount)
                    result.Add(kvp.Key);
            }

            return result;
        }

        public static string ProcessAttributeFilterText(string input, string attributeFilterText, string productTypePluralPropercase, string attributeSignatureText, bool enabledByAttributeCombination, bool useHtml = true)
        {
            var filterRegex = new Regex(@"{FilterStart}((.|\n|\r)*?){FilterEnd}");
            var noFilterRegex = new Regex(@"{NoFilterStart}((.|\n|\r)*?){NoFilterEnd}");
            var productTypeProperRegex = new Regex(@"{ProductTypeProperStart}((.|\n|\r)*?){ProductTypeProperEnd}");
            var productTypeRegex = new Regex(@"{ProductTypeStart}((.|\n|\r)*?){ProductTypeEnd}");
            var attributeSignatureRegex = new Regex(@"{AttributeSignatureStart}((.|\n|\r)*?){AttributeSignatureEnd}");

            if (useHtml)
            {
                if (enabledByAttributeCombination && !String.IsNullOrEmpty(attributeFilterText))
                {
                    input = filterRegex.Replace(input, $"<span class=\"filter-text\" data-original-contents=\"$1\">{attributeFilterText?.Trim()}</span> ");
                    input = input.Replace("{NoFilterStart}", "<span class=\"d-none no-filter-visible\">");
                    input = input.Replace("{NoFilterEnd}", "</span>");
                }
                else
                {
                    input = filterRegex.Replace(input, "<span class=\"filter-text\" data-original-contents=\"$1\">$1</span>");
                    //filterText = noFilterRegex.Replace(filterText, "<span class=\"no-filter-text\" data-original-contents=\"$1\">$1</span>");
                    input = filterRegex.Replace(input, $"<span class=\"filter-text\" data-original-contents=\"$1\">{attributeFilterText?.Trim()}</span> ");
                    input = input.Replace("{NoFilterStart}", "<span class=\"no-filter-visible\">");
                    input = input.Replace("{NoFilterEnd}", "</span>");
                }

                if (!String.IsNullOrEmpty(productTypePluralPropercase))
                {
                    input = productTypeProperRegex.Replace(input, $"<span class=\"product-type-propercase-text\" data-original-contents=\"$1\">{productTypePluralPropercase?.Trim()}</span> ");
                    input = productTypeRegex.Replace(input, $"<span class=\"product-type-text\" data-original-contents=\"$1\">{productTypePluralPropercase?.Trim().ToLowerInvariant()}</span> ");
                }

                if (!String.IsNullOrEmpty(attributeSignatureText))
                {
                    input = attributeSignatureRegex.Replace(input, $"<span class=\"attribute-signature-text\" data-original-contents=\"$1\">{attributeSignatureText?.Trim()}</span> ");
                }
                else
                {
                    input = attributeSignatureRegex.Replace(input, "<span class=\"attribute-signature-text\"></span>"); //placeholder necessary to receive ast as filters change
                }
            }
            else
            {
                if (enabledByAttributeCombination && !String.IsNullOrEmpty(attributeFilterText))
                {
                    input = filterRegex.Replace(input, $"{attributeFilterText?.Trim()}");
                    input = input.Replace("{NoFilterStart}", "<span class=\"d-none no-filter-visible\">");
                    input = input.Replace("{NoFilterEnd}", "</span>");
                }

                if (!String.IsNullOrEmpty(productTypePluralPropercase))
                {
                    input = productTypeProperRegex.Replace(input, productTypePluralPropercase?.Trim());
                    input = productTypeRegex.Replace(input, productTypePluralPropercase?.Trim().ToLowerInvariant());
                }

                if (!String.IsNullOrEmpty(attributeSignatureText))
                {
                    input = attributeSignatureRegex.Replace(input, attributeSignatureText?.Trim());
                }
                else
                {
                    input = attributeSignatureRegex.Replace(input, "");
                }
            }

            return input;
        }

        private static Regex ImageParentColumn = new Regex(@"g-col-([^0-9\-]+)?-?([0-9]+)");

        public static IHtmlString ProcessHTMLSection(HtmlPageSection section, BaseViewModel objBase, IEnumerable<ImageHotspot> overridingHotspots = null)
        {
            if (String.IsNullOrEmpty(section.Contents))
                return null;

            section.Contents = new Regex(@"\<\/?html\>").Replace(section.Contents, "");
            section.Contents = new Regex(@"\<\/?head\>").Replace(section.Contents, "");
            section.Contents = new Regex(@"\<\/?body\>").Replace(section.Contents, "");

            section.Contents = ProcessAttributeFilterText(section.Contents, objBase.AttributeFilterText, objBase.CategoryPageProductType?.NamePlural, objBase.AttributeSignatureText, objBase.FilterIndexCombinationEnabled, true);

            if (!String.IsNullOrEmpty(section.Contents))
            {
                section.Contents = ProcessHTMLImages(section.Contents, section.HtmlSectionID, section.ImageIDs, overridingHotspots ?? section.Hotspots, objBase);
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(section.Contents);

            if (HtmlHelperExtra.IsNonProductionVersion)
            {
                var nvc = new System.Collections.Specialized.NameValueCollection(HttpContext.Current.Request.QueryString);

                var anchorQuery = doc.DocumentNode.Descendants("a");

                var qs = Framework.ConstructQueryString(nvc);

                foreach (var aTag in anchorQuery.ToList())
                {
                    var currentHref = aTag.Attributes["href"]?.Value;

                    if (!String.IsNullOrEmpty(currentHref))
                    {
                        if (currentHref.IndexOf('?') == -1)
                        {
                            currentHref = $"{currentHref}?{qs}";
                        }
                        else
                        {
                            if (currentHref.LastIndexOf('?') > currentHref.LastIndexOf('/'))
                            {
                                currentHref = $"{currentHref}&{qs}";
                            }
                            else
                            {
                                currentHref = $"{currentHref}?{qs}";
                            }
                        }

                        aTag.Attributes.Remove("data-preview-overrides");

                        aTag.Attributes["href"].Value = currentHref.TrimEnd(new char[] { '?' });
                    }
                }
            }

            return new HtmlString(doc.DocumentNode.OuterHtml);// ProcessAttributeFilterText(doc.DocumentNode.OuterHtml, objBase.AttributeFilterText, objBase.CategoryPageProductType?.NamePlural, objBase.AttributeSignatureText));
        }

        public static string ProcessHTMLImages(
            string source,
            long? htmlSectionId,
            IEnumerable<long> imageIds,
            IEnumerable<ImageHotspot> imageHotspots,
            BaseViewModel objBase)
		{
			// 1. Create the base dummy context
			var request = new SimpleWorkerRequest("", "", new StringWriter());
			var fakeContext = new HttpContext(request);

			// 2. Build the fake session storage container
			var sessionItems = new SessionStateItemCollection();
			// Optional: Preload any data your view needs here
			// sessionItems["SomeKey"] = "SomeValue";

			var sessionContainer = new HttpSessionStateContainer(
				id: $"FakeSessionId-{DateTime.Now.Ticks}",
				sessionItems: sessionItems,
				staticObjects: new HttpStaticObjectsCollection(),
				timeout: 20,
				newSession: true,
				cookieMode: HttpCookieMode.UseCookies,
				mode: SessionStateMode.InProc,
				isReadonly: false
			);

			// 3. Use reflection to build the internal HttpSessionState wrapper
			var sessionStateType = typeof(HttpSessionState);
			var constructor = sessionStateType.GetConstructor(
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(IHttpSessionState) },
				null
			);

			if (constructor == null)
			{
				throw new InvalidOperationException("Could not find internal HttpSessionState constructor.");
			}

			var actualSessionState = (HttpSessionState)constructor.Invoke(new object[] { sessionContainer });

			// 4. CRITICAL: Force the HttpContext to cache the session by setting the internal field
			var sessionField = typeof(HttpContext).GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance);
			if (sessionField != null)
			{
				sessionField.SetValue(fakeContext, actualSessionState);
			}

			// No worries if your current context is already dead! Which it can be because you might well be calling this function in a background thread!
			var controller = ViewRenderer.CreateController<GenericController>();

            var doc = new HtmlDocument();

            doc.LoadHtml(source);

            if (imageIds.Any())
            {
                var htmlModelImagesInit = $@"<span class=""d-none"" data-bind=""init: {{ field: html.images, value: [{String.Join(",", imageIds.Select(i => $"imageViewModel({i})"))}] }}, remove""></span>";

                doc.DocumentNode.PrependChild(HtmlNode.CreateNode(htmlModelImagesInit));
            }

            var imgQuery = doc.DocumentNode.Descendants("img");

            var commentNodes = doc.DocumentNode.SelectNodes("//comment()");

            if (commentNodes != null)
            {
                foreach (var comment in commentNodes)
                {
                    comment.ParentNode.RemoveChild(comment);
                }
            }

            foreach (var imgTag in imgQuery.ToList())
            {
                if (Int64.TryParse(imgTag.Attributes["imageid"]?.Value, out long imageId))
                {
                    var parentNode = imgTag.ParentNode;

                    var imageSize = ImageSize.Four;

                    int? maxWidth = null;

                    if (Int32.TryParse(imgTag.GetDataAttribute("max-width")?.Value, out int mw))
                    {
                        maxWidth = mw;
                    }

                    while (!parentNode.GetClasses().Any(c => ImageParentColumn.Match(c).Success) && parentNode != doc.DocumentNode)
                    {
                        parentNode = parentNode.ParentNode;
                    }

                    var imageSizes = new List<BootstrapBreakpointImageSize>();

                    //if (!dgFramework.IsMobileDevice())
                    //{
                    foreach (var c in parentNode.GetClasses().Where(c => ImageParentColumn.Match(c).Success))
                    {
                        var matches = ImageParentColumn.Matches(c);

                        for (var i = 0; i < matches.Count; i++)
                        {
                            var match = matches[i];

                            if (match.Groups.Count == 3)
                            {
                                imageSizes.Add(new BootstrapBreakpointImageSize(
                                    bb: (BootstrapBreakpoint)Enum.Parse(typeof(BootstrapBreakpoint), String.IsNullOrEmpty(match.Groups[1].Value) ? "xs" : match.Groups[1].Value),
                                    i: (ImageSize)Convert.ToInt32(match.Groups[2].Value)
                                ));

                                imageSize = (ImageSize)Convert.ToInt32(match.Groups[2].Value);
                            }
                        }
                    }
                    //}
                    //else
                    //{

                    //}

                    var imageSourceSetSizes = ImageSourceSetSizes(imageId, imageSizes, maxWidth);

                    var altText = imgTag.Attributes["alt"]?.Value;
                    var titleText = imgTag.Attributes["title"]?.Value ?? altText;
                    var hideHotspots = false;

                    if (Boolean.TryParse(imgTag.GetDataAttribute("hide-hotspots")?.Value, out bool hh)) 
                    {
                        hideHotspots = hh;
                    }

                    var largeImageModalAvailable = Convert.ToBoolean(imgTag.GetDataAttribute("large-image-modal-available")?.Value ?? "false");
                    var folderWidth = Convert.ToInt16(InitialImageFolderSize(imageSize).ToString());
                    var width = folderWidth;
                    var height = width;

                    if (Int16.TryParse(imgTag.Attributes["width"]?.Value, out short w))
                        width = w;

                    if (Int16.TryParse(imgTag.Attributes["height"]?.Value, out short h))
                        height = h;

                    var hotspotImage = imageHotspots.SingleOrDefault(hi => hi.ImageID == imageId);

                    var hotspotDance = hideHotspots == false && hotspotImage != null && hotspotImage.Hotspots.Any();

                    var replacement = $@"<img src=""/image{Framework.WebPFallback}/{folderWidth}/{imageId}""
                                              class=""d-block w-100""
                                              alt=""{altText}""
                                              title=""{titleText}""
                                              width=""{width}""
                                              height=""{height}""
                                              loading=""{(imgTag == imgQuery.First() ? "eager" : "lazy")}""";

                    if (largeImageModalAvailable)
                    {
                        replacement += @"data-bind=""click: (data, event) => {{ window.largeImageModal.load($parent, event) }}""";
                    }

                    //replacement += $@"{HtmlHelperExtra.ImageSourceSetSizes(imageId, imageSize)}>";

                    replacement += $@"{imageSourceSetSizes}>";

                    // I believe we need to put together our order form groups up front -- we can't be calling foreachInit however many times we have images with hotspots on
                    // this leads (eventually) to the correct number of order form groups, but every time... the... products... the products! oh dear.

                    string hotspots = null;

                    if (hotspotDance)
                    {
                        hotspots += $@"
<div class=""carousel"">
    <div class=""collapse hotspot-collapse"" id=""html-section-{htmlSectionId}-image-{imageId}-hotspot-collapse"">
        <!-- ko with: html.getImageById({imageId}) -->
        <!-- ko foreachInit: {{ data: orderFormGroups, createElement: createOrderFormGroup }} -->
    ";
                        foreach (var hotspot in hotspotImage.Hotspots)
                        {
                            controller.ViewBag.OrderFormPrefix = $@"-hotspot-{Guid.NewGuid()}";

                            hotspots += $@"
            <div class=""hotspot-marker"" style=""left: {hotspot.x}; top: {hotspot.y}"" data-init>
                <div class=""hotspot-overlay"">
                    <div class=""hotspot-overlay-inner"">
                        <div class=""order-form-group"">
                            {ViewRenderer.RenderViewToString(controller.ControllerContext, "~/Views-New/ProductPages/Shared/_Hotspot.cshtml", hotspot.Product, true)}
                        </div>
                    </div>
                </div>
            </div>";
                        }

                        hotspots += $@"
        <!-- /ko -->
        <!-- /ko -->
    </div>
    <div class=""hotspot-products-btn-wrapper"">
        <i class=""btn btn-link hotspot-products-btn"" data-bs-target=""#html-section-{htmlSectionId}-image-{imageId}-hotspot-collapse"" data-bs-toggle=""collapse"" onclick=""this.closest('.g-col-6').classList.toggle('g-col-12')"">
            {Resource("ShowImageProducts", objBase.LanguageID, objBase.CountryID, objBase.Date)}
        </i>
    </div>
</div>";
                    }

                    var originalParent = imgTag.ParentNode;

                    var replacementNode = HtmlNode.CreateNode(replacement);

                    if (!String.IsNullOrEmpty(hotspots))
                    {
                        var hotspotsNode = HtmlNode.CreateNode(hotspots);

                        if (dgFramework.IsMobileDevice())
                        {
                            if (originalParent.Name == "a")
                            {
                                var grandparent = originalParent.ParentNode;
                                var originalAnchor = originalParent;
                                originalParent.ReplaceChild(replacementNode, imgTag);
                                grandparent.AppendChild(hotspotsNode);
                            }
                            else
                            {
                                //replacementNode.ParentNode.PrependChild(hotspotsNode);

                                var grandparent = originalParent.ParentNode;
                                var originalAnchor = originalParent;
                                hotspotsNode.AppendChild(replacementNode);
                                originalParent.RemoveChild(imgTag);
                                //originalParent.ReplaceChild(replacementNode, imgTag);
                                originalParent.AppendChild(hotspotsNode);
                            }
                        }
                        else
                        {
                            if (originalParent.Name == "a")
                            {
                                var grandparent = originalParent.ParentNode;
                                var originalAnchor = originalParent;
                                originalParent.ReplaceChild(replacementNode, imgTag);
                                hotspotsNode.AppendChild(originalAnchor);
                                grandparent.ReplaceChild(hotspotsNode, originalAnchor);
                            }
                            else
                            {
                                var grandparent = originalParent.ParentNode;
                                var originalAnchor = originalParent;
                                hotspotsNode.AppendChild(replacementNode);
                                originalParent.RemoveChild(imgTag);
                                //originalParent.ReplaceChild(replacementNode, imgTag);
                                originalParent.AppendChild(hotspotsNode);
                            }
                        }
                    }
                    else
                    {
                        imgTag.ParentNode.ReplaceChild(replacementNode, imgTag);
                    }
                }
                else
                {
                    imgTag.SetAttributeValue("loading", "lazy");
                }
            }

            return doc.DocumentNode.OuterHtml;
        }
    }
}