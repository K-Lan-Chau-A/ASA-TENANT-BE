# Hướng dẫn Frontend kết nối SignalR cho Multi-Shop

## **1. Cài đặt SignalR Client**

```bash
npm install @microsoft/signalr
```

## **2. Tạo SignalR Service cho Multi-Shop**

```javascript
// signalrService.js
import * as signalR from '@microsoft/signalr';

class SignalRService {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.currentShopId = null;
        this.currentUserId = null;
    }

    // Kết nối đến SignalR Hub
    async connect(shopId = null, userId = null) {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('http://localhost:5000/notificationHub')
                .withAutomaticReconnect()
                .build();

            // Lắng nghe các sự kiện
            this.setupEventHandlers();

            // Bắt đầu kết nối
            await this.connection.start();
            this.isConnected = true;
            console.log('SignalR Connected!');
            
            // Tham gia các group dựa trên shopId và userId
            if (shopId) {
                await this.joinShopGroup(shopId);
                this.currentShopId = shopId;
            }
            
            if (userId) {
                await this.joinUserGroup(userId);
                this.currentUserId = userId;
            }
            
            return true;
        } catch (error) {
            console.error('SignalR Connection Error:', error);
            return false;
        }
    }

    // Thiết lập các event handlers
    setupEventHandlers() {
        // Lắng nghe thông báo chung
        this.connection.on('ReceiveNotification', (data) => {
            console.log('Received Notification:', data);
            this.handleNotification(data);
        });

        // Lắng nghe thông báo thanh toán
        this.connection.on('PaymentSuccess', (data) => {
            console.log('Payment Success:', data);
            this.handlePaymentSuccess(data);
        });

        // Lắng nghe thông báo đơn hàng
        this.connection.on('OrderUpdate', (data) => {
            console.log('Order Update:', data);
            this.handleOrderUpdate(data);
        });
    }

    // Tham gia nhóm Shop (cho shop staff)
    async joinShopGroup(shopId) {
        if (this.connection && this.isConnected) {
            try {
                await this.connection.invoke('JoinShopGroup', shopId);
                this.currentShopId = shopId;
                console.log(`Joined Shop Group: ${shopId}`);
            } catch (error) {
                console.error('Error joining shop group:', error);
            }
        }
    }

    // Tham gia nhóm User (cho customer)
    async joinUserGroup(userId) {
        if (this.connection && this.isConnected) {
            try {
                await this.connection.invoke('JoinUserGroup', userId);
                this.currentUserId = userId;
                console.log(`Joined User Group: ${userId}`);
            } catch (error) {
                console.error('Error joining user group:', error);
            }
        }
    }

    // Tham gia nhóm Admin
    async joinAdminGroup() {
        if (this.connection && this.isConnected) {
            try {
                await this.connection.invoke('JoinAdminGroup');
                console.log('Joined Admin Group');
            } catch (error) {
                console.error('Error joining admin group:', error);
            }
        }
    }

    // Xử lý thông báo chung
    handleNotification(data) {
        // Kiểm tra xem thông báo có thuộc về shop hiện tại không
        if (data.data && data.data.shopId && this.currentShopId) {
            if (data.data.shopId !== this.currentShopId) {
                console.log('Notification not for current shop, ignoring...');
                return;
            }
        }

        // Hiển thị thông báo cho user
        this.showNotification(data.message, data.type || 'info');
        
        // Cập nhật UI nếu cần
        if (data.data) {
            this.updateUI(data.data);
        }
    }

    // Xử lý thông báo thanh toán thành công
    handlePaymentSuccess(data) {
        // Kiểm tra shop ID
        if (data.data && data.data.shopId && this.currentShopId) {
            if (data.data.shopId !== this.currentShopId) {
                return;
            }
        }

        // Hiển thị thông báo thanh toán thành công
        this.showNotification(
            `Thanh toán thành công cho đơn hàng #${data.data.orderId}`, 
            'success'
        );
        
        // Cập nhật trạng thái đơn hàng
        this.updateOrderStatus(data.data.orderId, 'paid');
    }

    // Xử lý cập nhật đơn hàng
    handleOrderUpdate(data) {
        // Kiểm tra shop ID
        if (data.data && data.data.shopId && this.currentShopId) {
            if (data.data.shopId !== this.currentShopId) {
                return;
            }
        }

        // Cập nhật danh sách đơn hàng
        this.refreshOrderList();
    }

    // Gửi thông báo đến shop cụ thể
    async sendToShop(shopId, message, data) {
        if (this.connection && this.isConnected) {
            try {
                await this.connection.invoke('SendToShop', shopId, message, data);
            } catch (error) {
                console.error('Error sending to shop:', error);
            }
        }
    }

    // Gửi thông báo đến user cụ thể
    async sendToUser(userId, message, data) {
        if (this.connection && this.isConnected) {
            try {
                await this.connection.invoke('SendToUser', userId, message, data);
            } catch (error) {
                console.error('Error sending to user:', error);
            }
        }
    }

    // Hiển thị thông báo (tùy chỉnh theo UI framework)
    showNotification(message, type = 'info') {
        // Ví dụ với toast notification
        if (window.toast) {
            window.toast(message, type);
        } else {
            // Fallback với alert
            alert(`${type.toUpperCase()}: ${message}`);
        }
    }

    // Cập nhật UI
    updateUI(data) {
        // Cập nhật state/context nếu cần
        if (window.updateOrderState) {
            window.updateOrderState(data);
        }
    }

    // Cập nhật trạng thái đơn hàng
    updateOrderStatus(orderId, status) {
        // Cập nhật trạng thái đơn hàng trong UI
        if (window.updateOrderStatus) {
            window.updateOrderStatus(orderId, status);
        }
    }

    // Làm mới danh sách đơn hàng
    refreshOrderList() {
        // Gọi API để lấy danh sách đơn hàng mới
        if (window.refreshOrders) {
            window.refreshOrders();
        }
    }

    // Ngắt kết nối
    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
            this.currentShopId = null;
            this.currentUserId = null;
            console.log('SignalR Disconnected');
        }
    }

    // Lấy thông tin kết nối hiện tại
    getConnectionInfo() {
        return {
            isConnected: this.isConnected,
            currentShopId: this.currentShopId,
            currentUserId: this.currentUserId
        };
    }
}

// Export singleton instance
export const signalRService = new SignalRService();
```

## **3. Sử dụng trong React Component (Shop Staff)**

```jsx
// ShopDashboard.jsx
import React, { useEffect, useState } from 'react';
import { signalRService } from './signalRService';

const ShopDashboard = () => {
    const [orders, setOrders] = useState([]);
    const [isConnected, setIsConnected] = useState(false);
    const [shopId, setShopId] = useState(null);

    useEffect(() => {
        // Lấy shopId từ context hoặc props
        const currentShopId = getCurrentShopId(); // Hàm lấy shop ID
        setShopId(currentShopId);

        // Kết nối SignalR với shopId
        const connectSignalR = async () => {
            const connected = await signalRService.connect(currentShopId, null);
            setIsConnected(connected);
        };

        connectSignalR();

        // Cleanup khi component unmount
        return () => {
            signalRService.disconnect();
        };
    }, []);

    // Hàm xử lý cập nhật đơn hàng
    const handleOrderUpdate = (orderData) => {
        // Chỉ cập nhật nếu đơn hàng thuộc về shop hiện tại
        if (orderData.shopId === shopId) {
            setOrders(prevOrders => 
                prevOrders.map(order => 
                    order.id === orderData.orderId 
                        ? { ...order, status: orderData.status }
                        : order
                )
            );
        }
    };

    // Đăng ký global functions
    useEffect(() => {
        window.updateOrderState = handleOrderUpdate;
        window.updateOrderStatus = (orderId, status) => {
            setOrders(prevOrders => 
                prevOrders.map(order => 
                    order.id === orderId 
                        ? { ...order, status }
                        : order
                )
            );
        };
        window.refreshOrders = () => {
            fetchOrders();
        };
    }, [shopId]);

    const getCurrentShopId = () => {
        // Lấy shop ID từ store, context, hoặc localStorage
        return localStorage.getItem('currentShopId') || 1;
    };

    const fetchOrders = async () => {
        try {
            const response = await fetch(`/api/orders?shopId=${shopId}`);
            const data = await response.json();
            setOrders(data);
        } catch (error) {
            console.error('Error fetching orders:', error);
        }
    };

    return (
        <div>
            <div>SignalR Status: {isConnected ? 'Connected' : 'Disconnected'}</div>
            <div>Current Shop ID: {shopId}</div>
            <div>
                {orders.map(order => (
                    <div key={order.id}>
                        Order #{order.id} - Status: {order.status}
                    </div>
                ))}
            </div>
        </div>
    );
};

export default ShopDashboard;
```

## **4. Sử dụng trong React Component (Customer)**

```jsx
// CustomerOrder.jsx
import React, { useEffect, useState } from 'react';
import { signalRService } from './signalRService';

const CustomerOrder = () => {
    const [orders, setOrders] = useState([]);
    const [isConnected, setIsConnected] = useState(false);
    const [userId, setUserId] = useState(null);

    useEffect(() => {
        // Lấy userId từ context hoặc props
        const currentUserId = getCurrentUserId();
        setUserId(currentUserId);

        // Kết nối SignalR với userId
        const connectSignalR = async () => {
            const connected = await signalRService.connect(null, currentUserId);
            setIsConnected(connected);
        };

        connectSignalR();

        // Cleanup khi component unmount
        return () => {
            signalRService.disconnect();
        };
    }, []);

    // Hàm xử lý cập nhật đơn hàng
    const handleOrderUpdate = (orderData) => {
        // Chỉ cập nhật nếu đơn hàng thuộc về user hiện tại
        if (orderData.customerId === userId) {
            setOrders(prevOrders => 
                prevOrders.map(order => 
                    order.id === orderData.orderId 
                        ? { ...order, status: orderData.status }
                        : order
                )
            );
        }
    };

    // Đăng ký global functions
    useEffect(() => {
        window.updateOrderState = handleOrderUpdate;
        window.updateOrderStatus = (orderId, status) => {
            setOrders(prevOrders => 
                prevOrders.map(order => 
                    order.id === orderId 
                        ? { ...order, status }
                        : order
                )
            );
        };
        window.refreshOrders = () => {
            fetchOrders();
        };
    }, [userId]);

    const getCurrentUserId = () => {
        // Lấy user ID từ store, context, hoặc localStorage
        return localStorage.getItem('currentUserId') || 1;
    };

    const fetchOrders = async () => {
        try {
            const response = await fetch(`/api/orders?customerId=${userId}`);
            const data = await response.json();
            setOrders(data);
        } catch (error) {
            console.error('Error fetching orders:', error);
        }
    };

    return (
        <div>
            <div>SignalR Status: {isConnected ? 'Connected' : 'Disconnected'}</div>
            <div>Current User ID: {userId}</div>
            <div>
                {orders.map(order => (
                    <div key={order.id}>
                        Order #{order.id} - Status: {order.status}
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CustomerOrder;
```

## **5. Test kết nối Multi-Shop**

```javascript
// test-multi-shop-signalr.js
import { signalRService } from './signalRService';

// Test kết nối cho Shop 1
const testShop1 = async () => {
    const connected = await signalRService.connect(1, null);
    console.log('Shop 1 Connection:', connected);
    
    if (connected) {
        await signalRService.joinShopGroup(1);
        console.log('Shop 1 joined group');
    }
};

// Test kết nối cho Shop 2
const testShop2 = async () => {
    const connected = await signalRService.connect(2, null);
    console.log('Shop 2 Connection:', connected);
    
    if (connected) {
        await signalRService.joinShopGroup(2);
        console.log('Shop 2 joined group');
    }
};

// Test kết nối cho Customer
const testCustomer = async () => {
    const connected = await signalRService.connect(null, 1);
    console.log('Customer Connection:', connected);
    
    if (connected) {
        await signalRService.joinUserGroup(1);
        console.log('Customer joined group');
    }
};

// Chạy test
testShop1();
testShop2();
testCustomer();
```

## **6. Cấu hình CORS cho Multi-Shop**

Đảm bảo backend đã cấu hình CORS đúng trong `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});
```

## **7. Luồng hoạt động Multi-Shop**

### **Shop Staff:**
1. Kết nối SignalR với `shopId`
2. Tham gia group `Shop_{shopId}`
3. Chỉ nhận thông báo của shop đó

### **Customer:**
1. Kết nối SignalR với `userId`
2. Tham gia group `User_{userId}`
3. Chỉ nhận thông báo của đơn hàng của mình

### **Admin:**
1. Kết nối SignalR
2. Tham gia group `Admin`
3. Nhận thông báo của tất cả shops

## **8. Xử lý lỗi và Reconnect**

```javascript
// Thêm vào SignalRService
setupEventHandlers() {
    // ... existing code ...
    
    // Xử lý lỗi kết nối
    this.connection.onclose((error) => {
        console.log('SignalR Connection Closed:', error);
        this.isConnected = false;
        
        // Tự động reconnect sau 5 giây
        setTimeout(() => {
            this.connect(this.currentShopId, this.currentUserId);
        }, 5000);
    });
    
    // Xử lý lỗi
    this.connection.onreconnecting((error) => {
        console.log('SignalR Reconnecting:', error);
    });
    
    this.connection.onreconnected((connectionId) => {
        console.log('SignalR Reconnected:', connectionId);
        this.isConnected = true;
        
        // Tham gia lại các group
        if (this.currentShopId) {
            this.joinShopGroup(this.currentShopId);
        }
        if (this.currentUserId) {
            this.joinUserGroup(this.currentUserId);
        }
    });
}
```

Bây giờ Frontend sẽ chỉ nhận thông báo đúng với shop của họ! 🎉
