using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketSLU
{
    class Program
    {

        //  在宣告區先行宣告 Socket 物件 
        static Socket[] SckSs;   // 一般而言 Server 端都會設計成可以多人同時連線. 
        static int SckCIndex;    // 定義一個指標用來判斷現下有哪一個空的 Socket 可以分配給 Client 端連線;
        static string LocalIP = "127.0.0.1"; // 其中 xxx.xxx.xxx.xxx 為本機IP
        static int SPort = 4002;
        static int RDataLen = 100; // 這裡的RDataLen為要傳送資料的長度, 這裡我隨用5個長度, 傳送 "ABCDE" 給Client端
                            // Hi All, 因為我寫Socket都是在傳電文用, 所以我習慣傳送固定長度~ ,此文沒有在處理非固定長度的資料喔~

        static void Main(string[] args)
        {

            Listen();
        }

        // 聆聽
        static public void Listen()
        {

            // 用 Resize 的方式動態增加 Socket 的數目
            Array.Resize(ref SckSs, 1);
            SckSs[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SckSs[0].Bind(new IPEndPoint(IPAddress.Parse(LocalIP), SPort));
            SckSs[0].Listen(10); // 進行聆聽; Listen( )為允許 Client 同時連線的最大數
            SckSWaitAccept();   // 另外寫一個函數用來分配 Client 端的 Socket

        }



        // 等待Client連線

        static private void SckSWaitAccept()
        {
            bool FlagFinded = false;
            for (int i = 1; i < SckSs.Length; i++)
            {
                if (SckSs[i] != null)
                {
                    if (SckSs[i].Connected == false)
                    {

                        SckCIndex = i;

                        FlagFinded = true;

                        break;

                    }

                }

            }

            // 如果 FlagFinded 為 false 表示目前並沒有多餘的 Socket 可供 Client 連線

            if (FlagFinded == false)
            {
                SckCIndex = SckSs.Length;
                Array.Resize(ref SckSs, SckCIndex + 1);
            }

            Thread SckSAcceptTd = new Thread(SckSAcceptProc);
            SckSAcceptTd.Start();  // 開始執行 SckSAcceptTd 這個執行緒


        }

        static private void SckSAcceptProc()
        {
            try
            {
                SckSs[SckCIndex] = SckSs[0].Accept();  // 等待Client 端連線
                int Scki = SckCIndex;
                SckSWaitAccept();
                long IntAcceptData;
                byte[] clientData = new byte[RDataLen];  // 其中RDataLen為每次要接受來自 Client 傳來的資料長度
                while (true)
                {
                    
                    IntAcceptData = SckSs[Scki].Receive(clientData);
                    string S = Encoding.Default.GetString(clientData);
                    Console.WriteLine(S);
                    Thread.Sleep(500);
                    SckSSend("FFEE&LIGHT&0&5&EEFF");

                    //if(S.Contains("0xF1"))
                    //{
                    //    SckSSend("FFEE&LIGHT&0&5&EEFF");
                    //}
                    //else
                    //{

                    //}

                }

            }

            catch
            {

                // 這裡若出錯主要是來自 SckSs[Scki] 出問題, 可能是自己 Close, 也可能是 Client 斷線, 自己加判斷吧~
            }
        }



        // Server 傳送資料給所有Client

        static private void SckSSend(string SendS)
        {
            for (int Scki = 1; Scki < SckSs.Length; Scki++)
            {
                if (null != SckSs[Scki] && SckSs[Scki].Connected == true)
                {
                    try
                    {
                         
                        SckSs[Scki].Send(Encoding.ASCII.GetBytes(SendS));
                    }
                    catch
                    {
                        // 這裡出錯, 主要是出在 SckSs[Scki] 出問題, 自己加判斷吧~

                    }

                }

            }

        }

    }
}
