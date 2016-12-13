using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using game.protobuf.data;
using System.IO;
using xk_System.Debug;
using UnityEngine;
using xk_System.Crypto;
using System.Collections;

namespace xk_System.Net
{
    public class NetManager : MonoBehaviour
    {
        public string ip = "192.168.1.7";
        public int port = 7878;
        // Use this for initialization
        private void Start()
        {
           // NetSystem.getSingle().init(ip, port);
        }

        // Update is called once per frame
        void Update()
        {
           // NetSystem.getSingle().ReceiveData();
        }

        private void OnDestroy()
        {
           // NetSystem.getSingle().CloseNet();
        }
    }

    internal  abstract class NetSystem
    {
        /// <summary>
        /// 不设置，则系统默认是8192
        /// </summary>
        protected const int receiveInfoPoolCapacity = 8192;
        protected const int sendInfoPoolCapacity = 8192;
        /// <summary>
        /// 毫秒数，不设置，系统默认为0
        /// </summary>
        protected const int receiveTimeOut = 10000;
        protected const int sendTimeOut = 5000;

        protected NetSendSystem mNetSendSystem;
        protected NetReceiveSystem mNetReceiveSystem;

        private static NetSystem single = null;
        protected Socket mSocket;
        protected Queue<string> mNetErrorQueue;

        static NetSystem_Thread single_Thread=new NetSystem_Thread();
        static NetSystem_1 single_1 = new NetSystem_1();

        public NetSystem()
        {
            mNetSendSystem = new NetSendSystem_Protobuf();
            mNetReceiveSystem = new NetReceiveSystem_Protobuf();
        }

        internal static T getSingle<T>() where T : NetSystem, new()
        {
            if (single == null)
            {
                single = new T();
            }
            return (T)single;
        }
        /// <summary>
        /// 默认调用处理类
        /// </summary>
        /// <returns></returns>
        internal static NetSystem_Thread getSingle()
        {
            return single_Thread;
        }

        public abstract void init(string ServerAddr, int ServerPort);

        protected abstract void ConnectServer();

        protected abstract void ReceiveInfo();

        internal abstract void SendInfo(byte[] msg);

        public void SendData(int command, object package)
        {
            mNetSendSystem.Send(this, command, package);
            if (mSocket.Connected)
            {
                
            }else
            {
                DebugSystem.LogError("Socket并未连接");
            }       
        }

        public void ReceiveData()
        {
            if (mSocket.Connected)
            {
                mNetReceiveSystem.HandleData();
            }
        }

        public void addListenFun(int command, Action<Package> fun)
        {
            mNetReceiveSystem.addListenFun(command,fun);
        }

        public void removeListenFun(int command, Action<Package> fun)
        {
            mNetReceiveSystem.removeListenFun(command, fun);
        }

        public virtual void CloseNet()
        {
            single = null;
           // mSocket.Shutdown(SocketShutdown.Receive);
            mSocket.Close();
            mSocket = null;
            mNetSendSystem.Destory();
            mNetReceiveSystem.Destory();
            DebugSystem.Log("关闭客户端TCP连接");
        }

    }
    /// <summary>
    /// 非线程网络实现方法
    /// </summary>
    internal class NetSystem_1:NetSystem
    {
        public override void init(string ServerAddr, int ServerPort)
        {
            try
            {
                IPEndPoint mIPEndPoint = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.Connect(mIPEndPoint);
                mSocket.ReceiveTimeout = receiveTimeOut;
                mSocket.SendTimeout = sendTimeOut;
                mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
                mSocket.SendBufferSize = sendInfoPoolCapacity;
                ConnectServer();
                DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
            }
            catch (SocketException e)
            {
                DebugSystem.LogError(e.SocketErrorCode + " | " + e.Message);
            }
            catch (Exception e)
            {
                DebugSystem.LogError("客户端初始化失败：" + e.Message);
            }
        }

        protected override void ConnectServer()
        {
           
        }

        protected override void ReceiveInfo()
        {

        }
        private ArrayList m_ReadFD = new ArrayList();
        private ArrayList m_WriteFD = new ArrayList();
        private ArrayList m_ExceptFD = new ArrayList();
        private bool Select()
        {
            try
            {
                this.m_ReadFD.Clear();
                this.m_WriteFD.Clear();
                this.m_ExceptFD.Clear();
                this.m_ReadFD.Add(this.mSocket);
                this.m_WriteFD.Add(this.mSocket);
                this.m_ExceptFD.Add(this.mSocket);
                Socket.Select(this.m_ReadFD, this.m_WriteFD, this.m_ExceptFD, 0);

            }catch(SocketException e)
            {
                DebugSystem.LogError(e.SocketErrorCode+" | "+e.Message);
                return false;
            }
            catch (Exception e)
            {
                DebugSystem.LogError(e.Message);
                return false;
            }
            return true;
        }
        private bool ProcessOutput()
        {
            if (this.m_WriteFD.Contains(this.mSocket))
            {
              //  DebugSystem.Log("Send");
                //SendInfo();
            }
            return true;
        }
        private bool ProcessInput()
        {
            if (this.m_ReadFD.Contains(this.mSocket))
            {
                byte[] mbyteStr = new byte[receiveInfoPoolCapacity];
                SocketError error;
                int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
                while (mSocket.Available > 0)
                {
                    for (int i = 0; i < Length; i++)
                    {
                        mStoreByteList.Add(mbyteStr[i]);
                    }
                    mbyteStr = new byte[receiveInfoPoolCapacity];
                    Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
                }

                byte[] mStr = null;
                if (mStoreByteList.Count > 0)
                {
                    for (int i = 0; i < Length; i++)
                    {
                        mStoreByteList.Add(mbyteStr[i]);
                    }
                    mStr = mStoreByteList.ToArray();
                    mStoreByteList.Clear();
                }
                else
                {
                    mStr = new byte[Length];
                    Array.Copy(mbyteStr, mStr, Length);
                }
                string Tag="收到消息:" + Length;
                DebugSystem.LogBitStream(Tag,mStr);
                mNetReceiveSystem.Receive(mStr);
            }
            return true;
        }

        private bool ProcessExcept()
        {
            if (this.m_ExceptFD.Contains(this.mSocket))
            {
                this.mSocket.Close();
                DebugSystem.Log("m_Socket.close(), NetSystem::ProcessExcept");
                return false;
            }
            return true;
        }

        List<byte> mStoreByteList = new List<byte>();

        public void Receive()
        {
            try
            {
                if ((this.mSocket != null) && this.mSocket.Connected)
                {
                    if (Select() && ProcessExcept())
                    {
                        ProcessInput();
                    }
                }
            }
            catch (SocketException e)
            {
                DebugSystem.LogError("接受异常： " + e.Message + " | " + e.SocketErrorCode);
                Thread.CurrentThread.Abort();
            }
            catch (Exception e)
            {
                DebugSystem.LogError("接受异常： " + e.Message);
            }
        }

        internal override void SendInfo(byte[] msg)
        {
            try
            {
                SocketError merror;
                mSocket.Send(msg, 0, msg.Length, SocketFlags.None, out merror);
                if (merror == SocketError.Success)
                {
                    string Tag = "发送消息:" + msg.Length;
                    DebugSystem.LogBitStream(Tag, msg);
                }
                else
                {
                    DebugSystem.LogError("发送失败: " + merror);
                }
            }
            catch (SocketException e)
            {
                DebugSystem.LogError(e.SocketErrorCode + " | " + e.Message);
            }
            catch (Exception e)
            {
                DebugSystem.LogError(e.Message);
            }
        }

        public override void CloseNet()
        {
            base.CloseNet();
        }

    }

    internal class NetSystem_2 : NetSystem
    {
        bool OrConnection = false;
        public override void init(string ServerAddr, int ServerPort)
        {
            try
            {
                IPEndPoint mIPEndPoint = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.Connect(mIPEndPoint);
                mSocket.ReceiveTimeout = receiveTimeOut;
                mSocket.SendTimeout = sendTimeOut;
                mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
                mSocket.SendBufferSize = sendInfoPoolCapacity;
                mSocket.Blocking = false;
                ConnectServer();
                DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
            }
            catch (SocketException e)
            {
                DebugSystem.LogError("客户端初始化失败000： " + e.SocketErrorCode + " | " + e.Message);
            }
            catch (Exception e)
            {
                DebugSystem.LogError("客户端初始化失败111：" + e.Message);
            }
        }

        protected override void ConnectServer()
        {
            
        }

        protected override void ReceiveInfo()
        {
            throw new NotImplementedException();
        }

        List<byte> mStoreByteList = new List<byte>();
        byte[] mbyteStr = new byte[receiveInfoPoolCapacity];

        public int Receive()
        {
            try
            {
                SocketError error;
                int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
                if (Length == -1)
                {
                    DebugSystem.Log(Length);
                }
                else if (Length == 0)
                {
                    if (error == SocketError.TimedOut)
                    {
                        DebugSystem.Log("连接超时");
                    }
                    else if (error == SocketError.Success)
                    {
                        DebugSystem.Log("服务器主动断开连接");
                        return -1;
                    }
                }
                else
                {
                    byte[] mStr = new byte[Length];
                    Array.Copy(mbyteStr, mStr, Length);

                    string Tag="收到消息:" + Length + " | " + mStr.Length + " | " + receiveInfoPoolCapacity;
                    DebugSystem.LogBitStream(Tag,mStr);
                    mNetReceiveSystem.Receive(mStr);
                }
            }
            catch (SocketException e)
            {
                DebugSystem.LogError("接受异常0000： " + e.Message + " | " + e.SocketErrorCode);
                return -1;
            }
            catch (Exception e)
            {
                DebugSystem.LogError("接受异常11111： " + e.Message + " | " + e.StackTrace);
                return -1;
            }
            return 0;

        }

        internal override void SendInfo(byte[] msg)
        {
            try
            {
                SocketError merror;
                mSocket.Send(msg, 0, msg.Length, SocketFlags.None, out merror);
                if (merror == SocketError.Success)
                {
                    string Tag="发送成功:" + msg.Length;
                    DebugSystem.LogBitStream(Tag,msg);
                }
                else
                {
                    DebugSystem.LogError("发送失败: " + merror);
                }

            }
            catch (SocketException e)
            {
                DebugSystem.LogError(e.SocketErrorCode + " | " + e.Message);
            }
            catch (Exception e)
            {
                DebugSystem.LogError(e.Message);
            }
        }

        public override void CloseNet()
        {
            base.CloseNet();
        }

    }

    /// <summary>
    /// 又可以用了，大哥
    /// 采取阻塞，线程方式，这种方法经测试是不可取的，原因在于Unity中几乎所有方法不是线程安全的
    /// 第一次是采用这个来连接网络，有些Unity方法，不能用在线程里使用:如协程
    /// </summary>
    internal class NetSystem_Thread : NetSystem
    {
        private List<Thread> ThreadPool;
        public override void init(string ServerAddr, int ServerPort)
        {
            try
            {
                IPEndPoint mIPEndPoint = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.Connect(mIPEndPoint);
                mSocket.ReceiveTimeout = receiveTimeOut;
                mSocket.SendTimeout = sendTimeOut;
                mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
                mSocket.SendBufferSize = sendInfoPoolCapacity;
                mSocket.Blocking = true;
                ConnectServer();
                DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
            }
            catch(SocketException e)
            {
                DebugSystem.LogError(e.SocketErrorCode+" | " + e.Message);
            }
            catch (Exception e)
            {
                DebugSystem.LogError("客户端初始化失败："+e.Message);
            }
        }

        protected override void ConnectServer()
        {
            NewStartThread_Receive();
        }

        private void NewStartThread_Receive()
        {
           // Thread mThread = new Thread(ReceiveInfo);
            Thread mThread = new Thread(Receive);
            mThread.IsBackground = false;
            mThread.Start();
            if (ThreadPool == null)
            {
                ThreadPool = new List<Thread>();
            }
            ThreadPool.Add(mThread);
        }

        List<byte> mStoreByteList = new List<byte>();
        byte[] mbyteStr = new byte[receiveInfoPoolCapacity];

        /// <summary>
        /// 用Socket.Avaiable，可以用来防止接受的流是个残废的流（不完整的流）比如(发了一条数据，不用Avaiable，则有可能得到的是一个多流加一个半流)
        /// </summary>
        protected override void ReceiveInfo()
        {
            while (mSocket!=null)
            {
                try
                {                                     
                    SocketError error;                  
                    int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);                   
                   // DebugSystem.LogBitStream(mbyteStr);
                   // DebugSystem.Log("Error: "+error);
                   // DebugSystem.Log("Available: " + mSocket.Available + " | " + Length);
                    if (Length == -1)
                    {
                        DebugSystem.LogError("接受长度："+Length);
                        CloseNet();
                        break;
                    }
                    else if (Length == 0)
                    {
                        if (error == SocketError.TimedOut)
                        {
                            //DebugSystem.LogError("连接超时");
                        }else if(error==SocketError.Success)
                        {
                            DebugSystem.LogError("服务器主动断开连接");
                            CloseNet();
                            break;
                        }
                    }
                    else if (mSocket.Available > 0)
                    {
                        for (int i = 0; i < Length; i++)
                        {
                            mStoreByteList.Add(mbyteStr[i]);
                        }
                    }
                    else
                    {
                        byte[] mStr = null;
                        if (mStoreByteList.Count > 0)
                        {
                            for (int i = 0; i < Length; i++)
                            {
                                mStoreByteList.Add(mbyteStr[i]);
                            }
                            mStr = mStoreByteList.ToArray();
                            mStoreByteList.Clear();
                        }
                        else
                        {
                            mStr = new byte[Length];
                            Array.Copy(mbyteStr, mStr, Length);
                        }
                        string Tag = "收到消息: " + Length + " | " + mStr.Length + " | " + receiveInfoPoolCapacity;
                        DebugSystem.LogBitStream(Tag, mStr);
                        mNetReceiveSystem.Receive(mStr);
                    }
                }
                catch (SocketException e)
                {
                    DebugSystem.LogError("接受异常0000： "+e.Message +" | "+e.SocketErrorCode);
                    break;
                }catch(Exception e)
                {
                    DebugSystem.LogError("接受异常11111： "+e.Message+" | "+e.StackTrace);
                    break;
                }
            }
            DebugSystem.LogError("网络线程结束");

        }

        private void Receive()
        {
            while (mSocket!=null)
            {
                try
                {
                    SocketError error;
                    int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
                    if (Length == -1)
                    {
                        DebugSystem.LogError("接受长度："+Length);
                        CloseNet();
                        break;
                    }
                    else if (Length == 0)
                    {
                        if (error == SocketError.TimedOut)
                        {
                            //DebugSystem.Log("连接超时");
                        }
                        else if (error == SocketError.Success)
                        {
                            DebugSystem.LogError("服务器主动断开连接");
                            CloseNet();
                            break;
                        }
                    }
                    else
                    {
                        byte[] mStr = new byte[Length];
                        Array.Copy(mbyteStr, mStr, Length);

                        string Tag="收到消息: " + Length+" | "+mStr.Length + " | " + receiveInfoPoolCapacity;
                        DebugSystem.LogBitStream(Tag,mStr);
                        mNetReceiveSystem.Receive(mStr);
                    }
                }
                catch (SocketException e)
                {
                    DebugSystem.LogError("接受异常0000： " + e.Message + " | " + e.SocketErrorCode);
                    break;
                }
                catch (Exception e)
                {
                    DebugSystem.LogError("接受异常11111： " + e.Message + " | " + e.StackTrace);
                    break;
                }
            }

           DebugSystem.LogError("网络线程结束");

        }

        internal override void SendInfo(byte[] msg)
        {
            try
            {
                SocketError merror;
                mSocket.Send(msg, 0, msg.Length, SocketFlags.None, out merror);
                string Tag = "";
                if (merror == SocketError.Success)
                {
                    Tag="发送成功:" + msg.Length;
                } else
                {
                    Tag="发送失败: " +merror;
                }
                DebugSystem.LogBitStream(Tag, msg);
            }
            catch(SocketException e)
            {
                DebugSystem.LogError(e.SocketErrorCode+" | "+e.Message);
            }catch(Exception e)
            {
                DebugSystem.LogError(e.Message);
            }
        }

        public override void CloseNet()
        {
            if (ThreadPool != null)
            {
                foreach (Thread t in ThreadPool)
                {
                    t.Abort();
                }
                ThreadPool.Clear();
            }
            base.CloseNet();     
        }

    }

    internal class NetSystem_Async:NetSystem
    {
        // 发送和接收的超时时间
        public int _sendTimeout = 3;
        public int _revTimeout = 3;

        public NetSystem_Async()
        {
            
        }

        public override void init(string ServerAddr, int ServerPort)
        {       
            if (mSocket != null && mSocket.Connected)
            {
                return;
            }
            try
            {
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.SendTimeout = _sendTimeout;
                mSocket.ReceiveTimeout = _revTimeout;

                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
                mSocket.BeginConnect(ipe, new System.AsyncCallback(ConnectionCallback), mSocket);
            }
            catch (System.Exception e)
            {
                DebugSystem.LogError(e.Message);

            }
        }

        protected override void ConnectServer()
        {

        }

        protected override void ReceiveInfo()
        {

        }

        internal override void SendInfo(byte[] msg)
        {
            NetBitStream mstream = new NetBitStream();
            mstream._bytes = msg;
            Send(mstream);
        }


        // 异步连接回调
        void ConnectionCallback(System.IAsyncResult ar)
        {
            Socket _socket = (Socket)ar.AsyncState;
            _socket.EndConnect(ar);
            try
            {
                NetBitStream stream = new NetBitStream();
                stream._socket = _socket;
                stream._socket.BeginReceive(stream._bytes, 0, stream.max_length, SocketFlags.None, new System.AsyncCallback(ReceiveInfo), stream);
            }
            catch (System.Exception e)
            {
                if (e.GetType() == typeof(SocketException))
                {
                    if (((SocketException)e).SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        
                    }
                    else
                    {

                    }
                }
                Disconnect(0);
            }
        }

        // 接收消息体
        void ReceiveInfo(System.IAsyncResult ar)
        {
            NetBitStream stream = (NetBitStream)ar.AsyncState;

            try
            {
                int read = stream._socket.EndReceive(ar);

                // 用户已下线
                if (read < 1)
                {
                    Disconnect(0);
                    return;
                }
                NetReceiveSystem.getSingle().Receive(stream._bytes);


                // 下一个读取
                mSocket.BeginReceive(stream._bytes, 0, stream.max_length, SocketFlags.None, new System.AsyncCallback(ReceiveInfo), stream);

            }
            catch (System.Exception e)
            {
                Disconnect(0);
            }
        }

        // 发送消息
        internal void Send(NetBitStream bts)
        {
            if (!mSocket.Connected)
                return;

            NetworkStream ns;
            lock (mSocket)
            {
                ns = new NetworkStream(mSocket);
            }

            if (ns.CanWrite)
            {
                try
                {
                    ns.BeginWrite(bts._bytes, 0, bts._Length, new System.AsyncCallback(SendCallback), ns);
                }
                catch (System.Exception)
                {
                    Disconnect(0);
                }
            }
        }

        //发送回调
        private void SendCallback(System.IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try
            {
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch (System.Exception)
            {
                Disconnect(0);
            }

        }

        // 关闭连接
        public void Disconnect(int timeout)
        {
            if (mSocket.Connected)
            {
                mSocket.Shutdown(SocketShutdown.Receive);
                mSocket.Close(timeout);
            }
            else
            {
                mSocket.Close();
            }

        }
    }

    internal class SocketSevice:NetSystem
    {
        SocketAsyncEventArgs ReceiveArgs;

        public override void init(string ServerAddr, int ServerPort)
        {
            try
            {
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress mIPAddress = IPAddress.Parse(ServerAddr);
                IPEndPoint mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort);
                mSocket.Connect(mIPEndPoint);
                ConnectServer();
                DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
            }
            catch (SocketException e)
            {
                DebugSystem.LogError("客户端初始化失败：" + e.Message +" | "+e.SocketErrorCode);
            }
        }

        protected override void ConnectServer()
        {
            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.Completed += Receive_Fun;
            ReceiveArgs.SetBuffer(new byte[receiveInfoPoolCapacity], 0, receiveInfoPoolCapacity);
            mSocket.ReceiveAsync(ReceiveArgs);
        }

        protected override void ReceiveInfo()
        {

        }

        internal override void SendInfo(byte[] msg)
        {
            SocketError mError=SocketError.SocketError;
            try
            {
                mSocket.Send(msg,0,msg.Length,SocketFlags.None,out mError);
            }catch(Exception e)
            {
                DebugSystem.LogError("发送字节失败： "+e.Message+" | "+mError.ToString());
            }
        }

        private List<byte> mStorebyteList = new List<byte>();

        private void Receive_Fun(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                   // DebugSystem.Log("接收数据个数： " + e.BytesTransferred);
                   // DebugSystem.Log("接收数据个数： " + e.Buffer.Length);
                   // DebugSystem.Log("接收数据个数： " + mSocket.Available);
                    if (mSocket.Available > 0)
                    {
                        foreach (byte b in e.Buffer)
                        {
                            mStorebyteList.Add(b);
                        }
                       // DebugSystem.LogError("传输字节数超出缓冲数组: "+mSocket.Available);
                    }
                    else
                    {
                        byte[] mbyteArray = null;
                        if (mStorebyteList.Count > 0)
                        {
                            for (int i = 0; i < e.BytesTransferred; i++)
                            {
                                mStorebyteList.Add(e.Buffer[i]);
                            }
                            mbyteArray = mStorebyteList.ToArray();
                            mStorebyteList.Clear();
                        }
                        else
                        {
                            mbyteArray = new byte[e.BytesTransferred];
                            Array.Copy(e.Buffer,mbyteArray,mbyteArray.Length);                         
                        }
                        mNetReceiveSystem.Receive(mbyteArray);
                    }             
                }
            }else
            {
                DebugSystem.Log("接收数据失败： " + e.SocketError.ToString());
            }
            mSocket.ReceiveAsync(e);
        }


        public override void CloseNet()
        {
            base.CloseNet();
        }


    }
    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    internal class NetSendReceiveSystem
    {
        public const bool OrEncryptionBitStream= true;
    }


    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    internal abstract class  NetSendSystem
    {
        private static NetSendSystem single;
        static NetSendSystem_Protobuf single_Protobuf = new NetSendSystem_Protobuf();
        protected PackageSendPool mSendPool = new PackageSendPool();
        protected Package mPackage=new Protobuf();  
        protected NetSendSystem()
        {
            
        }

        public static T getSingle<T>() where T:NetSendSystem,new()
        {
            if(single==null)
            {
                single = new T();
            }
            return (T)single;
        }

        public static NetSendSystem_Protobuf getSingle()
        {           
            return single_Protobuf;
        }

        public abstract void Send(NetSystem mNetSystem,int command,object data);

        public virtual void Destory()
        {
            mPackage.Destory();
        }
    }

    internal class NetSendSystem_Protobuf : NetSendSystem
    {
        public NetSendSystem_Protobuf():base()
        {

        }
        public override void Send(NetSystem mNetSystem, int command,object data)
        {
            byte[] stream= mPackage.SerializePackage(command,data);
            mSendPool.SendPackage(mNetSystem,stream);
        }

    }

    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public abstract class NetReceiveSystem
    {
        protected Dictionary<int, Action<Package>> mReceiveDic;
        protected Queue<Package> mNeedHandlePackageQueue;
        protected Queue<Package> mCanUsePackageQueue;
        private static NetReceiveSystem single;
        private static NetReceiveSystem_Protobuf single_Protobuf=new NetReceiveSystem_Protobuf();

        protected NetReceiveSystem()
        {
            mReceiveDic = new Dictionary<int, Action<Package>>();
            mNeedHandlePackageQueue = new Queue<Package>();
            mCanUsePackageQueue = new Queue<Package>();
        }

        public static T getSingle<T>() where T : NetReceiveSystem, new()
        {
            if (single == null)
            {
                single = new T();
            }
            return (T)single;
        }

        public static NetReceiveSystem_Protobuf getSingle()
        {
            return single_Protobuf;
        }

        public void addListenFun(int command, Action<Package> fun)
        {
            lock (mReceiveDic)
            {
                if (mReceiveDic.ContainsKey(command))
                {
                    if (CheckDataBindFunIsExist(command, fun))
                    {
                        DebugSystem.LogError("添加监听方法重复");
                        return;
                    }
                    mReceiveDic[command] += fun;
                }
                else
                {
                    mReceiveDic[command] = fun;
                }              
            }
        }

        private bool CheckDataBindFunIsExist(int command,Action<Package> fun)
        {
            Action<Package> mFunList = mReceiveDic[command];
            return DelegateUtility.CheckFunIsExist<Package>(mFunList, fun);
        }

        public void removeListenFun(int command, Action<Package> fun)
        {
            lock (mReceiveDic)
            {
                if (mReceiveDic.ContainsKey(command))
                {
                    mReceiveDic[command]-=fun;
                }
            }
        }
        public abstract void Receive(byte[] msg);

        public void HandleData()
        {
            if (mNeedHandlePackageQueue != null)
            {
                lock (mNeedHandlePackageQueue)
                {
                    while (mNeedHandlePackageQueue.Count > 0)
                    {
                        Package mPackage = mNeedHandlePackageQueue.Dequeue();
                        if (mReceiveDic.ContainsKey(mPackage.command))
                        {
                            mReceiveDic[mPackage.command](mPackage);
                        }
                        else
                        {
                            DebugSystem.LogError("没有找到相关命令的处理函数：" + mPackage.command);
                        }
                        lock(mCanUsePackageQueue)
                        {
                            mCanUsePackageQueue.Enqueue(mPackage);
                        }
                    }
                }
            }
        }

        public virtual void Destory()
        {
            lock(mNeedHandlePackageQueue)
            {
                while (mNeedHandlePackageQueue.Count > 0)
                {
                    Package mPackage = mNeedHandlePackageQueue.Dequeue();
                    mPackage.Destory();
                }
            }

            lock (mCanUsePackageQueue)
            {
                while (mCanUsePackageQueue.Count > 0)
                {
                    Package mPackage = mCanUsePackageQueue.Dequeue();
                    mPackage.Destory();
                }
            }
        }
    }

    public class NetReceiveSystem_Protobuf:NetReceiveSystem
    {
        private PackageReceivePool mPackageReceivePool = new PackageReceivePool();
        public NetReceiveSystem_Protobuf()
        {
            
        }

        public override void Receive(byte[] msg)
        {
            mPackageReceivePool.ReceiveInfo(msg);
            int PackageCout = 0;
            while (true)
            {
                byte[] mPackageByteArray= mPackageReceivePool.GetPackage();
                if (mPackageByteArray != null)
                {
                    Package mPackage = null;
                    lock (mCanUsePackageQueue)
                    {
                        if (mCanUsePackageQueue.Count == 0)
                        {
                            mPackage = new Protobuf();
                        }
                        else
                        {
                            mPackage = mCanUsePackageQueue.Dequeue();
                        }
                    }
                    mPackage.DeSerializeStream(mPackageByteArray);
                    lock (mNeedHandlePackageQueue)
                    {
                        mNeedHandlePackageQueue.Enqueue(mPackage);
                    }
                    PackageCout++;
                }else
                {
                    break;
                }
            }
            DebugSystem.LogError("解析包的数量： " + PackageCout);
        }
    }

    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络包体结构系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public abstract class Package
    {
        internal int command;
        protected byte[] data_byte=null;

        internal abstract byte[] SerializePackage(int command,object data);

        internal abstract void DeSerializeStream(byte[] msg);

        public abstract void getData<T>(T t) where T : new();

        public virtual void Destory()
        {

        }
    }

    public class Protobuf : Package
    {
        private GameProtocols serializer = new GameProtocols();
        private MemoryStream mst = new MemoryStream();
        NetOutputStream mOutputStream = new NetOutputStream();
        NetInputStream mInputStream = new NetInputStream();

        internal override byte[] SerializePackage(int command,object data)
        {
            //DebugSystem.Log("Send MemoryStream Length: "+mst.Length);
            mst.Position = 0;
            serializer.Serialize(mst, data);
            data_byte = mst.ToArray();
           /* if (data_byte.Length==0)
            {
                DebugSystem.LogError("序列化失败");
            }*/
            mOutputStream.SetData(command,data_byte);
            return mOutputStream.data;
        }

        internal override void DeSerializeStream(byte[] msg)
        {
            mInputStream.SetData(msg);
            data_byte = mInputStream.buffer;
            command = mInputStream.command;
           // DebugSystem.Log("接受命令："+command+" | "+data_byte.Length);
        }

        public override void getData<T>(T t)
        {
           // DebugSystem.Log("Receive MemoryStream Length: " + mst.Length);
            mst.SetLength(data_byte.Length);
            mst.Position = 0;
            mst.Write(data_byte,0,data_byte.Length);
            mst.Position = 0;
            serializer.Deserialize(mst,t, typeof(T)); //反序列化    
        }

        public override void Destory()
        {
            base.Destory();
            mst.Close();
        }
    }
    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受包信息池系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    internal class PackageReceivePool : NetEncryptionInputStream
    {
        private List<byte> mReceiveStreamQueue = new List<byte>();
       
        public void ReceiveInfo(byte[] mbyteArray)
        {
            mReceiveStreamQueue.AddRange(mbyteArray);
        }

        public byte[] GetPackage()
        {
            byte[] msg = mReceiveStreamQueue.ToArray();
            SetData(msg);
            if(BodyData==null)
            {
                return BodyData;
            }
            int Length = BodyData.Length + stream_head_Length + stream_tail_Length + msg_head_BodyLength;
            mReceiveStreamQueue.RemoveRange(0, Length);
            return BodyData;
        }
    }

    internal class PackageSendPool : NetEncryptionOutStream
    {
        public  void SendPackage(NetSystem mNetSystem,byte[] data)
        {
            SetData(data);
            mNetSystem.SendInfo(Encryption_data);
        }
    }

    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络编码输入输出流系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    internal class NetEncryptionStream
    {
        public const int stream_head_Length = 2;
        public const int stream_tail_Length = 2;

        public const int msg_head_BodyLength = 4;

    }

    /// <summary>
    /// 把数据拿出来
    /// </summary>
    internal class NetEncryptionInputStream:NetEncryptionStream
    {
       public byte[] BodyData=null;
       private byte[] mStreamHeadArray = new byte[stream_head_Length] { 7, 7 };
       private byte[] mStreamTailArray = new byte[stream_head_Length] { 7, 7 };

        public void SetData(byte[] data)
        {
            BodyData = null;
            if(data.Length-msg_head_BodyLength-stream_head_Length-stream_tail_Length<=0)
            {
                return;
            }
            byte[] mStreamHeadArray1 = new byte[stream_head_Length];
           // Array.Copy(data,0, mStreamHeadArray1,0,stream_head_Length);

            byte[] msg_BodyLength_Array= new byte[msg_head_BodyLength];
            Array.Copy(data, stream_head_Length,msg_BodyLength_Array, 0,msg_head_BodyLength);

            byte[] mStreamTailArray1 = new byte[stream_tail_Length];
            //Array.Copy(data,stream_head_Length+msg_head_BodyLength,mStreamTailArray1,0,stream_tail_Length);

            int Length = msg_BodyLength_Array[0] | msg_BodyLength_Array[1] << 8 | msg_BodyLength_Array[2] << 16 | msg_BodyLength_Array[3] << 24;
            if(Length<=0 || data.Length-msg_head_BodyLength-stream_head_Length-stream_tail_Length-Length<0)
            {
                return;
            }
            BodyData = new byte[Length];
            Array.Copy(data,stream_head_Length+msg_head_BodyLength,BodyData,0,Length);
        }
    }

    /// <summary>
    /// 为数据加个标志
    /// </summary>
    internal class NetEncryptionOutStream : NetEncryptionStream
    {
        public byte[] Encryption_data;
        byte[] mStreamHeadArray = new byte[stream_head_Length] { 7, 7 };
        byte[] mStreamTailArray = new byte[stream_head_Length] { 7, 7 };
        /// <summary>
        /// data 为加密数据流
        /// </summary>
        /// <param name="data"></param>
        public void SetData(byte[] data)
        {
            Encryption_data = null;
            int buffer_Length = data.Length;
            byte[] byte_head_BufferLength = new byte[msg_head_BodyLength];
            byte_head_BufferLength[0] = (byte)buffer_Length;
            byte_head_BufferLength[1] = (byte)(buffer_Length >> 8);
            byte_head_BufferLength[2] = (byte)(buffer_Length >> 16);
            byte_head_BufferLength[3] = (byte)(buffer_Length >> 24);

            Encryption_data = new byte[buffer_Length + msg_head_BodyLength+stream_head_Length+stream_tail_Length];
            Array.Copy(mStreamHeadArray, Encryption_data, stream_head_Length);
            Array.Copy(byte_head_BufferLength, 0,Encryption_data,stream_head_Length,msg_head_BodyLength);
            Array.Copy(data,0,Encryption_data,stream_head_Length+msg_head_BodyLength,buffer_Length);
            Array.Copy(mStreamTailArray,0,Encryption_data,msg_head_BodyLength+stream_head_Length+buffer_Length,stream_tail_Length);
        }

    }



    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络字节输入输出流系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    internal class NetStream:NetBitStream
    {
       public const int msg_head_command_length = 4;

       public  string Encrytption_Key= "1234567891234567";

       public  string Encrytption_iv = "1234567891234567";

    }

    internal class NetInputStream:NetStream
    {
        public byte[] buffer = null;
        public int command = -1;
        public NetInputStream()
        {
              buffer = null;
              command = -1;
        }

        public NetInputStream(byte[] data)
        {
            SetData(data);
        }

        private NetEncryptionInputStream mEncryptionStream = new NetEncryptionInputStream();
        public void SetData(byte[] data)
        {
            byte[] msg = EncryptionSystem.getSingle<Encryption_AES>().Decryption(data, Encrytption_Key, Encrytption_iv);

            int buffer_Length = msg.Length - msg_head_command_length;
            if(buffer_Length<=0)
            {
                command = -1;
                buffer = new byte[msg.Length];
                DebugSystem.LogError("接受数据异常："+msg.Length);
            }

            byte[] byte_head_command = new byte[msg_head_command_length];
            Array.Copy(msg, 0, byte_head_command, 0, msg_head_command_length);
            command = byte_head_command[0] | byte_head_command[1] << 8 | byte_head_command[2] << 16 | byte_head_command[3] << 24;

            buffer = new byte[buffer_Length];
            Array.Copy(msg, msg_head_command_length, buffer,0,buffer_Length);
        }


    }

    internal class NetOutputStream:NetStream
    {
        public byte[] data=null;
        private NetEncryptionOutStream mEncryptionStream = new NetEncryptionOutStream();
        public NetOutputStream()
        {
            data = null;
        }
        public NetOutputStream(int command,byte[] msg)
        {
            SetData(command,msg);
        }

        public void SetData(int command, byte[] msg)
        {
            if (msg == null || msg.Length == 0)
            {
                DebugSystem.LogError("发送数据失败：msg is Null Or Length is zero");
                return;
            }
            int buffer_Length = msg.Length;
            int sum_Length = msg_head_command_length + buffer_Length;
            data = new byte[sum_Length];

            byte[] byte_head_command = new byte[msg_head_command_length];
            byte_head_command[0] = (byte)command;
            byte_head_command[1] = (byte)(command >> 8);
            byte_head_command[2] = (byte)(command >> 16);
            byte_head_command[3] = (byte)(command >> 24);

            Array.Copy(byte_head_command, 0, data, 0, msg_head_command_length);
            Array.Copy(msg, 0, data, msg_head_command_length, buffer_Length);
           // DebugSystem.LogColor("发送Protobuf数据：");
           // DebugSystem.LogBitStream(data);
            data = EncryptionSystem.getSingle<Encryption_AES>().Encryption(data, Encrytption_Key, Encrytption_iv);
        }
    }


    internal class NetBitStream
    {
        public int max_length = 4096;

        public int _Length = 0;

        public const int BYTE_LEN = 1;

        public const int INT32_LEN = 4;

        public const int SHORT16_LEN = 2;

        public const int FLOAT_LEN = 4;

        public byte[] _bytes = null;

        public Socket _socket = null;

        public NetBitStream()
        {
            _Length = 0;
            _bytes = new byte[max_length];
        }

        public NetBitStream(int maxLength)
        {
            max_length = maxLength;
            _bytes = new byte[max_length];
        }

        // 写一个byte
        public void WriteByte(byte bt)
        {
            if (_Length> max_length)
                return;

            _bytes[_Length] = bt;
            _Length += BYTE_LEN;
        }


        // 写布尔型
        public void WriteBool(bool flag)
        {
            if (_Length + BYTE_LEN > max_length)
                return;

            // bool型实际是发送一个byte的值,判断是true或false
            byte b = (byte)'1';
            if (!flag)
                b = (byte)'0';

            _bytes[ _Length] = b;

            _Length += BYTE_LEN;
        }

        // 写整型
        public void WriteInt(int number)
        {
            if (_Length + INT32_LEN > max_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);

            bs.CopyTo(_bytes,  _Length);

            _Length += INT32_LEN;
        }

        // 写无符号整型
        public void WriteUInt(uint number)
        {
            if (_Length + INT32_LEN > max_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);

            bs.CopyTo(_bytes,  _Length);

            _Length += INT32_LEN;
        }


        // 写短整型
        public void WriteShort(short number)
        {
            if (_Length + SHORT16_LEN > max_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);

            bs.CopyTo(_bytes,  _Length);

            _Length += SHORT16_LEN;
        }

        // 写无符号短整型
        public void WriteUShort(ushort number)
        {
            if (_Length + SHORT16_LEN > max_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);

            bs.CopyTo(_bytes,  _Length);

            _Length += SHORT16_LEN;
        }


        //写浮点型 
        public void WriteFloat(float number)
        {
            if (_Length + FLOAT_LEN > max_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);

            bs.CopyTo(_bytes,  _Length);

            _Length += FLOAT_LEN;
        }


        // 写字符串
        public void WriteString(string str)
        {
            ushort len = (ushort)System.Text.Encoding.UTF8.GetByteCount(str);
            this.WriteUShort(len);

            if (_Length + len > max_length)
                return;

             System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, _bytes,  + _Length);
            _Length += len;

        }

        // 读一个字节
        public void ReadByte(out byte bt)
        {
            bt = 0;

            if (_Length + BYTE_LEN > max_length)
                return;

            bt = _bytes[_Length];

            _Length += BYTE_LEN;

        }

        // 读 bool
        public void ReadBool(out bool flag)
        {
            flag = false;

            if (_Length + BYTE_LEN > max_length)
                return;

            byte bt = _bytes[_Length];

            if (bt == (byte)'1')
                flag = true;
            else
                flag = false;

            _Length += BYTE_LEN;

        }



        // 读 int
        public void ReadInt(out int number)
        {
            number = 0;

            if (_Length + INT32_LEN > max_length)
                return;

            number = System.BitConverter.ToInt32(_bytes,  _Length);

            _Length += INT32_LEN;

        }

        // 读 uint
        public void ReadUInt(out uint number)
        {
            number = 0;

            if (_Length + INT32_LEN > max_length)
                return;

            number = System.BitConverter.ToUInt32(_bytes,  _Length);

            _Length += INT32_LEN;

        }

        // 读 short
        public void ReadShort(out short number)
        {
            number = 0;

            if (_Length + SHORT16_LEN > max_length)
                return;


            number = System.BitConverter.ToInt16(_bytes,  _Length);

            _Length += SHORT16_LEN;

        }

        // 读 ushort
        public void ReadUShort(out ushort number)
        {
            number = 0;

            if (_Length + SHORT16_LEN > max_length)
                return;


            number = System.BitConverter.ToUInt16(_bytes , _Length);

            _Length += SHORT16_LEN;
        }



        // 读取一个float
        public void ReadFloat(out float number)
        {
            number = 0;

            if (_Length + FLOAT_LEN > max_length)
                return;

            number = System.BitConverter.ToSingle(_bytes, _Length);

            _Length += FLOAT_LEN;

        }

        // 读取一个字符串
        public void ReadString(out string str)
        {
            str = "";

            ushort len = 0;
            ReadUShort(out len);

            if (_Length + len > max_length)
                return;

            //str = Encoding.UTF8.GetString(_bytes,  + _Length, (int)len);
            str = Encoding.Default.GetString(_bytes,  _Length, len);
            _Length += len;

        }
    }
}
