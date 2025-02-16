﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperShop.Data.Entities
{
    public class ProductTranslation
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string LanguageId { get; set; }
        public string Name { get; set; }
        public string SeoAlias { get; set; }
        public string Description { get; set; }
        public string SeoDescription { get; set; }
        public string SeoTitle { get; set; }
        public string Detail { get; set; }
        public Language Language { get; set; }
        public Product Product { get; set; }
    }
}