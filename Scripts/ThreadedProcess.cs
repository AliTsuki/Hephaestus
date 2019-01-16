using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ThreadedProcess
{
    bool m_IsDone = false;
    object m_Handle = new object();
    Thread m_Thread = null;

    public bool IsDone
    {
        get
        {
            bool tmp;
            lock(m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set
        {
            lock(m_Handle)
            {
                m_IsDone = value;
            }
        }
    }

    public virtual void Start()
    {
        m_Thread = new Thread(Run);
        m_Thread.Start();
    }

    public virtual void Abort()
    {
        m_Thread.Abort();
    }

    public virtual void ThreadFunction() { }

    public virtual void OnFinished() { }

    public virtual bool Update()
    {
        if(IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    void Run()
    {
        ThreadFunction();
        IsDone = true;
    }
}
