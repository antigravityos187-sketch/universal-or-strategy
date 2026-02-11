using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace V9_ExternalRemote
{
    [ComImport, Guid("A43788C1-D91B-11D3-8F39-00C04F365157"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IRtdUpdateEvent
    {
        void UpdateNotify();
        int HeartbeatInterval { get; set; }
        void Disconnect();
    }

    /// <summary>
    /// Specialized client for ThinkorSwim RTD (tos.rtd) using DYNAMIC binding to avoid GUID issues.
    /// </summary>
    public class TosRtdClient
    {
        private dynamic _rtdServer; // Use dynamic to bypass strict Interface casting
        private Dispatcher _dispatcher;
        private bool _isConnected;
        private Dictionary<int, string> _topicMap = new Dictionary<int, string>();
        private int _nextTopicId = 1;

        public event Action<string, object?>? OnDataUpdate;
        public event Action<bool>? OnConnectionStatusChanged;

        public TosRtdClient(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            Log("Client initialized (Dynamic + Polling).");
        }

        public bool IsConnected => _isConnected;

        public void Start()
        {
            Log("Starting connection...");
            try
            {
                Type rtdType = Type.GetTypeFromProgID("tos.rtd");
                if (rtdType == null)
                {
                    Log("Error: tos.rtd ProgID not found.");
                    OnConnectionStatusChanged?.Invoke(false);
                    return;
                }

                _rtdServer = Activator.CreateInstance(rtdType);
                // Pass our specialized callback object
                _rtdServer.ServerStart(new RtdUpdateEvent(this));
                
                _isConnected = true;
                Log("Connected to RTD Server.");
                OnConnectionStatusChanged?.Invoke(true);

                // Polling Fallback: Force RefreshData every 500ms if callbacks fail
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500); 
                timer.Tick += (s, e) => {
                    if (_isConnected) TriggerUpdate(); 
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                Log("Start Error: " + ex.ToString());
                _isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
            }
        }

        public int Subscribe(string key, object[] topics)
        {
            try
            {
                if (!_isConnected || _rtdServer == null) return -1;

                int topicId = _nextTopicId++;
                string flatTopics = string.Join(", ", topics);
                Log($"Subscribing ID {topicId}: [{flatTopics}] -> Key: {key}");
                
                _rtdServer.ConnectData(topicId, topics, true);
                
                _topicMap[topicId] = key;
                return topicId;
            }
            catch (Exception ex)
            {
                Log($"Subscribe Error: " + ex.Message);
                return -1;
            }
        }

        public int Subscribe(string symbol, string field, string study = "")
        {
            // Legacy wrapper - redirects to raw
            object[] topics;
            if (!string.IsNullOrEmpty(study))
                topics = new object[] { symbol, study, field };
            else
                topics = new object[] { field, symbol };
            
            return Subscribe($"{symbol}:{field}", topics);
        }

        public void UnsubscribeAll()
        {
            Log("Unsubscribing all topics...");
            try
            {
                if (_rtdServer == null) return;
                foreach (var topic in _topicMap.Keys)
                {
                    try { _rtdServer.DisconnectData(topic); } catch { }
                }
                _topicMap.Clear();
                _nextTopicId = 1; // Reset topic IDs to prevent stale data overlap
            }
            catch (Exception ex)
            {
                Log("UnsubscribeAll Error: " + ex.Message);
            }
        }

        // Keep nested for access to private _client, but simpler
        [ComVisible(true)]
        public class RtdUpdateEvent : IRtdUpdateEvent
        {
            private TosRtdClient _client;
            public int HeartbeatInterval { get; set; } = 1000;
            public RtdUpdateEvent(TosRtdClient client) { _client = client; }
            public void UpdateNotify() { _client.TriggerUpdate(); }
            public void Disconnect() { }
        }

        public void TriggerUpdate(int delayMs = 0)
        {
            if (delayMs > 0)
            {
                Task.Delay(delayMs).ContinueWith(_ => _dispatcher.BeginInvoke(new Action(ProcessUpdates)));
            }
            else
            {
                _dispatcher.BeginInvoke(new Action(ProcessUpdates));
            }
        }

        private void ProcessUpdates()
        {
            try
            {
                if (_rtdServer == null) return;

                int topicCount = 0;
                var data = _rtdServer.RefreshData(ref topicCount);

                if (topicCount > 0 && data != null)
                {
                    bool hasLoading = false;
                    Array? dataArray = data as Array;
                    
                    // Handle 2D Array [ID, Value]
                    if (dataArray != null && dataArray.Rank == 2)
                    {
                        int rows = dataArray.GetLength(0); // Should be 2 (ID and Value)
                        int cols = dataArray.GetLength(1); // Should be topicCount
                        
                        // Debug log every few cycles
                        if (DateTime.Now.Second % 5 == 0 && DateTime.Now.Millisecond < 100)
                            Log($"Refreshing {topicCount} topics (2D Array: {rows}x{cols})");

                        for (int i = 0; i < cols; i++)
                        {
                            try
                            {
                                object? rawId = dataArray.GetValue(0, i);
                                if (rawId == null) continue;
                                int topicId = Convert.ToInt32(rawId);
                                object? value = dataArray.GetValue(1, i);

                                string valueStr = value?.ToString() ?? "null";
                                
                                // Check for Loading or invalid states
                                if (valueStr.Contains("Loading") || valueStr == "0" || string.IsNullOrEmpty(valueStr))
                                    hasLoading = true;

                                if (_topicMap.ContainsKey(topicId))
                                {
                                    string key = _topicMap[topicId];
                                    
                                    // SHOTGUN LOG: Log everything to hits for debugging CUSTOM fields
                                    if (valueStr != "N/A" && valueStr != "null" && valueStr != "#N/A" && !valueStr.Contains("Loading"))
                                    {
                                         System.IO.File.AppendAllText("v9_shotgun_hits.txt", $"RECEIVED: {key} = {valueStr}\n");
                                    }

                                    OnDataUpdate?.Invoke(key, value);
                                }
                            }
                            catch (Exception ex) { Log("Item Error: " + ex.Message); }
                        }
                    }
                    else
                    {
                        Log($"Unexpected Data Rank: {dataArray.Rank}");
                    }

                    // FAST RETRY: If we got "Loading..." or empty values, kick off another check in 100ms
                    if (hasLoading)
                    {
                        TriggerUpdate(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Refresh Error: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            Log("Disconnecting...");
            _isConnected = false;
            OnConnectionStatusChanged?.Invoke(false);
        }

        public void Stop()
        {
            Log("Stopping server...");
            try 
            {
                UnsubscribeAll();
                _rtdServer?.ServerTerminate();
            } 
            catch {}
            _rtdServer = null!;
            _isConnected = false;
        }

        public void Heartbeat()
        {
            try
            {
                if (_rtdServer != null) 
                {
                    int val = _rtdServer.Heartbeat();
                    Log("Heartbeat: " + val);
                }
            }
            catch (Exception ex) { Log("Heartbeat Error: " + ex.Message); }
        }

        public void Log(string msg)
        {
            try
            {
                System.IO.File.AppendAllText("v9_remote_log.txt", $"{DateTime.Now:HH:mm:ss.fff} | {msg}\r\n");
            }
            catch {}
        }
    }
}
