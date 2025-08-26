using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationRepo _notificationRepo;
        public NotificationService(NotificationRepo notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }
    }
}
