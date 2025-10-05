using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IRealtimeNotifier
    {
        Task EmitLowStockAlertToShop(long shopId, object payload);
    }
}


