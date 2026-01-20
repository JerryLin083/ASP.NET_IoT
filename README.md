# ASP.NET_IOT 系統架構

## 核心技術

ASP.NET Core, PostgreSQL, EF Core, MQTTnet, SignalR, In-Memory Cache

## 架構流程圖

```
                                +----------------> [SignalR Hub] ----------------------> [Web Client] (即時更新)
                                |
                                |
[MQTT Broker] -> [MqttHandler] -+----------------> [Cache (In-Memory)] <--- (讀取) --- [DashboardController]
                                |
                                |
                                +----------------> [Data Channel] -> [MqttDbBatchService] -> [Database]
```

## 組件說明

- **MqttHandler**: **核心處理器**。接收到 MQTT 訊息後，執行以下三項任務：
  1.  **推播至 SignalR**: 將最新數據即時廣播給所有 Web 客戶端。
  2.  **更新快取**: 將最新數據寫入記憶體快取，供儀表板頁面快速載入。
  3.  **放入通道**: 將訊息排入 `Data Channel`，等待批次寫入資料庫。

- **SignalR Hub**: 提供即時通訊，接收來自 `MqttHandler` 的數據並推播至前端。

- **Cache (In-Memory)**: 儲存最新的感測器數據。

- **DashboardController**: 處理 Web 請求，從 `Cache` 中讀取數據以顯示頁面。

- **Data Channel**: 記憶體中的訊息佇列，用於解耦。

- **MqttDbBatchService**: 從通道讀取訊息，批次寫入資料庫以進行歷史存檔。

- **Database (PostgreSQL)**: 永久儲存所有感測器數據。

<h2>DEMO</h2>
<img src="https://github.com/JerryLin083/ASP.NET_IoT/blob/master/demo/demo.gif" alt="demo" width="600">
