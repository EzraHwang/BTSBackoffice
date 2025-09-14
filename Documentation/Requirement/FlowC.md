# PRD — Flow C: Orders & Payments (訂單與金流管理)

## 背景

本專案為售票後台管理系統，使用 .NET 8 MVC 開發。訂單與金流資料來源透過 **N8N API** 連接 Google Sheets 與 Stripe API，不直接連線資料庫。  
本 PRD 聚焦於 **Flow C (Orders & Payments)**：提供訂單查詢、訂單詳情檢視，以及金流摘要與退款操作。

---

## 功能需求

### 1. 訂單查詢
- 欄位：OrderId、User (Name/Email)、Amount、Currency、Status（Paid/Refunded/Failed）、CreatedAt。
- 支援篩選：
  - 訂單狀態（全部/Paid/Refunded/Failed）
  - 日期區間
  - 關鍵字（User Email、OrderId）
- 支援分頁與排序。

### 2. 訂單詳情
- 顯示完整資訊：
  - OrderId、User (Name, Email)
  - Amount、Currency
  - Status
  - CreatedAt
  - 票券清單（透過 N8N API 抓取 TicketViewModel 列表）
- 右側詳情面板或單獨頁面顯示。

### 3. 金流摘要
- KPI 指標：
  - **Total Successful Payments**（成功金額與交易數）
  - **Refunds**（退款金額與筆數）
  - **Failures**（失敗率）
  - **Net Revenue**（成功 − 退款）
- 顯示於 Payments 頁面。

### 4. 金流明細
- 欄位：PaymentId、OrderId、Amount、Currency、Status、3DS (通過/未通過)、CreatedAt。
- 點擊 PaymentId → 顯示 Stripe PaymentIntent 或 Charge 詳情（含原始 StripeId）。

### 5. 退款操作（Phase 2 可選）
- 管理員可在後台對已付款訂單申請退款。
- 系統呼叫 Stripe Refund API → 更新狀態並同步至 Google Sheet（透過 N8N API）。

### 6. 匯出功能
- 可將訂單與金流列表匯出成 CSV/Excel。

---

## 使用流程

1. 管理員進入 Orders 頁面 → 預設顯示最近 7 日訂單。  
2. 可依狀態、日期區間或關鍵字查詢。  
3. 點擊某一筆訂單 → 顯示詳情與票券清單。  
4. 切換至 Payments 頁面 → 顯示金流 KPI 與交易列表。  
5. （選配）管理員對某筆 Payment 點選「退款」 → 系統呼叫 Stripe Refund API → 回饋結果。

---

## 範例畫面 (文字示意)

### Orders List
```
---------------------------------------------------
| OrderId | User   | Amount | Status   | CreatedAt
---------------------------------------------------
| O001    | John   | $120   | Paid     | 2025-09-12
| O002    | Alice  | $50    | Refunded | 2025-09-13
```

### Payments Summary
```
-----------------------------------------
| Successful: $12,000 (320 txns)
| Refunds:    $500 (12 txns)
| Failures:   5%
-----------------------------------------
```

### Payment Details
```
---------------------------------------------------
| PaymentId | OrderId | Amount | Status  | 3DS
---------------------------------------------------
| P001      | O001    | $120   | Success | Passed
| P002      | O002    | $50    | Refunded| --
```

---

## Controller & 路由設計

```csharp
// OrdersController.cs
[Authorize]
[HttpGet("/orders")] public IActionResult Index(string status = "all") { ... }
[Authorize]
[HttpGet("/orders/{id}")] public IActionResult Detail(string id) { ... }

// PaymentsController.cs
[Authorize]
[HttpGet("/payments")] public IActionResult Index(string range = "7d") { ... }
[Authorize]
[HttpGet("/payments/{id}")] public IActionResult Detail(string id) { ... }
[Authorize]
[HttpPost("/payments/{id}/refund")] public IActionResult Refund(string id) { ... } // Phase 2
```

---

## ViewModel 範例

```csharp
public class OrderViewModel {
    public string OrderId { get; set; }
    public string User { get; set; }
    public string Email { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; } // Paid / Refunded / Failed
    public DateTime CreatedAt { get; set; }
    public List<TicketViewModel> Tickets { get; set; }
}

public class PaymentViewModel {
    public string PaymentId { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; } // Success / Refunded / Failed
    public bool ThreeDS { get; set; }
    public DateTime CreatedAt { get; set; }
    public string StripeId { get; set; }
}
```

---

## 非功能需求

- **時區**：所有時間以 GMT+8 顯示。
- **安全**：所有操作需 Session 驗證。
- **效能**：訂單與金流列表需支援分頁與快取。
- **UI/UX**：查詢與篩選需快速回應，錯誤訊息需清晰。

---

## API 需求

- `GET /api/orders?status=all&range=7d` → 回傳訂單列表。
- `GET /api/orders/{id}` → 回傳訂單詳情與票券清單。
- `GET /api/payments?range=7d` → 回傳金流摘要與交易列表。
- `GET /api/payments/{id}` → 回傳單筆金流詳情。
- `POST /api/payments/{id}/refund` → 退款操作（Phase 2）。

