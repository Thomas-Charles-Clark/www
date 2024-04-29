using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Interface;


namespace wwwDesignersGuild.Models.Menu
{


    public class cmsMenu : ICmsMenuRepository
    {

        public async Task<cmsMenuDefinition> loadMenuFromCms(byte languageId, short countryId, DateTime useDate, bool isTrade)
        {

            //  Is page in cache?
            cmsMenuDefinition menuStore;
            MenuCacheProvider objMenuCache = new MenuCacheProvider();
            menuStore = objMenuCache.Get(languageId, countryId, useDate, isTrade);
            
            if (menuStore == null)
            {
                menuStore = await Task.FromResult<cmsMenuDefinition>(loadMenuFromCmsDatabaseAsync(languageId, countryId, useDate, isTrade));
                objMenuCache.Set(languageId, countryId, useDate, menuStore, isTrade);
                menuStore.MenuLoadedFromCache = false;
            }
            else
            {
                menuStore.MenuLoadedFromCache = true;
            }

            menuStore.MobileMenuItems = menuStore.menuItems.Select(mi => new MobileMenuItem(mi)).ToList();

            var unbuilt = menuStore.MobileMenuItems.Where(i => i.IsMenuItemAlreadyBuilt == false).ToList();

            return menuStore;

        }

        private cmsMenuDefinition loadMenuFromCmsDatabaseAsync(byte languageId, short countryId, DateTime useDate, bool isTrade)
        {

            
            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.Menu_GenerateMultiple(languageId, countryId, useDate, isTrade);


            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsMenuDefinition menuStore = new cmsMenuDefinition
            {
                menuGroups = results.GetResult<menuGroup>().ToList<menuGroup>(),
                menuItems = results.GetResult<menuItem>().ToList<menuItem>()
            };

            return menuStore;


        }

    }


}