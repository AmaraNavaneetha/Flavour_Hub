using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Models;

namespace restapp.Services
{
    public class DBServices
    {
        //whenever we want any function like sliders to use in any where, then 
        //we need to create a class like this

        //this class is created to display the
        //slider data anywhere where we want to display the
        //slider in the entire project
        private readonly RestContext _dbContext;
        public DBServices()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string conStr = configuration.GetConnectionString("SQLServerConnection");

            var contextOptions = new DbContextOptionsBuilder<RestContext>().UseSqlServer(conStr).Options;
            _dbContext = new RestContext(contextOptions);
        }
        public List<Sliders> GetAllSliders()
        {
            return (from s in _dbContext.sliders
                    orderby s.DisplayOrderNo
                    select s).ToList();
        }
        public List<Category> GetAllCategories()
        {
            return (from c in _dbContext.categories
                    select c).ToList();
        }

        public List<SelectListItem>  GetCategorySelectItems()
        {
            List<SelectListItem> catList = new List<SelectListItem>();
            foreach (Category c in _dbContext.categories)
            {
                catList.Add(new SelectListItem(c.CategoryName, c.CategoryId.ToString()));
            }
            return catList;
        }

        public List<SelectListItem> GetItemTypeSelectListItem()
        {
            List<SelectListItem> iTypeList = new List<SelectListItem>();
            foreach (ItemType itype in _dbContext.itemTypes)
            {
                iTypeList.Add(new SelectListItem(itype.ItemTypeName, itype.ItemTypeId.ToString()));
            }
            return iTypeList;
        }

    }
}
