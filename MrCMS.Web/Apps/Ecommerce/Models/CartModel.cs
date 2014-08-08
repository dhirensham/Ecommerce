﻿using System;
using System.Collections.Generic;
using MrCMS.Entities.People;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Ecommerce.Entities.Cart;
using MrCMS.Web.Apps.Ecommerce.Entities.Discounts;
using MrCMS.Web.Apps.Ecommerce.Entities.Geographic;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;
using MrCMS.Web.Apps.Ecommerce.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MrCMS.Web.Apps.Ecommerce.Models.Shipping;
using MrCMS.Web.Apps.Ecommerce.Payment;
using MrCMS.Web.Apps.Ecommerce.Services.Shipping;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Apps.Ecommerce.Models
{
    public class CartModel
    {
        public CartModel()
        {
            Items = new List<CartItem>();
            AvailablePaymentMethods = new List<IPaymentMethod>();
            AvailableShippingMethods = new List<IShippingMethod>();
        }
        public Guid CartGuid { get; set; }
        public List<CartItem> Items { get; set; }

        public IEnumerable<CartItem> ShippableItems { get { return Items.Where(item => item.RequiresShipping); } }

        public bool Empty
        {
            get { return !Items.Any(); }
        }

        public decimal Subtotal
        {
            get { return Items.Sum(item => item.PricePreTax); }
        }

        public decimal TotalPreDiscount
        {
            get { return Items.Sum(item => item.Price); }
        }

        public virtual decimal ShippableTotalPreDiscount
        {
            get { return ShippableItems.Sum(item => item.Price); }
        }

        public virtual decimal ShippableCalculationTotal
        {
            get { return ShippableTotalPreDiscount - OrderTotalDiscount; }
        }

        public IDictionary<decimal, decimal> TaxRates
        {
            get
            {
                return Items.GroupBy(item => item.TaxRatePercentage)
                            .ToDictionary(items => items.Key,
                                          items => items.Sum(item => item.Price));
            }
        }

        public Discount Discount { get; set; }

        public decimal DiscountAmount
        {
            get
            {
                var discountAmount = OrderTotalDiscount;

                discountAmount += ItemDiscount;

                return discountAmount > TotalPreDiscount
                           ? TotalPreDiscount
                           : discountAmount;
            }
        }

        public decimal OrderTotalDiscount
        {
            get
            {
                return Discount == null
                           ? decimal.Zero
                           : Discount.GetDiscount(this);
            }
        }

        public decimal ItemDiscount
        {
            get
            {
                return Discount != null && Items.Any()
                           ? Items.Sum(item => item.DiscountAmount)
                           : decimal.Zero;
            }
        }

        public virtual decimal Total
        {
            get { return TotalPreShipping + ShippingTotal.GetValueOrDefault(); }
        }

        public virtual decimal TotalPreShipping
        {
            get { return TotalPreDiscount - OrderTotalDiscount; }
        }

        public decimal Tax
        {
            get
            {
                return ItemTax + ShippingTax.GetValueOrDefault();
            }
        }

        public decimal ItemTax
        {
            get { return Items.Sum(item => item.Tax); }
        }

        public User User { get; set; }

        public Guid UserGuid { get; set; }

        [Required]
        [DisplayName("Discount Code")]
        public string DiscountCode { get; set; }

        [Required]
        public string OrderEmail { get; set; }

        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }

        public IShippingMethod ShippingMethod { get; set; }

        [DisplayFormat(DataFormatString = "{0:£0.00}")]
        public decimal? ShippingTotal
        {
            get
            {
                if (ShippingMethod == null) return null;
                var shippingAmount = ShippingMethod.GetShippingTotal(this);
                return shippingAmount == ShippingAmount.NoneAvailable
                    ? (decimal?) null
                    : shippingAmount.Amount;
            }
        }
        public decimal? ShippingTax
        {
            get
            {
                if (ShippingMethod == null) return null;
                var shippingAmount = ShippingMethod.GetShippingTax(this);
                return shippingAmount == ShippingAmount.NoneAvailable
                    ? (decimal?) null
                    : shippingAmount.Amount;
            }
        }
        public decimal? ShippingPreTax { get { return ShippingTotal - ShippingTax; } }
        public decimal? ShippingTaxPercentage
        {
            get { return ShippingMethod == null ? (decimal?)null : ShippingMethod.TaxRatePercentage; }
        }

        public virtual decimal Weight
        {
            get { return Items.Any() ? Items.Sum(item => item.Weight) : decimal.Zero; }
        }

        public IEnumerable<IPaymentMethod> AvailablePaymentMethods { get; set; }
        public IList<IShippingMethod> AvailableShippingMethods { get; set; }

        public IPaymentMethod PaymentMethod { get; set; }
        public bool PaymentMethodSet { get { return PaymentMethod != null; } }

        public string PaymentMethodSystemName
        {
            get { return PaymentMethod == null ? string.Empty : PaymentMethod.SystemName; }
        }

        public string PaymentMethodAction
        {
            get { return PaymentMethod == null ? string.Empty : PaymentMethod.ActionName; }
        }
        public string PaymentMethodController
        {
            get { return PaymentMethod == null ? string.Empty : PaymentMethod.ControllerName; }
        }

        [DisplayName("Billing Address same as Shipping Address?")]
        public bool BillingAddressSameAsShippingAddress { get; set; }

        public bool NeedToSetBillingAddress { get { return !BillingAddressSameAsShippingAddress && BillingAddress == null; } }

        public bool HasDiscount
        {
            get { return Discount != null && OrderTotalDiscount != decimal.Zero; }
        }

        public decimal ItemQuantity
        {
            get { return Items.Sum(item => item.Quantity); }
        }

        public string PayPalExpressToken { get; set; }
        public string PayPalExpressPayerId { get; set; }

        public bool IsPayPalTransaction
        {
            get { return !string.IsNullOrWhiteSpace(PayPalExpressToken) && !string.IsNullOrWhiteSpace(PayPalExpressPayerId); }
        }

        public bool RequiresShipping
        {
            get { return ShippableItems.Any(); }
        }


        public bool CanCheckout
        {
            get { return !CannotCheckoutReasons.Any(); }
        }

        public bool AnyStandardPaymentMethodsAvailable { get; set; }
        public bool CanEnterPaymentFlow { get { return CanCheckout && AnyStandardPaymentMethodsAvailable; } }
        public bool PayPalExpressAvailable { get; set; }
        public bool CanUsePayPalExpress { get { return CanCheckout && PayPalExpressAvailable; } }

        public bool CanPlaceOrder
        {
            get { return !CannotPlaceOrderReasons.Any(); }
        }

        public IEnumerable<string> CannotCheckoutReasons
        {
            get
            {
                if (!Items.Any())
                    yield return "You have nothing in your cart";
                foreach (CartItem item in Items)
                {
                    if (!item.CanBuy(this))
                        yield return item.Error(this);
                }
            }
        }

        public IEnumerable<string> CannotPlaceOrderReasons
        {
            get
            {
                foreach (var cannotCheckoutReason in CannotCheckoutReasons)
                    yield return cannotCheckoutReason;
                if (RequiresShipping && ShippingAddress == null)
                    yield return "Shipping address is not set";
                if (RequiresShipping && ShippingMethod == null)
                    yield return "Shipping method is not set";
                if (BillingAddress == null)
                    yield return "Billing address is not set";
                if (MrCMSApplication.Get<ISession>().QueryOver<Order>().Where(order => order.Guid == CartGuid).Any())
                    yield return "Order has already been placed";
            }
        }
    }
}