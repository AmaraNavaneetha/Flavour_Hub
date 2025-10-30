using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Models;
using Microsoft.Extensions.Configuration;

namespace restapp.Services
{
    public class DBServices
    {
        private readonly RestContext _dbContext;

        // Use this constructor for Dependency Injection (Best Practice)
        public DBServices(RestContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ⭐ CORRECTED: Using 's.Status' instead of 's.SliderStatus'
        // Public data: Filter to only active sliders
        public List<Sliders> GetAllSliders()
        {
            return (from s in _dbContext.sliders
                    where s.Status == true // CORRECTED property name
                    orderby s.DisplayOrderNo
                    select s).ToList();
        }

        // NEW: Method to get only ACTIVE categories for the public menu
        public List<Category> GetActiveCategories()
        {
            return _dbContext.categories
                    .Where(c => c.CategoryStatus == true)
                    .ToList();
        }

        // Admin data: Gets all categories (active or not)
        public List<Category> GetAllCategories()
        {
            return (from c in _dbContext.categories
                    select c).ToList();
        }

        public List<SelectListItem> GetCategorySelectItems()
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

        // Filters by CategoryId AND ensures food item IsAvailable is TRUE
        public List<FoodItem> GetFoodItemsByCategory(int categoryId)
        {
            return _dbContext.fooditems
                .Include(f => f.category)
                .Include(f => f.itemType)
                .Where(f => f.CategoryId == categoryId && f.IsAvailable == true)
                .ToList();
        }

        // NOTE: Your original parameterless constructor is omitted here for brevity 
        // but should remain in your DBServices.cs if you use it in other places.
    }
}