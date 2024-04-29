using System.Data;
using System.Linq;
using wwwDesignersGuild.Models.BasketClasses;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_Basket : FixedPageViewModel<Basket>
    {
        public bool IsEmpty
        {
            get
            {
                return !Definition.BasketItems.Where(p => p.SaveForLater == false).Any();
            }
        }

        public string PromotionCodeFromQueryString { get; set; }

        public FixedPageViewModel_Basket(
            Basket objDefinition,
            BaseViewModel objBase)
            : base(objDefinition, objBase)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            Initialise();

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"FixedPageViewModel_Basket initialised in {sw.Elapsed}");
#endif
        }

        new protected void Initialise()
        {
            #region Preamble

            Definition.BasketItems.ForEach(bi =>
            {
                bi.DeliveryOptions = Definition.ProductDeliveryOptions.Where(pdo => pdo.ProductID == bi.ProductID).ToList();
            });

            #endregion

            #region The Bit in the Middle



            #endregion

            #region Postamble



            #endregion
        }
    }
}