using System.Web.Mvc;
using FakeItEasy;
using FluentAssertions;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Controllers;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;
using MrCMS.Web.Apps.Ecommerce.Services.Misc;
using MrCMS.Web.Apps.Ecommerce.Services.Orders;
using Xunit;

namespace MrCMS.EcommerceApp.Tests.Admin.Controllers
{
    public class OrderControllerTests
    {
        private readonly IOrderService _orderService;
        private readonly OrderController _orderController;
        private readonly IOptionService _optionService;
        private readonly IOrderSearchService _orderSearchService;
        private readonly IOrderShippingService _orderShippingService;
        private readonly SiteSettings _ecommerceSettings;

        public OrderControllerTests()
        {
            _orderService = A.Fake<IOrderService>();
            _optionService = A.Fake<IOptionService>();
            _orderSearchService = A.Fake<IOrderSearchService>();
            _orderShippingService = A.Fake<IOrderShippingService>();
            _ecommerceSettings = new SiteSettings() { DefaultPageSize = 10 };
            _orderController = new OrderController(_orderService,
                _orderSearchService, _orderShippingService, _optionService, _ecommerceSettings, A.Fake<IExportOrdersService>(), A.Fake<IUserService>());
        }

        //[Fact]
        //public void OrderController_Index_ReturnsAViewResult()
        //{
        //    var result = _orderController.Index();

        //    result.Should().BeOfType<ViewResult>();
        //}

        //[Fact]
        //public void OrderController_Index_ShouldCallOrderServiceGetAllPagedWithPassedArgument()
        //{
        //    var result = _orderController.Index(123);

        //    A.CallTo(() => _orderService.GetPaged(123, 10)).MustHaveHappened();
        //}

        //[Fact]
        //public void OrderController_Index_ShouldReturnResultOfOrderServiceCallAsModel()
        //{
        //    var pagedList = A.Fake<IPagedList<Order>>();
        //    A.CallTo(() => _orderService.GetPaged(123, 10)).Returns(pagedList);
        //    var result = _orderController.Index(123);

        //    result.Model.Should().Be(pagedList);
        //}

        [Fact]
        public void OrderController_Edit_ReturnsAViewResult()
        {
            var order = new Order();

            var result = _orderController.Edit(order);

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void OrderController_Edit_ReturnsThePassedObjectAsTheModel()
        {
            var order = new Order();

            var result = _orderController.Edit(order);

            result.As<ViewResult>().Model.Should().Be(order);
        }
    }
}