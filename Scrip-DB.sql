-- ===== RESET TENANT DATABASE - FULL SCRIPT =====

-- Drop existing tables if they exist (in reverse order of dependencies)
DROP TABLE IF EXISTS report_detail CASCADE;
DROP TABLE IF EXISTS report CASCADE;
DROP TABLE IF EXISTS promotion_product CASCADE;
DROP TABLE IF EXISTS promotion CASCADE;
DROP TABLE IF EXISTS transaction CASCADE;
DROP TABLE IF EXISTS nfc CASCADE;
DROP TABLE IF EXISTS inventory_transaction CASCADE;
DROP TABLE IF EXISTS order_detail CASCADE;
DROP TABLE IF EXISTS "order" CASCADE;
DROP TABLE IF EXISTS shift CASCADE;
DROP TABLE IF EXISTS voucher CASCADE;
DROP TABLE IF EXISTS product_unit CASCADE;
DROP TABLE IF EXISTS product CASCADE;
DROP TABLE IF EXISTS fcm CASCADE;
DROP TABLE IF EXISTS chat_message CASCADE;
DROP TABLE IF EXISTS log_activity CASCADE;
DROP TABLE IF EXISTS notification CASCADE;
DROP TABLE IF EXISTS zalopay CASCADE;
DROP TABLE IF EXISTS user_feature CASCADE;
DROP TABLE IF EXISTS "user" CASCADE;
DROP TABLE IF EXISTS customer CASCADE;
DROP TABLE IF EXISTS rank CASCADE;
DROP TABLE IF EXISTS unit CASCADE;
DROP TABLE IF EXISTS category CASCADE;
DROP TABLE IF EXISTS prompt CASCADE;
DROP TABLE IF EXISTS shop_subscription CASCADE;
DROP TABLE IF EXISTS shop CASCADE;

-- Set timezone
ALTER DATABASE "ASA-TENANT-DB" SET TIMEZONE = 'Asia/Ho_Chi_Minh';
SET TIMEZONE = 'Asia/Ho_Chi_Minh';

-- Extension cho email không phân biệt hoa/thường
CREATE EXTENSION IF NOT EXISTS citext;

-- ===== CREATE TABLES =====

-- 1. Shop
CREATE TABLE shop (
    shop_id BIGSERIAL PRIMARY KEY,
    shop_name VARCHAR(150),
    address TEXT,
    shop_token VARCHAR(150) NULL,
    bank_name VARCHAR(255) NULL,
    bank_code VARCHAR(50) NULL,
    bank_num VARCHAR(50) NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    status SMALLINT,
    qrcode_url TEXT NULL,
    sepay_api_key TEXT NULL,
    current_request INT DEFAULT 0,
    current_account INT DEFAULT 0
);

-- 2. Shop Subscription
CREATE TABLE shop_subscription (
    shop_subscription_id BIGSERIAL PRIMARY KEY,
    shop_id BIGINT REFERENCES shop(shop_id),
    platform_product_id BIGINT,
    start_date TIMESTAMPTZ NOT NULL,
    end_date TIMESTAMPTZ NOT NULL,
    status SMALLINT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 3. Prompt
CREATE TABLE prompt (
    prompt_id BIGSERIAL PRIMARY KEY,
    title VARCHAR(150),
    content TEXT,
    description TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 4. Category
CREATE TABLE category (
    category_id BIGSERIAL PRIMARY KEY,
    category_name VARCHAR(100),
    description TEXT,
    shop_id BIGINT REFERENCES shop(shop_id)
);

-- 5. Unit
CREATE TABLE unit (
    unit_id BIGSERIAL PRIMARY KEY,
    name VARCHAR(50),
    shop_id BIGINT REFERENCES shop(shop_id)
);

-- 6. Rank
CREATE TABLE rank (
    rank_id SERIAL PRIMARY KEY,
    rank_name VARCHAR(100) NOT NULL,
    benefit FLOAT,
    threshold FLOAT,
    shop_id BIGINT NOT NULL REFERENCES shop(shop_id) ON DELETE CASCADE
);

-- 7. Customer
CREATE TABLE customer (
    customer_id BIGSERIAL PRIMARY KEY,
    phone VARCHAR(20),
    full_name VARCHAR(150),
    birthday DATE,
    gender SMALLINT,
    rank_id INT REFERENCES rank(rank_id),
    status SMALLINT,
    shop_id BIGINT REFERENCES shop(shop_id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    spent NUMERIC(18,2),
    avatar TEXT,
    email CITEXT
);

-- 8. User
CREATE TABLE "user" (
    user_id BIGSERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE,
    password VARCHAR(255),
    full_name VARCHAR(150),
    phone_number VARCHAR(20),
    citizen_id_number VARCHAR(20),
    status SMALLINT,
    shop_id BIGINT REFERENCES shop(shop_id),
    role SMALLINT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    avatar TEXT
);

-- 9. User Feature
CREATE TABLE user_feature (
    user_feature_id BIGSERIAL PRIMARY KEY,
    user_id BIGINT REFERENCES "user"(user_id) ON DELETE CASCADE,
    feature_id BIGINT NOT NULL,
    feature_name VARCHAR(64),
    is_enabled BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 10. ZaloPay
CREATE TABLE zalopay (
    zalopay_id BIGSERIAL PRIMARY KEY,
    shop_id BIGINT REFERENCES shop(shop_id),
    app_id VARCHAR(64),
    key1 TEXT,
    key2 TEXT,
    callback_url TEXT,
    status SMALLINT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 11. Notification
CREATE TABLE notification (
    notification_id BIGSERIAL PRIMARY KEY,
    shop_id BIGINT REFERENCES shop(shop_id),
    user_id BIGINT REFERENCES "user"(user_id),
    title VARCHAR(150),
    content TEXT,
    type SMALLINT,
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 12. Log Activity
CREATE TABLE log_activity (
    log_activity_id BIGSERIAL PRIMARY KEY,
    user_id BIGINT REFERENCES "user"(user_id),
    content TEXT,
    type SMALLINT,
    shop_id BIGINT REFERENCES shop(shop_id),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 13. Chat Message
CREATE TABLE chat_message (
    chat_message_id BIGSERIAL PRIMARY KEY,
    shop_id BIGINT REFERENCES shop(shop_id),
    user_id BIGINT REFERENCES "user"(user_id),
    content TEXT,
    sender VARCHAR(10),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 14. FCM
CREATE TABLE fcm (
    fcm_id BIGSERIAL PRIMARY KEY,
    user_id BIGINT REFERENCES "user"(user_id),
    fcm_token VARCHAR(255) UNIQUE,
    UniqueId VARCHAR(100) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    LastLogin TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 15. Product
CREATE TABLE product (
    product_id BIGSERIAL PRIMARY KEY,
    category_id BIGINT REFERENCES category(category_id),
    product_name VARCHAR(150),
    quantity INT,
    is_low INT,
    cost NUMERIC(18,2),
    price NUMERIC(18,2),
    image_url TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    update_at TIMESTAMPTZ DEFAULT NOW(),
    barcode VARCHAR(64),
    discount NUMERIC(18,2),
    status SMALLINT,
    unit_id_fk BIGINT REFERENCES unit(unit_id),
    shop_id BIGINT REFERENCES shop(shop_id),
    is_low_stock_notified BOOLEAN DEFAULT FALSE
);

-- 16. Product Unit
CREATE TABLE product_unit (
    product_unit_id BIGSERIAL PRIMARY KEY,
    product_id BIGINT REFERENCES product(product_id),
    unit_id BIGINT REFERENCES unit(unit_id),
    conversion_factor NUMERIC(12,4),
    price NUMERIC(18,2),
    shop_id BIGINT REFERENCES shop(shop_id)
);

-- 17. Voucher
CREATE TABLE voucher (
    voucher_id BIGSERIAL PRIMARY KEY,
    value NUMERIC(18,2),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    type SMALLINT,
    expired TIMESTAMPTZ,
    shop_id BIGINT REFERENCES shop(shop_id),
    code VARCHAR(50) UNIQUE
);

-- 18. Shift
CREATE TABLE shift (
    shift_id BIGSERIAL PRIMARY KEY,
    user_id BIGINT REFERENCES "user"(user_id),
    start_date TIMESTAMPTZ,
    closed_date TIMESTAMPTZ,
    status SMALLINT,
    revenue NUMERIC(18,2),
    opening_cash NUMERIC(18,2),
    shop_id BIGINT REFERENCES shop(shop_id)
);

-- 19. Order
CREATE TABLE "order" (
    order_id BIGSERIAL PRIMARY KEY,
    datetime TIMESTAMPTZ,
    customer_id BIGINT REFERENCES customer(customer_id),
    total_price NUMERIC(18,2),
    total_discount NUMERIC(18,2),      -- Tổng số tiền giảm
    final_price NUMERIC(18,2),         -- Tổng tiền cuối cùng sau giảm
    payment_method VARCHAR(30),
    status SMALLINT,
    shift_id BIGINT REFERENCES shift(shift_id),
    shop_id BIGINT REFERENCES shop(shop_id),
    voucher_id BIGINT REFERENCES voucher(voucher_id),
    discount NUMERIC(18,2),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    note TEXT
);

-- 20. Order Detail
CREATE TABLE order_detail (
    order_detail_id BIGSERIAL PRIMARY KEY,
    quantity INT,
    product_unit_id BIGINT REFERENCES product_unit(product_unit_id),
    product_id BIGINT REFERENCES product(product_id),
    total_price NUMERIC(18,2),
    order_id BIGINT REFERENCES "order"(order_id)
);

-- 21. Inventory Transaction
CREATE TABLE inventory_transaction (
    inventory_transaction_id BIGSERIAL PRIMARY KEY,
    type SMALLINT,
    product_id BIGINT REFERENCES product(product_id),
    order_id BIGINT REFERENCES "order"(order_id),
    unit_id BIGINT REFERENCES unit(unit_id),
    quantity INT,
    image_url TEXT,
    price NUMERIC(18,2),
    shop_id BIGINT REFERENCES shop(shop_id),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 22. Promotion
CREATE TABLE promotion (
    promotion_id BIGSERIAL PRIMARY KEY,
    start_date DATE,
    end_date DATE,
    start_time TIME,
    end_time TIME,
    value NUMERIC(18,2),
    type SMALLINT,
    status SMALLINT,
    name VARCHAR(150),
    shop_id BIGINT REFERENCES shop(shop_id)
);

-- 23. Promotion Product
CREATE TABLE promotion_product (
    promotion_product_id BIGSERIAL PRIMARY KEY,
    promotion_id BIGINT REFERENCES promotion(promotion_id),
    product_id BIGINT REFERENCES product(product_id)
);

-- 24. Report
CREATE TABLE report (
    report_id BIGSERIAL PRIMARY KEY,
    type SMALLINT,
    start_date DATE,
    end_date DATE,
    create_at TIMESTAMPTZ DEFAULT NOW(),
    revenue NUMERIC(18,2),
    order_counter INT,
    shop_id BIGINT REFERENCES shop(shop_id),
    gross_profit NUMERIC(18,2),
    cost NUMERIC(18,2)
);

-- 25. Report Detail
CREATE TABLE report_detail (
    report_detail_id BIGSERIAL PRIMARY KEY,
    report_id BIGINT REFERENCES report(report_id),
    product_id BIGINT REFERENCES product(product_id),
    quantity INT
);

-- 26. NFC
CREATE TABLE nfc (
    nfc_id BIGSERIAL PRIMARY KEY,
    status SMALLINT,
    balance NUMERIC(18,2),
    customer_id BIGINT REFERENCES customer(customer_id),
    nfc_code VARCHAR(64) UNIQUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    last_used_date TIMESTAMPTZ
);

-- 27. Transaction
CREATE TABLE transaction (
    transaction_id BIGSERIAL PRIMARY KEY,
    order_id BIGINT REFERENCES "order"(order_id),
    user_id BIGINT REFERENCES "user"(user_id),
    shop_id BIGINT REFERENCES shop(shop_id),
    payment_status VARCHAR(16),
    app_trans_id VARCHAR(50),
    zp_trans_id VARCHAR(50),
    return_code INT,
    return_message VARCHAR(255),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 1. Tạo cửa hàng
INSERT INTO shop (shop_name, address, status, qrcode_url) VALUES
('Tạp Hóa Minh Hạnh', '123 Đường Nguyễn Văn Cừ, Quận 5, TP.HCM', 1, 'https://qr.example.com/shop001');

-- 2. Shop Subscription
INSERT INTO shop_subscription (shop_id, platform_product_id, start_date, end_date, status) VALUES
(1, 2, '2025-01-01 00:00:00'::TIMESTAMPTZ, '2025-12-31 23:59:59'::TIMESTAMPTZ, 0),
(1, 1, '2024-01-01 00:00:00'::TIMESTAMPTZ, '2024-12-31 23:59:59'::TIMESTAMPTZ, 1);

-- 3. Prompt Templates
INSERT INTO prompt (title, content, description) VALUES
('Tư vấn bán hàng', 'Bạn là nhân viên bán hàng tại cửa hàng tạp hóa. Hãy tư vấn sản phẩm phù hợp với nhu cầu khách hàng một cách thân thiện và chuyên nghiệp.', 'Template cho AI tư vấn bán hàng'),
('Báo cáo doanh số', 'Phân tích doanh thu và đưa ra báo cáo chi tiết về tình hình kinh doanh của cửa hàng theo ngày/tuần/tháng.', 'Template để AI tạo báo cáo doanh thu'),
('Quản lý kho hàng', 'Theo dõi tồn kho, cảnh báo hết hàng và đề xuất nhập hàng dựa trên dữ liệu bán hàng.', 'Template cho AI quản lý tồn kho'),
('Chăm sóc khách hàng', 'Hỗ trợ giải đáp thắc mắc, xử lý khiếu nại và tư vấn chương trình khuyến mãi cho khách hàng.', 'Template cho AI chăm sóc khách hàng');

-- 4. Units
INSERT INTO unit (name, shop_id) VALUES
('Chai', 1),
('Lon', 1),
('Gói', 1),
('Cái', 1),
('Hộp', 1);

-- 5. Categories
INSERT INTO category (category_name, description, shop_id) VALUES
('Nước giải khát', 'Các loại nước uống có gas và không gas', 1),
('Bánh khoai tây', 'Bánh snack giòn rụm các loại', 1),
('Kẹo', 'Các loại kẹo ngọt cho trẻ em và người lớn', 1);

-- 6. Ranks
INSERT INTO rank (rank_name, benefit, threshold, shop_id) VALUES
('Đồng',      0.01,  1000000,   1),
('Bạc',       0.02,  5000000,   1),
('Vàng',      0.03,  10000000,  1),
('Bạch Kim',  0.05,  20000000,  1),
('Kim Cương', 0.07,  NULL,      1);

-- 7. Customers
INSERT INTO customer (shop_id, full_name, phone, email, birthday, gender, status, spent, rank_id, created_at) VALUES
(1, 'Nguyễn Văn An', '0901234567', 'ngochau1310@gmail.com', '1990-05-15', 1, 1, 2500000, 2, '2025-10-01 08:30:00'::TIMESTAMPTZ),
(1, 'Trần Thị Bình', '0912345678', 'thib@example.com', '1985-10-20', 0, 1, 1800000, 2, '2025-10-02 09:15:00'::TIMESTAMPTZ),
(1, 'Lê Minh Cường', '0923456789', 'cuong@example.com', '1992-03-08', 1, 1, 950000, 1, '2025-10-03 10:00:00'::TIMESTAMPTZ),
(1, 'Phạm Thị Dung', '0934567890', 'dung@example.com', '1988-12-25', 0, 1, 450000, 1, '2025-10-05 11:20:00'::TIMESTAMPTZ);

-- 8. Users
INSERT INTO "user" (username, password, status, shop_id, role, avatar) VALUES
('admin01', '$2a$11$cXr.pHhel84XmsT6bgwzlOcHxrLF.MgjWHbyNL.9ED1Ls/lKDiVhS', 1, 1, 1, 'https://avatar.example.com/admin.jpg'),
('nhanvien01', '$2a$11$cXr.pHhel84XmsT6bgwzlOcHxrLF.MgjWHbyNL.9ED1Ls/lKDiVhS', 1, 1, 2, 'https://avatar.example.com/nv1.jpg'),
('nhanvien02', '$2a$11$cXr.pHhel84XmsT6bgwzlOcHxrLF.MgjWHbyNL.9ED1Ls/lKDiVhS', 1, 1, 2, 'https://avatar.example.com/nv2.jpg');

-- 9. User Features
INSERT INTO user_feature (user_id, feature_id, feature_name, is_enabled) VALUES
-- Admin có tất cả quyền
(1, 1, 'Xuất báo cáo', TRUE),
(1, 2, 'Tư vấn AI', TRUE),
(1, 3, 'Quản lí người dùng', TRUE),
(1, 4, 'Bán hàng', TRUE),
(1, 5, 'Quản lí sản phẩm', TRUE),
(1, 6, 'Quản lí voucher', TRUE),
(1, 7, 'Quản lí promotion', TRUE),
(1, 8, 'Quản lí category', TRUE),
(1, 9, 'Quản lí khách hàng', TRUE),
-- Nhân viên 01 có quyền bán hàng và tư vấn
(2, 2, 'Tư vấn AI', TRUE),
(2, 4, 'Bán hàng', TRUE),
(2, 9, 'Quản lí khách hàng', TRUE),
-- Nhân viên 02 có quyền bán hàng
(3, 2, 'Tư vấn AI', TRUE),
(3, 4, 'Bán hàng', TRUE);

-- 10. Products (Số lượng ban đầu - sẽ được điều chỉnh sau khi nhập kho và bán hàng)
INSERT INTO product (category_id, product_name, quantity, is_low, cost, price, barcode, image_url, discount, status, unit_id_fk, shop_id) VALUES
-- Nước giải khát
(1, 'Pepsi Cola 330ml', 100, 10, 8000, 12000, '8934673001234','https://www.lottemart.vn/media/catalog/product/cache/0x0/8/9/8934588013027-1-1.jpg.webp', 0, 1, 1, 1),
(1, 'Coca Cola 330ml', 133, 10, 8500, 12000, '8934673001235','https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQowvqmp0hU5U9tiRxyAmAsW2ukDVgrpkBQkw&s', 0, 1, 1, 1),
(1, '7Up 330ml', 75, 10, 7500, 11000, '8934673001236','https://cdn.tgdd.vn/Products/Images/2443/319488/bhx/nuoc-ngot-7-up-vi-chanh-chai-330ml-202312301036328194.jpg', 0, 1, 1, 1),
(1, 'Pepsi Cola Lon 330ml', 190, 10, 9000, 13000, '8934673001237','https://cdn.tgdd.vn/Products/Images/2443/88121/bhx/thung-24-lon-nuoc-ngot-pepsi-cola-320ml-202405140910328596.jpg', 0, 1, 2, 1),
(1, 'Coca Cola Lon 330ml', 170, 10, 9500, 13000, '8934673001238','https://cdnv2.tgdd.vn/webmwg/comment/ef/4a/ef4a44fb0c806a130d74efca2ee6ce87.jpg', 0, 1, 2, 1),
(1, '7Up Lon 330ml', 98, 10, 8500, 12000, '8934673001239','https://cdn.tgdd.vn/Products/Images/2443/76446/bhx/nuoc-ngot-7-up-lon-330ml-202312252102017018.jpg', 0, 1, 2, 1),
-- Bánh khoai tây
(2, 'Poca Vì Tự Nhiên 54g', 188, 10, 6000, 9000, '8934673002234','https://cdnv2.tgdd.vn/bhx-static/bhx/Products/Images/3364/193606/bhx/snack-poca-khoai-tay-tu-nhien-goi-52g_202510150941133229.jpg', 0, 1, 3, 1),
(2, 'Poca Vì Cay 54g', 141, 10, 6000, 9000, '8934673002235','https://product.hstatic.net/200000495609/product/snack-poca-vi-muc-cay-dac-biet-banh-keo-an-vat-imnuts-n1_b75b1cfcb05641e982f8cc55912cee8b_master.jpg', 0, 1, 3, 1),
(2, 'Lays Vì Tự Nhiên 56g', 173, 10, 7000, 10000, '8934673002236','https://product.hstatic.net/200000352097/product/a138b002816dfdb5c290cdb4631d16ec_4385524afb404d6a847d3d42c0f52b1e_1024x1024.png', 0, 1, 3, 1),
(2, 'Lays Vì Cà Chua 56g', 118, 10, 7000, 10000, '8934673002237','https://img08.weeecdn.net/product/image/502/574/301032507DC379D4.png!c750x0_q80_t1.auto', 0, 1, 3, 1),
(2, 'Oishi Vì Tôm Chua Ngọt 60g', 96, 10, 8000, 12000, '8934673002238','https://product.hstatic.net/200000495609/product/snack-tom-cay-oishi-du-vi-goi-lon-68g-banh-keo-an-vat-imnuts-03_ac70449375bc4b3998c72422e9c3d7f5.jpg', 0, 1, 3, 1),
(2, 'Oishi Snack Bí Đỏ 60g', 78, 10, 8000, 12000, '8934673002239','https://www.lottemart.vn/media/catalog/product/cache/0x0/8/9/8934803012880.jpg.webp', 0, 1, 3, 1),
-- Kẹo
(3, 'Kẹo Dẻo Haribo 100g', 147, 10, 15000, 22000, '8934673003234','https://bizweb.dktcdn.net/thumb/1024x1024/100/436/921/products/8691216090439-1-209bf20bb99c4b3298dd9ed3d64704e5-bd04cb8ed15345718d45810f9e771c9c-master.jpg?v=1631854528787', 0, 1, 3, 1),
(3, 'Kẹo Dẻo Ricola 80g', 118, 10, 12000, 18000, '8934673003235','https://cdn.upharma.vn/unsafe/3840x0/filters:quality(90)/san-pham/20714.png', 0, 1, 3, 1),
(3, 'Kẹo Mút Chupa Chups', 189, 10, 2000, 3000, '8934673003236','https://bizweb.dktcdn.net/thumb/grande/100/514/431/products/3b43ab3e8f836e2bb1e286a63d2f355b.jpg?v=1716027827830', 0, 1, 4, 1),
(3, 'Kẹo Mút Alpenliebe', 179, 10, 1500, 2500, '8934673003237','https://minhcaumart.vn//media/com_eshop/products/resized/8935001708452-500x500.webp', 0, 1, 4, 1),
(3, 'Kẹo Ngậm Halls Chanh', 99, 10, 8000, 12000, '8934673003238','https://www.guardian.com.vn/media/catalog/product/cache/30b2b44eba57cd45fd3ef9287600968e/9/e/9e0e24742624fdcaee948073623b6a043622271ae835d2516144fd86525a1710.jpeg', 0, 1, 5, 1),
(3, 'Kẹo Ngậm Ricola Bạc Hà', 79, 10, 10000, 15000, '8934673003239','https://www.lottemart.vn/media/catalog/product/cache/0x0/7/6/7610700948460-1.jpg.webp', 0, 1, 5, 1);

-- 11. Product Units
INSERT INTO product_unit (product_id, unit_id, conversion_factor, price, shop_id) VALUES
(1, 1, 1.0000, 12000, 1), (2, 1, 1.0000, 12000, 1), (3, 1, 1.0000, 11000, 1),
(4, 2, 1.0000, 13000, 1), (5, 2, 1.0000, 13000, 1), (6, 2, 1.0000, 12000, 1),
(7, 3, 1.0000, 9000, 1),  (8, 3, 1.0000, 9000, 1),  (9, 3, 1.0000, 10000, 1),
(10, 3, 1.0000, 10000, 1), (11, 3, 1.0000, 12000, 1), (12, 3, 1.0000, 12000, 1),
(13, 3, 1.0000, 22000, 1), (14, 3, 1.0000, 18000, 1), (15, 4, 1.0000, 3000, 1),
(16, 4, 1.0000, 2500, 1),  (17, 5, 1.0000, 12000, 1), (18, 5, 1.0000, 15000, 1);

-- 12. Shifts
INSERT INTO shift (user_id, start_date, closed_date, status, revenue, opening_cash, shop_id) VALUES
(1, '2025-10-15 08:00:00'::TIMESTAMPTZ, '2025-10-15 20:00:00'::TIMESTAMPTZ, 2, 1250000, 500000, 1),
(2, '2025-10-16 08:00:00'::TIMESTAMPTZ, '2025-10-16 20:00:00'::TIMESTAMPTZ, 2, 980000, 500000, 1),
(3, '2025-10-17 08:00:00'::TIMESTAMPTZ, '2025-10-17 20:00:00'::TIMESTAMPTZ, 2, 1150000, 500000, 1),
(1, '2025-10-18 08:00:00'::TIMESTAMPTZ, '2025-10-18 20:00:00'::TIMESTAMPTZ, 2, 890000, 500000, 1),
(2, '2025-10-19 08:00:00'::TIMESTAMPTZ, NULL, 1, 0, 500000, 1);

-- 13. Vouchers
INSERT INTO voucher (value, type, expired, shop_id, code) VALUES
(10000, 1, '2025-12-31 23:59:59'::TIMESTAMPTZ, 1, 'GIAM10K'),
(5000, 1, '2025-12-31 23:59:59'::TIMESTAMPTZ, 1, 'GIAM5K'),
(15, 2, '2025-12-31 23:59:59'::TIMESTAMPTZ, 1, 'GIAM15P');

-- 14. Orders
INSERT INTO "order" (datetime, customer_id, total_price, payment_method, status, shift_id, shop_id, voucher_id, discount, note) VALUES
('2025-10-15 10:30:00'::TIMESTAMPTZ, 1, 75000, 1, 1, 1, 1, NULL, 0, 'Khách hàng VIP'),
('2025-10-15 14:15:00'::TIMESTAMPTZ, 2, 45000, 2, 1, 1, 1, 1, 10000, 'Sử dụng voucher giảm 10k'),
('2025-10-15 16:45:00'::TIMESTAMPTZ, 3, 28000, 1, 1, 1, 1, NULL, 0, NULL),
('2025-10-16 09:20:00'::TIMESTAMPTZ, 4, 65000, 3, 1, 2, 1, NULL, 0, 'Thanh toán NFC'),
('2025-10-16 11:30:00'::TIMESTAMPTZ, 1, 52000, 1, 1, 2, 1, NULL, 0, NULL),
('2025-10-17 08:45:00'::TIMESTAMPTZ, 2, 89000, 2, 1, 3, 1, NULL, 0, 'Chuyển khoản ngân hàng'),
('2025-10-17 13:20:00'::TIMESTAMPTZ, 3, 45000, 1, 1, 3, 1, 2, 5000, 'Voucher 5k'),
('2025-10-18 10:15:00'::TIMESTAMPTZ, 4, 72000, 3, 1, 4, 1, NULL, 0, 'NFC Card'),
('2025-10-19 09:00:00'::TIMESTAMPTZ, 1, 96000, 1, 1, 5, 1, NULL, 0, 'Mua sỉ nước ngọt'),
('2025-10-19 09:45:00'::TIMESTAMPTZ, 2, 64000, 2, 1, 5, 1, NULL, 0, 'Chuyển khoản MB Bank'),
('2025-10-19 10:30:00'::TIMESTAMPTZ, 3, 55000, 1, 1, 5, 1, 2, 5000, 'Dùng voucher'),
('2025-10-19 11:15:00'::TIMESTAMPTZ, 4, 83000, 3, 1, 5, 1, NULL, 0, 'Thanh toán NFC'),
('2025-10-19 12:00:00'::TIMESTAMPTZ, 1, 108000, 1, 1, 5, 1, NULL, 0, 'Khách quen'),
('2025-10-19 13:30:00'::TIMESTAMPTZ, 2, 41000, 4, 1, 5, 1, NULL, 0, 'Thanh toán ATM'),
('2025-10-19 14:15:00'::TIMESTAMPTZ, 3, 76000, 1, 1, 5, 1, 3, 13500, 'Áp dụng voucher 15%'),
('2025-10-19 15:00:00'::TIMESTAMPTZ, 4, 92000, 2, 1, 5, 1, NULL, 0, 'Banking'),
('2025-10-19 15:45:00'::TIMESTAMPTZ, 1, 58000, 1, 0, 5, 1, NULL, 0, 'Đang chờ thanh toán'),
('2025-10-19 16:20:00'::TIMESTAMPTZ, 2, 0, 1, 2, 5, 1, NULL, 0, 'Đơn hàng bị hủy');

-- 15. Order Details
INSERT INTO order_detail (quantity, product_unit_id, product_id, total_price, order_id) VALUES
-- Order 1
(3, 1, 1, 36000, 1), (2, 7, 7, 18000, 1), (1, 15, 15, 3000, 1), (1, 17, 17, 12000, 1), (1, 13, 13, 22000, 1),
-- Order 2
(2, 2, 2, 24000, 2), (1, 9, 9, 10000, 2), (2, 15, 15, 6000, 2), (1, 11, 11, 12000, 2), (1, 14, 14, 18000, 2),
-- Order 3
(2, 4, 4, 26000, 3), (1, 16, 16, 2500, 3),
-- Order 4
(3, 5, 5, 39000, 4), (2, 8, 8, 18000, 4), (1, 18, 18, 15000, 4),
-- Order 5
(2, 1, 1, 24000, 5), (2, 9, 9, 20000, 5), (1, 15, 15, 3000, 5),
-- Order 6
(4, 2, 2, 48000, 6), (3, 7, 7, 27000, 6), (2, 13, 13, 44000, 6),
-- Order 7
(2, 3, 3, 22000, 7), (1, 10, 10, 10000, 7), (2, 16, 16, 5000, 7), (1, 17, 17, 12000, 7),
-- Order 8
(3, 4, 4, 39000, 8), (2, 11, 11, 24000, 8), (1, 14, 14, 18000, 8),
-- Order 9
(4, 1, 1, 48000, 9), (3, 2, 2, 36000, 9), (2, 15, 15, 6000, 9), (1, 16, 16, 2500, 9),
-- Order 10
(2, 4, 4, 26000, 10), (3, 7, 7, 27000, 10), (2, 15, 15, 6000, 10),
-- Order 11
(3, 3, 3, 33000, 11), (2, 9, 9, 20000, 11), (1, 17, 17, 12000, 11),
-- Order 12
(2, 5, 5, 26000, 12), (4, 8, 8, 36000, 12), (2, 11, 11, 24000, 12), (1, 13, 13, 22000, 12),
-- Order 13
(5, 1, 1, 60000, 13), (3, 9, 9, 30000, 13), (2, 13, 13, 44000, 13),
-- Order 14
(2, 2, 2, 24000, 14), (1, 10, 10, 10000, 14), (1, 16, 16, 2500, 14), (1, 18, 18, 15000, 14),
-- Order 15
(3, 4, 4, 39000, 15), (2, 7, 7, 18000, 15), (2, 12, 12, 24000, 15), (3, 15, 15, 9000, 15),
-- Order 16
(4, 5, 5, 52000, 16), (3, 8, 8, 27000, 16), (2, 14, 14, 36000, 16),
-- Order 17 (Đang chờ thanh toán - không trừ kho)
(3, 3, 3, 33000, 17), (2, 6, 6, 24000, 17), (1, 17, 17, 12000, 17),
-- Order 18 (Đơn bị hủy)
(2, 1, 1, 24000, 18), (2, 7, 7, 18000, 18);

-- 16. Inventory Transactions
-- BƯỚC 1: NHẬP KHO BAN ĐẦU (type = 2)
INSERT INTO inventory_transaction (type, product_id, order_id, unit_id, quantity, price, shop_id, created_at) VALUES
-- Nhập kho nước giải khát
(2, 1, NULL, 1, 120, 8000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 2, NULL, 1, 150, 8500, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 3, NULL, 1, 80, 7500, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 4, NULL, 2, 200, 9000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 5, NULL, 2, 180, 9500, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 6, NULL, 2, 100, 8500, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
-- Nhập kho bánh khoai tây
(2, 7, NULL, 3, 200, 6000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 8, NULL, 3, 150, 6000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 9, NULL, 3, 180, 7000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 10, NULL, 3, 120, 7000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 11, NULL, 3, 100, 8000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 12, NULL, 3, 80, 8000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
-- Nhập kho kẹo
(2, 13, NULL, 3, 150, 15000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 14, NULL, 3, 120, 12000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 15, NULL, 4, 200, 2000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 16, NULL, 4, 180, 1500, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 17, NULL, 5, 100, 8000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ),
(2, 18, NULL, 5, 80, 10000, 1, '2025-10-01 08:00:00'::TIMESTAMPTZ);

-- BƯỚC 2: BÁN HÀNG (type = 1) - Các đơn đã thanh toán (status = 1)
-- Order 1 (đã thanh toán)
INSERT INTO inventory_transaction (type, product_id, order_id, unit_id, quantity, price, shop_id, created_at) VALUES
(1, 1, 1, 1, 3, 12000, 1, '2025-10-15 10:30:00'::TIMESTAMPTZ),
(1, 7, 1, 3, 2, 9000, 1, '2025-10-15 10:30:00'::TIMESTAMPTZ),
(1, 15, 1, 4, 1, 3000, 1, '2025-10-15 10:30:00'::TIMESTAMPTZ),
(1, 17, 1, 5, 1, 12000, 1, '2025-10-15 10:30:00'::TIMESTAMPTZ),
(1, 13, 1, 3, 1, 22000, 1, '2025-10-15 10:30:00'::TIMESTAMPTZ),
-- Order 2
(1, 2, 2, 1, 2, 12000, 1, '2025-10-15 14:15:00'::TIMESTAMPTZ),
(1, 9, 2, 3, 1, 10000, 1, '2025-10-15 14:15:00'::TIMESTAMPTZ),
(1, 15, 2, 4, 2, 3000, 1, '2025-10-15 14:15:00'::TIMESTAMPTZ),
(1, 11, 2, 3, 1, 12000, 1, '2025-10-15 14:15:00'::TIMESTAMPTZ),
(1, 14, 2, 3, 1, 18000, 1, '2025-10-15 14:15:00'::TIMESTAMPTZ),
-- Order 3
(1, 4, 3, 2, 2, 13000, 1, '2025-10-15 16:45:00'::TIMESTAMPTZ),
(1, 16, 3, 4, 1, 2500, 1, '2025-10-15 16:45:00'::TIMESTAMPTZ),
-- Order 4
(1, 5, 4, 2, 3, 13000, 1, '2025-10-16 09:20:00'::TIMESTAMPTZ),
(1, 8, 4, 3, 2, 9000, 1, '2025-10-16 09:20:00'::TIMESTAMPTZ),
(1, 18, 4, 5, 1, 15000, 1, '2025-10-16 09:20:00'::TIMESTAMPTZ),
-- Order 5
(1, 1, 5, 1, 2, 12000, 1, '2025-10-16 11:30:00'::TIMESTAMPTZ),
(1, 9, 5, 3, 2, 10000, 1, '2025-10-16 11:30:00'::TIMESTAMPTZ),
(1, 15, 5, 4, 1, 3000, 1, '2025-10-16 11:30:00'::TIMESTAMPTZ),
-- Order 6
(1, 2, 6, 1, 4, 12000, 1, '2025-10-17 08:45:00'::TIMESTAMPTZ),
(1, 7, 6, 3, 3, 9000, 1, '2025-10-17 08:45:00'::TIMESTAMPTZ),
(1, 13, 6, 3, 2, 22000, 1, '2025-10-17 08:45:00'::TIMESTAMPTZ),
-- Order 7
(1, 3, 7, 1, 2, 11000, 1, '2025-10-17 13:20:00'::TIMESTAMPTZ),
(1, 10, 7, 3, 1, 10000, 1, '2025-10-17 13:20:00'::TIMESTAMPTZ),
(1, 16, 7, 4, 2, 2500, 1, '2025-10-17 13:20:00'::TIMESTAMPTZ),
(1, 17, 7, 5, 1, 12000, 1, '2025-10-17 13:20:00'::TIMESTAMPTZ),
-- Order 8
(1, 4, 8, 2, 3, 13000, 1, '2025-10-18 10:15:00'::TIMESTAMPTZ),
(1, 11, 8, 3, 2, 12000, 1, '2025-10-18 10:15:00'::TIMESTAMPTZ),
(1, 14, 8, 3, 1, 18000, 1, '2025-10-18 10:15:00'::TIMESTAMPTZ),
-- Order 9
(1, 1, 9, 1, 4, 12000, 1, '2025-10-19 09:00:00'::TIMESTAMPTZ),
(1, 2, 9, 1, 3, 12000, 1, '2025-10-19 09:00:00'::TIMESTAMPTZ),
(1, 15, 9, 4, 2, 3000, 1, '2025-10-19 09:00:00'::TIMESTAMPTZ),
(1, 16, 9, 4, 1, 2500, 1, '2025-10-19 09:00:00'::TIMESTAMPTZ),
-- Order 10
(1, 4, 10, 2, 2, 13000, 1, '2025-10-19 09:45:00'::TIMESTAMPTZ),
(1, 7, 10, 3, 3, 9000, 1, '2025-10-19 09:45:00'::TIMESTAMPTZ),
(1, 15, 10, 4, 2, 3000, 1, '2025-10-19 09:45:00'::TIMESTAMPTZ),
-- Order 11
(1, 3, 11, 1, 3, 11000, 1, '2025-10-19 10:30:00'::TIMESTAMPTZ),
(1, 9, 11, 3, 2, 10000, 1, '2025-10-19 10:30:00'::TIMESTAMPTZ),
(1, 17, 11, 5, 1, 12000, 1, '2025-10-19 10:30:00'::TIMESTAMPTZ),
-- Order 12
(1, 5, 12, 2, 2, 13000, 1, '2025-10-19 11:15:00'::TIMESTAMPTZ),
(1, 8, 12, 3, 4, 9000, 1, '2025-10-19 11:15:00'::TIMESTAMPTZ),
(1, 11, 12, 3, 2, 12000, 1, '2025-10-19 11:15:00'::TIMESTAMPTZ),
(1, 13, 12, 3, 1, 22000, 1, '2025-10-19 11:15:00'::TIMESTAMPTZ),
-- Order 13
(1, 1, 13, 1, 5, 12000, 1, '2025-10-19 12:00:00'::TIMESTAMPTZ),
(1, 9, 13, 3, 3, 10000, 1, '2025-10-19 12:00:00'::TIMESTAMPTZ),
(1, 13, 13, 3, 2, 22000, 1, '2025-10-19 12:00:00'::TIMESTAMPTZ),
-- Order 14
(1, 2, 14, 1, 2, 12000, 1, '2025-10-19 13:30:00'::TIMESTAMPTZ),
(1, 10, 14, 3, 1, 10000, 1, '2025-10-19 13:30:00'::TIMESTAMPTZ),
(1, 16, 14, 4, 1, 2500, 1, '2025-10-19 13:30:00'::TIMESTAMPTZ),
(1, 18, 14, 5, 1, 15000, 1, '2025-10-19 13:30:00'::TIMESTAMPTZ),
-- Order 15
(1, 4, 15, 2, 3, 13000, 1, '2025-10-19 14:15:00'::TIMESTAMPTZ),
(1, 7, 15, 3, 2, 9000, 1, '2025-10-19 14:15:00'::TIMESTAMPTZ),
(1, 12, 15, 3, 2, 12000, 1, '2025-10-19 14:15:00'::TIMESTAMPTZ),
(1, 15, 15, 4, 3, 3000, 1, '2025-10-19 14:15:00'::TIMESTAMPTZ),
-- Order 16
(1, 5, 16, 2, 4, 13000, 1, '2025-10-19 15:00:00'::TIMESTAMPTZ),
(1, 8, 16, 3, 3, 9000, 1, '2025-10-19 15:00:00'::TIMESTAMPTZ),
(1, 14, 16, 3, 2, 18000, 1, '2025-10-19 15:00:00'::TIMESTAMPTZ);
-- Order 17 (status = 0, đang chờ thanh toán - KHÔNG trừ kho)
-- Order 18 (status = 2, đã hủy - KHÔNG trừ kho)

-- 17. NFC Cards
INSERT INTO nfc (status, balance, customer_id, nfc_code, last_used_date) VALUES
(1, 500000, 1, 'NFC001VAN', '2025-10-19 09:00:00'::TIMESTAMPTZ),
(1, 200000, 2, 'NFC002BINH', '2025-10-16 09:20:00'::TIMESTAMPTZ);

-- 18. Notifications
INSERT INTO notification (shop_id, user_id, title, content, type, is_read) VALUES
(1, 1, 'Sản phẩm sắp hết hàng', 'Oishi Snack Bí Đỏ chỉ còn 78 sản phẩm', 1, FALSE),
(1, 2, 'Ca làm việc mới', 'Bạn đã được phân công ca làm việc hôm nay', 2, TRUE),
(1, 1, 'Doanh thu ngày hôm qua', 'Tổng doanh thu: 1,250,000 VNĐ', 3, FALSE),
(1, 1, 'Báo cáo tuần', 'Doanh thu tuần này tăng 15% so với tuần trước', 3, FALSE);

-- 19. Log Activities (Cập nhật theo 19 type)
INSERT INTO log_activity (user_id, content, type, shop_id, created_at) VALUES
(1, 'Admin đăng nhập vào hệ thống', 1, 1, '2025-10-15 07:55:00'::TIMESTAMPTZ),
(1, 'Admin mở ca làm việc', 15, 1, '2025-10-15 08:00:00'::TIMESTAMPTZ),
(1, 'Tạo đơn hàng #1', 7, 1, '2025-10-15 10:30:00'::TIMESTAMPTZ),
(1, 'Áp dụng giảm giá cho đơn hàng #2', 8, 1, '2025-10-15 14:15:00'::TIMESTAMPTZ),
(1, 'Tạo đơn hàng #2', 7, 1, '2025-10-15 14:15:00'::TIMESTAMPTZ),
(1, 'Tạo đơn hàng #3', 7, 1, '2025-10-15 16:45:00'::TIMESTAMPTZ),
(1, 'Admin đóng ca làm việc', 16, 1, '2025-10-15 20:00:00'::TIMESTAMPTZ),
(1, 'Tạo báo cáo ngày 15/10', 17, 1, '2025-10-15 20:05:00'::TIMESTAMPTZ),
(1, 'Admin đăng xuất', 2, 1, '2025-10-15 20:10:00'::TIMESTAMPTZ),
(2, 'Nhân viên 01 đăng nhập', 1, 1, '2025-10-16 07:55:00'::TIMESTAMPTZ),
(2, 'Nhân viên 01 mở ca', 15, 1, '2025-10-16 08:00:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #4', 7, 1, '2025-10-16 09:20:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #5', 7, 1, '2025-10-16 11:30:00'::TIMESTAMPTZ),
(2, 'Nhân viên 01 đóng ca', 16, 1, '2025-10-16 20:00:00'::TIMESTAMPTZ),
(2, 'Nhân viên 01 đăng xuất', 2, 1, '2025-10-16 20:05:00'::TIMESTAMPTZ),
(3, 'Nhân viên 02 đăng nhập', 1, 1, '2025-10-17 07:55:00'::TIMESTAMPTZ),
(3, 'Nhân viên 02 mở ca', 15, 1, '2025-10-17 08:00:00'::TIMESTAMPTZ),
(3, 'Tạo đơn hàng #6', 7, 1, '2025-10-17 08:45:00'::TIMESTAMPTZ),
(3, 'Áp dụng voucher cho đơn hàng #7', 8, 1, '2025-10-17 13:20:00'::TIMESTAMPTZ),
(3, 'Tạo đơn hàng #7', 7, 1, '2025-10-17 13:20:00'::TIMESTAMPTZ),
(3, 'Nhân viên 02 đóng ca', 16, 1, '2025-10-17 20:00:00'::TIMESTAMPTZ),
(3, 'Nhân viên 02 đăng xuất', 2, 1, '2025-10-17 20:05:00'::TIMESTAMPTZ),
(1, 'Admin đăng nhập', 1, 1, '2025-10-18 07:55:00'::TIMESTAMPTZ),
(1, 'Admin mở ca', 15, 1, '2025-10-18 08:00:00'::TIMESTAMPTZ),
(1, 'Tạo đơn hàng #8', 7, 1, '2025-10-18 10:15:00'::TIMESTAMPTZ),
(1, 'Admin đóng ca', 16, 1, '2025-10-18 20:00:00'::TIMESTAMPTZ),
(1, 'Tạo báo cáo tuần', 17, 1, '2025-10-18 20:05:00'::TIMESTAMPTZ),
(1, 'Xuất báo cáo tuần', 18, 1, '2025-10-18 20:10:00'::TIMESTAMPTZ),
(1, 'Admin đăng xuất', 2, 1, '2025-10-18 20:15:00'::TIMESTAMPTZ),
(2, 'Nhân viên 01 đăng nhập', 1, 1, '2025-10-19 07:55:00'::TIMESTAMPTZ),
(2, 'Nhân viên 01 mở ca', 15, 1, '2025-10-19 08:00:00'::TIMESTAMPTZ),
(1, 'Admin thêm sản phẩm mới vào kho', 9, 1, '2025-10-19 08:30:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #9', 7, 1, '2025-10-19 09:00:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #10', 7, 1, '2025-10-19 09:45:00'::TIMESTAMPTZ),
(2, 'Áp dụng voucher cho đơn hàng #11', 8, 1, '2025-10-19 10:30:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #11', 7, 1, '2025-10-19 10:30:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #12', 7, 1, '2025-10-19 11:15:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #13', 7, 1, '2025-10-19 12:00:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #14', 7, 1, '2025-10-19 13:30:00'::TIMESTAMPTZ),
(2, 'Áp dụng voucher cho đơn hàng #15', 8, 1, '2025-10-19 14:15:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #15', 7, 1, '2025-10-19 14:15:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #16', 7, 1, '2025-10-19 15:00:00'::TIMESTAMPTZ),
(2, 'Tạo đơn hàng #17', 7, 1, '2025-10-19 15:45:00'::TIMESTAMPTZ),
(2, 'Hủy đơn hàng #18', 7, 1, '2025-10-19 16:20:00'::TIMESTAMPTZ);

-- 20. Chat Messages
INSERT INTO chat_message (shop_id, user_id, content, sender) VALUES
(1, 1, 'Hôm nay bán được bao nhiêu?', 'user'),
(1, 1, 'Hôm nay cửa hàng đã có 10 đơn hàng với tổng doanh thu ước tính khoảng 700,000 VNĐ.', 'ai'),
(1, 2, 'Sản phẩm nào bán chạy nhất?', 'user'),
(1, 2, 'Pepsi và Coca Cola là những sản phẩm bán chạy nhất hôm nay.', 'ai');

-- 21. ZaloPay Config
INSERT INTO zalopay (shop_id, app_id, key1, key2, callback_url, status) VALUES
(1, '2554', 'example_key1_hash', 'example_key2_hash', 'https://webhook.shop.com/zalopay', 1);

-- 22. Transactions
INSERT INTO transaction (order_id, user_id, shop_id, payment_status, app_trans_id, zp_trans_id, return_code, return_message) VALUES
(4, 2, 1, 'SUCCESS', '251016_SHOP_001', 'ZP_251016_12345', 1, 'Giao dịch thành công'),
(12, 2, 1, 'SUCCESS', '251019_SHOP_002', 'ZP_251019_23456', 1, 'Giao dịch thành công'),
(16, 2, 1, 'SUCCESS', '251019_SHOP_003', 'ZP_251019_34567', 1, 'Giao dịch thành công');

-- 23. Promotions
INSERT INTO promotion (start_date, end_date, start_time, end_time, value, type, status, name, shop_id) VALUES
('2025-10-01', '2025-10-31', '08:00:00', '20:00:00', 5000, 1, 1, 'Khuyến mãi tháng 10', 1),
('2025-10-15', '2025-10-30', '14:00:00', '18:00:00', 10, 2, 1, 'Happy Hour - Giảm 10%', 1);

-- 24. Promotion Products
INSERT INTO promotion_product (promotion_id, product_id) VALUES
(1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6),
(2, 7), (2, 8), (2, 9), (2, 10), (2, 11), (2, 12);

-- 25. FCM Tokens
INSERT INTO fcm (fcm_token, user_id, uniqueid) VALUES
('fcm_token_admin_example_123456789', 1, 'UID_ADMIN_001'),
('fcm_token_nv1_example_987654321', 2, 'UID_NV1_001'),
('fcm_token_nv2_example_456789123', 3, 'UID_NV2_001');

-- 26. Reports (Chỉ có type 1: Tuần và type 2: Tháng)
INSERT INTO report (type, start_date, end_date, revenue, order_counter, shop_id, gross_profit, cost) VALUES
(1, '2025-10-14', '2025-10-20', 4270000, 16, 1, 2135000, 2135000),
(2, '2025-10-01', '2025-10-31', 5500000, 18, 1, 2750000, 2750000);

-- 27. Report Details
INSERT INTO report_detail (report_id, product_id, quantity) VALUES
-- Report tuần (14-20/10)
(1, 1, 17), (1, 2, 11), (1, 3, 5), (1, 4, 10), (1, 5, 9), (1, 6, 0),
(1, 7, 10), (1, 8, 9), (1, 9, 8), (1, 10, 2), (1, 11, 4), (1, 12, 2),
(1, 13, 3), (1, 14, 2), (1, 15, 11), (1, 16, 4), (1, 17, 2), (1, 18, 1),
-- Report tháng (1-31/10)
(2, 1, 25), (2, 2, 18), (2, 3, 8), (2, 4, 15), (2, 5, 12), (2, 6, 2),
(2, 7, 14), (2, 8, 10), (2, 9, 15), (2, 10, 6), (2, 11, 8), (2, 12, 5),
(2, 13, 10), (2, 14, 7), (2, 15, 12), (2, 16, 8), (2, 17, 5), (2, 18, 4);