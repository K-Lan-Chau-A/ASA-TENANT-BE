using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IRealtimeNotifier
    {
        Task EmitLowStockAlertToShop(long shopId, object payload);
        Task EmitSubscriptionExpiryReminderToUser(long userId, object payload);
        Task EmitCustomerRankUpToShop(long shopId, object payload);
    }
}


