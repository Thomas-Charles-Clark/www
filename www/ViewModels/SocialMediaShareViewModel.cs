using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels
{
    public class SocialMediaShareViewModel
    {
        public string DirectionClass { get; set; }
        public bool ButtonOnly { get; set; }
        public BaseViewModel Base { get; set; }

        public SocialMediaShareViewModel(string directionClass, bool buttonOnly, BaseViewModel objBase)
        {
            Base = objBase;
            ButtonOnly = buttonOnly;
            DirectionClass = directionClass;
        }
    }
}