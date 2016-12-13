using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using game.protobuf.data;
using System.IO;

namespace Test
{
    class Class1
    {
        static void Main()
        {
            /* Console.BackgroundColor = ConsoleColor.Green;
             Console.WriteLine("序列化开始");
             Console.ResetColor();
             ClientSendData mSenddata = new ClientSendData();
             mSenddata.SenderName = "xuke";
             mSenddata.TalkMsg = "Protobuf Study";

             GameProtocols mm = new GameProtocols();

             MemoryStream ms = new MemoryStream();
             mm.Serialize(ms,mSenddata);

             Console.BackgroundColor = ConsoleColor.Blue;
             Console.WriteLine("反序列化开始");
             Console.ResetColor();
             ClientSendData mSendData = new ClientSendData();
             DateTime mtime1 = DateTime.Now;
             for (int i = 0; i < 1000; i++)
             {
                 MemoryStream mm1 = new MemoryStream(ms.ToArray());
                // ClientSendData mSendData = new ClientSendData();
                 object data = mm.Deserialize(mm1, mSendData, typeof(ClientSendData));
                 ClientSendData mReceiveData = new ClientSendData();
                 mReceiveData = (ClientSendData)data;
                 Console.WriteLine(mReceiveData.SenderName + " | " + mReceiveData.TalkMsg);
             }
             DateTime mtime2 = DateTime.Now;
             Console.WriteLine("花费了多长时间："+(mtime2 - mtime1).TotalSeconds);
             Console.WriteLine("创建了多少对象："+GameProtocols.CreateNewObjectCout);*/

            long mtime1 = DateTime.Now.Ticks;
            AAA m = new AAA();
            for (int i = 0; i < 1000000; i++)
            {
                //AAA m = new AAA();
                m.i += 10;
            }
            long mtime2 = DateTime.Now.Ticks;
            Console.WriteLine("花费了多长时间：" + (mtime2 - mtime1));
           // Console.WriteLine(""+m.i);
        }
    }

    class AAA
    {
        public int[] mm = new int[1000];
        public int i;
    }
}
