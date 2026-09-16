using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = 3;
        private static readonly float TestTimeout = 12.0f;

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 50;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try
                {
                    Setup();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[PlayModeTest] Setup threw exception: " + e);
                    FinishTest(true, e.Message);
                    return;
                }
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = elapsed >= TestTimeout;

            try
            {
                bool complete = Tick(elapsed);
                if (complete || timedOut)
                {
                    if (timedOut && !complete)
                    {
                        Debug.LogWarning("[PlayModeTest] Test timed out after " + elapsed + "s");
                    }
                    FinishTest(timedOut && !complete, timedOut ? "Test timed out after " + TestTimeout + "s" : null);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PlayModeTest] Tick threw exception: " + e);
                FinishTest(true, e.Message);
            }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;

            string resultJson;
            try
            {
                resultJson = GetResult();
            }
            catch (System.Exception e)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = "GetResult() threw: " + e.Message,
                    logs = _capturedLogs.ToArray()
                });
            }

            if (isError && errorMessage != null)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = errorMessage,
                    logs = _capturedLogs.ToArray()
                });
            }

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("[SAO]") || message.Contains("TEST_RESULT"))
            {
                _capturedLogs.Add("[" + type + "] " + message);
            }
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
            {
                AssetDatabase.DeleteAsset(scriptPath);
            }
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public bool scanningCompleted;
            public bool gameStarted;
            public bool swordMenuSpawned;
            public int spawnedSwordCount;
            public bool enemySpawned;
            public bool combatTested;
            public float playerHealth;
            public float playerStamina;
            public int playerScore;
            public string[] logs;
        }

        private static bool _scanDone = false;
        private static bool _gameStarted = false;
        private static bool _swordsSpawned = false;
        private static int _swordCount = 0;
        private static bool _combatTested = false;
        private static bool _enemyDetected = false;

        private static void Setup()
        {
            Debug.Log("[Test] PlayMode Setup initiated. Waiting for ARSceneManager scanning...");
        }

        private static bool Tick(float elapsed)
        {
            // 1. Check if Scanning finished and MainMenu reached
            if (!_scanDone)
            {
                var gmObj = GameObject.Find("ARGameManager");
                if (gmObj != null)
                {
                    var gm = gmObj.GetComponent<ARGameManager>();
                    if (gm != null && (gm.currentState == GameState.MainMenu || gm.currentState == GameState.Gameplay))
                    {
                        _scanDone = true;
                        Debug.Log("[Test] Room scan complete. Starting Game...");
                        gm.StartGame();
                        _gameStarted = true;
                    }
                }
            }

            // 2. Once in Gameplay, spawn sword menu
            if (_gameStarted && !_swordsSpawned && elapsed > 2.0f)
            {
                var smObj = GameObject.Find("SwordManager");
                if (smObj != null)
                {
                    var sm = smObj.GetComponent<SwordMenuManager>();
                    if (sm != null)
                    {
                        sm.SpawnSwordMenu();
                        _swordsSpawned = true;
                        Debug.Log("[Test] Sword menu spawned.");
                    }
                }
            }

            // 3. Check spawned swords and test combat
            if (_swordsSpawned && !_combatTested && elapsed > 3.5f)
            {
                var swords = Object.FindObjectsOfType<SwordInteraction>();
                _swordCount = swords.Length;
                Debug.Log("[Test] Active swords found: " + _swordCount);

                // Find enemy
                var enemyStats = Object.FindObjectOfType<EnemyStats>();
                if (enemyStats != null)
                {
                    _enemyDetected = true;
                    Debug.Log("[Test] Found enemy: " + enemyStats.gameObject.name + " with HP=" + enemyStats.currentHealth);

                    // Test combat hit
                    if (swords.Length > 0)
                    {
                        SwordInteraction sword = swords[0];
                        if (Camera.main != null)
                        {
                            sword.Equip(Camera.main.transform, false);
                        }

                        // Apply damage to enemy
                        enemyStats.TakeDamage(35f);
                        if (PlayerStats.Instance != null)
                        {
                            PlayerStats.Instance.UseStamina(20f);
                        }

                        Debug.Log("[Test] Combat hit applied! Enemy HP is now: " + enemyStats.currentHealth);
                        _combatTested = true;
                    }
                }
            }

            // Finish after 6 seconds
            return elapsed >= 6.0f;
        }

        private static string GetResult()
        {
            var pStats = PlayerStats.Instance;
            bool success = _scanDone && _gameStarted && _swordsSpawned && _swordCount > 0 && _enemyDetected && _combatTested;

            var result = new TestResult
            {
                success = success,
                scanningCompleted = _scanDone,
                gameStarted = _gameStarted,
                swordMenuSpawned = _swordsSpawned,
                spawnedSwordCount = _swordCount,
                enemySpawned = _enemyDetected,
                combatTested = _combatTested,
                playerHealth = pStats != null ? pStats.currentHealth : 0f,
                playerStamina = pStats != null ? pStats.currentStamina : 0f,
                playerScore = pStats != null ? pStats.score : 0,
                logs = _capturedLogs.ToArray()
            };

            return JsonUtility.ToJson(result);
        }
    }
}
