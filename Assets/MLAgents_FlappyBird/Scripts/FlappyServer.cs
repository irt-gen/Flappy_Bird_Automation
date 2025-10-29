// // FlappyServer.cs
//
// using System;
//
// using System.Collections;
//
// using System.Net;
//
// using System.Net.Sockets;
//
// using System.Text;
//
// using Newtonsoft.Json;
//
// using UnityEngine;
//  
// public class FlappyServer : MonoBehaviour
//
// {
//
//     [Serializable]
//
//     public class Msg
//
//     {
//
//         public string cmd;    // "reset" or "step"
//
//         public int action;    // 0 or 1
//
//     }
//  
//     [Serializable]
//
//     public class Reply
//
//     {
//
//         public float[] obs;
//
//         public float   reward;
//
//         public bool    done;
//
//     }
//  
//     [Header("Server Settings")]
//
//     [SerializeField] int port = 5005;
//  
//     private TcpListener listener;
//
//     private Bird        bird;
//
//     private Level       level;
//  
//     private int pipesCount     = 0;
//
//     private int lastPipeCount  = 0;
//  
//     void Awake()
//
//     {
//
//         bird  = Bird.GetInstance();
//
//         level = FindObjectOfType<Level>();
//
//         level.OnPipePassed += (s,e) => pipesCount++;
//
//     }
//  
//     void Start()
//
//     {
//
//         listener = new TcpListener(IPAddress.Loopback, port);
//
//         listener.Start();
//
//         Debug.Log($"FlappyServer ▶ Listening on port {port}");
//
//         StartCoroutine(NetworkLoop());
//
//     }
//  
//     IEnumerator NetworkLoop()
//
//     {
//
//         var buffer = new byte[4096];
//  
//         while (true)
//
//         {
//
//             // 1) Accept a new client if pending
//
//             if (listener.Pending())
//
//             {
//
//                 var client = listener.AcceptTcpClient();
//
//                 Debug.Log("FlappyServer ▶ Client connected");
//
//                 var stream = client.GetStream();
//  
//                 // 2) Read until client disconnects
//
//                 while (client.Connected)
//
//                 {
//
//                     if (stream.DataAvailable)
//
//                     {
//
//                         int len = stream.Read(buffer, 0, buffer.Length);
//
//                         var json = Encoding.UTF8.GetString(buffer, 0, len);
//
//                         var req  = JsonConvert.DeserializeObject<Msg>(json);
//  
//                         Debug.Log($"FlappyServer Recieved Action: {req.action}");
//                         if (req.cmd == "reset")
//
//                         {
//                             Debug.Log($"FlappyServer Recieved Action: {req.action}");
//                             var obs0 = ResetGame();
//
//                             Send(stream, new Reply { obs = obs0, reward = 0f, done = false });
//
//                             Debug.Log("FlappyServer ▶ reset() → obs[0]=" + obs0[0]);
//
//                         }
//
//                         else if (req.cmd == "step")
//
//                         {
//
//                             Debug.Log("FlappyServer ▶ step(action=" + req.action + ")");
//
//                             var (obs, reward, done) = StepGame(req.action);
//
//                             Send(stream, new Reply { obs = obs, reward = reward, done = done });
//
//                             if (done)
//
//                                 Debug.Log("FlappyServer ▶ done=true");
//
//                         }
//
//                     }
//
//                     // yield one frame so physics & bird.Update() run
//
//                     yield return null;
//
//                 }
//  
//                 Debug.Log("FlappyServer ▶ Client disconnected");
//
//                 client.Close();
//
//             }
//
//             yield return null;
//
//         }
//
//     }
//  
//     private float[] ResetGame()
//
//     {
//
//         // 1) Reset physics & position
//
//         bird.Reset();
//
//         level.Reset();
//
//         pipesCount    = 0;
//
//         lastPipeCount = 0;
//  
//         // 2) Start the game (first jump)
//
//         bird.StartGame();
//  
//         // 3) Return the post-jump observation
//
//         return GetObservations();
//
//     }
//  
//     private (float[] obs, float reward, bool done) StepGame(int action)
//
//     {
//
//         // Agent flap
//
//         if (action == 1)
//
//         {
//
//             bird.Jump();
//
//             Debug.Log("FlappyServer ▶ bird.Jump()");
//
//         }
//  
//         // Compute reward & done
//
//         float reward = 0.01f;  // survival bonus
//
//         if (pipesCount > lastPipeCount)
//
//         {
//
//             reward += (pipesCount - lastPipeCount) * 1.0f;
//
//             lastPipeCount = pipesCount;
//
//             Debug.Log($"FlappyServer ▶ pipe passed! total={pipesCount}");
//
//         }
//  
//         bool done = false;
//
//         if (bird.IsDead)
//
//         {
//
//             reward -= 1f;
//
//             done = true;
//
//         }
//  
//         return (GetObservations(), reward, done);
//
//     }
//  
//     private float[] GetObservations()
//
//     {
//
//         var obsList = new System.Collections.Generic.List<float>();
//  
//         // 1) Bird Y position  (normalize to [0,1])
//
//         float worldH = 100f;
//
//         obsList.Add((bird.transform.position.y + worldH/2f) / worldH);
//  
//         // 2) Next pipe X pos
//
//         var pipe = level.GetNextPipeComplete();
//
//         float px = pipe != null ? pipe.pipeBottom.GetXPosition() : 100f;
//
//         obsList.Add(px / 100f);
//  
//         // 3) Bird Y velocity
//
//         obsList.Add(bird.GetVelocityY() / 200f);
//  
//         return obsList.ToArray();
//
//     }
//  
//     private void Send(NetworkStream stream, Reply reply)
//
//     {
//
//         // Append a newline so Python _recv() returns on '\n'
//
//         var json = JsonConvert.SerializeObject(reply) + "\n";
//
//         var data = Encoding.UTF8.GetBytes(json);
//
//         stream.Write(data, 0, data.Length);
//
//     }
//  
//     void OnApplicationQuit()
//
//     {
//
//         listener.Stop();
//
//     }
//
// }
//
//  