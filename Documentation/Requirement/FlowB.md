# PRD — Flow B: Tickets (票券管理工作台)

## 背景

本專案為售票後台管理系統，使用 .NET 8 MVC 開發。所有票券與訂單資料來源透過 **N8N API** 串接 Google Sheets，不直接連線資料庫。  
本 PRD 聚焦於 **Flow B (Tickets)**：提供票券查詢、詳情檢視，以及手動操作（發券、重寄、作廢）。

---

## 功能需求

### 1. 票券列表
- 欄位：TicketId、OrderId、Buyer、Email、Type（門票/車票）、Status（待發送/已發送/已撤銷）、CreatedAt。
- 支援篩選：
  - 狀態（全部/待發送/已發送/已撤銷）
  - 票券類型（門票/車票）
  - 日期區間
  - 關鍵字（Buyer Email、OrderId）
- 支援分頁與排序。

### 2. 票券詳情
- 顯示完整資訊：
  - TicketId、OrderId
  - Buyer (Name, Email)
  - Type（門票/車票）
  - 狀態
  - 建立時間、發送時間
  - **業主回信內容**（透過 N8N API 取得 Gmail 整合結果）
- 右側詳情面板或單獨頁面顯示。

### 3. 票券操作
- **手動發券**：呼叫 N8N API 觸發寄送程序。
- **重寄票券**：再次發送票券給使用者。
- **作廢票券**：狀態更新為「已撤銷」。
- 操作結果需即時回饋（成功/失敗訊息）。

### 4. 記錄與審計
- 所有操作需記錄在 Audit Log（登入帳號、時間、操作、票券Id）。
- 可寫回 Google Sheet 或內部檔案。

---

## 使用流程

1. 管理員進入 Tickets 頁面 → 預設顯示最近 7 日票券。  
2. 管理員可透過篩選或搜尋找到目標票券。  
3. 點擊某一列 → 顯示詳情（右側 Panel 或新頁面）。  
4. 管理員可進行操作：發券 / 重寄 / 作廢。  
5. 系統透過 N8N API 呼叫 → 更新狀態並回饋。  

---

## 範例畫面 (文字示意)

### Tickets List
```
----------------------------------------------------------
| TicketId | OrderId | Buyer   | Type  | Status   | Action
----------------------------------------------------------
| T001     | O001    | John    | Park  | Pending  | [Send]
| T002     | O002    | Alice   | Train | Issued   | [Resend]
| T003     | O003    | Mark    | Park  | Voided   | [--]
```

### Ticket Detail Panel
```
TicketId: T001
OrderId: O001
Buyer: John (john@email)
Type: Park Ticket
Status: Pending
Created: 2025-09-14
Owner Reply: "Approved, please issue"
[Send Ticket] [Void Ticket]
```

---

## Controller & 路由設計

```csharp
// TicketsController.cs
[Authorize]
[HttpGet("/tickets")] public IActionResult Index(string status = "all") { ... }
[Authorize]
[HttpGet("/tickets/{id}")] public IActionResult Detail(string id) { ... }
[Authorize]
[HttpPost("/tickets/{id}/send")] public IActionResult Send(string id) { ... }
[Authorize]
[HttpPost("/tickets/{id}/resend")] public IActionResult Resend(string id) { ... }
[Authorize]
[HttpPost("/tickets/{id}/void")] public IActionResult Void(string id) { ... }
```

---

## ViewModel 範例

```csharp
public class TicketViewModel {
    public string TicketId { get; set; }
    public string OrderId { get; set; }
    public string Buyer { get; set; }
    public string Email { get; set; }
    public string Type { get; set; } // Park / Train
    public string Status { get; set; } // Pending / Issued / Voided
    public DateTime CreatedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string OwnerReply { get; set; }
}
```

---

## 非功能需求

- **時區**：所有時間以 GMT+8 顯示。
- **安全**：所有操作需 Session 驗證。
- **效能**：票券列表查詢需支援分頁，避免一次載入過多資料。
- **UI/UX**：操作按鈕需有 Loading 狀態，錯誤訊息需明確顯示。

---

## API 需求

- `GET /api/tickets?status=all&range=7d` → 回傳票券列表。
- `GET /api/tickets/{id}` → 回傳票券詳情。
- `POST /api/tickets/{id}/send` → 手動發送票券。
- `POST /api/tickets/{id}/resend` → 重寄票券。
- `POST /api/tickets/{id}/void` → 作廢票券。

