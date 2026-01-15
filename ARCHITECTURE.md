# ASP.NET IoT 專案架構說明

本文件旨在說明此 ASP.NET IoT 專案的軟體架構、設計模式以及資料流。

## 核心技術棧

- **Web 框架**: ASP.NET Core MVC
- **資料庫**: PostgreSQL
- **資料存取**: Entity Framework Core
- **即時通訊**: SignalR
- **IoT 通訊**: MQTT (使用 MQTTnet 套件)
- **背景任務**: BackgroundService
- **服務間通訊**: `System.Threading.Channels` (記憶體佇列)
- **快取**: `IMemoryCache`

## 架構模式

此專案採用了多種設計模式，形成一個高效率、可擴展的事件驅動架構。

1.  **模型-視圖-控制器 (MVC)**: 用於組織 Web 應用程式的結構，分離了業務邏輯、資料和使用者介面。
2.  **背景服務 (Background Services)**: 使用託管服務 (`IHostedService`) 在應用程式背景長時間執行任務，例如監聽 MQTT 訊息和批次寫入資料庫。
3.  **生產者-消費者模式 (Producer-Consumer)**:
    - `MqttWorker` 作為生產者，接收來自 IoT 裝置的訊息。
    - `MqttHandler` 處理訊息並將其寫入一個共享的通道 (`ChannelService`)。
    - `MqttDbBatchService` 作為消費者，從通道中讀取訊息並進行後續處理。
4.  **依賴注入 (Dependency Injection - DI)**: 專案大量使用 DI 來解耦元件，所有服務 (如 `IMqttHandler`, `IContextHandler`, `IChannelService`) 都在 `Program.cs` 中註冊和管理其生命週期。
5.  **發布-訂閱模式 (Publish-Subscribe)**:
    - **MQTT**: IoT 裝置將資料發布到 MQTT Broker 的特定主題，而 `MqttWorker` 訂閱這些主題以接收資料。
    - **SignalR**: `MqttHandler` 將即時資料廣播 (Publish) 給所有連接到 `SensorHub` 的 Web 客戶端 (Subscribers)。

## 專案結構與元件職責

-   `Controllers/`:
    -   `DashboardController.cs`: 處理儀表板頁面的 HTTP 請求。它會從記憶體快取 (`IMemoryCache`) 中讀取最新的感測器資料，用於頁面的初始載入。
-   `Views/`:
    -   `Dashboard/Index.cshtml`: 儀表板的視圖，使用 SignalR 客戶端連接到後端以接收即時資料更新。
-   `Hubs/`:
    -   `SensorHub.cs`: SignalR 中樞，作為一個廣播通道，讓後端 (`MqttHandler`) 能夠將即時感測器讀數推送到所有已連接的前端客戶端。
-   `Data/`:
    -   `IoTAppContext.cs`: Entity Framework 的 `DbContext`，定義了 `Devices` 和 `SensorReadings` 兩個資料表，負責與 PostgreSQL 資料庫進行互動。
-   `Models/`:
    -   `Entities/`: 定義了資料庫實體，如 `Device.cs` 和 `SensorReading.cs`。
    -   `Mqtt/`: 定義了用於 MQTT 訊息反序列化的資料傳輸物件 (DTO)，如 `SensorTopic.cs` 和 `SensorPayload.cs`。
-   `BackgroundServices/`:
    -   `MqttWorker.cs`: 連接到 MQTT Broker，訂閱指定的 Topic。當收到訊息時，會將其轉發給 `MqttHandler` 進行處理。它也負責處理連線中斷和自動重連。
    -   `MqttDbBatchService.cs`: 作為消費者，從 `ChannelService` 中讀取訊息。它會將訊息收集成一個批次，然後呼叫 `IContextHandler` 將整批資料一次性寫入資料庫，以提高效能並減少資料庫負載。
    -   `Service/`:
        -   `MqttHandler.cs`: 這是資料處理的核心分派器。它解析收到的 MQTT 訊息，然後執行三個動作：
            1.  **更新快取**: 將最新的讀數存入 `IMemoryCache`。
            2.  **廣播即時資料**: 透過 `SensorHub` 將訊息推送給前端。
            3.  **排入佇列**: 將訊息放入 `ChannelService` 以待後續的資料庫儲存。
        -   `ContextHandler.cs`: 負責與 `DbContext` 互動，執行高效率的批次資料庫操作。它會先查詢批次中已存在的裝置，然後一次性插入所有新的感測器讀數和裝置資訊。
        -   `ChannelService.cs`: `System.Threading.Channels` 的一個簡單封裝，提供一個執行緒安全的記憶體佇列，作為 `MqttHandler` (生產者) 和 `MqttDbBatchService` (消費者) 之間的緩衝區。

## 系統架構圖

```mermaid
graph TD
    subgraph "外部系統 (External Systems)"
        IoTDevice("IoT 裝置")
        MqttBroker("MQTT Broker")
        Browser("使用者瀏覽器 (Client)")
    end

    subgraph "ASP.NET IoT 應用程式"
        MqttWorker["MqttWorker (Background Service)"]
        MqttHandler["MqttHandler (處理器)"]
        
        subgraph "即時路徑 (Real-time Path)"
            style Cache fill:#dae8fc,stroke:#6c8ebf,stroke-width:2px
            Cache["IMemoryCache (快取)"]
            SignalRHub["SensorHub (SignalR)"]
        end
        
        subgraph "資料庫路徑 (Database Path)"
            style Channel fill:#f9f,stroke:#333,stroke-width:2px
            Channel["Channel (記憶體佇列)"]
            MqttDbBatchService["MqttDbBatchService (Background Service)"]
            DB["PostgreSQL 資料庫"]
        end

        subgraph "HTTP 請求路徑 (HTTP Request Path)"
            Controller["DashboardController"]
        end
    end

    %% --- 資料流 1: 即時 IoT 數據處理 ---
    IoTDevice -- "1. 發布 MQTT 訊息" --> MqttBroker
    MqttBroker -- "2. 推送訊息" --> MqttWorker
    MqttWorker -- "3. 轉發原始訊息" --> MqttHandler
    MqttHandler -- "4a. 更新最新讀數" --> Cache
    MqttHandler -- "4b. 廣播即時訊息" --> SignalRHub
    MqttHandler -- "4c. 寫入佇列以待儲存" --> Channel
    SignalRHub -- "5. 推送給瀏覽器" --> Browser
    Channel -- "6. 讀取訊息批次" --> MqttDbBatchService
    MqttDbBatchService -- "7. 寫入資料庫" --> DB

    %% --- 資料流 2: 使用者初次載入頁面 ---
    Browser -.->| "A. 載入頁面 (HTTP GET)" | Controller
    Controller -.->| "B. 讀取初始資料" | Cache
    Cache -.->| "C. 返回快取資料" | Controller
    Controller -.->| "D. 回應 HTML 視圖" | Browser
```

## 資料流 (Data Flow)

1.  **資料傳入 (Ingestion)**:
    -   一個 IoT 裝置將包含感測器讀數的 JSON 酬載發布到 MQTT Broker 的一個主題上 (例如 `iot/user1/area1/zone1`)。
2.  **訊息接收 (Reception)**:
    -   `MqttWorker` 訂閱了該主題，接收到此訊息。
    -   它將主題和酬載字串傳遞給 `MqttHandler`。
3.  **處理與分派 (Processing & Dispatch)**:
    -   `MqttHandler` 解析主題和酬載，將其轉換為強型別的 `SensorTopic` 和 `SensorPayload` 物件。
    -   它將 `SensorPayload` 存入以 `area` 和 `zone` 為鍵的記憶體快取中，覆蓋舊值。
    -   它透過 `SensorHub` 將原始的主題和酬載廣播給所有 SignalR 客戶端。前端儀表板上的 JavaScript 程式碼接收到此訊息並即時更新 UI。
    -   它將包含所有解析後資訊的 `MqttMessage` 物件寫入 `ChannelService` 的佇列中。
4.  **批次儲存 (Batch Storage)**:
    -   `MqttDbBatchService` 在背景執行緒中等待 `ChannelService` 中的資料。
    -   它以固定時間間隔（例如 5 秒）或在訊息累積到一定數量（例如 10 條）時，從通道中取出所有待處理的訊息。
    -   它將這批訊息傳遞給 `ContextHandler`。
    -   `ContextHandler` 查詢資料庫以識別新舊裝置，然後將所有新的 `SensorReading` 記錄一次性新增到 `DbContext` 中，最後呼叫 `SaveChangesAsync()` 將所有變更寫入 PostgreSQL 資料庫。
5.  **資料呈現 (Presentation)**:
    -   當使用者首次訪問儀表板 URL (例如 `/Dashboard?area=area1&zone=zone1`) 時，`DashboardController` 會嘗試從記憶體快取中讀取最新的資料並將其傳遞給視圖，以便頁面能立即顯示當前狀態。
    -   頁面載入後，其 JavaScript 會建立一個到 `SensorHub` 的 SignalR 連線，並開始監聽 `ReceiveReading` 事件，以在資料到達時即時更新頁面內容。
