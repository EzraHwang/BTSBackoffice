# PRD — Flow A: Dashboard (登入＋KPI＋統計圖表)

## 背景

本專案為售票後台管理系統，使用 .NET 8 MVC 開發。後端資料來源透過 **N8N API** 連接 Google Sheets 與 Stripe API，不直接連線資料庫。\
本 PRD 聚焦於 **Flow A (Dashboard)**：包含登入機制、票券銷售 KPI 展示，以及統計圖表。

---

## 功能需求

### 1. 登入

- **固定帳號/密碼登入**（存於 appsettings.json 或環境變數）。
- 成功登入後建立 **Session**，導向 Dashboard 頁面。
- 登入錯誤 → 顯示錯誤提示。
- 登出 → 清除 Session，返回登入頁。

### 2. Dashboard KPI

顯示於首頁的 **核心指標**：

- **Total Revenue**：總收入
- **Tickets Sold**：已售出票數
- **Refunds**：退款金額
- **Net Revenue**：收入 − 退款
- **To Settle Vendors**：應付款（對業主/供應商）
- **To Payout Stripe**：待入帳金額（Stripe）

### 3. 統計圖表

- **折線圖**：每日收入（依選定區間）
- **長條圖**：每日票數（依選定區間）
- **圓餅圖**：票券類型占比（門票 vs 車票）

### 4. 時間區間切換

- 預設：Today
- 其他選項：7 days、30 days、Custom Range
- 切換區間時，後端重新查詢 N8N / Stripe API，更新 KPI 與圖表。

---

## 使用流程

1. 管理員輸入帳號密碼 → 系統驗證。
2. 成功登入 → 導入 Dashboard，預設顯示今日 KPI 與統計。
3. 管理員可切換區間（7 日/30 日/自訂）→ 頁面即時更新圖表與數據。

---

## 範例畫面 (文字示意)

### Login Page

```
+--------------------------------+
|   Admin Login                  |
|   Username: [________]         |
|   Password: [________]         |
|   [ Login ]                    |
+--------------------------------+
```

### Dashboard Page

```
---------------------------------------------
| Total Revenue: $12,300   | Tickets: 542   |
---------------------------------------------
| Refunds: $800            | Net: $11,500   |
---------------------------------------------
| To Settle: $5,000        | To Payout: $3k |
---------------------------------------------

[ Chart: Daily Revenue / Tickets ]
[ Chart: Ticket Type Pie ]

Time Range: [ Today | 7 Days | 30 Days | Custom ]
```

---

## Controller & 路由設計

```csharp
// AuthController.cs
[HttpGet("/login")] public IActionResult Login() => View();
[HttpPost("/login")] public IActionResult Login(LoginModel model) { ... }
[HttpGet("/logout")] public IActionResult Logout() { ... }

// DashboardController.cs
[Authorize]
[HttpGet("/dashboard")] public IActionResult Index(string range = "today") {
    var data = _dashboardService.GetSummary(range);
    return View(data);
}
```

---

## ViewModel 範例

```csharp
public class DashboardViewModel {
    public decimal TotalRevenue { get; set; }
    public int TicketsSold { get; set; }
    public decimal Refunds { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal ToSettleVendors { get; set; }
    public decimal ToPayoutStripe { get; set; }
    public List<SeriesPoint> DailyRevenue { get; set; }
    public List<SeriesPoint> DailyTickets { get; set; }
    public List<PiePoint> TicketTypeDistribution { get; set; }
    public DateTime LastSync { get; set; }
}

public class SeriesPoint {
    public DateOnly Date { get; set; }
    public decimal Value { get; set; }
}

public class PiePoint {
    public string Label { get; set; }
    public int Count { get; set; }
}
```

---

## 非功能需求

- **時區**：所有數據顯示以 GMT+8。
- **安全**：Session-based Auth，登入失敗 5 次鎖定 15 分鐘。
- **效能**：Dashboard 查詢需 < 3 秒（可用 MemoryCache 緩存 30–60 秒）。
- **UI/UX**：KPI 卡片需有 Skeleton 載入效果；錯誤狀態需提示「資料暫時不可用」。

---

## API 需求

- `GET /api/dashboard?range=today|7d|30d|custom` → 回傳 DashboardViewModel JSON。
- `POST /api/auth/login` → 驗證帳號密碼，回傳 Session。
- `POST /api/auth/logout` → 清除 Session。

