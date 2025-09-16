namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order status enumeration
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 10,

        /// <summary>
        /// Processing
        /// </summary>
        Processing = 20,

        /// <summary>
        /// Complete
        /// </summary>
        Complete = 30,

        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled = 40,

        /// <summary>
        /// BackOrder
        /// </summary>
        BackOrder = 50,
             /// <summary>
             /// Offer
             /// </summary>
        Offer = 60
    }

    public enum PaymentTerms
    {
        Days7 = 1,
        Days14 = 2,
        COD = 3,
        OSA = 4

    }

}
