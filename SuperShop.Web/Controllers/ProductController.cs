﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using SuperShop.ViewModels.Catalog.Products;
using SuperShop.ViewModels.Common;
using SuperShop.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperShop.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductApiUser _productApiClient;
        private readonly ICategoryApiUser _categoryApiClient;

        public ProductController(IProductApiUser productApiClient, ICategoryApiUser categoryApiClient)
        {
            _productApiClient = productApiClient;
            _categoryApiClient = categoryApiClient;
        }

        public async Task<IActionResult> Index(string keyword, int? categoryid, int pageIndex = 1, int pageSize = 10)
        {
            var languageid = HttpContext.Session.GetString("DefaultLanguage");
            var request = new GetProductPagingRequest()
            {
                Keyword = keyword,
                PageIndex = pageIndex,
                PageSize = pageSize,
                LanguageId = languageid,
                CategoryId = categoryid
            };
            var data = await _productApiClient.GetPaging(request);
            ViewBag.Keyword = keyword;
            var category = await _categoryApiClient.GetAll(languageid);
            ViewBag.Categories = category.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = categoryid.HasValue && categoryid.Value == x.Id
            });
            if (TempData["result"] != null)
            {
                ViewBag.SuccessMsg = TempData["result"];
            }
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelState);
            }
            var result = await _productApiClient.CreateProduct(request);
            if (result)
            {
                TempData["result"] = "Create is Successfull !";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Create is unsuccess !");
            return View(request);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            return View(new ProductDeleteRequest()
            {
                Id = id
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] ProductDeleteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            var result = await _productApiClient.DeleteProduct(request.Id);
            if (result)
            {
                TempData["result"] = "Delete is successful !";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Delete is unsuccess !");
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var languageId = HttpContext.Session.GetString("DefaultLanguage");
            var request = await _productApiClient.GetById(id, languageId);
            var product = new ProductUpdateRequest()
            {
                Id = request.Id,
                Name = request.Name,
                Price = (int)request.Price,
                OriginalPrice = (int)request.OriginalPrice,
                Stock = request.Stock,
                Description = request.Description,
                SeoAlias = request.SeoAlias,
                SeoTitle = request.SeoTitle,
                SeoDescription = request.SeoDescription,
                Detail = request.Detail,
            };
            return View(product);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Edit([FromForm] ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            var result = await _productApiClient.UpdateProduct(request);
            if (result)
            {
                TempData["result"] = "Update is successful !";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Update is unsuccessful !");
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> CategoryAssign(int id)
        {
            var categoryAssign = await GetCategory(id);
            return View(categoryAssign);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryAssign(CategoryAssignRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelState);
            }
            var result = await _productApiClient.CategoryAssign(request.Id, request);
            if (result.IsSuccessed)
            {
                TempData["result"] = "Add category is successful !";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result.Message);
            var category = await GetCategory(request.Id);
            return View(category);
        }

        private async Task<CategoryAssignRequest> GetCategory(int id)
        {
            var languageId = HttpContext.Session.GetString("DefaultLanguage");

            var productObj = await _productApiClient.GetById(id, languageId);
            var categories = await _categoryApiClient.GetAll(languageId);
            var categoryAssignRequest = new CategoryAssignRequest();
            foreach (var role in categories)
            {
                categoryAssignRequest.Categories.Add(new SelectItem()
                {
                    Id = role.Id.ToString(),
                    Name = role.Name,
                    Selected = productObj.Categories.Contains(role.Name)
                });
            }
            return categoryAssignRequest;
        }
    }
}