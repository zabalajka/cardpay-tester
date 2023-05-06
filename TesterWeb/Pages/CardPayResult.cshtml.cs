using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TesterWeb.Pages
{
    public class CardPayResult : PageModel
    {
        private readonly ILogger<CardPayResult> _logger;

        public CardPayResult(ILogger<CardPayResult> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}