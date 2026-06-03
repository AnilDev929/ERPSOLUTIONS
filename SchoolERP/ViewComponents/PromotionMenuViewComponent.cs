using Microsoft.AspNetCore.Mvc;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.ViewComponents
{
    public class PromotionMenuViewComponent : ViewComponent
    {
        private readonly IPromotionService _promotionService;
        private readonly IAcademicYearService _yearService;

        public PromotionMenuViewComponent(
            IPromotionService promotionService, IAcademicYearService yearService)
        {
            _promotionService = promotionService;
            _yearService = yearService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var year = await _yearService.GetCurrentAcademicYearAsync();
            var state = await _promotionService.GetPromotionStateAsync(year.AcademicYearID);
            return View(state);
        }
    }
}
