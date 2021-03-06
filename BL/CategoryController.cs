﻿using DL.Repositories;
using Model;
using System.Collections.Generic;

namespace BL
{
    public class CategoryController
    {
        private CategoryRepository categoryRepository;

        public CategoryController()
        {
            categoryRepository = new CategoryRepository();
        }

        public void AddNewCategory(Category category)
        {
            categoryRepository.Create(category);
        }

        public void DeleteCategory(string categoryName)
        {
            categoryRepository.Delete(categoryName);
        }

        public void UpdateCategoryName(string currentName, string newName)
        {
            categoryRepository.Update(currentName, newName);
        }

        public List<Category> GetCategories()
        {
            return categoryRepository.GetAll();
        }
    }
}